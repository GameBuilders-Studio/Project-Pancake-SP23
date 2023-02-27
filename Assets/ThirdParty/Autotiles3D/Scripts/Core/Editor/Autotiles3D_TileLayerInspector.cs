using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace Autotiles3D
{
    [CustomEditor(typeof(Autotiles3D_TileLayer))]
    public class Autotiles3D_TileLayerInspector : Editor
    {
        Autotiles3D_TileLayer _tileLayer;
        Autotiles3D_Grid Grid => _tileLayer.Grid;
        public float Unit => Grid.Unit;
        private int _LayerIndex => Grid.LayerIndex;
        private int _TileRotation;
        private bool _WasPreviousShift;
        private int _ControlID = 0;
        private Vector3 _MousePositionGUI;
        private Ray _MouseRay;
        private const int _sp1 = 120;
        private const int _sp2 = 150;
        private const int _sp3Controls = 150;
        private const int _sp4 = 170;
        private const int fatHeightThin = 30;
        private const int fatHeight = 40;
        private bool _ResetHover;
        private bool _HasRenderedHover;
        private bool _showControls;
        enum PullMode
        {
            Face,
            Plane
        }
        private PullMode _PullMode;
        private bool _OutOfBounds;
        private Dictionary<int, TileData> _TileData = new Dictionary<int, TileData>();
        public class TileData
        {
            public int TileId;
            public Texture2D Thumbnail;
            public bool HasAnyRandomize;
            public TileData(int tileID)
            {
                TileId = tileID;
            }
        }

        //broken blocks
        List<string> _broken = new List<string>(); //missing tile
        int _totalBrokenAmt = 0;
        List<Autotiles3D_BlockBehaviour> _brokenBlocks = new List<Autotiles3D_BlockBehaviour>();
        List<RepairBlock> _repair = new List<RepairBlock>(); //missing tile
        class RepairBlock
        {
            public int Group;
            public int TileName;
        }

        //Push Pull
        private List<InternalNode> _ppNodes = new List<InternalNode>();
        private List<InternalNode> _selectedNodes = new List<InternalNode>();
        private Vector3Int _faceNormalInternal;
        private Vector3 _faceNormalWorld, _ppOffset;
        private Plane _ppPlane;
        private bool _isPulling, _isPushing;
        private int _pIndex;

        //gridraycast 
        public List<Vector3> visits = new List<Vector3>(); //(used only for debugging)

        private void OnEnable()
        {
            _tileLayer = (Autotiles3D_TileLayer)target;
            _tileLayer.Grid = _tileLayer.GetComponentInParent<Autotiles3D_Grid>();
            Autotiles3D_TileLayer.IS_EDITING = true;

            if (!Application.isPlaying)
            {
                foreach (Transform t in _tileLayer.transform)
                {
                    if (t.gameObject.name == "HoverInstance")
                        DestroyImmediate(t.gameObject);
                }
            }

            _tileLayer.LoadedGroups = Autotiles3D_Utility.LoadTileGroups();

            if (_tileLayer.Group == null && _tileLayer.LoadedGroups.Count > 0)
                _tileLayer.Group = _tileLayer.LoadedGroups[0];

            CreateTileData();

            _tileLayer.VerifyLayer();
            CheckForBrokenBlocks();

            //try auto repair and recheck once
            if (_totalBrokenAmt > 0)
            {
                TryRepairBrokenBlocksViaOnAnchor();
                CheckForBrokenBlocks();
            }

            Tools.hidden = true;
        }
        private void OnDisable()
        {
            _tileLayer.DestroyHoverInstance();
            Tools.hidden = false;
        }
        public void OnMouseEnterSceneWindow()
        {
        }
        public void OnMouseExitSceneWindow()
        {
            _tileLayer.DestroyHoverInstance();
        }
        public void DrawPlane(Vector3 position, Vector3 normal)
        {

            Vector3 v3;

            if (normal.normalized != Vector3.forward)
                v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
            else
                v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude; ;

            var corner0 = position + v3;
            var corner2 = position - v3;
            var q = Quaternion.AngleAxis(90.0f, normal);
            v3 = q * v3;
            var corner1 = position + v3;
            var corner3 = position - v3;

            Debug.DrawLine(corner0, corner2, Color.green, 0.01f);
            Debug.DrawLine(corner1, corner3, Color.green, 0.01f);
            Debug.DrawLine(corner0, corner1, Color.green, 0.01f);
            Debug.DrawLine(corner1, corner2, Color.green, 0.01f);
            Debug.DrawLine(corner2, corner3, Color.green, 0.01f);
            Debug.DrawLine(corner3, corner0, Color.green, 0.01f);
            Debug.DrawRay(position, normal, Color.red);
        }

        void CheckForBrokenBlocks()
        {
            _broken.Clear();
            _brokenBlocks.Clear();
            _repair.Clear();
            _totalBrokenAmt = 0;
            var nodes = _tileLayer.GetAllInternalNodes();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Instance == null)
                {
                    Debug.LogError("Internal node with missing Instance");
                    continue;
                }
                if (nodes[i].GetTile() == null || nodes[i].Block.GetTile() == null)
                {
                    _totalBrokenAmt++;
                    if (!_broken.Contains(nodes[i].Instance.name))
                    {
                        _broken.Add(nodes[i].Instance.name);
                        _brokenBlocks.Add(nodes[i].Block);
                        _repair.Add(new RepairBlock());
                    }
                }
            }
        }

        void TryRepairBrokenBlocksViaOnAnchor()
        {
            _tileLayer.Anchors.Values.ToList().ForEach(a => a.TryAutoRepairBrokenBlocks());
        }

        private void OnSceneGUI()
        {
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(Grid.transform.position, Grid.transform.rotation, Vector3.one * Unit);
            Handles.matrix = rotationMatrix;

            _ResetHover = false;
            _HasRenderedHover = false;
            _tileLayer.transform.localPosition = Vector3.zero;
            _tileLayer.transform.localRotation = Quaternion.identity;


            //render layer outline
            if (Autotiles3D_TileLayer.SHOW_LAYER_OUTLINE)
            {
                var positions = _tileLayer.GetAllInternalPositions();
                foreach (var internalPosition in positions)
                {
                    Handles.DrawWireCube(internalPosition, Vector3.one);
                }
            }

            if (!Autotiles3D_TileLayer.IS_EDITING)
                return;

            _ControlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
            _MousePositionGUI = Event.current.mousePosition;
            _MouseRay = HandleUtility.GUIPointToWorldRay(_MousePositionGUI);
            Event e = Event.current;


            if (e.type == EventType.MouseLeaveWindow)
                _tileLayer.DestroyHoverInstance();

            if (Tools.hidden)
                Tools.hidden = true;

            if (e.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(_ControlID);
            }
            if (e.type == EventType.ScrollWheel)
            {
                int scroll = (e.delta.y > 0) ? -1 : 1;
                if (e.alt)
                {
                    Grid.SetLayerIndex(_LayerIndex + scroll);
                    Event.current.Use();
                }
                else if (e.control)
                {
                    int delta = (scroll > 0) ? 90 : -90;
                    _TileRotation += delta;
                    _ResetHover = true;
                    Event.current.Use();
                }
                else if (e.shift)
                {
                }
            }

            if (Grid.GridSize == LevelSize.Finite)
                Grid.SetLayerIndex(Mathf.Clamp(_LayerIndex, 0, Grid.Height - 1));


            Grid.DrawLevelGrid(_ControlID);
            DrawHoverSurroundGrid(_tileLayer.LocalHoverPosition);

            //Grid Selection Calculation
            var normal = Grid.transform.TransformDirection(Vector3Int.up).normalized;
            var planeposition = Grid.transform.TransformPoint(new Vector3(0, (_LayerIndex - 0.0f) * Unit, 0));
            Plane plane = new Plane(normal, planeposition);
            //DrawPlane(planeposition, normal);

            plane.Raycast(_MouseRay, out float distance);
            Vector3 worldHit = _MouseRay.GetPoint(distance);

            _tileLayer.LocalHoverPosition = Vector3Int.RoundToInt(Grid.transform.InverseTransformPoint(worldHit) / Unit);

            bool shiftLiftedUp = !e.shift && _WasPreviousShift;
            if (shiftLiftedUp)
                _ResetHover = true;


            if (e.shift && (!_isPulling && !_isPushing))
            {
                bool succesfullHit = false;
                if (!_tileLayer.IsLayerEmpty)
                {
                    if (Autotiles3D_GridRaycast.GridRayCast(_tileLayer, _MouseRay.origin, _MouseRay.GetPoint(50), out Vector3Int internalHit, out Vector3Int internalHitNormal, out visits))
                    {
                        if (_tileLayer.ContainsKey(internalHit))
                        {
                            Autotiles3D_BlockBehaviour block = _tileLayer.GetInternalNode(internalHit).Block;

                            if (block == null)
                                return;
                            if (block.gameObject.name == "HoverInstance")
                                return;

                            succesfullHit = true;
                            _ppNodes.Clear();
                            _selectedNodes.Clear();
                            _faceNormalInternal = internalHitNormal;
                            _faceNormalWorld = Grid.ToWorldDirection(_faceNormalInternal);

                            if (_faceNormalInternal != Vector3.up && _faceNormalInternal != Vector3.down && _faceNormalInternal != Vector3.right && _faceNormalInternal != Vector3.left && _faceNormalInternal != Vector3.forward && _faceNormalInternal != Vector3.back)
                            {
                                Debug.LogError("not aligned facenormal : " + _faceNormalInternal);
                            }

                            //toggle extrusion mode
                            if (e.type == EventType.ScrollWheel)
                            {
                                _PullMode = (PullMode)1 - (int)_PullMode;
                                Event.current.Use();
                            }

                            //layer to block height
                            if (e.isMouse && e.button == 2)
                            {
                                Grid.SetLayerIndex(internalHit.y);
                            }

                            if (_PullMode == PullMode.Face)
                            {
                                if (_tileLayer.ContainsKey(internalHit))
                                {
                                    Autotiles3D_Tile tile = _tileLayer.GetInternalNode(internalHit).GetTile();
                                    if (tile != null)
                                    {
                                        _ppNodes.Add(_tileLayer.GetInternalNode(internalHit));
                                        _selectedNodes.Add(_tileLayer.GetInternalNode(internalHit));
                                        Handles.DrawWireDisc(_selectedNodes[0].InternalPosition + (Vector3)_faceNormalInternal * 0.5f, _faceNormalInternal, 0.5f);
                                    }
                                }
                            }
                            else if (_PullMode == PullMode.Plane)
                            {
                                //getall neighbors in plane
                                var nodes = Autotiles3D_EditorUtility.GetAllUnblockedContAdjacentNodesDepthFirst(_tileLayer, internalHit, _faceNormalInternal).ToList();
                                foreach (var node in nodes)
                                {
                                    if (_tileLayer.ContainsKey(node))
                                    {
                                        Autotiles3D_Tile tile = _tileLayer.GetInternalNode(node).GetTile();
                                        if (tile != null)
                                        {
                                            _ppNodes.Add(_tileLayer.GetInternalNode(node));
                                            _selectedNodes.Add(_tileLayer.GetInternalNode(node));
                                            Handles.DrawWireDisc(node + (Vector3)_faceNormalInternal * 0.5f, _faceNormalInternal, 0.5f);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.LogError("Autotiles: InternalNode GridRaycast went wrong.");
                        }
                    }
                }



                if (e.type == EventType.MouseDown)
                {
                    if (e.button == 0 || e.button == 1 && _selectedNodes.Count > 0)
                    {
                        //_ShiftLock = true;
                        if (succesfullHit)
                        {
                            Vector3 zeroInternalPosition = _selectedNodes[0].InternalPosition;
                            Vector3 perpendicular = Vector3.Cross(_faceNormalWorld, _MouseRay.origin - Grid.transform.TransformPoint(zeroInternalPosition));
                            _ppPlane = new Plane(Grid.transform.TransformPoint(zeroInternalPosition), Grid.transform.TransformPoint(zeroInternalPosition + _faceNormalInternal), Grid.transform.TransformPoint(zeroInternalPosition) + perpendicular);
                            _ppPlane.Raycast(_MouseRay, out float enter);
                            var pullHit = _MouseRay.GetPoint(enter);
                            _ppOffset = Vector3.Project(pullHit, _faceNormalWorld);

                            if (e.button == 0)
                            {
                                _isPulling = true;
                            }
                            else if (e.button == 1)
                            {
                                _isPushing = true;
                            }
                            _pIndex = 0;
                            Autotiles3D_Settings.IsLocked = true;
                            e.Use();
                        }
                    }

                }
            }

            if (_isPulling || _isPushing)
            {
                _ppPlane.Raycast(_MouseRay, out float enter);
                var ppWorldHit = _MouseRay.GetPoint(enter);
                //make sure user can only pull "positive" face for pull, "negative" face for push
                var ppDelta = Vector3.Project(ppWorldHit, _faceNormalWorld);

                int index = 0;
                if (_isPulling)
                {
                    if (Vector3.Dot(Vector3.Project(ppWorldHit, _faceNormalWorld) - _ppOffset, _faceNormalWorld) >= 0)
                    {
                        ppDelta -= _ppOffset;
                        index = (int)((ppDelta.magnitude / Unit) + 0.5f);
                    }
                }
                else if (_isPushing)
                {
                    if (Vector3.Dot(Vector3.Project(ppWorldHit, _faceNormalWorld) - _ppOffset, _faceNormalWorld) <= 0)
                    {
                        ppDelta -= _ppOffset;
                        index = (int)((ppDelta.magnitude / Unit) + 0.5f);
                    }
                }

                if (_pIndex != index)
                {
                    EditorUtility.SetDirty(_tileLayer);
                }

                //calculte all extrusion points
                var extrusionData = CalculateExtrusionPositions(_selectedNodes, index, _faceNormalInternal, _isPulling);

                //render extrusion points
                RenderExtrusion(extrusionData, _isPulling);

                //place/unplace extrusion points
                if (e.type == EventType.MouseUp)
                {
                    if (_isPulling && e.button == 0)
                    {
                        //pos, rot and tile
                        List<Vector3Int> positions = new List<Vector3Int>();
                        List<Quaternion> rotations = new List<Quaternion>();
                        List<int> tileIDs = new List<int>();
                        List<string> tileNames = new List<string>();
                        List<string> groupNames = new List<string>();

                        foreach (var data in extrusionData)
                        {
                            var list = data.Value;
                            var node = data.Key;
                            for (int i = 0; i < list.Count; i++)
                            {
                                positions.Add(list[i]);
                                rotations.Add(node.LocalRotation);
                                tileIDs.Add(node.TileID);
                                tileNames.Add(node.TileName);
                                groupNames.Add(node.TileGroupName);
                            }
                        }

                        _tileLayer.TryPlacementMany(positions, rotations, tileIDs, tileNames, groupNames);
                    }
                    else if (_isPushing && e.button == 1)
                    {
                        var positions = extrusionData.Values.SelectMany(pos => pos);
                        _tileLayer.TryUnplacingMany(positions/*, waitForDestroy: false *//*true*/);
                    }
                }

                //let go of shift or mouse up
                if ((!e.shift && _WasPreviousShift) || e.type == EventType.MouseUp)
                {
                    _ResetHover = true;
                    _isPulling = false;
                    _isPushing = false;
                    _selectedNodes.Clear();
                }
                _pIndex = index;

            }
            else
            {


                if (e.isKey)
                {
                    if (_tileLayer.HotKeySelection(e.keyCode))
                    {
                        _ResetHover = true;
                        e.Use();
                    }
                    if (e.keyCode == KeyCode.Escape)
                        ExitEditingMode();
                }



                if (!e.shift)
                {
                    RenderHoverInstance(_tileLayer.LocalHoverPosition, Quaternion.AngleAxis(_TileRotation, Vector3.up));
                }

                if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag || e.type == EventType.MouseUp)
                {
                    GridSelection(e.type, e, _tileLayer.LocalHoverPosition);
                    Grid.OnGridSelection?.Invoke(e.type, e, _tileLayer.LocalHoverPosition);
                }
                Autotiles3D_Settings.IsLocked = false;
            }



            if (e.type == EventType.MouseEnterWindow)
            {
                OnMouseEnterSceneWindow();
            }
            else if (e.type == EventType.MouseLeaveWindow)
            {
                OnMouseExitSceneWindow();
            }

            if (!_HasRenderedHover)
                _tileLayer.DestroyHoverInstance();


            // REFRESHING ANY CHANGES MADE TO INTERNAL NODES
            _tileLayer.VerifyNodes();
            _tileLayer.RefreshNodes();

            _WasPreviousShift = e.shift;
            _tileLayer.PrevLocalHoverPosition = _tileLayer.LocalHoverPosition;

            _tileLayer.HideGridOutlineRenderer = (e.shift || (!_isPulling && !_isPushing)) ? true : false;


            if (Autotiles3D_TileLayer.SHOW_HOVER_GIZMO)
            {
                if (_tileLayer.HoverInstance.instance != null)
                {
                    //revert handle matrix
                    Handles.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                    var position = _tileLayer.HoverInstance.instance.transform.position;
                    float size = 0.5f * Unit;
                    Color cache = Handles.color;
                    Handles.color = Color.red;
                    Handles.ArrowHandleCap(_ControlID, position, Quaternion.LookRotation(_tileLayer.HoverInstance.instance.transform.right), size, EventType.Repaint);
                    Handles.color = Color.green;
                    Handles.ArrowHandleCap(_ControlID, position, Quaternion.LookRotation(_tileLayer.HoverInstance.instance.transform.up), size, EventType.Repaint);
                    Handles.color = Color.blue;
                    Handles.ArrowHandleCap(_ControlID, position, Quaternion.LookRotation(_tileLayer.HoverInstance.instance.transform.forward), size, EventType.Repaint);
                    Handles.color = cache;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIStyle centerButton = new GUIStyle(GUI.skin.button);
            centerButton.alignment = TextAnchor.MiddleCenter;

            GUIStyle centerLabel = new GUIStyle(GUI.skin.label);
            centerButton.alignment = TextAnchor.MiddleCenter;

            var richButton = new GUIStyle(GUI.skin.button);
            richButton.wordWrap = true;
            richButton.richText = true;
            richButton.normal.textColor = Color.white;
            richButton.alignment = TextAnchor.MiddleCenter;

            List<InternalNode> allInternalNodes = _tileLayer.GetAllInternalNodes();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Layer Name:"/* , GUILayout.Width(_sp1) */);
            string displayName = EditorGUILayout.DelayedTextField(_tileLayer.LayerName/* , GUILayout.Width(_sp1) */);
            EditorGUIUtility.labelWidth = 0;
            if (displayName != _tileLayer.LayerName)
            {
                _tileLayer.gameObject.name = "Tile Layer: " + displayName;
                _tileLayer.LayerName = displayName;
                CreateTileData();
            }
            EditorGUILayout.EndHorizontal();
            //layer index and manual setting

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current layer height:"/* , GUILayout.Width(_sp1) */);
            int newLayerHeight = EditorGUILayout.DelayedIntField(_LayerIndex/* , GUILayout.Width(_sp1) */);
            if (newLayerHeight != _LayerIndex)
            {
                Grid.SetLayerIndex(newLayerHeight);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Total amount of tiles:");
            EditorGUILayout.LabelField(allInternalNodes.Count().ToString());
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh All", richButton, GUILayout.Height(fatHeightThin)))
            {
                foreach (var anchor in _tileLayer.Anchors)
                {
                    _tileLayer.VerifyAllImmediate(anchor.Value);
                }
            }
            if (GUILayout.Button("Randomize All", richButton, GUILayout.Height(fatHeightThin)))
            {
                foreach (var anchor in _tileLayer.Anchors)
                {
                    _tileLayer.UpdateAllImmediate(anchor.Value);
                }
            }
            if (GUILayout.Button("Clear All", richButton, GUILayout.Height(fatHeightThin)))
            {
                foreach (var anchor in _tileLayer.Anchors)
                {
                    _tileLayer.RemoveAllBlocks(anchor.Value);
                }
                //verify layer  (removes anchors)
                _tileLayer.VerifyLayer();
            }

            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            if (Autotiles3D_TileLayer.SHOW_HOVER_GIZMO)
            {
                if (GUILayout.Button("Gizmo <color=lime>on</color>", richButton, GUILayout.Height(fatHeightThin)))
                {
                    Autotiles3D_TileLayer.SHOW_HOVER_GIZMO = false;
                }
            }
            else
            {
                if (GUILayout.Button("Gizmo <color=red>off</color>", richButton, GUILayout.Height(fatHeightThin)))
                {
                    Autotiles3D_TileLayer.SHOW_HOVER_GIZMO = true;
                }
            }
            if (Autotiles3D_TileLayer.SHOW_LAYER_OUTLINE)
            {
                if (GUILayout.Button("  Outline <color=lime>on   </color>", richButton, GUILayout.Height(fatHeightThin)))
                {
                    Autotiles3D_TileLayer.SHOW_LAYER_OUTLINE = false;
                }
            }
            else
            {
                if (GUILayout.Button("  Outline <color=red>off  </color>", richButton, GUILayout.Height(fatHeightThin)))
                {
                    Autotiles3D_TileLayer.SHOW_LAYER_OUTLINE = true;
                }
            }
            if (!Autotiles3D_TileLayer.IS_EDITING)
            {
                if (GUILayout.Button("<color=red>Not Editing</color>", richButton, GUILayout.Height(fatHeightThin)))
                    Autotiles3D_TileLayer.IS_EDITING = true;
            }
            else
            {
                if (GUILayout.Button("<color=lime>Editing</color>", richButton, GUILayout.Height(fatHeightThin)))
                    ExitEditingMode();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Settings", centerButton, GUILayout.Height(fatHeightThin)))
            {
                EditorWindow.GetWindow<Autotiles3D_SettingsWindow>("Autotiles 3D Settings", typeof(SceneView));
            }
            EditorGUILayout.EndVertical();



            bool hasClickedDialogueBox = false;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                var groupNames = _tileLayer.LoadedGroups.Select(g => g.name).ToList();
                var activeGroupName = _tileLayer.Group != null ? _tileLayer.Group.name : "";
                int index = Math.Max(groupNames.IndexOf(activeGroupName), 0);
                EditorGUILayout.BeginHorizontal();
                GUILayout.ExpandWidth(false);
                EditorStyles.popup.fixedHeight = fatHeight;
                int newIndex = EditorGUILayout.Popup(index, groupNames.ToArray(), GUILayout.Width(_sp1 * 2), GUILayout.Height(fatHeight));
                EditorStyles.popup.fontSize = 0;
                EditorStyles.popup.fixedHeight = 0;
                if (index != newIndex)
                {
                    _tileLayer.Group = _tileLayer.LoadedGroups[newIndex];
                    CreateTileData();
                    _tileLayer.ResetActiveTileID();
                }
                if (GUILayout.Button("Show", GUILayout.Width(_sp1), GUILayout.Height(fatHeight)))
                {
                    Selection.activeObject = _tileLayer.Group;
                }

                EditorGUILayout.EndHorizontal();


                #region Search debug of internal nodes. Use if neeed

                /*
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _TileLayer.SearchedInternalPosition = EditorGUILayout.Vector3IntField("Search Layer", _TileLayer.SearchedInternalPosition);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                _TileLayer.SearchedInternalNode = null;
                if (_TileLayer.InternalNodes.ContainsKey(_TileLayer.SearchedInternalPosition))
                    _TileLayer.SearchedInternalNode = _TileLayer.InternalNodes[_TileLayer.SearchedInternalPosition];

                if (_TileLayer.SearchedInternalNode == null)
                    EditorGUILayout.LabelField("Not existing");
                else
                {
                    EditorGUILayout.LabelField($"{_TileLayer.SearchedInternalNode.Tile.DisplayName}");
                    if (_TileLayer.SearchedInternalNode.Instance == null)
                        GUI.enabled = false;
                    if (GUILayout.Button("Ping"))
                        Selection.activeGameObject = _TileLayer.SearchedInternalNode.Instance;
                    GUI.enabled = true;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                */

                #endregion

                GUIStyle offsetLabel = new GUIStyle(GUI.skin.label);
                offsetLabel.alignment = TextAnchor.MiddleCenter;
                offsetLabel.padding = new RectOffset(0, 0, 13, 13);
                offsetLabel.richText = true;
                offsetLabel.wordWrap = true;
                offsetLabel.normal.textColor = Color.white;
                offsetLabel.alignment = TextAnchor.MiddleLeft;

                foreach (var tile in _tileLayer.Tiles)
                {
                    int tileID = tile.TileID;
                    var anchor = _tileLayer.GetAnchor(tileID);

                    //ALL START
                    EditorGUILayout.BeginHorizontal();

                    //PREVIEW
                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.BeginHorizontal();
                    if (_tileLayer.ActiveTileID == tileID)
                    {
                        var linestyle = new GUIStyle();
                        linestyle.normal.background = EditorGUIUtility.whiteTexture;
                        linestyle.margin = new RectOffset(0, 0, 5, 5);
                        linestyle.fixedHeight = 35;
                        GUILayout.Box("", linestyle, GUILayout.Width(2));
                    }

                    //verify thumbnail - sometimes this is missing, probably because AssetPreview.GetAssetPreview is faulty?
                    if (_TileData[tileID].Thumbnail == null && tile.Default != null)
                    {
                        var dtex = AssetPreview.GetAssetPreview(tile.Default);
                        if (dtex != null)
                            _TileData[tileID].Thumbnail = dtex;
                    }

                    if (_TileData[tileID].Thumbnail != null)
                    {
                        GUIContent previewContent = new GUIContent(_TileData[tileID].Thumbnail);
                        if (GUILayout.Button(previewContent, GUILayout.Width(fatHeight), GUILayout.Height(fatHeight)))
                        {
                            _tileLayer.SetActiveTileID(tileID);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();

                    bool guiEnabled = false;
                    int amount = 0;
                    int bakedAmount = 0;

                    if (anchor != null)
                    {
                        guiEnabled = true;
                        amount = anchor.Childcount;
                        bakedAmount = anchor.BakeCount;
                    }

                    int nonBake = amount - bakedAmount;
                    GUI.enabled = guiEnabled;


                    string bakePostfix = "";
                    if (bakedAmount > 0)
                    {
                        bakePostfix = $"(Baked:{bakedAmount})";
                    }
                    if (nonBake < 500)
                        EditorGUILayout.LabelField($" {amount}  <color=yellow> {tile.Name} </color> {bakePostfix}", offsetLabel);
                    else
                        EditorGUILayout.LabelField($" {amount}  <color=yellow> {tile.Name} </color> <color=red>(high) </color> {bakePostfix}", offsetLabel);

                    GUILayout.FlexibleSpace();

                    guiEnabled = GUI.enabled;
                    if (!_tileLayer.Anchors.ContainsKey(tileID))
                    {
                        GUI.enabled = false;
                    }
                    if (GUILayout.Button("Hide", centerButton, GUILayout.Height(fatHeight)))
                    {
                        _tileLayer.Anchors[tileID].ToggleViews(false);
                    }
                    if (GUILayout.Button("Show", centerButton, GUILayout.Height(fatHeight)))
                    {
                        _tileLayer.Anchors[tileID].ToggleViews(true);
                    }
                    GUI.enabled = guiEnabled;


                    string msg = "Bake";
                    if (anchor != null)
                    {
                        if (anchor.BakedParent != null)
                        {
                            msg = "Rebake";
                            if (GUILayout.Button("Unbake", centerButton, GUILayout.Height(fatHeight)))
                            {
                                if (Application.isPlaying)
                                {
                                    Debug.LogError("Autoiles3D: Can't unbake while application is playing.");
                                }
                                else
                                {
                                    Undo.RegisterCompleteObjectUndo(_tileLayer, "Unbake");
                                    Undo.DestroyObjectImmediate(anchor.BakedParent);

                                    // anchor.FetchAllBlocks();
                                    var blocks = anchor.GetBlocks();
                                    blocks.ForEach((b) => b.IsBaked = false);

                                    //refresh all nodes
                                    _tileLayer.VerifyAllImmediate(anchor);

                                    //turn all blocks visible
                                    anchor.ToggleViews(true, true);

                                    //rest bake count
                                    anchor.SetBakeCount(0);
                                }
                            }
                        }
                    }
                    if (GUILayout.Button(msg, centerButton, GUILayout.Height(fatHeight)))
                    {
                        if (Application.isPlaying)
                        {
                            Debug.LogError("Autoiles3D: Can't bake while application is playing.");
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog($"Bake all {tile.Name} meshes?", $"Do you want to bake all {tile.Name} meshes into a single one? \nAll baked meshes will be disabled, but not deleted. You can always re-enable them later or even rebake your meshes after more changes to the layer have been made.", "Yes", "No"))
                            {
                                Undo.RegisterCompleteObjectUndo(_tileLayer, "Bake");
                                hasClickedDialogueBox = true;
                                if (anchor.BakedParent != null)
                                {
                                    Undo.DestroyObjectImmediate(anchor.BakedParent);
                                }

                                _tileLayer.VerifyAllImmediate(anchor);

                                if (anchor != null && anchor.Childcount > 0)
                                {
                                    anchor.BakedParent = new GameObject("BakedParent");
                                    anchor.BakedParent.transform.SetParent(anchor.transform);
                                    anchor.BakedParent.transform.SetSiblingIndex(0);

                                    string path = "Assets/Autotiles3D/Content/CombinedMeshes";
                                    var blocks = anchor.GetBlocks();
                                    Autotiles3D_MeshCombiner.CombineMeshes(blocks.Select(c => c.gameObject).ToList(), ref anchor.BakedParent, path);

                                    //disable view of backed blocks
                                    foreach (var block in blocks)
                                    {
                                        if (block != null)
                                        {
                                            block.View.SetActive(false);
                                            block.IsBaked = true;
                                        }
                                    }

                                    anchor.SetBakeCount(anchor.Childcount);
                                }
                            }
                        }
                        GUIUtility.ExitGUI();
                    }
                    bool wasEnabled = GUI.enabled;
                    GUI.enabled = _TileData[tileID].HasAnyRandomize;
                    if (GUILayout.Button("Randomize", centerButton, GUILayout.Height(fatHeight)))
                    {
                        _tileLayer.UpdateAllImmediate(anchor);
                    }
                    GUI.enabled = wasEnabled;
                    if (GUILayout.Button("Clear", centerButton, GUILayout.Height(fatHeight)))
                    {
                        _tileLayer.RemoveAllBlocks(anchor);
                        _tileLayer.VerifyLayer();
                    }

                    EditorGUILayout.EndHorizontal();

                    GUI.enabled = true;

                    if (amount >= 500 && !Autotiles3D_Settings.EditorInstance.SuppressTileAmountWarning)
                    {
                        EditorGUILayout.LabelField($"<color=red> {amount} </color> <color=yellow> {tile.Name} </color> Tiles. This amount is very high! Consider", Autotiles3D_Utility.RichStyle, GUILayout.Width(_sp2));
                        EditorGUILayout.LabelField($"a) <color=red>reducing </color> amount of tiles, \nb)<color=red> baking </color> meshes or \nc) working with <color=red> multiple layers </color> to improve perfomance.", Autotiles3D_Utility.RichStyle, GUILayout.Width(_sp2));
                        Autotiles3D_Settings.EditorInstance.SuppressTileAmountWarning = EditorGUILayout.Toggle("Suppress warning", Autotiles3D_Settings.EditorInstance.SuppressTileAmountWarning);
                        GUILayout.Space(EditorGUIUtility.singleLineHeight);
                    }

                    EditorGUILayout.EndVertical();

                    //END ALL
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Show Controls", GUILayout.Width(_sp1));
            _showControls = EditorGUILayout.Toggle(_showControls, GUILayout.Width(_sp1));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            if (_showControls)
            {
                RenderControls();
            }
            EditorGUILayout.EndVertical();

            if (_totalBrokenAmt > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"<color=red>{_totalBrokenAmt} Broken tiles found! </color>", Autotiles3D_Utility.RichStyle);

                EditorGUILayout.LabelField("Blocks in this layer are missing (or can't find) their respective tile data!");
                EditorGUILayout.LabelField("This might have happened because you deleted tiles from your TileGroups or due to version a version upgrade from before version 1.3.");
                EditorGUILayout.LabelField("Don't worry, this is <color=cyan>easy to fix </color>!", Autotiles3D_Utility.RichStyle);
                EditorGUILayout.LabelField("Just specify the TileGroup and TileName once for the blocks below!");
                GUILayout.Space(20);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Block Name", GUILayout.Width(_sp4));
                EditorGUILayout.LabelField("Tile Group", GUILayout.Width(_sp4));
                EditorGUILayout.LabelField("Tile Name", GUILayout.Width(_sp4));

                EditorGUILayout.EndHorizontal();

                var groups = _tileLayer.LoadedGroups;
                var groupNames = groups.Select(g => g.name).ToList();
                var groupNamesArray = groupNames.ToArray();
                for (int i = 0; i < _broken.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(_broken[i], GUILayout.Width(_sp4));
                    int groupIndex = EditorGUILayout.Popup(_repair[i].Group, groupNamesArray, GUILayout.Width(_sp4));
                    _repair[i].Group = groupIndex;

                    string[] tileNames = { };
                    Autotiles3D_TileGroup group = null;
                    if (groups.Count > 0 && groupIndex < groups.Count && groupIndex >= 0)
                    {
                        tileNames = groups[groupIndex].Tiles.Select(t => t.Name).ToArray();
                        group = groups[groupIndex];
                    }
                    int tileIndex = EditorGUILayout.Popup(_repair[i].TileName, tileNames, GUILayout.Width(_sp4));
                    _repair[i].TileName = tileIndex;

                    if (group == null)
                        GUI.enabled = false;
                    if (GUILayout.Button("Link"))
                    {
                        if (group != null)
                        {
                            var tile = group.GetTileByIndex(tileIndex);
                            if (tile != null)
                            {
                                Autotiles3D_BlockBehaviour block = _brokenBlocks[i];
                                if (block != null)
                                    Autotiles3D_Utility.RepairBlocks(block, block.Anchor.GetBlocks(), group.name, tile.Name);
                            }
                        }

                        //recalculate broken blocks
                        CheckForBrokenBlocks();
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }


                EditorGUILayout.EndVertical();
            }


            serializedObject.ApplyModifiedProperties();
        }


        private void ExitEditingMode()
        {
            Autotiles3D_TileLayer.IS_EDITING = false;
            _tileLayer.DestroyHoverInstance();
            OnInspectorGUI();
        }


        private void DrawHoverSurroundGrid(Vector3 pos)
        {
            //might be useful, use if needed
            /*
            Vector3 a, b;
            pos = pos + new Vector3(-0.5f, -0.5f, -0.5f);
            Vector3[] rf = { Vector3.right, Vector3.forward };
            for (int i = 0; i < 2; i++)
            {
                a = pos - rf[i];
                b = pos + 2 * rf[i];
                Handles.DrawLine(a, b);
                Handles.DrawLine(a + rf[1 - i], b + rf[1 - i]);
            }
            */
        }

        public void RenderHoverInstance(Vector3Int internalPosition, Quaternion localRotation)
        {
            var activeTile = Autotiles3D_Utility.GetTile(_tileLayer.ActiveTileID);
            if (_tileLayer.ActiveTileID == -1 || activeTile == null)
            {
                _tileLayer.DestroyHoverInstance();
                return;
            }

            if (_tileLayer.ContainsKey(internalPosition))
            {
                _tileLayer.DestroyHoverInstance();
                Handles.DrawWireCube(internalPosition, Vector3.one);
                return;
            }

            _OutOfBounds = Grid.GridSize == LevelSize.Infinite ? false : Grid.IsExceedingLevelGrid(internalPosition);


            if (_tileLayer.PrevLocalHoverPosition != internalPosition || _ResetHover)
            {
                GameObject prefab = activeTile.Default;
                var rule = activeTile.GetRule(_tileLayer.GetNeighborsBoolSelfSpace(internalPosition, localRotation), out int[] addedRotation);
                if (rule != null)
                    prefab = rule.Object;


                if (_OutOfBounds)
                {
                    _tileLayer.DestroyHoverInstance();
                }
                else if (prefab != null)
                {

                    var hoverInstance = _tileLayer.HoverInstance.instance;

                    if (_tileLayer.HoverPrefabObject != prefab || hoverInstance == null)
                    {
                        _tileLayer.DestroyHoverInstance();
                        hoverInstance = PrefabUtility.InstantiatePrefab(prefab, _tileLayer.transform) as UnityEngine.GameObject;
                        hoverInstance.name = "HoverInstance";
                        hoverInstance.layer = 1 << 0;
                        _tileLayer.HoverInstance = (_tileLayer.ActiveTileID, hoverInstance);
                        _tileLayer.HoverPrefabObject = prefab;
                    }
                    _tileLayer.HoverInstance.instance.transform.position = Grid.ToWorldPoint((Vector3)internalPosition * Grid.Unit);


                    if (addedRotation[0] > -1)
                    {
                        Vector3 axis = Vector3.right;
                        if (addedRotation[0] == 1)
                            axis = Vector3.up;
                        else if (addedRotation[0] == 2)
                            axis = Vector3.forward;
                        localRotation *= Quaternion.AngleAxis(addedRotation[1], axis);
                    }

                    _tileLayer.HoverInstance.instance.transform.rotation = Grid.transform.rotation * localRotation;

                }
            }
            _HasRenderedHover = true;



        }

        #region render extrusion

        Color _extrudingColor;

        Dictionary<InternalNode, List<Vector3Int>> CalculateExtrusionPositions(List<InternalNode> selectedNodes, int index, Vector3Int faceNormalInternal, bool pulling)
        {
            Dictionary<InternalNode, List<Vector3Int>> extrusions = new Dictionary<InternalNode, List<Vector3Int>>();

            for (int i = 0; i < selectedNodes.Count; i++)
            {
                extrusions.Add(selectedNodes[i], new List<Vector3Int>());
                var list = extrusions[selectedNodes[i]];

                Vector3Int position = selectedNodes[i].InternalPosition;
                if (pulling)
                {
                    for (int j = 1; j <= index; j++)
                    {
                        Vector3Int extrudePos = position + j * faceNormalInternal;
                        if (!_tileLayer.ContainsKey(extrudePos))
                            list.Add(extrudePos);
                    }
                }
                else
                {
                    for (int j = 0; j < index; j++)
                    {
                        Vector3Int extrudePos = position - j * faceNormalInternal;
                        if (_tileLayer.ContainsKey(extrudePos))
                            list.Add(extrudePos);
                    }
                }
            }

            return extrusions;
        }
        public void RenderExtrusion(Dictionary<InternalNode, List<Vector3Int>> extrusions, bool pulling)
        {
            Handles.color = pulling ? Color.white : Color.red;

            foreach (var group in extrusions)
            {
                var positions = group.Value;
                for (int i = 0; i < positions.Count; i++)
                {
                    Handles.DrawWireCube(positions[i], Vector3.one * 1.07f);
                }
            }
        }

        #endregion
        public void GridSelection(EventType eventType, Event e, Vector3Int internalPosition)
        {
            if (_OutOfBounds)
                return;

            if (!e.alt && !e.control)
            {
                if (!e.shift)
                {
                    if (eventType == EventType.MouseDown || eventType == EventType.MouseDrag)
                    {
                        if (e.button == 0)
                        {
                            if (!_tileLayer.ContainsKey(internalPosition))
                            {
                                _tileLayer.TryPlacementSingle(internalPosition, Quaternion.AngleAxis(_TileRotation, Vector3.up), _tileLayer.ActiveTileID);
                            }
                            e.Use();
                        }
                        else if (e.button == 1)
                        {
                            if (_tileLayer.ContainsKey(internalPosition))
                                _tileLayer.TryUnplacingSingle(internalPosition);
                            e.Use();
                        }
                    }
                }
                else
                {

                }
            }
        }
        void CreateTileData()
        {
            _TileData.Clear();
            foreach (var tile in _tileLayer.Tiles)
            {
                TileData data = new TileData(tile.TileID);
                data.HasAnyRandomize = tile.HasAnyRandomizeEnabled();

                _TileData.Add(tile.TileID, data);


                Texture2D tex = null;//Resources.Load("Icons/square") as Texture2D;
                if (tile.Default != null)
                {

                    var dtex = AssetPreview.GetAssetPreview(tile.Default);
                    if (dtex != null)
                        tex = dtex;
                }
                data.Thumbnail = tex;
            }
        }

        void RenderControls()
        {
            //add tile
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Place tile", GUILayout.Width(_sp3Controls));
            EditorGUILayout.LabelField("Left mouse button (click or drag)");
            EditorGUILayout.EndHorizontal();

            //remove tile
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Remove tile", GUILayout.Width(_sp3Controls));
            EditorGUILayout.LabelField("Right mouse button (click or drag)");
            EditorGUILayout.EndHorizontal();

            //change layer index
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Move placing layer", GUILayout.Width(_sp3Controls));
            EditorGUILayout.LabelField("Alt + Mouse wheel (scroll)");
            EditorGUILayout.EndHorizontal();

            //layer index to block height
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Layer to block height", GUILayout.Width(_sp3Controls));
            EditorGUILayout.LabelField("Shift + Mouse wheel (click on block)");
            EditorGUILayout.EndHorizontal();

            //extrue tiles
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Extrude tiles", GUILayout.Width(_sp3Controls));
            EditorGUILayout.LabelField("Shift + Left Mouse Button (click on block and pull)");
            EditorGUILayout.EndHorizontal();

            //remove tiles
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Reverse exture tiles", GUILayout.Width(_sp3Controls));
            EditorGUILayout.LabelField("Shift + Right Mouse Button (click on block and push)");
            EditorGUILayout.EndHorizontal();

            //extrusion mode
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Switch extrusion mode", GUILayout.Width(_sp3Controls));
            EditorGUILayout.LabelField("Shift + Mouse wheel (scroll)");
            EditorGUILayout.EndHorizontal();

            //rotating blocks
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rotate block (Y-Axis)", GUILayout.Width(_sp3Controls));
            EditorGUILayout.LabelField("Left Control + Mouse wheel (scroll)");
            EditorGUILayout.EndHorizontal();

            //quickselect
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Quickselect tiles", GUILayout.Width(_sp3Controls));
            EditorGUILayout.LabelField("Top row keys 1-9 (while scene focused)");
            EditorGUILayout.EndHorizontal();

            //exit edit mode
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Exit edit mode", GUILayout.Width(_sp3Controls));
            EditorGUILayout.LabelField("Escape");
            EditorGUILayout.EndHorizontal();
        }
    }
}