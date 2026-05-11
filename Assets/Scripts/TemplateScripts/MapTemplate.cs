using UnityEngine;

public enum MapType { Main, Part}

[CreateAssetMenu(fileName = "MapTemplate", menuName = "Map Template")]
public class MapTemplate : ScriptableObject
{
    public string id;
    public string mapName;
    public MapType mapType;
}
