using System;
using System.Collections;
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
    [SerializeField] WanderingNpc npc;
    [SerializeField] PlayFabManager fabManager;
    public List<WanderingNpc> npcs;
    public float scroll_, scrollSpeed, nearDistance;
    public Transform selected;
    public bool spawned;
    public GameObject chList;
    [SerializeField] Transform chListTr;
    [SerializeField] CharacterListBtn chBtn;
    void Start()
    {
        if (PlayFabManager.tutorialEnded || LobbyTutorial.afterTuto) {
            GenerateNpc();
        }
    }

    void GenerateNpc() {
        StartCoroutine(genNpc());
    }

    IEnumerator genNpc() {
        while (!CharacterManager.ownChecked) {
            Debug.Log("waiting");
            yield return null;
        }

        foreach (Character ch in CharacterManager.instance.list) {
            Debug.Log(ch.id + " : " + CharacterManager.instance.IsOwn(ch));
            if (/*CharacterManager.instance.IsOwn(ch)*/true) {
                var wn = Instantiate(npc, new Vector3(UnityEngine.Random.Range(-8f, 8f), -3.22f), Quaternion.identity);
                var btn = Instantiate(chBtn, chListTr);

                wn.character = ch.id;
                wn.ForceWhite = true;

                btn.target = wn;
                btn.manager = this;

                npcs.Add(wn);
            }
        }

        spawned = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (spawned) {
            if (selected == null) {
                float scroll = Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;

                scroll_ = Mathf.Clamp(scroll_ - scroll, -10, 10);

                if (Input.GetMouseButton(1)) {
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    scroll_ = Mathf.Clamp(mousePos.x, -10, 10);
                }
                if (Input.GetMouseButton(0)) {
                    var clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    var hits = Physics2D.RaycastAll(clickPos, Vector2.zero);

                    nearDistance = 10;

                    foreach (var hit in hits)
                    {
                        float dir = Vector2.Distance(hit.transform.position, clickPos);
                        Debug.Log(hit.transform.tag);
                        if (nearDistance > dir && hit.transform.tag == "npc")
                        {
                            nearDistance = dir;
                            selected = hit.transform;
                            Debug.Log("checked!!!!!!!!");
                        }
                    }

                    if (selected != null) {
                        OnSelectNpc(selected);
                    }
                }
            } else {
                if (Input.GetKeyDown(KeyCode.Escape)) {
                    ReleaseNpc();
                }
            }

            CamManager.main.transform.position = Vector3.Lerp(CamManager.main.transform.position, new Vector3(scroll_, CamManager.main.transform.position.y, CamManager.main.transform.position.z), 6 * Time.deltaTime);
        }
    }

    public void OnSelectNpc(Transform transform) {
        scroll_ = transform.position.x;
        CamManager.main.CloseUp(3.6f, 0, 0.2f);
        CamManager.main.Offset(new Vector2(0, -1), 0.2f);

        selected = transform;

        WanderingNpc wandNpc = transform.GetComponent<WanderingNpc>();

        if (wandNpc != null) {
            wandNpc.action = "selected";
            wandNpc.isMoving = false;
            wandNpc.ForceWhite = false;

            wandNpc.ch.render.sortingOrder = 1;
        }

        chList.SetActive(true);
    }

    public void OpenChList() {
        OnSelectNpc(npcs[0].transform);
    }

    public void ReleaseNpc() {
        if (selected != null) {
            WanderingNpc wandNpc = selected.GetComponent<WanderingNpc>();

            if (wandNpc != null) {
                wandNpc.action = "idle";
                wandNpc.isMoving = false;
                wandNpc.ForceWhite = true;

                wandNpc.ch.render.sortingOrder = 0;
            }
        }

        selected = null;

        CamManager.main.CloseOut(0.2f);
        CamManager.main.Offset(Vector2.zero, 0.2f);

        chList.SetActive(false);
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
        PhotonNetwork.NickName = NetworkManager.instance.playerName;
        PhotonNetwork.CreateRoom(roomName.text, new RoomOptions { MaxPlayers = 8, PublishUserId=true }, null);

        LoadingController.LoadScene("GameScene");
    }

    public void AddRoom() {
        inputPanel.SetActive(true);
    }
}
