using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviourPunCallbacks
{
    [SerializeField] string roomScene = "Room";

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        // If someone opened MainMenu directly, bounce to Bootstrap to connect.
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("Bootstrap");
            return;
        }

        // Ensure we’re in the lobby so Create/Join will work.
        if (!PhotonNetwork.InLobby) PhotonNetwork.JoinLobby();
    }

    // Buttons
    public void OnCreateRoom()
    {
        var opts = new RoomOptions { MaxPlayers = 4, PublishUserId = true };
        PhotonNetwork.CreateRoom("Room_" + Random.Range(1000, 9999), opts);
    }

    public void OnJoinRandom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnOptions() { Debug.Log("Options clicked"); }
    public void OnExit()    { Application.Quit(); }

    // Photon callbacks
    public override void OnJoinRandomFailed(short code, string msg)
    {
        // If no open room, make one so flow continues.
        OnCreateRoom();
    }

    public override void OnCreatedRoom()
    {
        // Master loads Room; others will follow automatically.
        if (PhotonNetwork.IsMasterClient) PhotonNetwork.LoadLevel(roomScene);
    }

    public override void OnJoinedRoom()
    {
        // In case we joined an existing room and we are master (e.g., first in), load Room.
        if (PhotonNetwork.IsMasterClient) PhotonNetwork.LoadLevel(roomScene);
        // Non-masters will auto follow when master loads the scene.
    }

    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.InLobby) PhotonNetwork.JoinLobby();
    }
}
