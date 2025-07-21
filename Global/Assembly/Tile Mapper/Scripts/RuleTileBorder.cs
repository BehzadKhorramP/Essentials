using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadApper.TileMap
{
    [CreateAssetMenu(fileName = "BorderRuleTile", menuName = "Tiles/BorderRuleTile")]
    public class RuleTileBorder : RuleTileScaleable
    {
        public override bool RuleMatch(int neighbor, TileBase other)
        {
            switch (neighbor)
            {
                case TilingRuleOutput.Neighbor.This: return other == this;
                case TilingRuleOutput.Neighbor.NotThis: return other != null && other != this;
            }

            return true;
        }
        
    }
}
