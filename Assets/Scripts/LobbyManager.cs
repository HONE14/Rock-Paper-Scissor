using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField newRoomInputField;
    [SerializeField] TMP_Text feedbackText;
    [SerializeField] Button StartGameButton;
    [SerializeField] GameObject roomPanel;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] GameObject RoomListObject;
    [SerializeField] GameObject playerListObject;
    [SerializeField] RoomItem roomItemPrefab;
    [SerializeField] PlayerItem playerItemPrefab;
    List<RoomItem> roomItemList = new List<RoomItem>();
    List<PlayerItem> playerItemList = new List<PlayerItem>();
    Dictionary<string, RoomInfo> roomInfoCache = new Dictionary<string, RoomInfo>();
    private void Start()
    {
        PhotonNetwork.JoinLobby();
        roomPanel.SetActive(false);
    }
    public void ClickCreateRoom()
    {
        feedbackText.text = "";
        if(newRoomInputField.text.Length < 3)
        {
            feedbackText.text = "Room Name min 3 characters";
            return;
        }
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 5;
        PhotonNetwork.CreateRoom(newRoomInputField.text, roomOptions);
    }
    public void ClickStartGame(string levelName)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(levelName);
        }
    }
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("Created Room: " + PhotonNetwork.CurrentRoom.Name);
        feedbackText.text = "Created room: " + PhotonNetwork.CurrentRoom.Name;
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        feedbackText.text = "Joined room: " + PhotonNetwork.CurrentRoom.Name;
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        roomPanel.SetActive(true);
        UpdatePlayerList();
        SetStartGameButton();
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayerList();
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayerList();
    }
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        SetStartGameButton();
    }
    private void SetStartGameButton()
    {
        StartGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        StartGameButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= 2;
    }
    private void UpdatePlayerList()
    {
        foreach(var item in playerItemList)
        {
            Destroy(item.gameObject);
        }
        playerItemList.Clear();
        foreach(var(id, player) in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerListObject.transform);
            newPlayerItem.Set(player);
            playerItemList.Add(newPlayerItem);
            if(player == PhotonNetwork.LocalPlayer)
                newPlayerItem.transform.SetAsFirstSibling();
        }
        SetStartGameButton();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(returnCode+","+message);
        feedbackText.text = returnCode.ToString()+": "+message;
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(var roomInfo in roomList)
        {
            roomInfoCache[roomInfo.Name] = roomInfo;
        }
        Debug.Log("Room Updated");
        foreach(var item in this.roomItemList)
        {
            Destroy(item.gameObject);
        }
        this.roomItemList.Clear();
        var roomInfoList = new List<RoomInfo>(roomInfoCache.Count);
        foreach(var roomInfo in roomInfoCache.Values)
        {
            if(roomInfo.IsOpen)
                roomInfoList.Add(roomInfo);
        }
        foreach(var roomInfo in roomInfoCache.Values)
        {
            if(roomInfo.IsOpen == false)
                roomInfoList.Add(roomInfo);
        }
        foreach(var roomInfo in roomList)
        {
            if(roomInfo.IsVisible == false || roomInfo.MaxPlayers == 0)
                continue;
            RoomItem newRoomItem = Instantiate(roomItemPrefab, RoomListObject.transform);
            newRoomItem.Set(this, roomInfo);
            this.roomItemList.Add(newRoomItem);
        }
    }
}
