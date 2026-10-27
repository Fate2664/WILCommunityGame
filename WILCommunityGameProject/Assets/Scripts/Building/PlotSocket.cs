using UnityEngine;

namespace WILCommunityGame
{
    public class PlotSocket : EdgeSocket
    {
        public LandManager Plot => GetComponentInParent<LandManager>();

        public override bool CanAcceptPart(BuildPieceType pieceType)
        {
            return Plot != null && pieceType == BuildPieceType.BuildableObject;
        }
    }
}
