using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Autotiles3D
{
    public static class Autotiles3D_EditorUtility
    {
   
        public static HashSet<Vector3Int> GetAllUnblockedContAdjacentNodesDepthFirst(Autotiles3D_TileLayer tileLayer, Vector3Int internalPosition, Vector3Int faceNormalLocal, HashSet<Vector3Int> results = null)
        {
            if (results == null)
                results = new HashSet<Vector3Int>();


            if (!results.Contains(internalPosition))
                results.Add(internalPosition);

            var neighbors = tileLayer.GetNeighborsPosition(internalPosition);

            foreach (var n in neighbors.ToArray())
            {
                if (results.Contains(n))
                {
                    neighbors.Remove(n);
                    continue;
                }

                var delta = internalPosition - n;
                var d = Mathf.Abs(Vector3.Dot(delta, faceNormalLocal));
                if (d != 0) //gets rid of any nodes not on the same plane
                {
                    neighbors.Remove(n);
                    continue;
                }

                if (Math.Abs(delta.x + delta.y + delta.z) != 1)
                {
                    neighbors.Remove(n);
                    continue;
                }

                //gets rid of diagonal blocks

                if (tileLayer.ContainsKey(n + faceNormalLocal))// || tileLayer.Nodes.ContainsKey(n - faceNormal))
                {
                    neighbors.Remove(n);
                    continue;
                }

                if (!results.Contains(n))
                {
                    results.Add(n);

                    var depthFirst = GetAllUnblockedContAdjacentNodesDepthFirst(tileLayer, n, faceNormalLocal, results);
                    foreach (var node in depthFirst)
                    {
                        if (!results.Contains(node))
                            results.Add(node);
                    }
                }
            }

            return results;
        }


    }
}