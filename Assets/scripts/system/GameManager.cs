using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    PhotonView pv;
    public GameObject CharacterPanel;

    public string state = "none";

    public bool applyPlayerInput;
    public List<Vector2> spawnPoses;
    public TMP_Text title;
    public readonly Vector3 spawnPos = new(-55.73f, -19.55f);
    void Start()
    {
        pv = GetComponent<PhotonView>();
        CharacterPanel.SetActive(true);

        Instance = this;

        applyPlayerInput = true;

        if (PhotonNetwork.InLobby) {
            PhotonNetwork.JoinRoom(NetworkManager.instance.savedRoom);
        }
    }

    public void StartGame() {
        StartCoroutine(stgm());
    }
    public void SetTitle(string str) {
        object[] obj = {
            str,
        };
        pv.RPC("setTitle", RpcTarget.All, obj);
    }
    [PunRPC]
    void setTitle(string str) {
        title.text = str;
    }

    public void SetStarted(bool bol) {
        object[] obj = {
            bol,
        };
        pv.RPC("setStarted", RpcTarget.All, obj);
    }
    [PunRPC]
    void setState(string st) {
        state = st;
    }

    IEnumerator stgm() {
        setState("starting");
        foreach (Player pl in Player.players) {
            pl.SetState("ready");
            pl.SetEnergy(0);
        }

        List<Vector2> poses = spawnPoses;
        var shuffled = poses.OrderBy( x => UnityEngine.Random.value ).ToList( );

        for (int i = 0; i < shuffled.Count; i++) {
            if (i < Player.players.Count) {
                Player.players[i].SetPos(shuffled[i]);
            }
        }

        yield return new WaitForSeconds(2f);

        for (int i = 5; i > 0; i--) {
            SetTitle(i.ToString());

            yield return new WaitForSeconds(1f);
        }

        SetTitle("<color=\"green\">GameStart!</color>");

        ChatManager.Instance.SendComment("<color=\"orange\">준비 단계가 시작되었습니다. 다른 참가자들을 처치하면 금화를 획득합니다.</color>");

        foreach (Player pl in Player.players) {
            pl.SetState("ingame");
        }

        setState("started");

        yield return new WaitForSeconds(1.5f);

        SetTitle("");

        yield break;
    }

    public void Spawn() {
        PhotonNetwork.Instantiate("Player", spawnPos, quaternion.identity);
    }

    public void ForceEnd() {
        if (PhotonNetwork.IsMasterClient) {
            GameEnd();
        }
    }

    public void GameEnd() {
        foreach (Player pl in Player.players) {
            pl.SetPos(spawnPos);

            pl.state = "room";
        }

        state = "none";
    }

    void Update()
    {
        if (state == "none") {
            if (Input.GetKeyDown(KeyCode.F)) {
                CharacterPanel.SetActive(!CharacterPanel.activeSelf);
            }
        }
    }
}
