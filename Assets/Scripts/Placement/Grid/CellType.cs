public enum TileType
{
    Ground,
    Soil
}

public class CellType : UnityEngine.MonoBehaviour
{
    // This class can be used to tag GameObjects with a tile type if needed.
    public TileType type;
}
