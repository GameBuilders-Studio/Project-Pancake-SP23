using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Autotiles3D
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Autotiles3D_BlockBehaviour), true)]
    public class Autotiles3D_BlockBehaviourInspector : Editor
    {
        private Autotiles3D_BlockBehaviour _baseBlock;
        bool _canRandomize;
        bool _debug;
        public virtual void OnEnable()
        {
            _baseBlock = (Autotiles3D_BlockBehaviour)target;
            _canRandomize = CheckIfCanRandomize();
        }

        string updateGroup;
        string updateTile;

        bool CheckIfCanRandomize()
        {
            var node = _baseBlock.GetInternalNode();
            if (node == null)
                return false;

            return node.TryRandomize(out GameObject random);
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Autotiles3D_Tile tile = _baseBlock.GetTile();
            Autotiles3D_TileGroup group = null;
            if (tile == null)
            {
                EditorGUILayout.LabelField("<color=red> LINK TO TILE IS MISSING </color>", Autotiles3D_Utility.RichStyle);
                EditorGUILayout.LabelField("This might be due to migration to Autotiles3D version 1.3 or because you have deleted TileGroups/Tiles");
                EditorGUILayout.LabelField("Don't panic, this is not critical. However you won't be able to use new features (such as \"View Tile\" or \"Randomize\") for this block until you reconnect to your tile");
                EditorGUILayout.LabelField("If you would like to reconnect, you can manually update the TileGroup's name and the tile's name for this block here once, and Autotiles3D automatically links things up again!");
                updateGroup = EditorGUILayout.DelayedTextField("Tile Group:", updateGroup);
                updateTile = EditorGUILayout.DelayedTextField("Tile Name:", updateTile);

                bool wasEnabled = GUI.enabled;
                bool linkEnabled = false;
                if (!string.IsNullOrEmpty(updateGroup) && !string.IsNullOrEmpty(updateTile))
                    linkEnabled = true;
                GUI.enabled = linkEnabled;

                if (GUILayout.Button("Link"))
                {
                    Autotiles3D_Utility.RepairBlocks(_baseBlock, _baseBlock.Anchor.GetBlocks(), updateGroup, updateTile);
                }
                GUI.enabled = wasEnabled;
            }
            else
            {
                group = tile.GetGroup();
            }


            InternalNode internalNode = _baseBlock.GetInternalNode();
            if (internalNode != null)
            {
                if (internalNode.RuleID != _baseBlock.RuleID)
                {
                    EditorGUILayout.LabelField($"rule id mismatch in internalnode: {internalNode.RuleID}");
                }
                if (internalNode.TileID != _baseBlock.TileID)
                {
                    EditorGUILayout.LabelField($"tile id mismatch in internalnode: {internalNode.TileID}");
                }
            }
            else
            {
                EditorGUILayout.LabelField("Internal node missing! This should not happen. Have you messed with the hierarchy?");
            }


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            string isBaked = _baseBlock.IsBaked ? "(IS BAKED)" : "";

            string name = (tile != null) ? tile.Name : "Missing Name";


            EditorGUILayout.LabelField($"Tile: {name} ({_baseBlock.GroupName}) {isBaked}");
            EditorGUILayout.LabelField($"Position:{_baseBlock.InternalPosition}");
            EditorGUILayout.LabelField($"Rotation:{_baseBlock.LocalRotation}");
            _debug = EditorGUILayout.Toggle("Debug", _debug);
            if (_debug)
            {
                EditorGUILayout.LabelField($"Tile ID: {_baseBlock.TileID}");
                EditorGUILayout.LabelField($"Tile Rule: {_baseBlock.RuleID}");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();



            bool isEnabled = (group != null);
            GUI.enabled = isEnabled;
            if (GUILayout.Button("View Tile"))
            {
                EditorGUIUtility.PingObject(group);
                Selection.activeObject = group;
            }
            GUI.enabled = true;

            if (GUILayout.Button("Refresh"))
            {

                InternalNode node = _baseBlock.GetInternalNode();
                if (node != null)
                {
                    node.VerifyInstance();
                }
            }

            isEnabled = !_baseBlock.IsBaked && tile != null && _canRandomize;
            GUI.enabled = isEnabled;
            if (GUILayout.Button("Randomize"))
            {
                InternalNode node = _baseBlock.Randomize();
                if (node != null)
                {
                    //focus instance of node (could have changed if randomization was succesful)
                    if (node.Instance != null)
                        Selection.activeObject = node.Instance;
                }
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

        }
    }

}