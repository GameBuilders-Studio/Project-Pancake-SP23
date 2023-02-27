#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Autotiles3D
{
    public static class Autotiles3D_ExtensionUtility
    {
        public static void DeleteInstance(this InternalNode node)
        {
            if (node.Instance != null)
            {
                node.Instance.SetActive(true);
                if (Autotiles3D_Settings.EditorInstance.UseUndoAPI)
                    Undo.DestroyObjectImmediate(node.Instance);
                else
                    GameObject.DestroyImmediate(node.Instance);
            }
        }

        public static void DisableInstance(this InternalNode node)
        {
            if (node.Instance != null)
                node.Instance.SetActive(false);
        }
        public static void EnableInstance(this InternalNode node)
        {
            if (node.Instance != null)
                node.Instance.SetActive(true);
        }

        public static void ResetInstance(this InternalNode node)
        {
            node.DeleteInstance();
            node.UpdateInstance();
        }

        /// <summary>
        /// similar to update instance, but will try to keep the current skin/gameobject if possible. will change instance if necessary because of neighbor changes
        /// </summary>
        /// <param name="node"></param>
        public static void VerifyInstance(this InternalNode node)
        {
            if (node.Instance == null)
            {
                node.UpdateInstance();
            }
            else
            {
                //dont allow baked node to update
                if (node.Block != null && node.Block.IsBaked)
                    return;

                GameObject prefab = null;
                var tile = node.GetTile();
                if (tile == null)
                {
                    Debug.LogError("Autotiles3D: Internal node  is missing tile");
                    return;
                }
                int[] addedRotation = new int[] { -1, 0 };
                if (tile.HasRules)
                {
                    bool[] neighbors = node.Layer.GetNeighborsBoolSelfSpace(node.InternalPosition, node.LocalRotation);
                    var rule = tile.GetRule(neighbors, out addedRotation);

                    if (rule != null)
                    {
                        if (rule.RuleID != node.RuleID)
                        {
                            node.SetRuleID(rule.RuleID);
                            prefab = (rule.Random) ? rule.GetRandomObject() : rule.Object;
                        }
                    }
                    else
                    {
                        if (node.RuleID != -1)
                        {
                            node.SetRuleID(-1);
                            prefab = (tile.Random) ? tile.GetRandomDefault() : tile.Default;
                        }
                    }
                }
                else
                {
                    if (node.RuleID != -1)
                    {
                        prefab = (tile.Random) ? tile.GetRandomDefault() : tile.Default;
                        node.SetRuleID(-1);
                    }
                }

                if (addedRotation[0] > -1) //if rotated around an axis, add the rotation 
                {
                    Vector3 axis = Vector3.right;
                    if (addedRotation[0] == 1)
                        axis = Vector3.up;
                    else if (addedRotation[0] == 2)
                        axis = Vector3.forward;
                    node.LocalRotation *= Quaternion.AngleAxis(addedRotation[1], axis);
                }

                if (prefab != null)
                {
                    if (node.Instance == null || (node.Instance != null && PrefabUtility.GetCorrespondingObjectFromSource(node.Instance) != prefab))
                    {
                        node.CreateInstance(prefab);
                    }

                    if (node.Instance.name != prefab.name)
                        node.Instance.name = prefab.name;
                }


                if (node.Block != null)
                {
                    node.Block.OnInstanceUpdate(node);
                }

                if (node.Instance != null)
                {
                    node.UpdateInstanceTransform();
                }

#if UNITY_EDITOR
                if (node.Instance != null)
                    EditorUtility.SetDirty(node.Instance);
                if (node.Layer != null)
                    EditorUtility.SetDirty(node.Layer);
#endif
            }
        }
        /// <summary>
        /// in addition to what verify instance does, this function also allows for randomization of the instance if possible
        /// </summary>
        public static void UpdateInstance(this InternalNode node)
        {

            //dont allow baked node to update
            if (node.Block != null && node.Block.IsBaked)
                return;

            GameObject prefab = null;
            int[] addedRotation = new int[] { -1, 0 };

            var tile = node.GetTile();
            if (tile == null)
            {
                tile = node.GetTile();
                if (tile == null)
                {

                    Debug.LogError($"Autotiles3D: Internal node is can't retrieve tile {node.TileName} {node.TileID}");
                    return;
                }
            }

            if (tile.HasRules)
            {
                bool[] neighbors = node.Layer.GetNeighborsBoolSelfSpace(node.InternalPosition, node.LocalRotation);
                var rule = tile.GetRule(neighbors, out addedRotation);

                if (rule != null)
                {
                    node.SetRuleID(rule.RuleID);
                    prefab = (rule.Random) ? rule.GetRandomObject() : rule.Object;
                }
                else
                {
                    prefab = (tile.Random) ? tile.GetRandomDefault() : tile.Default;
                    node.SetRuleID(-1);
                }
            }
            else
            {
                prefab = (tile.Random) ? tile.GetRandomDefault() : tile.Default;
                node.SetRuleID(-1);
            }


            if (addedRotation[0] > -1) //if rotated around an axis, add the rotation 
            {
                Vector3 axis = Vector3.right;
                if (addedRotation[0] == 1)
                    axis = Vector3.up;
                else if (addedRotation[0] == 2)
                    axis = Vector3.forward;
                node.LocalRotation *= Quaternion.AngleAxis(addedRotation[1], axis);
            }

            if (prefab != null)
            {
                if (node.Instance == null || (node.Instance != null && PrefabUtility.GetCorrespondingObjectFromSource(node.Instance) != prefab))
                {
                    node.CreateInstance(prefab);
                }

                if (node.Instance.name != prefab.name)
                    node.Instance.name = prefab.name;
            }


            if (node.Block != null)
            {
                node.Block.OnInstanceUpdate(node);
            }

            if (node.Instance != null)
            {
                node.UpdateInstanceTransform();
            }

#if UNITY_EDITOR
            if (node.Instance != null)
                EditorUtility.SetDirty(node.Instance);
            if (node.Layer != null)
                EditorUtility.SetDirty(node.Layer);
#endif
        }


        #region DEBUGGING
        private static void DebugNeighbors(InternalNode node, bool[] neighbors)
        {
            string middle = node.InternalPosition.ToString();
            middle += $"\n{neighbors[9]}  {neighbors[10]}  {neighbors[11]}";
            middle += $"\n{neighbors[12]}  {neighbors[13]}  {neighbors[14]}";
            middle += $"\n{neighbors[15]}  {neighbors[16]}  {neighbors[17]}";
            Debug.Log(middle);
        }
        #endregion

        public static void Randomize(this InternalNode node, bool dontAllowSame = true)
        {

            if (node.TryRandomize(out GameObject random, dontAllowSame))
            {
                node.CreateInstance(random);
                node.UpdateInstanceTransform();

                if (node.Block != null)
                    node.Block.OnInstanceUpdate(node);
            }
            else
            {
                Debug.Log("Autotiles3D: No randomization possible");
            }
        }

        public static bool TryRandomize(this InternalNode node, out GameObject random, bool dontAllowSame = true)
        {
            random = null;

            if (node.Block.IsBaked)
            {
                Debug.LogWarning("No Randomization for baked tiles allowed!");
                return false;
            }


            int[] addedRotation = new int[] { -1, 0 };
            bool[] neighbors = node.Layer.GetNeighborsBoolSelfSpace(node.InternalPosition, node.LocalRotation);
            var tile = node.GetTile();
            if (tile == null)
            {
                Debug.LogError("Tile missing");
                return false;
            }

            Autotiles3D_Rule rule = tile.GetRule(neighbors, out addedRotation);
            GameObject instance = dontAllowSame ? node.Instance : null;

            //case we can onyl place default:
            if (rule == null)
            {
                random = tile.GetRandomDefaultExclude(instance);
            }
            //case: a rule applies.
            else
            {
                random = rule.GetRandomObjectExclude(instance);
            }

            return random != null;
        }

        public static void UpdateInstanceTransform(this InternalNode node)
        {
            if (node.Instance != null)
            {
                node.Instance.transform.position = node.Layer.Grid.ToWorldPoint((Vector3)node.InternalPosition * node.Layer.Grid.Unit);
                node.Instance.transform.rotation = node.Layer.Grid.transform.rotation * node.LocalRotation;
            }
        }

        public static void CreateInstance(this InternalNode node, GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("Skin Prefab is null!");
                return;
            }

            if (node.TileID == -1)
            {
                Debug.LogError("Tile ID unset. Do you have any unlinked blocks in your scene?");
                return;
            }
            var anchor = node.Layer.GetAnchor(node.TileID);
            if (anchor == null)
                anchor = node.Layer.EnsureAnchor(node.TileGroupName, node.TileName, node.TileID);

            var newInstance = PrefabUtility.InstantiatePrefab(prefab, anchor.transform) as UnityEngine.GameObject;

            var newBlock = newInstance.GetComponent<Autotiles3D_BlockBehaviour>();
            if (node.Block != null && newBlock != null)
            {
                var viewCache = newBlock.View;
                CopyComponent(node.Block, newBlock);
                newBlock.View = viewCache;
            }

            if (node.Instance != null)
            {
                if (Autotiles3D_Settings.EditorInstance.UseUndoAPI)
                    Undo.DestroyObjectImmediate(node.Instance);
                else
                    GameObject.DestroyImmediate(node.Instance);
            }

            node.Instance = newInstance;

            if (Autotiles3D_Settings.EditorInstance.UseUndoAPI)
                Undo.RegisterCreatedObjectUndo(node.Instance, "InstanceUpdate");
        }


        public static bool IsEqual(this Autotiles3D_Tile tile, Autotiles3D_Tile compare)
        {
            if (tile == compare)
                return true;
            if (tile.TileID == compare.TileID)
                return true;
            if (tile.Name == compare.Name)
                return true;
            return false;
        }

        public static void CopyComponent<T>(this T original, T destination) where T : Component
        {
            System.Type originalType = original.GetType();
            System.Type destinationType = destination.GetType();
            if (destinationType.IsSubclassOf(originalType))
            {
                FieldInfo[] originalFields = originalType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (FieldInfo field in originalFields)
                    field.SetValue(destination, field.GetValue(original));
            }
            else if (originalType.IsSubclassOf(destinationType))
            {
                FieldInfo[] destinationFields = destinationType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (FieldInfo field in destinationFields)
                    field.SetValue(destination, field.GetValue(original));

            }
        }

        /// <summary>
        /// utility function for deepcloing any serializable class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepClone<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        public static Autotiles3D_Anchor EnsureAnchor(this Autotiles3D_TileLayer layer, string group, string tileName, int tileID)
        {
            if (!layer.Anchors.ContainsKey(tileID))
            {
                var anchorObject = new GameObject("Anchor " + tileName);
                anchorObject.transform.SetParent(layer.transform);
                var anchor = anchorObject.AddComponent<Autotiles3D_Anchor>();
                anchor.SetTileID(tileID);

                layer.Anchors.Add(tileID, anchor);
                EditorUtility.SetDirty(layer);
            }

            layer.Anchors[tileID].transform.name = "Anchor " + tileName;
            layer.Anchors[tileID].transform.localPosition = Vector3.zero;
            layer.Anchors[tileID].transform.localRotation = Quaternion.identity;
            return layer.Anchors[tileID];
        }


        public static List<Vector3Int> GetNeighborsPosition(this Autotiles3D_TileLayer layer, Vector3Int internalPosition)
        {
            var myNeighbors = new List<Vector3Int>();
            Vector3Int iteration;
            for (int x = (int)internalPosition.x - 1; x <= (int)internalPosition.x + 1; x++)
            {
                for (int y = (int)internalPosition.y - 1; y <= (int)internalPosition.y + 1; y++)
                {
                    for (int z = (int)internalPosition.z - 1; z <= (int)internalPosition.z + 1; z++)
                    {
                        iteration = new Vector3Int(x, y, z);
                        if (iteration == internalPosition)
                            continue;
                        if (layer.ContainsKey(iteration))
                            myNeighbors.Add(iteration);
                    }
                }
            }
            return myNeighbors;
        }

        public static bool[] GetNeighborsBoolSelfSpace(this Autotiles3D_TileLayer layer, Vector3Int internalPosition, Quaternion localRotation)
        {
            bool[] neighbors = new bool[27];
            Vector3Int iteration;
            int deltax, deltay, deltaz;
            deltay = 0;
            for (int y = -1; y <= 1; y++)
            {
                deltaz = 0;
                for (int z = 1; z >= -1; z--)
                {
                    deltax = 0;
                    for (int x = -1; x <= 1; x++)
                    {
                        iteration = Vector3Int.RoundToInt(internalPosition + localRotation * new Vector3Int(x, y, z));

                        if (iteration != internalPosition)
                        {
                            if (layer.ContainsKey(iteration))
                                neighbors[deltay * 9 + deltaz * 3 + deltax] = true;
                        }
                        deltax++;
                    }
                    deltaz++;
                }
                deltay++;
            }
            return neighbors;
        }
    }

}

#endif
