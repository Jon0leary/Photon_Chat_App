using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomUI : MonoBehaviourPunCallbacks
{
    [Header("Panels")]
    public GameObject lobbyPanel;
    public GameObject hudPanel;

    [Header("Texts")]
    public TMP_Text roomNameText;
    public TMP_Text statusText;

    [Header("Color Buttons")]
    public Button redBtn, blueBtn, yellowBtn, greenBtn;

    [Header("Prefabs")]
    public GameObject pawnRed, pawnBlue, pawnYellow, pawnGreen;

    private readonly HashSet<string> taken = new();
    private GameObject localPawn;

    bool _inRoom = false;

    void Start()
    {
        lobbyPanel.SetActive(true);
        hudPanel.SetActive(false);

        // Disable color buttons until we’re really in the room
        SetColorButtons(false);

        if (PhotonNetwork.InRoom)
        {
            _inRoom = true;
            if (roomNameText) roomNameText.text = PhotonNetwork.CurrentRoom?.Name ?? "(no room)";
            RefreshTakenFromRoomProps();
            UpdateButtons();
        }
        else
        {
            if (statusText) statusText.text = "Joining room...";
        }
    }

    public override void OnJoinedRoom()
    {
        _inRoom = true;
        if (roomNameText) roomNameText.text = PhotonNetwork.CurrentRoom?.Name ?? "(no room)";
        RefreshTakenFromRoomProps();
        UpdateButtons();                // enables buttons now
    }
    
    private void RefreshTakenFromRoomProps()
    {
        taken.Clear();
        var csv = PhotonNetwork.CurrentRoom?.CustomProperties?[NetKeys.ROOM_TAKEN] as string;
        if (string.IsNullOrEmpty(csv)) return;

        foreach (var s in csv.Split(','))
        {
            var t = s.Trim();
            if (!string.IsNullOrEmpty(t)) taken.Add(t);
        }
    }

    // ---- Color selection ----
    public void OnPickRed() => TrySpawn("Red", pawnRed);
    public void OnPickBlue()   => TrySpawn("Blue", pawnBlue);
    public void OnPickYellow() => TrySpawn("Yellow", pawnYellow);
    public void OnPickGreen()  => TrySpawn("Green", pawnGreen);

    private void TrySpawn(string color, GameObject prefab)
    {
        if (!_inRoom || PhotonNetwork.CurrentRoom == null)
        {
            if (statusText) statusText.text = "Still joining… try again in a sec.";
            return;
        }

        if (taken.Contains(color))
        {
            if (statusText) statusText.text = color + " is already taken.";
            return;
        }

        // prevent double clicks while we spawn
        SetColorButtons(false);

        taken.Add(color);
        PushTakenToRoom();

        var ht = new ExitGames.Client.Photon.Hashtable { { NetKeys.PLAYER_COLOR, color } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(ht);

        var pos = new Vector3(Random.Range(-4f, 4f), 1f, Random.Range(-4f, 4f));
        PhotonNetwork.Instantiate(prefab.name, pos, Quaternion.identity);

        lobbyPanel.SetActive(false);
        hudPanel.SetActive(true);

        if (statusText) statusText.text = "Spawned as " + color + ".";
    }

    private void PushTakenToRoom()
    {
        if (PhotonNetwork.CurrentRoom == null) return;   // <-- guard
        var csv = string.Join(",", taken);
        var ht = new ExitGames.Client.Photon.Hashtable { { NetKeys.ROOM_TAKEN, csv } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
    }
    
    private void UpdateButtons()
    {
        // enable only once in-room
        SetColorButtons(_inRoom);
        if (!_inRoom) return;

        redBtn.interactable &= !taken.Contains("Red");
        blueBtn.interactable &= !taken.Contains("Blue");
        yellowBtn.interactable &= !taken.Contains("Yellow");
        greenBtn.interactable &= !taken.Contains("Green");

        if (statusText) statusText.text = "Pick a color";
    }

    private void SetColorButtons(bool v)
    {
        if (redBtn)    redBtn.interactable = v;
        if (blueBtn)   blueBtn.interactable = v;
        if (yellowBtn) yellowBtn.interactable = v;
        if (greenBtn)  greenBtn.interactable = v;
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable changed)
    {
        if (!_inRoom) return;
        if (changed.ContainsKey(NetKeys.ROOM_TAKEN))
        {
            RefreshTakenFromRoomProps();
            UpdateButtons();
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (!_inRoom) return;
        var color = otherPlayer.CustomProperties?[NetKeys.PLAYER_COLOR] as string;
        if (!string.IsNullOrEmpty(color))
        {
            taken.Remove(color);
            PushTakenToRoom();
            UpdateButtons();
        }
    }

    public void OnLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
