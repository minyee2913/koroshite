using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject roomPanel;
    [SerializeField] GameObject inputPanel;
    [SerializeField] GameObject loading;
    public InputField nametag;
    [SerializeField] InputField roomName;
    [SerializeField] TMP_Text error;
    [SerializeField] ScrollRect scroll;
    [SerializeField] RoomBtn btn;

    void Awake() {
        loading.SetActive(true);
    }

    public void Error(string txt) {
        error.text = txt;

        Invoke("errorEnd", 1.5f);
    }

    void errorEnd() {
        error.text = "";
    }

    public void Connected() {
        loading.SetActive(false);

        roomPanel.SetActive(true);
    }

    public void CloseRoomInput() {
        inputPanel.SetActive(false);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoom(roomList);
    }

    public void UpdateRoom(List<RoomInfo> rooms) {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        foreach (RoomInfo rm in rooms) {
            RoomBtn btnn = Instantiate(btn, scroll.content);
            btnn.countShadow.text = btnn.playerCount.text = "(" + rm.PlayerCount + "/" + rm.MaxPlayers + ")";
            btnn.mainName.text = btnn.nameShadow.text = rm.Name;
            btnn.owner.text = "집주인: " + rm.CustomProperties["owner"];
        }
    }

    public void CreateRoom() {
        PhotonNetwork.NickName = nametag.text;
        PhotonNetwork.CreateRoom(roomName.text, new RoomOptions { MaxPlayers = 8 }, null);

        LoadingController.LoadScene("GameScene");
    }

    public void AddRoom() {
        if (nametag.text.Length <= 0) {
            Error("이름을 먼저 입력하세요!");

            return;
        }

        inputPanel.SetActive(true);
    }
}
