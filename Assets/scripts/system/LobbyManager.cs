using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject roomPanel;
    [SerializeField] GameObject inputPanel;
    [SerializeField] InputField roomName;
    [SerializeField] Text error;
    [SerializeField] ScrollRect scroll;
    [SerializeField] RoomBtn btn;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenRooms() {
        roomPanel.SetActive(true);

        roomPanel.transform.localScale = new Vector3(0.8f, 0.8f);
        roomPanel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutCubic);
    }

    public void CloseRooms() {
        roomPanel.SetActive(false);
    }

    public void CloseRoomInput() {
        inputPanel.SetActive(false);
    }

    public void Error(string txt) {
        error.text = txt;

        Invoke("errorEnd", 1.5f);
    }

    void errorEnd() {
        error.text = "";
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoom(roomList);
    }

    public void UpdateRoom(List<RoomInfo> rooms) {
        foreach (Transform child in scroll.content) {
            Destroy(child.gameObject);
        }
        foreach (RoomInfo rm in rooms) {
            var cp = rm.CustomProperties;
            RoomBtn btnn = Instantiate(btn, scroll.content);
            btnn.countShadow.text = btnn.playerCount.text = "(" + rm.PlayerCount + "/" + rm.MaxPlayers + ")";
            btnn.mainName.text = btnn.nameShadow.text = rm.Name;
            btnn.owner.text = "집주인: " + cp["owner"];
        }
    }
    public InputField nametag;
    public void CreateRoom() {
        PhotonNetwork.NickName = nametag.text;
        PhotonNetwork.CreateRoom(roomName.text, new RoomOptions { MaxPlayers = 8, PublishUserId=true }, null);

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
