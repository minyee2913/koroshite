using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
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
    [SerializeField] GameObject cover_1, cover_2, cover;
    public InputField nametag;
    [SerializeField] InputField roomName;
    [SerializeField] TMP_Text error;
    [SerializeField] ScrollRect scroll;
    [SerializeField] RoomBtn btn;

    [SerializeField] NetworkManager network;
    [SerializeField] GameObject startBtn;
    [SerializeField] Image dead;
    [SerializeField] Sprite dead_sp, dead_sp1, dead_sp2;
    [SerializeField] Volume vol;
    [SerializeField] Text comment;
    public GameObject afterConnected, afterLogin, emitter;
    bool SINEing;
    float sineTime;

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

        title.transform.position = def + new Vector2(-1, 1);
        title.transform.DOScale(Vector2.one, 0.3f).SetEase(Ease.OutCubic);
        left_bottom.SetActive(false);
        right_bottom.SetActive(false);
        right_top.SetActive(false);

        titleO.SetActive(true);
        titleH.SetActive(true);

        yield return new WaitForSeconds(1f);

        loading.SetActive(true);

        yield return new WaitForSeconds(1f);

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

        StartCoroutine(connected());
    }

    IEnumerator connected() {
        yield return new WaitForSeconds(1f);
        dead.gameObject.SetActive(true);
        vol.weight = 0;

        cover_2.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        cover_1.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        cover.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        dead.sprite = dead_sp;
        Vector2 dPos = dead.transform.localPosition;
        for (int i = 0; i < 10; i++) {
            dead.transform.localPosition = dPos + new Vector2(Random.Range(10f, -10f), Random.Range(10f, -10f));
            yield return new WaitForSeconds(0.01f);
        }
        

        yield return new WaitForSeconds(1f);

        afterConnected.transform.position = afterConnected.transform.position + new Vector3(0, -200);
        afterConnected.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        afterConnected.transform.DOMoveY(0, 0.5f).SetEase(Ease.InSine);
    }

    public void AfterLogin(){
        afterConnected.SetActive(false);
        afterLogin.SetActive(true);
    }

    public void GoToLobby() {
        if (PlayFabManager.tutorialEnded) {
            LoadingController.LoadScene("Lobby");
        } else {
            StartCoroutine(GoToTuto());
        }
    }

    IEnumerator GoToTuto() {
        afterLogin.SetActive(false);
        comment.gameObject.SetActive(true);
        comment.text = "";

        dead.transform.DOScaleY(-1f, 0.5f);
        dead.transform.DOLocalMoveY(-525.68f, 0.5f);
        yield return new WaitForSeconds(0.5f);

        Vector2 dPos = dead.transform.localPosition;
        for (int i = 0; i < 10; i++) {
            dead.transform.localPosition = dPos + new Vector2(Random.Range(10f, -10f), Random.Range(10f, -10f));
            yield return new WaitForSeconds(0.01f);
        }

        dead.transform.DOScaleY(-1f, 1f).SetEase(Ease.OutCubic);

        dead.transform.DOLocalMoveY(0f, 0.1f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.2f);

        dead.sprite = dead_sp1;

        yield return new WaitForSeconds(0.2f);

        dead.sprite = dead_sp2;

        sineTime = 0;
        SINEing = true;

        emitter.SetActive(true);

        yield return new WaitForSeconds(4f);

        comment.text = "우주에는 다양한 세계가 있고";

        yield return new WaitForSeconds(2f);

        comment.text = "세계에는 각기 다른 시대의 영웅들이 있다.";

        yield return new WaitForSeconds(2.5f);

        comment.text = "";

        yield return new WaitForSeconds(2f);

        comment.text = "그들은 각자의 사명에 살아가고";

        yield return new WaitForSeconds(2f);

        comment.text = "사명을 위해 자신의 모든 것을 불태우지.";

        yield return new WaitForSeconds(2.5f);

        comment.text = "";

        yield return new WaitForSeconds(2f);


        comment.text = "만약, 이 영웅들이 한번 사명을 잊게 된다면";

        yield return new WaitForSeconds(3f);

        comment.text = "다시 사명을 주었을때 그들은 사명을 어떻게 받아들일까..?";

        yield return new WaitForSeconds(3f);

        comment.text = "";

        yield return new WaitForSeconds(2f);

        LoadingController.LoadScene("Tutorial");
    }

    void Update() {
        if (SINEing) {
            sineTime += Time.deltaTime;
            dead.transform.localPosition = new Vector3(0, Mathf.Sin(sineTime) * 40);
        }
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
