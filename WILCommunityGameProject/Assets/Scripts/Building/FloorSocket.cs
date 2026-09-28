using UnityEngine;

namespace WILCommunityGame
{
    public class FloorSocket : EdgeSocket
    {
        [SerializeField] bool acceptsFloorPieces = true;

        public override bool CanAcceptPart(BuildPieceType pieceType)
        {
            return acceptsFloorPieces && (pieceType == BuildPieceType.Floor || pieceType == BuildPieceType.BuildableObject);
        }
    }
}