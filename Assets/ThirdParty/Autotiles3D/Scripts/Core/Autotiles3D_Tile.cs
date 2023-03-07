using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Autotiles3D
{
    public enum Neighbor
    {
        Left = 0,
        Right = 1,
        Back = 2,
        Front = 3,
        Down = 4,
        Up = 5
    }

    public enum Relation
    {
        None = 0,
        Arrow = 1,
        Edge = 2
    }

    [System.Serializable]
    public class Autotiles3D_Rule
    {
        public bool IsActive = true;
        public GameObject Object;
        public bool Random = false;

        //this list is exluding the original "Object" Gameobject. 
        //The reason for this is simply because the Randomized Objects list was added in a later Autotiles3D update.
        //For backwards compatability "Object" is therefore kept.
        public List<GameObject> Randoms = new List<GameObject>();
        public Relation[] Relations = new Relation[27];//0 nothing, 1 arrow 2 edge , order left right back front down up
        public bool[] AllowRotation = new bool[3];
        [SerializeField] private int _ruleID = -1;
        public Autotiles3D_Rule()
        {
            _ruleID = System.Guid.NewGuid().GetHashCode();
        }
        public int RuleID
        {
            get
            {
                if (_ruleID == -1)
                    _ruleID = System.Guid.NewGuid().GetHashCode();
                return _ruleID;
            }
        }
        public Autotiles3D_Rule(Autotiles3D_Rule copy)
        {
            _ruleID = System.Guid.NewGuid().GetHashCode();
            IsActive = copy.IsActive;
            Object = copy.Object;
            Randoms = new List<GameObject>();
            foreach (var o in copy.Randoms)
            {
                Randoms.Add(o);
            }
            Relations = Array.ConvertAll(copy.Relations, r => r);
            AllowRotation = Array.ConvertAll(copy.AllowRotation, r => r);
        }
        public GameObject GetRandomObject()
        {
            int index = UnityEngine.Random.Range(0, Randoms.Count + 1); //last index is the default "Object"
            if (index == Randoms.Count)
                return Object;
            return Randoms[index];
        }
#if UNITY_EDITOR
        /// <summary>
        /// Returns a random object, but will exclude "instance". Can return null if no applicable object left.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public GameObject GetRandomObjectExclude(GameObject instance)
        {
            List<GameObject> objectList = new List<GameObject>() { Object };
            objectList.AddRange(Randoms);

            if (objectList.Count == 0)
                return null;

            if (instance != null)
            {
                //remove if instance's source object is the same
                for (int i = objectList.Count - 1; i >= 0; i--)
                {
                    if (PrefabUtility.GetCorrespondingObjectFromSource(instance) == objectList[i])
                    {
                        objectList.RemoveAt(i);
                    }
                }
            }

            //return null if no other object is available
            if (objectList.Count == 0)
                return null;

            int index = UnityEngine.Random.Range(0, objectList.Count);
            return objectList[index];

        }
#endif
    }

    [System.Serializable]
    public class Autotiles3D_Tile
    {
        [SerializeField] private string _group; //name of the TileGroup this belongs to
        [SerializeField] private int _tileID = -1;

        public GameObject Default;
        public List<GameObject> Randoms = new List<GameObject>();
        public string Tag => _group + _name;
        public bool Random = false;

        [SerializeField]
        [FormerlySerializedAs("DisplayName")]
        private string _name;

        public string Name => _name;

        public List<Autotiles3D_Rule> Rules = new List<Autotiles3D_Rule>();
        public bool HasRules => Rules.Count > 0;
        public int TileID
        {
            get
            {
                if (_tileID == -1)
                    _tileID = System.Guid.NewGuid().GetHashCode();
                return _tileID;
            }
        }
        public void ResetID()
        {
            _tileID = 0;
        }
        public void SetGroupName(string group)
        {
            _group = group;
        }
        public bool HasAnyRandomizeEnabled()
        {
            return Random || Rules.Any(r => r.Random);
        }
        public string Group => _group;
        [NonSerialized] private Autotiles3D_TileGroup cachedGroup;
        public Autotiles3D_TileGroup GetGroup()
        {
            if (cachedGroup != null)
            {
                if (cachedGroup.name == _group)
                    return cachedGroup;
                cachedGroup = null;
            }

            if (string.IsNullOrEmpty(_group))
                return cachedGroup;
#if UNITY_EDITOR

            List<Autotiles3D_TileGroup> groups = Autotiles3D_Utility.LoadTileGroups();
            foreach (var group in groups)
            {
                if (group.name == _group)
                {
                    cachedGroup = group;
                    break;
                }
            }
#endif
            return cachedGroup;
        }
        public Autotiles3D_Tile(string displayName)
        {
            _name = displayName;
        }
        public Autotiles3D_Tile(Autotiles3D_Tile copy)
        {
            Default = copy.Default;
            Randoms = new List<GameObject>();
            foreach (var random in Randoms)
            {
                Randoms.Add(random);
            }
            _name = copy.Name + "(Copy)";
            foreach (var rule in copy.Rules)
            {
                Rules.Add(new Autotiles3D_Rule(rule));
            }
        }

        public GameObject GetRandomDefault()
        {
            int index = UnityEngine.Random.Range(0, Randoms.Count + 1); //last index is the default "Object"
            if (index == Randoms.Count)
                return Default;
            return Randoms[index];
        }

#if UNITY_EDITOR
        /// <summary>
        /// same method as in Autotiles3D_Rule, but for the Default Object
        /// </summary>
        public GameObject GetRandomDefaultExclude(GameObject instance)
        {
            List<GameObject> objectList = new List<GameObject>() { Default };
            objectList.AddRange(Randoms);

            if (objectList.Count == 0)
                return null;

            if (instance != null)
            {
                //remove if instance's source object is the same
                for (int i = objectList.Count - 1; i >= 0; i--)
                {
                    if (PrefabUtility.GetCorrespondingObjectFromSource(instance) == objectList[i])
                    {
                        objectList.RemoveAt(i);
                    }
                }
            }
            //return null if no other object is available
            if (objectList.Count == 0)
                return null;

            int index = UnityEngine.Random.Range(0, objectList.Count);
            return objectList[index];
        }
        public Autotiles3D_Rule GetRule(bool[] neighbors, out int[] addedRotation)
        {
            addedRotation = new int[] { -1, 0 };

            if (!HasRules)
                return null;

            foreach (var rule in Rules)
            {
                if (!rule.IsActive)
                    continue;

                if (DoesRulePass(rule, neighbors, out int[] succesfullRotation))
                {
                    addedRotation = succesfullRotation;
                    return rule;
                }
            }

            return null;
        }
        private bool DoesRulePass(Autotiles3D_Rule rule, bool[] neighbors, out int[] rotation)
        {
            rotation = new int[] { 0, 0 }; //index of rotation axis , angle axis ratation in degrees

            if (rule == null)
                return false;

            bool passEdges = DoEdgesMatch(rule.Relations, neighbors);
            bool passArrows = DoArrowsMatch(rule.Relations, neighbors);

            if (passEdges && passArrows)
                return true;

            for (int h = 0; h < 3; h++)
            {
                var checkedRotation = rule.Relations;
                rotation[0] = -1;
                rotation[1] = 0;

                if (rule.AllowRotation[h])
                {
                    for (int i = 0; i < 3; i++)
                    {
                        rotation[1] += 90;
                        var rotated = RotateRelationsClockwise(checkedRotation, h);
                        passEdges = DoEdgesMatch(rotated, neighbors);
                        passArrows = DoArrowsMatch(rotated, neighbors);
                        if (passEdges && passArrows)
                        {
                            rotation[0] = h;
                            return true;
                        }
                        checkedRotation = rotated;
                    }
                }
            }
            return false;
        }
        private bool DoEdgesMatch(Relation[] relations, bool[] neighbors)
        {
            for (int i = 0; i < relations.Length; i++)
            {
                if (relations[i] == Relation.Edge && neighbors[i])
                    return false;
            }
            return true;
        }
        private bool DoArrowsMatch(Relation[] relations, bool[] neighbors)
        {
            for (int i = 0; i < relations.Length; i++)
            {
                if (relations[i] == Relation.Arrow && !neighbors[i])
                    return false;
            }
            return true;
        }

        //dont judge, at least its fast
        private Relation[] RotateRelationsClockwise(Relation[] relations, int rotationIndex)
        {
            Relation[] rotated = new Relation[27];

            switch (rotationIndex)
            {
                case 0: //clockwise around X
                    rotated[0] = relations[18];
                    rotated[1] = relations[19];
                    rotated[2] = relations[20];
                    rotated[3] = relations[9];
                    rotated[4] = relations[10];
                    rotated[5] = relations[11];
                    rotated[6] = relations[0];
                    rotated[7] = relations[1];
                    rotated[8] = relations[2];
                    rotated[9] = relations[21];
                    rotated[10] = relations[22];
                    rotated[11] = relations[23];
                    rotated[12] = relations[12];
                    rotated[13] = relations[13];
                    rotated[14] = relations[14];
                    rotated[15] = relations[3];
                    rotated[16] = relations[4];
                    rotated[17] = relations[5];
                    rotated[18] = relations[24];
                    rotated[19] = relations[25];
                    rotated[20] = relations[26];
                    rotated[21] = relations[15];
                    rotated[22] = relations[16];
                    rotated[23] = relations[17];
                    rotated[24] = relations[6];
                    rotated[25] = relations[7];
                    rotated[26] = relations[8];
                    break;
                case 1: //clockwise around Y
                    rotated[0] = relations[6];
                    rotated[1] = relations[3];
                    rotated[2] = relations[0];
                    rotated[3] = relations[7];
                    rotated[4] = relations[4];
                    rotated[5] = relations[1];
                    rotated[6] = relations[8];
                    rotated[7] = relations[5];
                    rotated[8] = relations[2];
                    rotated[9] = relations[15];
                    rotated[10] = relations[12];
                    rotated[11] = relations[9];
                    rotated[12] = relations[16];
                    rotated[13] = relations[13];
                    rotated[14] = relations[10];
                    rotated[15] = relations[17];
                    rotated[16] = relations[14];
                    rotated[17] = relations[11];
                    rotated[18] = relations[24];
                    rotated[19] = relations[21];
                    rotated[20] = relations[18];
                    rotated[21] = relations[25];
                    rotated[22] = relations[22];
                    rotated[23] = relations[19];
                    rotated[24] = relations[26];
                    rotated[25] = relations[23];
                    rotated[26] = relations[20];
                    break;
                case 2: //clockwise around Z
                    rotated[0] = relations[18];
                    rotated[1] = relations[9];
                    rotated[2] = relations[0];
                    rotated[3] = relations[21];
                    rotated[4] = relations[12];
                    rotated[5] = relations[3];
                    rotated[6] = relations[24];
                    rotated[7] = relations[15];
                    rotated[8] = relations[6];
                    rotated[9] = relations[19];
                    rotated[10] = relations[10];
                    rotated[11] = relations[1];
                    rotated[12] = relations[22];
                    rotated[13] = relations[13];
                    rotated[14] = relations[4];
                    rotated[15] = relations[25];
                    rotated[16] = relations[16];
                    rotated[17] = relations[7];
                    rotated[18] = relations[20];
                    rotated[19] = relations[11];
                    rotated[20] = relations[2];
                    rotated[21] = relations[23];
                    rotated[22] = relations[14];
                    rotated[23] = relations[5];
                    rotated[24] = relations[26];
                    rotated[25] = relations[17];
                    rotated[26] = relations[8];
                    break;
            }
            return rotated;
        }
        private const float _width = 80;
        public void RenderTileGUI(out bool dirty, UnityEngine.Object context)
        {
            dirty = false;
            EditorGUI.BeginChangeCheck();
            if (TileID == -1)
                dirty = true;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUIUtility.labelWidth = 120;
            var newName = EditorGUILayout.DelayedTextField("Name", Name);
            if (newName != Name)
            {
                //check for duplicate names
                int increase = 1;
                while (Autotiles3D_Utility.DoesTileExist(-1, newName, _group))
                {
                    newName += $"({increase++})";
                }
                Undo.RegisterCompleteObjectUndo(context, "Name Change");
                _name = newName;
            }

            bool random = EditorGUILayout.Toggle("Random", Random);
            if (random != Random)
            {
                Undo.RegisterCompleteObjectUndo(context, "Tile Random Change");
                Random = random;
            }

            string defaultLabel = Random ? "Random Default 1" : "Default";

            var defaultGO = EditorGUILayout.ObjectField(defaultLabel, Default, typeof(GameObject), allowSceneObjects: false) as GameObject;
            if (defaultGO != Default)
            {
                Undo.RegisterCompleteObjectUndo(context, "Default GO Change");
                Default = defaultGO;
            }

            if (Random)
            {
                int remove = -1;
                for (int i = 0; i < Randoms.Count + 1; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (i < Randoms.Count)
                    {
                        GameObject updatedObject = EditorGUILayout.ObjectField($"Random Default {i + 2}", Randoms[i], typeof(GameObject), allowSceneObjects: false) as GameObject;
                        if (Randoms[i] != updatedObject)
                        {
                            Undo.RegisterCompleteObjectUndo(context, "Random Tile Object Change");
                            Randoms[i] = updatedObject;
                        }
                        if (updatedObject == null)
                            remove = i;
                    }
                    else
                    {
                        GameObject newAdd = EditorGUILayout.ObjectField("New random option", null, typeof(GameObject), allowSceneObjects: false) as GameObject;
                        if (newAdd != null)
                        {
                            Undo.RegisterCompleteObjectUndo(context, "Add Default Tile Object");
                            Randoms.Add(newAdd);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (remove > -1)
                {
                    Randoms.RemoveAt(remove);
                }
            }
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndVertical();

            if (Default != null)
            {
                var texture = AssetPreview.GetAssetPreview(Default);
                GUIContent content = new GUIContent(texture);
                EditorGUILayout.LabelField(content, GUILayout.Height(100/*EditorGUIUtility.singleLineHeight * 2*/));

            }
            EditorGUILayout.EndHorizontal();

            char upArrow = '\u25B2';
            char downArrow = '\u25BC';

            EditorGUILayout.LabelField("Rules");

            foreach (var rule in Rules.ToArray())
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Rule", GUILayout.Width(_width));
                GUILayout.FlexibleSpace();
                if (!rule.Random)
                {
                    var ruleObject = EditorGUILayout.ObjectField(rule.Object, typeof(GameObject), allowSceneObjects: false, GUILayout.Width(_width * 2)) as GameObject;
                    if (rule.Object != ruleObject)
                    {
                        Undo.RegisterCompleteObjectUndo(context, "Rule Object Change");
                        rule.Object = ruleObject;
                    }
                }

                if (GUILayout.Button("Remove", GUILayout.Width(_width)))
                {
                    Undo.RegisterCompleteObjectUndo(context, "Rule Remove");
                    Rules.Remove(rule);
                }
                if (rule.IsActive)
                {
                    Color cache = GUI.color;
                    GUI.color = Color.green;
                    if (GUILayout.Button("Disable", GUILayout.Width(_width)))
                    {
                        Undo.RegisterCompleteObjectUndo(context, "Rule Disable");
                        rule.IsActive = false;
                    }
                    GUI.color = cache;
                }
                else
                {
                    Color cache = GUI.color;
                    GUI.color = Color.red;
                    if (GUILayout.Button("Enable", GUILayout.Width(_width)))
                    {
                        Undo.RegisterCompleteObjectUndo(context, "Rule Enable");
                        rule.IsActive = true;

                    }
                    GUI.color = cache;

                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical();

                bool ruleRandom = EditorGUILayout.Toggle("Random", rule.Random);
                if (rule.Random != ruleRandom)
                {
                    Undo.RegisterCompleteObjectUndo(context, "Tile Random Change");
                    rule.Random = ruleRandom;
                }

                if (rule.Random)
                {
                    var ruleObject = EditorGUILayout.ObjectField("Random Option 1", rule.Object, typeof(GameObject), allowSceneObjects: false) as GameObject;
                    if (rule.Object != ruleObject)
                    {
                        Undo.RegisterCompleteObjectUndo(context, "Rule Object Change");
                        rule.Object = ruleObject;
                    }

                    int remove = -1;

                    for (int i = 0; i < rule.Randoms.Count + 1; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (i < rule.Randoms.Count)
                        {
                            GameObject updatedObject = EditorGUILayout.ObjectField($"Random Option {i + 2}", rule.Randoms[i], typeof(GameObject), allowSceneObjects: false) as GameObject;
                            if (rule.Randoms[i] != updatedObject)
                            {
                                Undo.RegisterCompleteObjectUndo(context, "Rule Object Change");
                                rule.Randoms[i] = updatedObject;
                            }
                            if (updatedObject == null)
                                remove = i;
                        }
                        else
                        {
                            GameObject newAdd = EditorGUILayout.ObjectField("Add new random option", null, typeof(GameObject), allowSceneObjects: false) as GameObject;
                            if (newAdd != null)
                            {
                                Undo.RegisterCompleteObjectUndo(context, "Add Rule Object");
                                rule.Randoms.Add(newAdd);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (remove > -1)
                    {
                        rule.Randoms.RemoveAt(remove);
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.LabelField("Free Axis Rotation");
                EditorGUIUtility.labelWidth = 13;
                EditorGUILayout.BeginHorizontal();

                var ruleRot0 = EditorGUILayout.Toggle("X", rule.AllowRotation[0], GUILayout.Width(30));
                if (ruleRot0 != rule.AllowRotation[0])
                {
                    Undo.RegisterCompleteObjectUndo(context, "Rule rotation");
                    rule.AllowRotation[0] = ruleRot0;
                }
                var ruleRot1 = EditorGUILayout.Toggle("Y", rule.AllowRotation[1], GUILayout.Width(30));
                if (ruleRot1 != rule.AllowRotation[1])
                {
                    Undo.RegisterCompleteObjectUndo(context, "Rule rotation");
                    rule.AllowRotation[1] = ruleRot1;
                }
                var ruleRot2 = EditorGUILayout.Toggle("Z", rule.AllowRotation[2], GUILayout.Width(30));
                if (ruleRot2 != rule.AllowRotation[2])
                {
                    Undo.RegisterCompleteObjectUndo(context, "Rule rotation");
                    rule.AllowRotation[2] = ruleRot2;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < 3; i++)
                {
                    EditorGUILayout.BeginVertical();
                    GUIContent[] content = new GUIContent[9];
                    for (int j = 0; j < content.Length; j++)
                    {
                        if (i == 1 && j == 4)
                        {
                            if (rule.Object != null)
                                content[j] = new GUIContent(AssetPreview.GetAssetPreview(rule.Object));
                            else
                                content[j] = new GUIContent();
                        }
                        else
                        {
                            content[j] = GetContent(rule.Relations[i * 9 + j], 0);
                        }
                    }
                    GUILayout.Space((3 - (i + 1)) * 15);
                    int selection = GUILayout.SelectionGrid(-1, content, 3, GUILayout.Width(100), GUILayout.Height(100));
                    if (selection > -1)
                    {
                        selection = i * 9 + selection; //from 0 to 26
                        if (selection != 13)
                        {
                            var tempSelection = Autotiles3D_EnumUtility.Next(rule.Relations[selection]);
                            if (rule.Relations[selection] != tempSelection)
                            {
                                Undo.RegisterCompleteObjectUndo(context, "Rule change");
                                rule.Relations[selection] = tempSelection;
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.BeginVertical();
                var thumbnail = AssetPreview.GetAssetPreview(rule.Object);
                EditorGUILayout.LabelField(new GUIContent(thumbnail), GUILayout.Height(80), GUILayout.Width(_width));
                EditorGUIUtility.labelWidth = 0;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(upArrow.ToString(), GUILayout.Width(24)))
                {
                    int index = Rules.IndexOf(rule);
                    if (index > 0)
                    {
                        Undo.RegisterCompleteObjectUndo(context, "Rule Move Up");
                        Rules.Remove(rule);
                        Rules.Insert(index - 1, rule);
                    }
                }
                if (GUILayout.Button(downArrow.ToString(), GUILayout.Width(24)))
                {
                    int index = Rules.IndexOf(rule);
                    if (index < Rules.Count - 1)
                    {
                        Undo.RegisterCompleteObjectUndo(context, "Rule Move Down");
                        Rules.Remove(rule);
                        Rules.Insert(index + 1, rule);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+"))
            {
                Undo.RegisterCompleteObjectUndo(context, "Rule Add");
                Rules.Add(new Autotiles3D_Rule());
            }
            if (GUILayout.Button("-"))
            {
                if (Rules.Count > 0)
                {
                    Undo.RegisterCompleteObjectUndo(context, "Rule Remove");
                    Rules.RemoveAt(Rules.Count - 1);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck() || GUI.changed)
            {
                dirty = true;
                Autotiles3D_Utility.ClearCache(TileID);
            }
        }
        private GUIContent GetContent(Relation relation, int direction)
        {
            switch (relation)
            {
                case Relation.None:
                    return new GUIContent(Resources.Load("Icons/square") as Texture);
                case Relation.Arrow:
                    return new GUIContent(Resources.Load("Icons/check") as Texture);
                case Relation.Edge:
                    return new GUIContent(Resources.Load("Icons/cross") as Texture);
            }
            return new GUIContent();
        }
#endif
    }
}





