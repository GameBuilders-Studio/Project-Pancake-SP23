using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Linq;
using System.IO;

namespace Autotiles3D
{
	public static class Autotiles3D_MeshCombiner
	{
		public static bool is32bit = true;

		/// <summary>
		/// Bakes meshes found in the specified gameobjects children
		/// </summary>
		/// <param name="roots">algorithm looks down the whole hierarchy of the roots to find meshfilters</param>
		/// <param name="newParent">new parent of the created mesh</param>
		/// <param name="path">path to which the baked mesh will be saved as an asset</param>
		/// <returns></returns>
		public static bool CombineMeshes(List<GameObject> roots, ref GameObject newParent, string path)
		{
			// Verify root objects
			if (roots.Where(r=>r == null).ToList().Count > 0 || newParent == null)
			{
				Debug.LogError("Autotiles/MeshCombiner: Invalid root objects");
				return false;
			}

			// Remember the original position of the object. 
			// For the operation to work, the position must be temporarily set to (0,0,0).
			Vector3 originalPosition = newParent.transform.position;
			newParent.transform.position = Vector3.zero;

			// Locals
			Dictionary<Material, List<MeshFilter>> materialToMeshFilterList = new Dictionary<Material, List<MeshFilter>>();
			List<GameObject> combinedObjects = new List<GameObject>();

			MeshFilter[] meshFilters = roots.SelectMany(v => v.GetComponentsInChildren<MeshFilter>(includeInactive: true)).ToArray();

			// Go through all mesh filters and establish the mapping between the materials and all mesh filters using it.
			foreach (var meshFilter in meshFilters)
			{
				var meshRenderer = meshFilter.GetComponent<MeshRenderer>();
				if (meshRenderer == null)
				{
					Debug.LogWarning("Autotiles/MeshCombiner: The Mesh Filter on object " + meshFilter.name + " has no Mesh Renderer component attached. Skipping.");
					continue;
				}

				var materials = meshRenderer.sharedMaterials;
				if (materials == null)
				{
					Debug.LogWarning("Autotiles/MeshCombiner: The Mesh Renderer on object " + meshFilter.name + " has no material assigned. Skipping.");
					continue;
				}

				// If there are multiple materials on a single mesh, cancel.
				if (materials.Length > 1)
				{
					// Rollback: return the object to original position
					newParent.transform.position = originalPosition;
					Debug.LogError("Autotiles/MeshCombiner: Objects with multiple materials on the same mesh are not supported. Create multiple meshes from this object's sub-meshes in an external 3D tool and assign separate materials to each. Operation cancelled.");
					return false;
				}
				var material = materials[0];

				// Add material to mesh filter mapping to dictionary
				if (materialToMeshFilterList.ContainsKey(material)) materialToMeshFilterList[material].Add(meshFilter);
				else materialToMeshFilterList.Add(material, new List<MeshFilter>() { meshFilter });
			}

			// For each material, create a new merged object, in the scene and in the assets folder.
			foreach (var entry in materialToMeshFilterList)
			{
				List<MeshFilter> meshesWithSameMaterial = entry.Value;
				// Create a convenient material name
				string materialName = entry.Key.ToString().Split(' ')[0];

				CombineInstance[] combine = new CombineInstance[meshesWithSameMaterial.Count];
				for (int i = 0; i < meshesWithSameMaterial.Count; i++)
				{
					combine[i].mesh = meshesWithSameMaterial[i].sharedMesh;
					combine[i].transform = meshesWithSameMaterial[i].transform.localToWorldMatrix;
				}

				// Create a new mesh using the combined properties
				var format = is32bit ? IndexFormat.UInt32 : IndexFormat.UInt16;
				Mesh combinedMesh = new Mesh { indexFormat = format };
				combinedMesh.CombineMeshes(combine);

				// Create asset
				materialName += "_" + combinedMesh.GetInstanceID();
				string fullPath = Application.dataPath;
				int index = fullPath.LastIndexOf("/");
				fullPath = fullPath.Substring(0, index) + "/" + path;
				if (!Directory.Exists(fullPath))
				{
					//if it doesn't, create it
					Directory.CreateDirectory(fullPath);
				}
				AssetDatabase.CreateAsset(combinedMesh, path + "/" + materialName + ".asset");

				// Create game object
				string goName = (materialToMeshFilterList.Count > 1) ? "CombinedMeshes_" + materialName : "CombinedMeshes_" + newParent.name;
				GameObject combinedObject = new GameObject(goName);
				var filter = combinedObject.AddComponent<MeshFilter>();
				filter.sharedMesh = combinedMesh;
				var renderer = combinedObject.AddComponent<MeshRenderer>();
				renderer.sharedMaterial = entry.Key;
				combinedObjects.Add(combinedObject);
			}
	
			//set parents for combinedObjects
			foreach (var combinedObject in combinedObjects)
				combinedObject.transform.parent = newParent.transform;

			//// Create prefab
			//Object prefab = PrefabUtility.CreateEmptyPrefab("Assets/" + resultGO.name + ".prefab");
			//PrefabUtility.ReplacePrefab(resultGO, prefab, ReplacePrefabOptions.ConnectToPrefab);

			// Disable the original and return both to original positions
			newParent.transform.position = originalPosition;
			return true;
		}
	}

}