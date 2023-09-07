using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Autotiles3D
{
    [CustomEditor(typeof(Autotiles3D_TileGroup))]
    public class Autotiles3D_TileGroupInspector : Editor
    {
        Autotiles3D_TileGroup _group;

        public static Autotiles3D_Tile CopyBuffer;
        private void OnEnable()
        {
            _group = (Autotiles3D_TileGroup)target;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            char upArrow = '\u25B2';
            char downArrow = '\u25BC';

            EditorGUI.BeginChangeCheck();

            foreach (var tile in _group.Tiles.ToArray())
            {
                tile.SetGroupName(_group.name);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                tile.RenderTileGUI(out bool dirty, _group);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(upArrow.ToString(), GUILayout.Width(24)))
                {
                    int index = _group.Tiles.IndexOf(tile);
                    if (index > 0)
                    {
                        Undo.RegisterCompleteObjectUndo(_group, "Tile Move");
                        _group.Tiles.Remove(tile);
                        _group.Tiles.Insert(index - 1, tile);
                    }
                }
                if (GUILayout.Button(downArrow.ToString(), GUILayout.Width(24)))
                {
                    int index = _group.Tiles.IndexOf(tile);
                    if (index < _group.Tiles.Count - 1)
                    {
                        Undo.RegisterCompleteObjectUndo(_group, "Tile Move");
                        _group.Tiles.Remove(tile);
                        _group.Tiles.Insert(index + 1, tile);
                    }
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Copy"))
                {
                    Debug.Log($"Copying tile {tile.Name}");
                    Autotiles3D_TileGroupInspector.CopyBuffer = tile;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Space(20);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add new tile"))
            {
                Undo.RegisterCompleteObjectUndo(_group, "Tile Add");
                _group.Tiles.Add(new Autotiles3D_Tile("New Tile"));
            }
            if (GUILayout.Button("Remove last tile"))
            {
                if (_group.Tiles.Count > 0)
                {
                    Undo.RegisterCompleteObjectUndo(_group, "Tile Remove");
                    _group.Tiles.RemoveAt(_group.Tiles.Count - 1);
                }
            }
            if (Autotiles3D_TileGroupInspector.CopyBuffer != null)
            {
                if (GUILayout.Button($"Paste copied tile ({CopyBuffer.Name})"))
                {
                    Debug.Log($"Pasting copied tile!");
                    _group.Tiles.Add(new Autotiles3D_Tile(CopyBuffer));
                }
            }
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck() || GUI.changed)
            {
                //fix: mapbuilding in deserialization callback is not always called when inspecting.
                _group.ConstructMapping();
                EditorUtility.SetDirty(_group);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
