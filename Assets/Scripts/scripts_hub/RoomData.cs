using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomData", menuName = "VR/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("Scene Settings")]
    [Tooltip("Nazwa sceny do wczytania (dok³adnie taka jak w Build Settings)")]
    public string sceneName;

    [Tooltip("Opcjonalna nazwa pokoju do wyœwietlenia w UI")]
    public string displayName;
}