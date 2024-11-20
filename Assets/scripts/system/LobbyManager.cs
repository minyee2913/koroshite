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
    [SerializeField] GameObject showSk;
    [SerializeField] GameObject startBtn;
    [SerializeField] GameObject storyPanel;
    public bool storyOpened;
    float updateRoomInterval;
    void Start()
    {
        if (PlayFabManager.tutorialEnded || LobbyTutorial.afterTuto) {
            GenerateNpc();
        }

        if (NetworkManager.instance != null) {
            UpdateRoom(NetworkManager.instance.rooms);
        }
    }

    void GenerateNpc() {
        StartCoroutine(genNpc());
    }

    public void OpenSelectedSkill() {
        WanderingNpc wandNpc = selected.GetComponent<WanderingNpc>();

        SoundManager.Instance.Play("select");

        if (wandNpc != null) {
            SkillInfo.Instance.Open(wandNpc.ch);
        }
    }

    IEnumerator genNpc() {
        while (!CharacterManager.ownChecked) {
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

        SoundManager.Instance.Play("lastdead");
    }

    // Update is called once per frame
    void Update()
    {
        if (spawned) {
            if (selected == null || !storyOpened || !roomPanel.activeSelf || !Pause.Instance.inPause) {
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
                        if (nearDistance > dir && hit.transform.tag == "npc")
                        {
                            nearDistance = dir;
                            selected = hit.transform;
                        }
                    }

                    if (selected != null) {
                        SoundManager.Instance.Play("select");
                        OnSelectNpc(selected);
                    }
                }
            } else if (selected != null) {
                if (Input.GetKeyDown(KeyCode.Escape)) {
                    ReleaseNpc();
                    SkillInfo.Instance.Close();
                }
            }

            updateRoomInterval += Time.deltaTime;

            if (updateRoomInterval > 10) {
                updateRoomInterval = 0;
                if (NetworkManager.instance != null) {
                    UpdateRoom(NetworkManager.instance.rooms);
                }
            }

            CamManager.main.transform.position = Vector3.Lerp(CamManager.main.transform.position, new Vector3(scroll_, CamManager.main.transform.position.y, CamManager.main.transform.position.z), 6 * Time.deltaTime);
        }
    }

    public void OnSelectNpc(Transform transform) {
        if (selected != null) {
            ReleaseNpc(true);
        }

        Pause.Instance.gameObject.SetActive(false);

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
        showSk.SetActive(true);
        startBtn.SetActive(false);
        storyPanel.SetActive(false);
    }

    public void OpenStory() {
        if (selected != null) {
            ReleaseNpc();
        }
        startBtn.SetActive(false);
        storyPanel.SetActive(true);
        storyOpened = true;

        SoundManager.Instance.Play("select");
    }

    public void CloseStory() {
        startBtn.SetActive(true);
        storyPanel.SetActive(false);
        storyOpened = false;

        SoundManager.Instance.Play("typing");
    }

    public void OpenChList() {
        OnSelectNpc(npcs[0].transform);

        SoundManager.Instance.Play("select");
    }

    public void ReleaseNpc(bool bySelect = false) {
        if (selected != null) {
            WanderingNpc wandNpc = selected.GetComponent<WanderingNpc>();

            if (wandNpc != null) {
                wandNpc.action = "idle";
                wandNpc.isMoving = false;
                wandNpc.ForceWhite = true;

                wandNpc.ch.render.sortingOrder = 0;
            }
        }

        if (!bySelect) {
            selected = null;

            CamManager.main.CloseOut(0.2f);
            CamManager.main.Offset(Vector2.zero, 0.2f);

            Pause.Instance.gameObject.SetActive(true);

            chList.SetActive(false);
            showSk.SetActive(false);
            startBtn.SetActive(true);
        }
    }

    public void OpenRooms() {
        roomPanel.SetActive(true);

        SoundManager.Instance.Play("select");

        roomPanel.transform.localScale = new Vector3(0.8f, 0.8f);
        roomPanel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutCubic);
    }

    public void CloseRooms() {
        roomPanel.SetActive(false);

        SoundManager.Instance.Play("typing");
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
