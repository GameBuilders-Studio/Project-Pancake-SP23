using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Autotiles3D
{
    [CreateAssetMenu(menuName = "Autotiles3D/TileGroup")]
    public class Autotiles3D_TileGroup : ScriptableObject, ISerializationCallbackReceiver
    {
        //serialized data
        public List<Autotiles3D_Tile> Tiles = new List<Autotiles3D_Tile>();
        private Dictionary<int, Autotiles3D_Tile> _map = new Dictionary<int, Autotiles3D_Tile>();
        private Dictionary<string, Autotiles3D_Tile> _nameMap = new Dictionary<string, Autotiles3D_Tile>();
        public Autotiles3D_Tile GetTile(int id)
        {
            if (_map.TryGetValue(id, out Autotiles3D_Tile tile))
            {
                return tile;
            }
            return null;
        }
        public Autotiles3D_Tile GetTileByIndex(int index)
        {
            if (index >= 0 && index < Tiles.Count)
                return Tiles[index];
            return null;
        }
        public Autotiles3D_Tile GetTile(string name)
        {
            if (_nameMap.TryGetValue(name, out Autotiles3D_Tile tile))
            {
                return tile;
            }

            //backwards compatibility
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (Tiles[i].Name == name)
                    return Tiles[i];
            }
            return null;
        }
        public void UpdateTilesWithGroupName()
        {
            foreach (var tile in Tiles)
            {
                tile.SetGroupName(this.name);
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void ConstructMapping()
        {
            _map = new Dictionary<int, Autotiles3D_Tile>();
            _nameMap = new Dictionary<string, Autotiles3D_Tile>();
            for (int i = 0; i < Tiles.Count; i++)
            {
                _map.Add(Tiles[i].TileID, Tiles[i]);

                if (!_nameMap.ContainsKey(Tiles[i].Name))
                    _nameMap.Add(Tiles[i].Name, Tiles[i]);
                else
                    Debug.LogWarning($"Autotiles3D:  You have multiple tiles with the same name (duplicated name: {Tiles[i].Name}). This is no longer permitted since Autotiles 1.3.\n Make sure to change your tiles to unique names");
            }
        }
        public void OnAfterDeserialize()
        {
            ConstructMapping();
        }

        public void OnBeforeSerialize()
        {
        }
    }

}