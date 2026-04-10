using UnityEngine;

namespace WILCommunityGame
{
    public enum BuildPieceType {Floor, Wall, Door}
    
    public class BuildPart : MonoBehaviour
    {
        [SerializeField] BuildPieceType pieceType =  BuildPieceType.Floor;
        
        public BuildPieceType Type => pieceType;
        
        public void SetPieceType(BuildPieceType value) => pieceType = value;
    }
}
