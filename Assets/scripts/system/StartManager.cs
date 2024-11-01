using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ExitGames.Client.Photon;
using Kino;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject left_bottom;
    [SerializeField] GameObject right_bottom;
    [SerializeField] GameObject right_top;
    [SerializeField] GameObject title;
    [SerializeField] GameObject titleK;
    [SerializeField] GameObject titleO;
    [SerializeField] GameObject titleS;
    [SerializeField] GameObject titleH;
    [SerializeField] GameObject roomPanel;
    [SerializeField] GameObject inputPanel;
    [SerializeField] GameObject loading;
    public InputField nametag;
    [SerializeField] InputField roomName;
    [SerializeField] TMP_Text error;
    [SerializeField] ScrollRect scroll;
    [SerializeField] RoomBtn btn;

    [SerializeField] NetworkManager network;
    [SerializeField] GameObject startBtn;

    void Awake() {
        loading.SetActive(false);

        if (NetworkManager.instance == null) {
            StartCoroutine(anim());
        }
    }

    IEnumerator anim() {
        Vector2 pos;
        Vector2 def = canvas.transform.position;
        right_bottom.SetActive(false);
        right_top.SetActive(false);
        title.SetActive(false);

        left_bottom.SetActive(true);

        pos = left_bottom.transform.position;

        left_bottom.transform.DOMove(def, 0.5f);
        left_bottom.transform.DOScale(Vector3.one * 2, 0.5f);

        yield return new WaitForSeconds(1f);

        left_bottom.transform.DOMove(pos, 0.4f);
        left_bottom.transform.DOScale(Vector3.zero, 0.4f);

        yield return new WaitForSeconds(0.4f);

        right_bottom.SetActive(true);

        pos = right_bottom.transform.position;

        right_bottom.transform.DOMove(def, 0.5f);
        right_bottom.transform.DOScale(Vector3.one * 2, 0.5f);

        yield return new WaitForSeconds(1f);

        right_bottom.transform.DOMove(pos, 0.4f);
        right_bottom.transform.DOScale(Vector3.zero, 0.4f);

        yield return new WaitForSeconds(0.4f);

        right_top.SetActive(true);

        pos = right_top.transform.position;

        right_top.transform.DOMove(def, 0.5f);
        right_top.transform.DOScale(Vector3.one * 2, 0.5f);

        yield return new WaitForSeconds(1f);

        right_top.transform.DOMove(pos, 0.4f);
        right_top.transform.DOScale(Vector3.zero, 0.4f);

        yield return new WaitForSeconds(1f);

        pos = title.transform.position;

        title.transform.localScale = Vector3.one * 2;
        title.SetActive(true);

        titleK.SetActive(true);
        titleO.SetActive(false);
        titleS.SetActive(false);
        titleH.SetActive(false);

        title.transform.position = def + new Vector2(0, 1);

        titleK.transform.rotation = Quaternion.Euler(0, 45.6f, -12.224f);
        titleK.transform.localPosition = new Vector3(-201, 87);

        titleK.transform.DOLocalMove(new Vector3(-59.58f, -37), 0.3f).SetEase(Ease.OutElastic);
        titleK.transform.DORotate(Vector3.zero, 0.3f);

        yield return new WaitForSeconds(0.4f);

        titleS.SetActive(true);

        titleS.transform.rotation = Quaternion.Euler(0, 45.6f, -12.224f);
        titleS.transform.localPosition = new Vector3(201, -87);

        titleS.transform.DOLocalMove(new Vector3(49f, -146), 0.3f).SetEase(Ease.OutElastic);
        titleS.transform.DORotate(new Vector3(0, 0, 4.281f), 0.3f);

        yield return new WaitForSeconds(0.3f);

        title.transform.position = pos;
        title.transform.DOScale(Vector2.one, 0.3f).SetEase(Ease.Linear);
        left_bottom.transform.DOScale(Vector2.one, 0.3f).SetEase(Ease.Linear);
        right_bottom.transform.DOScale(Vector2.one, 0.3f).SetEase(Ease.Linear);
        right_top.transform.DOScale(Vector2.one, 0.3f).SetEase(Ease.Linear);

        titleO.SetActive(true);
        titleH.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        loading.SetActive(true);

        yield return new WaitForSeconds(2f);

        network.gameObject.SetActive(true);
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

        startBtn.SetActive(true);
    }

    public void GoToLobby() {
        LoadingController.LoadScene("Lobby");
    }

    public void CloseRoomInput() {
        inputPanel.SetActive(false);
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
