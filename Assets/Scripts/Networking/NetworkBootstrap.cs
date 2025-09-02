using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkBootstrap : MonoBehaviourPunCallbacks
{
    [SerializeField] string mainMenuScene = "MainMenu";
    [SerializeField] string roomScene     = "Room";
    [SerializeField] bool devSkipMenuToRoom = true;   // toggle per build

    void Start()
    {
        PhotonNetwork.NickName = "Player " + Random.Range(1, 999);
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
        else PhotonNetwork.JoinLobby();
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (devSkipMenuToRoom) { SceneManager.LoadScene(roomScene); return; }
#endif
        SceneManager.LoadScene(mainMenuScene);
    }
}