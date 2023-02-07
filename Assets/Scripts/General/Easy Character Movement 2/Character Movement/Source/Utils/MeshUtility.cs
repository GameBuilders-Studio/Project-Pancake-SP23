using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyCharacterMovement
{
    public static class MeshUtility
    {
        private const int kMaxVertices = 1024;
        private const int kMaxTriangles = kMaxVertices * 3;

        private static readonly List<Vector3> _vertices = new List<Vector3>(kMaxVertices);

        private static readonly List<ushort> _triangles16 = new List<ushort>(kMaxTriangles);
        private static readonly List<int> _triangles32 = new List<int>();

        private static readonly List<ushort> _scratchBuffer16 = new List<ushort>(kMaxTriangles);
        private static readonly List<int> _scratchBuffer32 = new List<int>();

        public static Vector3 FindMeshOpposingNormal(Mesh sharedMesh, ref RaycastHit inHit)
        {
            Vector3 v0, v1, v2;

            if (sharedMesh.indexFormat == IndexFormat.UInt16)
            {
                _triangles16.Clear();

                int subMeshCount = sharedMesh.subMeshCount;
                if (subMeshCount == 1)
                    sharedMesh.GetTriangles(_triangles16, 0);
                else
                {
                    for (int i = 0; i < subMeshCount; i++)
                    {
                        sharedMesh.GetTriangles(_scratchBuffer16, i);

                        _triangles16.AddRange(_scratchBuffer16);
                    }
                }
                
                sharedMesh.GetVertices(_vertices);

                v0 = _vertices[_triangles16[inHit.triangleIndex * 3 + 0]];
                v1 = _vertices[_triangles16[inHit.triangleIndex * 3 + 1]];
                v2 = _vertices[_triangles16[inHit.triangleIndex * 3 + 2]];
            }
            else
            {
                _triangles32.Clear();

                int subMeshCount = sharedMesh.subMeshCount;
                if (subMeshCount == 1)
                    sharedMesh.GetTriangles(_triangles32, 0);
                else
                {
                    for (int i = 0; i < subMeshCount; i++)
                    {
                        sharedMesh.GetTriangles(_scratchBuffer32, i);

                        _triangles32.AddRange(_scratchBuffer32);
                    }
                }
                
                sharedMesh.GetVertices(_vertices);

                v0 = _vertices[_triangles32[inHit.triangleIndex * 3 + 0]];
                v1 = _vertices[_triangles32[inHit.triangleIndex * 3 + 1]];
                v2 = _vertices[_triangles32[inHit.triangleIndex * 3 + 2]];
            }

            Matrix4x4 mtx = inHit.transform.localToWorldMatrix;

            Vector3 p0 = mtx.MultiplyPoint3x4(v0);
            Vector3 p1 = mtx.MultiplyPoint3x4(v1);
            Vector3 p2 = mtx.MultiplyPoint3x4(v2);

            Vector3 u = p1 - p0;
            Vector3 v = p2 - p0;

            Vector3 worldNormal = Vector3.Cross(u, v);

            return worldNormal.normalized;
        }

        public static void FlushBuffers()
        {
            _vertices.Clear();

            _scratchBuffer16.Clear();
            _scratchBuffer32.Clear();

            _triangles16.Clear();
            _triangles32.Clear();
        }
    }
}
