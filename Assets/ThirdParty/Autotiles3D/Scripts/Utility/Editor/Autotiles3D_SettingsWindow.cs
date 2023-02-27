using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Autotiles3D
{
    public class Autotiles3D_SettingsWindow : EditorWindow
    {
        [MenuItem("Tools/Autotiles3D/Settings")]
        static void OpenSettingsWindow()
        {
#if UNITY_2020_1_OR_NEWER
            EditorWindow.CreateWindow<Autotiles3D_SettingsWindow>("Autotiles 3D Settings", typeof(SceneView));
#else

            var window = EditorWindow.CreateInstance<Autotiles3D_SettingsWindow>();// ("Autotiles 3D Settings", typeof(SceneView));
            window.Show();
#endif
        }

        private void OnGUI()
        {

            var tileGroups = Autotiles3D_Utility.LoadTileGroups();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"Found <color=yellow>{tileGroups.Count}</color> TileGroups in Resources", RichStyle);

            foreach (var tileGroup in tileGroups)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{tileGroup.name}");
                EditorGUILayout.LabelField($"Amount of tiles: {tileGroup.Tiles.Count}", GUILayout.Width(120));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Ping & View Scriptable Object"))
                {
                    EditorGUIUtility.PingObject(tileGroup);
                    Selection.activeObject = tileGroup;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create new TileGroup"))
            {
                Autotiles3D_Utility.EnsureFolders();
                var newTileGroup = CreateInstance<Autotiles3D_TileGroup>();
                string uniquepath = AssetDatabase.GenerateUniqueAssetPath("Assets/Autotiles3D/Resources/NewTileGroup.asset");
                AssetDatabase.CreateAsset(newTileGroup, uniquepath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                EditorGUIUtility.PingObject(newTileGroup);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("<color=yellow>Remark </color>:", RichStyle);
            EditorGUILayout.LabelField("Not using the Undo API (e.g. disabling undo functionality) is <color=lime>recommended</color> because Unity's Undo functionality does not scale well with placing/removing lots of objects.", RichStyle);
            EditorGUILayout.LabelField("You can enable this feature, but please be aware that editor perfomance can slow down over time when working with thousands of tiles", RichStyle);

            EditorGUIUtility.labelWidth = 200;
            bool useUndo = EditorGUILayout.Toggle("Use Undo API (recommended off)", Autotiles3D_Settings.EditorInstance.UseUndoAPI);
            if (useUndo != Autotiles3D_Settings.EditorInstance.UseUndoAPI)
            {
                Autotiles3D_Settings.EditorInstance.SetUndoAPI(useUndo);
            }
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndVertical();
        }

        public static GUIStyle RichStyle
        {
            get
            {
                var style = new GUIStyle();
                style.richText = true;
                style.normal.textColor = Color.white;
                return style;
            }
        }

    }
}