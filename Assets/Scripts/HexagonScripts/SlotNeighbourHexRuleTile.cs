using UnityEngine;
using UnityEngine.Tilemaps;

namespace HexagonScripts
{
    [CreateAssetMenu(fileName = "SlotNeighbourHexRuleTile", menuName = "2D/Tiles/Slot Neighbour Hex Rule Tile")]
    public class SlotNeighbourHexRuleTile : HexagonalRuleTile
    {
        [Tooltip("SlotTile")]
        public TileBase slotTile;

        public override bool RuleMatch(int neighbor, TileBase other)
        {
            return neighbor switch
            {
                TilingRuleOutput.Neighbor.This => other != null && other == slotTile,
                TilingRuleOutput.Neighbor.NotThis => other != slotTile,
                _ => base.RuleMatch(neighbor, other)
            };
        }
    }
}