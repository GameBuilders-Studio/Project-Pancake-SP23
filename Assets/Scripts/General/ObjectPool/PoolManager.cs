using System;
using System.Collections;
using System.Collections.Generic;
using GameBuilders.Collections;
using UnityEngine;

[System.Serializable]
public struct InitialPooledItem
{
	public string name;
	public GameObject prefab;
	public int size;
}

public class PoolManager : Singleton<PoolManager>
{
	public bool logStatus;
	public Transform root;

	public InitialPooledItem[] initialPooledItems;

	private Dictionary<GameObject, ObjectPool<GameObject>> prefabLookup;
	private Dictionary<GameObject, ObjectPool<GameObject>> instanceLookup; 
	public Dictionary<String, GameObject> namedPrefabLookup;
	
	private bool dirty = false;
	
	void Awake()
	{
		prefabLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
		instanceLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
		namedPrefabLookup = new Dictionary<String, GameObject>();

		foreach (InitialPooledItem item in initialPooledItems)
		{
			warmPool(item.prefab, item.size);
			namedPrefabLookup[item.name] = item.prefab;
		}
	}

	void Update()
	{
		if (logStatus && dirty)
		{
			PrintStatus();
			dirty = false;
		}
	}

	public void warmPool(GameObject prefab, int size)
	{
		if (prefabLookup.ContainsKey(prefab))
		{
			throw new Exception("Pool for prefab " + prefab.name + " has already been created");
		}
		var pool = new ObjectPool<GameObject>(() => { return InstantiatePrefab(prefab); }, size);
		prefabLookup[prefab] = pool;
		dirty = true;
	}

	public GameObject spawnObject(GameObject prefab)
	{
		return spawnObject(prefab, Vector3.zero, Quaternion.identity);
	}

	public GameObject spawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		if (!prefabLookup.ContainsKey(prefab))
		{
			warmPool(prefab, 1);
		}

		ObjectPool<GameObject> pool = prefabLookup[prefab];

		var clone = pool.GetItem();
		clone.transform.SetPositionAndRotation(position, rotation);
		clone.SetActive(true);

		instanceLookup.Add(clone, pool);
		dirty = true;
		return clone;
	}

	IEnumerator ReleaseAfterTime(GameObject clone, float time)
	{
		yield return new WaitForSeconds(time);
		releaseObject(clone);
	}

	public void releaseObject(GameObject clone)
	{
		clone.SetActive(false);
		if (instanceLookup.ContainsKey(clone))
		{
			instanceLookup[clone].ReleaseItem(clone);
			instanceLookup.Remove(clone);
			clone.transform.SetParent(root);
			dirty = true;
		}
		else
		{
			Debug.LogWarning("No pool contains the object: " + clone.name, clone);
		}
	}

	public void releaseAllObjects()
	{
		foreach (var go in new List<GameObject>(instanceLookup.Keys))
		{
			releaseObject(go);
		}
	}

	private GameObject InstantiatePrefab(GameObject prefab)
	{
		var go = Instantiate(prefab) as GameObject;
		if (root != null) { go.transform.SetParent(root); }
		go.SetActive(false);
		return go;
	}

	public void PrintStatus()
	{
		foreach (var keyValuePair in prefabLookup)
		{
			Debug.Log(string.Format(
				"Object Pool for Prefab: {0} In Use: {1} Total {2}", 
				keyValuePair.Key.name, keyValuePair.Value.CountUsedItems, keyValuePair.Value.Count
			));
		}
	}

	#region Static API

	public static void WarmPool(GameObject prefab, int size)
	{
		Instance.warmPool(prefab, size);
	}

	public static GameObject SpawnObject(GameObject prefab)
	{
		return Instance.spawnObject(prefab);
	}

	public static GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		return Instance.spawnObject(prefab, position, rotation);
	}

	public static GameObject SpawnByName(string name)
	{
		if (!Instance.namedPrefabLookup.ContainsKey(name))
		{
			Debug.LogError($"Pooled object {name} does not exist!");
			return null;
		}
		GameObject go = Instance.namedPrefabLookup[name];
		return Instance.spawnObject(go);
	}

	// avoid this unless you are short on time!
	public static GameObject SpawnByName(String name, Vector3 position, Quaternion rotation)
	{
		if (!Instance.namedPrefabLookup.ContainsKey(name))
		{
			Debug.LogError($"Pooled object {name} does not exist!");
			return null;
		}
		GameObject go = Instance.namedPrefabLookup[name];
		return Instance.spawnObject(go, position, rotation);
	}

	public static void ReleaseObject(GameObject clone)
	{
		Instance.releaseObject(clone);
	}

	public static void ReleaseObject(GameObject clone, float delaySeconds)
	{
		Instance.StartCoroutine(Instance.ReleaseAfterTime(clone, delaySeconds));
	}

	public static void ReleaseAllObjects()
	{
		Instance.releaseAllObjects();
	}

	#endregion
}