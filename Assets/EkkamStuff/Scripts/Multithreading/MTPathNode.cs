using Unity.Collections;
using UnityEngine;

namespace Ekkam
{
    public struct MTPathNode
    {
        public Vector2Int gridPosition;
        public bool isBlocked;
        private int gCost;
        private int hCost;

        public Vector2Int parentPosition;
        
        public Vector2Int nodePosition;
        // public NativeArray<Vector2Int> neighbours;

        public int GCost
        {
            get => gCost;
            set
            {
                gCost = value;
                UpdateFCost();
            }
        }

        public int HCost
        {
            get => hCost;
            set
            {
                hCost = value;
                UpdateFCost();
            }
        }

        public int FCost { get; private set; }
        private void UpdateFCost()
        {
            FCost = GCost + HCost;
        }
        
        public static MTPathNode CreateNode(Vector2Int gridPosition, bool isBlocked, int gCost, int hCost)
        {
            var node = new MTPathNode
            {
                gridPosition = gridPosition,
                isBlocked = isBlocked,
                GCost = gCost,
                HCost = hCost
            };
            return node;
        }
    }
    
    // public struct NodeNeighbors
    // {
    //     public Vector2Int NodePosition;
    //     public NativeArray<Vector2Int> Neighbors;
    // }
}