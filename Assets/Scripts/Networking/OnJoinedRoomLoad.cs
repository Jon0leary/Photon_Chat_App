using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;


public class OnJoinedRoomLoad : MonoBehaviourPunCallbacks
{
    [SerializeField] private string roomScene = "Room";
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(roomScene);
        }
    }
}