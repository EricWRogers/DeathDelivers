using UnityEngine;

[CreateAssetMenu(menuName = "Map Maker/Tile")]
public class MapTile : ScriptableObject
{
    public string tileName;
    public GameObject prefab;

    public Vector3Int size = Vector3Int.one;

    // Optional later
    public bool canRotate = true;
    public bool canStack = false;
}
