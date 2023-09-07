using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Autotiles3D
{
    public enum GridMode
    {
        Center,
        Origin,
    }

    public enum LevelSize
    {
        Finite,
        Infinite
    }
    public class Autotiles3D_Grid : MonoBehaviour
    {
        public LevelSize GridSize;
        [Min(0)]
        public int Height = 5;
        [Min(0)]
        public int Width = 5;
        [Min(0)]
        public float Unit = 1f;
        public List<Autotiles3D_TileLayer> TileLayers = new List<Autotiles3D_TileLayer>();
        [SerializeField] private int _LayerIndex;
        public int LayerIndex { get { return _LayerIndex; } protected set { _LayerIndex = value; } }

        //ACTION TO SUBSCRIBE TO
        public Action<EventType, Event, Vector3> OnGridSelection;
        public Action<EventType, Event, Vector3> OnGridHover;

        public void SetLayerIndex(int index)
        {
            _LayerIndex = index;
        }

        #region EXPOSED API
        /// <summary>
        /// Is the internalPosition exceeding the boundaries of the grid
        /// </summary>
        /// <param name="internalPosition"></param>
        /// <returns></returns>
        public bool IsExceedingLevelGrid(Vector3 internalPosition)
        {
            if (GridSize == LevelSize.Infinite)
                return false;

            if (internalPosition.x < 0 || internalPosition.x >= Width)
                return true;
            if (internalPosition.y < 0 || internalPosition.y >= Height)
                return true;
            if (internalPosition.z < 0 || internalPosition.z >= Width)
                return true;
            return false;
        }

        /// <summary>
        /// Returns a list of all the blockbehaviours at the internalPosition. Can return multiple blocks if working with multiple layers
        /// </summary>
        /// <param name="internalPosition">the integer position of the block inside the grid</param>
        /// <returns></returns>
        public List<Autotiles3D_BlockBehaviour> GetBlocks(Vector3Int internalPosition)
        {
            var blocks = new List<Autotiles3D_BlockBehaviour>();
            foreach (var layer in TileLayers)
            {
                if (layer.ContainsKey(internalPosition))
                    blocks.Add(layer.GetInternalNode(internalPosition).Block);
            }

            return blocks;
        }
        /// <summary>
        /// Grid to world position
        /// </summary>
        /// <param name="internalPosition"></param>
        /// <returns></returns>
        public Vector3 ToWorldPoint(Vector3 internalPosition)
        {
            return transform.TransformPoint(internalPosition);
        }
        /// <summary>
        /// Grid to world direction
        /// </summary>
        /// <param name="internalDirection"></param>
        /// <returns></returns>
        public Vector3 ToWorldDirection(Vector3 internalDirection)
        {
            return transform.TransformDirection(internalDirection);
        }
        #endregion
#if UNITY_EDITOR
        Color outlineGridColor = new Color(0.811f, 0.811f, 0.811f, 0.686f);
        Color red = new Color(1.000f, 0.000f, 0.000f, 0.686f);
        Color green = new Color(0.172f, 1.000f, 0.000f, 0.686f);
        Color blue = new Color(0.000f, 0.568f, 1.000f, 0.686f);
        public void DrawLevelGrid(int controlID, bool drawCurrentLayer = true)
        {
            Vector3 offset = -Vector3.one * 0.5f;

            float drawWidth = (float)Width;
            float halfWidth = (float)Width / 2f;

            //Draw Color Handles
            Handles.color = red;
            Handles.DrawLine(offset, offset + drawWidth * Vector3.right);
            if (GridSize == LevelSize.Infinite)
                Handles.ArrowHandleCap(controlID, offset + (drawWidth + 0.2f) * Vector3.right, Quaternion.AngleAxis(90, Vector3.up), 0.5f, EventType.Repaint);

            Handles.color = green;
            Handles.DrawLine(offset, offset + Height * Vector3.up);
            if (GridSize == LevelSize.Infinite)
                Handles.ArrowHandleCap(controlID, offset + (Height + 0.2f) * Vector3.up, Quaternion.AngleAxis(90, Vector3.left), 0.5f, EventType.Repaint);

            Handles.color = blue;
            Handles.DrawLine(offset, offset + drawWidth * Vector3.forward);
            if (GridSize == LevelSize.Infinite)
                Handles.ArrowHandleCap(controlID, offset + (drawWidth + 0.2f) * Vector3.forward, Quaternion.identity, 0.5f, EventType.Repaint);
            Handles.color = outlineGridColor;

            //Draw Grid of Current Layer
            if (drawCurrentLayer)
            {
                Vector3 localCenter = new Vector3(halfWidth, 0, halfWidth);
                var vert1 = offset + localCenter + halfWidth * (Vector3.forward + Vector3.right);
                var vert2 = offset + localCenter + halfWidth * (Vector3.forward - Vector3.right);
                var vert3 = offset + localCenter + halfWidth * (-Vector3.forward - Vector3.right);
                var vert4 = offset + localCenter + halfWidth * (-Vector3.forward + Vector3.right);
                var verts = new List<Vector3> { vert1, vert2, vert3, vert4 };
                for (int i = 0; i < verts.Count; i++)
                    verts[i] += Vector3.up * _LayerIndex;

                Handles.DrawSolidRectangleWithOutline(verts.ToArray(), new Color(0.2f, 0.2f, 0.2f, 0.2f), outlineGridColor);
            }
        }
#endif
    }
}