using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Autotiles3D
{
    [CustomEditor(typeof(Autotiles3D_Grid), true)]
    public class Autotiles3D_GridInspector : Editor
    {
        Autotiles3D_Grid _grid;
        private string _gridHierarchyName
        {
            get
            {
                if (_grid.GridSize == LevelSize.Finite)
                    return $"Grid ({_grid.Width}x{_grid.Height}x{_grid.Width})";
                else
                    return "Infinite Grid";
            }
        }
        public virtual void OnEnable()
        {
            _grid = (Autotiles3D_Grid)target;

            EnsureTransformIsOnGrid();

            var layers = _grid.GetComponentsInChildren<Autotiles3D_TileLayer>();
            _grid.TileLayers = layers.ToList();
        }

        public override void OnInspectorGUI()
        {
            _grid.GridSize = (LevelSize)EditorGUILayout.EnumPopup("Grid Size", _grid.GridSize);
            _grid.Height = EditorGUILayout.IntSlider("Height", _grid.Height, 1, 50);
            _grid.Width = EditorGUILayout.IntSlider("Width", _grid.Width, 1, 50);
            var scale = EditorGUILayout.Slider("Unit Scale", _grid.Unit, 0.1f, 10f);
            if (scale != _grid.Unit)
            {
                _grid.Unit = scale;
                foreach (var tilelayer in _grid.TileLayers)
                {
                    var nodes = tilelayer.GetAllInternalNodes();
                    foreach (var node in nodes)
                        node.UpdateInstanceTransform();
                }
            }

            EnsureTransformIsOnGrid();

            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Add new tile layer"))
            {
                var child = new GameObject();
                child.transform.SetParent(_grid.transform);
                child.AddComponent<Autotiles3D_TileLayer>();
            }

            EditorGUILayout.EndVertical();

            //rename grid gameobject
            if (_grid.gameObject.name != _gridHierarchyName)
                _grid.gameObject.name = _gridHierarchyName;

            if (GUI.changed)
                EditorUtility.SetDirty(_grid);
        }

        private void OnSceneGUI()
        {
            int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(_grid.transform.position, _grid.transform.rotation, Vector3.one * _grid.Unit);
            Handles.matrix = rotationMatrix;

            EnsureTransformIsOnGrid();
            _grid.DrawLevelGrid(controlID);
        }

        private void EnsureTransformIsOnGrid()
        {
            _grid.transform.position = Vector3Int.RoundToInt(_grid.transform.position);
        }


    }
}