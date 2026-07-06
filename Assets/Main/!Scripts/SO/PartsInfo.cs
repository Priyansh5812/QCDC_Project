using UnityEngine;
using AYellowpaper.SerializedCollections;
[CreateAssetMenu(fileName = "PartsInfo", menuName = "Scriptable Objects/PartsInfo")]
public class PartsInfo : ScriptableObject
{
    [SerializedDictionary("PartID", "Description")]
    public SerializedDictionary<int, PartInfo> descReg;
}

[System.Serializable]
public struct PartInfo
{
    public string partName;
    public string description;
}