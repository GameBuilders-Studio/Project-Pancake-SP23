using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Reflection;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Autotiles3D
{
    [System.Serializable]
    public class InternalNode
    {
        public Autotiles3D_TileLayer Layer;
        public Vector3Int InternalPosition; // integer position inside the grid != local position of the transform
        public Quaternion LocalRotation = Quaternion.identity;

        [FormerlySerializedAs("Block")]
        [SerializeField] Autotiles3D_BlockBehaviour _block;
        [SerializeField] private GameObject _instance;
        [SerializeField] private string _group; //added in update 1.3
        [SerializeField] private string _tileName; //added in update 1.3
        [SerializeField] private int _tileID = -1; //added in update 1.3
        [SerializeField] private int _ruleID = -1; //added in update 1.3 , -1 signifies that standard object is used (no rule applies), might need testing

        public int TileID { get => _tileID; }
        public int RuleID { get => _ruleID; }

        /// <summary>
        /// -1 means default object is being used
        /// </summary>
        public void SetRuleID(int ruleId)
        {
            _ruleID = ruleId;
        }
        public string TileGroupName => _group;
        public string TileName => _tileName;
        public InternalNode(Autotiles3D_TileLayer layer, string group, string tileName, int tileID, Vector3Int internalPosition, Quaternion localRotation, GameObject instance = null)
        {
            Layer = layer;
            UpdateTileInfo(tileID, tileName, group);

            LocalRotation = localRotation;
            InternalPosition = internalPosition;

            if (instance != null)
            {
                Instance = instance;
            }
        }

        public void UpdateTileInfo(int tileID, string tileName, string group)
        {
            _tileID = tileID;
            _tileName = tileName;
            _group = group;
        }

#if UNITY_EDITOR
        public Autotiles3D_Tile GetTile()
        {
            return Autotiles3D_Utility.GetTile(_tileID, _tileName, _group);
        }
#endif

        public GameObject Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }

        public Autotiles3D_BlockBehaviour Block
        {
            get
            {
                if (_block == null)
                {
                    if (_instance != null)
                    {
                        _block = _instance.GetComponent<Autotiles3D_BlockBehaviour>();
                        if (_block == null)
                        {
                            //if instance is missing, add blockbehaviour
                            _block = _instance.AddComponent<Autotiles3D_BlockBehaviour>();
                            _block.View = _instance;
                        }
                    }
                }
                return _block;
            }
        }
    }

    public class Autotiles3D_TileLayer : MonoBehaviour, ISerializationCallbackReceiver
    {
        public string LayerName;
        private Dictionary<Vector3Int, InternalNode> _internalNodes = new Dictionary<Vector3Int, InternalNode>();
        public Dictionary<int, Autotiles3D_Anchor> Anchors = new Dictionary<int, Autotiles3D_Anchor>(); // tileID, anchor
        public bool ContainsKey(Vector3Int internalPosition)
        {
            return _internalNodes.ContainsKey(internalPosition);
        }
        public InternalNode GetInternalNode(Vector3Int internalPosition)
        {
            _internalNodes.TryGetValue(internalPosition, out InternalNode node);
            return node;
        }
        public List<Vector3Int> GetAllInternalPositions()
        {
            return _internalNodes.Keys.ToList();
        }
        public List<InternalNode> GetAllInternalNodes()
        {
            return _internalNodes.Values.ToList();
        }
        public bool IsLayerEmpty => _internalNodes.Count() == 0;
        public Autotiles3D_Anchor GetAnchor(int tileID)
        {
            if (Anchors.ContainsKey(tileID))
                return Anchors[tileID];
            return null;
        }
        public Autotiles3D_TileGroup Group;
        public List<Autotiles3D_Tile> Tiles => Group != null ? Group.Tiles : new List<Autotiles3D_Tile>();
        private int _activeTileID = -1;
        public int ActiveTileID => _activeTileID;
        public void SetActiveTileID(int tileID)
        {
            _activeTileID = tileID;
        }
        public void ResetActiveTileID()
        {
            _activeTileID = -1;
            if (Tiles.Count > 0)
                _activeTileID = Tiles[0].TileID;
        }
        private Autotiles3D_Grid _Grid;
        public Autotiles3D_Grid Grid
        {
            get
            {
                if (_Grid == null)
                    _Grid = GetComponentsInParent<Autotiles3D_Grid>(includeInactive: true)[0];
                return _Grid;
            }
            set
            {
                _Grid = value;
            }
        }

        #region Serialization
        [SerializeField] private List<Vector3Int> _NodesKeys = new List<Vector3Int>();
        [SerializeField] private List<InternalNode> _NodesValues = new List<InternalNode>();
        [SerializeField] private List<int> _AnchorKeys = new List<int>();
        [SerializeField] private List<Autotiles3D_Anchor> _AnchorValues = new List<Autotiles3D_Anchor>();
        public void OnBeforeSerialize()
        {
            _NodesKeys = _internalNodes.Keys.ToList();
            _NodesValues = _internalNodes.Values.ToList();
            _AnchorKeys = Anchors.Keys.ToList();
            _AnchorValues = Anchors.Values.ToList();
        }

        public void OnAfterDeserialize()
        {
            _internalNodes = new Dictionary<Vector3Int, InternalNode>();
            for (int i = 0; i < _NodesKeys.Count; i++)
            {
                _internalNodes.Add(_NodesKeys[i], _NodesValues[i]);
            }
            Anchors = new Dictionary<int, Autotiles3D_Anchor>();
            for (int i = 0; i < _AnchorKeys.Count; i++)
            {
                Anchors.Add(_AnchorKeys[i], _AnchorValues[i]);
            }
        }
        #endregion

#if UNITY_EDITOR

        public static bool IS_EDITING;
        public static bool SHOW_LAYER_OUTLINE;
        public static bool SHOW_HOVER_GIZMO;

        public List<Autotiles3D_TileGroup> LoadedGroups = new List<Autotiles3D_TileGroup>();
        private List<Vector3Int> _toVerify = new List<Vector3Int>();
        private List<Vector3Int> _toUpdate = new List<Vector3Int>();
        public bool RequireNodeVerification => _toVerify.Count > 0;
        public bool RequiresNodeUpdate => _toUpdate.Count > 0;

        #region HOVER
        private (int tileID, GameObject instance) _hoverInstance;
        public (int tileID, GameObject instance) HoverInstance { get => _hoverInstance; set => _hoverInstance = value; }

        public GameObject HoverPrefabObject; //the object of the rule that won, not the instance
        public Vector3Int LocalHoverPosition;
        public Vector3Int PrevLocalHoverPosition;

        #endregion
        public bool HideGridOutlineRenderer { get; set; }
        public Vector3Int SearchedInternalPosition;
        public InternalNode SearchedInternalNode;

        public void VerifyLayer()
        {
            Autotiles3D_Anchor[] anchors = GetComponentsInChildren<Autotiles3D_Anchor>();

            //remove deleted blocks from nodes. update internal nodes with block tile info if missing or different
            var nodes = GetAllInternalNodes();
            foreach (var internalNode in nodes)
            {
                if (internalNode.Instance == null)
                    _internalNodes.Remove(internalNode.InternalPosition);
            }

            //add blocks to nodes 
            foreach (var anchor in anchors)
            {
                //remove any anchors with no blocks
                var blocks = anchor.GetBlocks();
                if (blocks.Count == 0)
                {
                    DestroyImmediate(anchor.gameObject);
                    continue;
                }

                foreach (var block in blocks)
                {
                    if (!_internalNodes.ContainsKey(block.InternalPosition))
                    {
                        _internalNodes.Add(block.InternalPosition, new InternalNode(this, block.GroupName, block.TileName, block.TileID, block.InternalPosition, block.LocalRotation, block.gameObject));
                        if (!Anchors.ContainsKey(block.TileID))
                            this.EnsureAnchor(block.GroupName, block.TileName, block.TileID);
                    }
                }
            }

            //rebuild dictionary
            Anchors.Clear();
            foreach (var anchor in anchors)
            {
                if (anchor == null)
                    continue;

                if (!Anchors.ContainsKey(anchor.TileID))
                {
                    Anchors.Add(anchor.TileID, anchor);
                }
                else
                {
                    Debug.LogWarning($"Autotiles3D: Duplicate Anchor {anchor.TileID} {anchor.name}");
                }
            }
        }

        public void UpdateNeighborsCubeAlgorithm(HashSet<Vector3Int> internalPositions)
        {
            (int minX, int maxX) = ((int)internalPositions.Min(l => l.x), (int)internalPositions.Max(l => l.x));
            (int minY, int maxY) = ((int)internalPositions.Min(l => l.y), (int)internalPositions.Max(l => l.y));
            (int minZ, int maxZ) = ((int)internalPositions.Min(l => l.z), (int)internalPositions.Max(l => l.z));

            var myNeighbors = new List<Vector3Int>();
            Vector3Int iteration;
            for (int x = minX - 1; x <= maxX + 1; x++)
            {
                for (int y = minY - 1; y <= maxY + 1; y++)
                {
                    for (int z = minZ - 1; z <= maxZ + 1; z++)
                    {
                        iteration = new Vector3Int(x, y, z);

                        if (_internalNodes.ContainsKey(iteration))
                            RequireVerification(iteration); //this used to be UpdateNode before 1.3
                    }
                }
            }
        }
        void UpdateNeighbors(Vector3Int originalInternalPosition) //slow if called repeatly while overlapping
        {
            var neighbors = this.GetNeighborsPosition(originalInternalPosition);
            foreach (var neighbor in neighbors)
            {
                if (_internalNodes.ContainsKey(neighbor))
                    RequireVerification(neighbor); ////this used to be UpdateNode before 1.3
            }
        }


        /// <summary>
        /// Adds a single node to the layer
        /// </summary>
        /// <param name="internalPosition"></param>
        /// <param name="localRotation"></param>
        /// <param name="tile"></param>
        public void TryPlacementSingle(Vector3Int internalPosition, Quaternion localRotation, int tileID)
        {
            var tile = Autotiles3D_Utility.GetTile(tileID);
            if (tile == null)
                return;

            if (Autotiles3D_Settings.EditorInstance.UseUndoAPI)
                Undo.RegisterCompleteObjectUndo(this, "TryPlacementSingle");

            AddNodeInternal(tile.TileID, tile.Name, tile.Group, internalPosition, localRotation);
            UpdateNeighbors(internalPosition);
        }
        /// <summary>
        /// Adds multiple nodes to the layer
        /// </summary>
        /// <param name="localPositions"></param>
        /// <param name="localRotations"></param>
        /// <param name="tiles"></param>
        public void TryPlacementMany(List<Vector3Int> localPositions, List<Quaternion> localRotations, List<int> tileIDs, List<string> tileNames, List<string> groupNames)
        {
            int size = localPositions.Count;
            if (size == 0)
                return;

            if (Autotiles3D_Settings.EditorInstance.UseUndoAPI)
                Undo.RegisterCompleteObjectUndo(this, "TryPlacementMany");

            for (int i = 0; i < size; i++)
            {
                AddNodeInternal(tileIDs[i], tileNames[i], groupNames[i], localPositions[i], localRotations[i]);
            }

            HashSet<Vector3Int> hashedPositions = new HashSet<Vector3Int>(localPositions);
            UpdateNeighborsCubeAlgorithm(hashedPositions);

        }

        /// <summary>
        /// Removes a single node from the layer
        /// </summary>
        /// <param name="internalPosition"></param>
        public void TryUnplacingSingle(Vector3Int internalPosition)
        {
            if (Autotiles3D_Settings.EditorInstance.UseUndoAPI)
                Undo.RegisterCompleteObjectUndo(this, "TryUnPlacementSingle");

            RemoveNodeInternal(internalPosition);
            UpdateNeighbors(internalPosition);
        }

        /// <summary>
        /// Removes multiple nodes together (better performance than removing one by one)
        /// </summary>
        /// <param name="internalPosition"></param>
        /// <param name="waitForDestroy">used internally for extruding/inv. extruding nodes and should NOT be used if you're not knowing what it does</param>
        public void TryUnplacingMany(IEnumerable<Vector3Int> internalPosition)
        {
            if (!internalPosition.Any())
                return;

            if (Autotiles3D_Settings.EditorInstance.UseUndoAPI)
                Undo.RegisterCompleteObjectUndo(this, "TryUnplacingMany");

            foreach (var p in internalPosition)
            {
                RemoveNodeInternal(p);
            }
            HashSet<Vector3Int> hashedPositions = new HashSet<Vector3Int>(internalPosition);
            UpdateNeighborsCubeAlgorithm(hashedPositions);

        }

        /// <summary>
        /// Refreshes a single node which checks the neighboring conditions anew and updates the gameobject instance accordingly.
        /// </summary>
        /// <param name="node"></param>
        public void RequireUpdate(Vector3Int internalPosition)
        {
            if (_internalNodes.TryGetValue(internalPosition, out InternalNode node))
            {
                if (node.Block != null)
                    if (node.Block.IsBaked)
                        return;
                _toUpdate.Add(internalPosition);
            }
        }

        public void RequireVerification(Vector3Int internalPosition)
        {
            if (_internalNodes.TryGetValue(internalPosition, out InternalNode node))
            {
                if (node.Block != null)
                    if (node.Block.IsBaked)
                        return;
                _toVerify.Add(internalPosition);
            }
        }

        public void UpdateAllImmediate(Autotiles3D_Anchor anchor)
        {
            if (anchor == null)
                return;

            var blocks = anchor.GetBlocks();
            foreach (var block in blocks)
            {
                if (_internalNodes.ContainsKey(block.InternalPosition))
                {
                    InternalNode node = _internalNodes[block.InternalPosition];
                    node.UpdateInstance();
                }
            }
        }
        public void VerifyAllImmediate(Autotiles3D_Anchor anchor)
        {
            if (anchor == null)
                return;

            List<Autotiles3D_BlockBehaviour> blocks = anchor.GetBlocks();
            foreach (var block in blocks)
            {
                if (_internalNodes.ContainsKey(block.InternalPosition))
                {
                    InternalNode node = _internalNodes[block.InternalPosition];
                    node.VerifyInstance();
                }
            }
        }
        public void VerifyNodes()
        {
            Autotiles3D_Settings.IsLocked = true;
            for (int i = 0; i < _toVerify.Count; i++)
            {
                if (_internalNodes.TryGetValue(_toVerify[i], out InternalNode node))
                {
                    node.VerifyInstance();
                }
            }
            _toVerify.Clear();
            Autotiles3D_Settings.IsLocked = false;
        }
        public void RefreshNodes()
        {
            Autotiles3D_Settings.IsLocked = true;
            for (int i = 0; i < _toUpdate.Count; i++)
            {
                if (_internalNodes.TryGetValue(_toUpdate[i], out InternalNode node))
                {
                    node.UpdateInstance();
                }
            }
            _toUpdate.Clear();
            Autotiles3D_Settings.IsLocked = false;
        }

        private void AddNodeInternal(int tileId, string tileName, string group, Vector3Int internalPosition, Quaternion localRotation)
        {
            if (!_internalNodes.ContainsKey(internalPosition))
            {
                //check for boundaries
                if (Grid.GridSize == LevelSize.Finite)
                {
                    if (Grid.IsExceedingLevelGrid(internalPosition))
                        return;
                }

                _internalNodes.Add(internalPosition, new InternalNode(this, group, tileName, tileId, internalPosition, localRotation));

                if (!Anchors.TryGetValue(tileId, out Autotiles3D_Anchor anchor))
                {
                    this.EnsureAnchor(group, tileName, tileId);
                    anchor = Anchors[tileId];
                }

                anchor.IncreaseBlockCount();

                RequireUpdate(internalPosition);
            }
        }

        private void RemoveNodeInternal(Vector3Int internalPosition)
        {
            if (_internalNodes.TryGetValue(internalPosition, out InternalNode node))
            {
                //dont allow remove of baked nodes
                if (node.Block != null)
                {
                    if (node.Block.IsBaked)
                    {
                        Debug.Log("Autotiles3D: Will not remove baked blocks.");
                        return;
                    }
                }
                node.Block.Anchor.DecreaseBlockCount();
                node.DeleteInstance();
                _internalNodes.Remove(internalPosition);
            }
        }
        public void DestroyHoverInstance()
        {
            if (_hoverInstance.instance != null)
            {
                DestroyImmediate(_hoverInstance.instance);
                _hoverInstance.tileID = -1;
                _hoverInstance.instance = null;
            }
        }
        public void ToggleView(Autotiles3D_Tile tile, bool enable)
        {
            var matches = _internalNodes.Where(p => p.Value.TileID == tile.TileID).ToList();
            foreach (var match in matches)
                match.Value.Block?.ToggleView(enable);
        }
        public void RemoveAllBlocks(Autotiles3D_Anchor anchor)
        {
            if (anchor != null)
            {
                var blocks = anchor.GetBlocks();
                blocks.ForEach(b => RemoveNodeInternal(b.InternalPosition));
            }
        }
        public void RemoveAll()
        {
            foreach (var placement in _internalNodes.ToArray())
                RemoveNodeInternal(placement.Key);
        }
        #region hotkeys
        void OnGUI()
        {
            var e = Event.current;
            if (e?.isKey == true)
            {
                if (e.type == EventType.KeyDown)
                {
                    HotKeySelection(e.keyCode);
                }
            }
        }

        public bool HotKeySelection(KeyCode keycode)
        {
            switch (keycode)
            {
                case KeyCode.Alpha1:
                    return TryHotkey(0);
                case KeyCode.Alpha2:
                    return TryHotkey(1);
                case KeyCode.Alpha3:
                    return TryHotkey(2);
                case KeyCode.Alpha4:
                    return TryHotkey(3);
                case KeyCode.Alpha5:
                    return TryHotkey(4);
                case KeyCode.Alpha6:
                    return TryHotkey(5);
                case KeyCode.Alpha7:
                    return TryHotkey(6);
                case KeyCode.Alpha8:
                    return TryHotkey(7);
                case KeyCode.Alpha9:
                    return TryHotkey(8);
                case KeyCode.Alpha0:
                    return TryHotkey(9);
            }
            return false;
        }

        private bool TryHotkey(int hotkey)
        {
            if (hotkey < Tiles.Count && hotkey >= 0)
            {
                SetActiveTileID(Tiles[hotkey].TileID);
                return true;
            }
            return false;
        }
        #endregion
#endif
    }

}