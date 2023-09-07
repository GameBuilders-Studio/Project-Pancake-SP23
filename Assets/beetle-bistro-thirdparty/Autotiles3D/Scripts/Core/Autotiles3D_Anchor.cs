using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;
using System;

namespace Autotiles3D
{
    public class Autotiles3D_Anchor : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("TileID")]
        [HideInInspector]
        private int _tileID;
        [HideInInspector] public GameObject BakedParent;
        [HideInInspector][SerializeField] private int _childCount;  //serializng for perfomance
        [HideInInspector][SerializeField] private int _bakeCount;
        public int Childcount => _childCount;
        public int BakeCount => _bakeCount;

        public void IncreaseBlockCount()
        {
            _childCount++;
        }
        public void DecreaseBlockCount()
        {
            _childCount--;
        }
        public void SetBakeCount(int count)
        {
            _bakeCount = count;
        }
        public int TileID => _tileID;
        public void SetTileID(int tileID)
        {
            _tileID = tileID;
        }
        public List<Autotiles3D_BlockBehaviour> GetBlocks()
        {
            var children = GetComponentsInChildren<Autotiles3D_BlockBehaviour>(true).ToList();
            _childCount = children.Count();
            return children;
        }

        public Autotiles3D_TileLayer Layer => transform.GetComponentInParent<Autotiles3D_TileLayer>();

        public void ToggleViews(bool enable, bool includeBaked = false)
        {
            var blocks = GetBlocks();
            foreach (var block in blocks)
            {
                if (block.View == null)
                    continue;
                if (enable)
                {
                    if (includeBaked)
                        block.View.SetActive(true);
                    else if (!block.IsBaked)
                        block.View.SetActive(true);
                }
                else
                {
                    if (includeBaked)
                        block.View.SetActive(false);
                    else if (!block.IsBaked)
                        block.View.SetActive(false);
                }
            }
        }

#if UNITY_EDITOR
        //v1.3 Tries to repair block and tile link based on the tileID stored in the anchor
        public void TryAutoRepairBrokenBlocks()
        {
            var tile = Autotiles3D_Utility.GetTile(_tileID, "", "");
            if (tile != null)
            {
                var blocks = GetBlocks();
                foreach (var block in blocks)
                {
                    if (block.GetTile() == null)
                    {
                        Autotiles3D_Utility.RepairBlocks(block, blocks, tile.Group, tile.Name, tile.TileID);
                    }
                }
            }
        }
#endif
    }
}

