using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/// Attach in the Room scene when playtesting.
/// Press Play in Editor (open Room) -> connects and joins "DEV_ROOM".
public class DevQuickStart : MonoBehaviourPunCallbacks
{
    [SerializeField] bool enableInEditor = true;
    [SerializeField] bool enableInBuild  = false;
    [SerializeField] string roomName = "DEV_ROOM";
    [SerializeField] byte   maxPlayers = 4;

    void Awake()
    {
#if UNITY_EDITOR
        if (!enableInEditor) { enabled = false; return; }
#else
        if (!enableInBuild)  { enabled = false; return; }
#endif
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
        else
            OnConnectedToMaster();
    }

    public override void OnConnectedToMaster()
    {
        // Skip lobby for speed: join/create right away
        var opts = new RoomOptions { MaxPlayers = maxPlayers, PublishUserId = true };
        PhotonNetwork.JoinOrCreateRoom(roomName, opts, TypedLobby.Default);
    }
}
