using System.Collections;
using System.Linq;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;


namespace Autotiles3D
{
    public class Autotiles3D_Settings : ScriptableObject
    {
        private static Autotiles3D_Settings _settings;
        public const string SettingsPath = "Assets/Autotiles3D/Content/Autotiles3D_Settings.asset";

        [FormerlySerializedAs("UseUndoAPI")]
        [SerializeField]
        private bool _useUndoAPI = false; //recommend off, because Unitys Undo API is incredible slow and inefficient.
        public bool UseUndoAPI => _useUndoAPI;
        public bool SuppressTileAmountWarning = false;

        public void SetUndoAPI(bool value)
        {
            _useUndoAPI = value;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        //for editor use
        public static bool IsLocked;

#if UNITY_EDITOR


        public static Autotiles3D_Settings EditorInstance
        {
            get
            {
                if (_settings == null)
                {
                    var settings = AssetDatabase.LoadAssetAtPath<Autotiles3D_Settings>(SettingsPath);
                    if (settings == null)
                    {
                        settings = ScriptableObject.CreateInstance<Autotiles3D_Settings>();
                        AssetDatabase.CreateAsset(settings, SettingsPath);
                        AssetDatabase.SaveAssets();
                    }
                    _settings = settings;
                }
                return _settings;
            }
        }
        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(EditorInstance);
        }
#endif
    }



}
