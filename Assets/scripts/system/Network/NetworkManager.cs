using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
using UnityEngine;
using PlayFab;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance = null;
    public Vector2 roomPos;
    public int state = 0;
    public string savedRoom;
    public string playerName;
    public List<RoomInfo> rooms = new();

    private void Awake() {
        if (instance != null) {
            Destroy(this.gameObject);
        }
        instance = this;

        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        PhotonNetwork.EnableCloseConnection = true;

        DontDestroyOnLoad(gameObject);

        Lobby();
    }

    public void Lobby() {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        rooms = roomList;
    }

    public override void OnConnectedToMaster() {
        GameObject startObj = GameObject.Find("StartManager");

        if (startObj != null) {
            StartManager start = startObj.GetComponent<StartManager>();

            start.Connected();

            PhotonNetwork.JoinLobby();
        }
        if (SceneManager.GetActiveScene().name == "GameScene") {
            PhotonNetwork.NickName = "test" + UnityEngine.Random.Range(1, 1000);
            PhotonNetwork.JoinOrCreateRoom("그녀는 그녀를 그녀했어", new RoomOptions { MaxPlayers = 8 }, null);
        }
        // PhotonNetwork.NickName = "test" + UnityEngine.Random.Range(1, 1000);
        // PhotonNetwork.JoinOrCreateRoom("그녀는 그녀를 그녀했어", new RoomOptions { MaxPlayers = 8 }, null);
    }

    public bool SetNickName() {
        GameObject startObj = GameObject.Find("StartManager");

        if (startObj != null) {
            StartManager start = startObj.GetComponent<StartManager>();

            if (start.nametag.text.Length <= 0) {
                start.Error("이름을 먼저 입력하세요!");

                return false;
            }

            PhotonNetwork.NickName = start.nametag.text;

            return true;
        }

        return false;
    }

    public override void OnCreatedRoom()
    {
    }

    public override void OnJoinedRoom()
    {

        StartCoroutine(OnJoin());
    }

    IEnumerator OnJoin() {
        yield return new WaitForSeconds(2);

        state = 1;

        GameObject Obj = GameObject.Find("GameManager");
        while (Obj == null) {
            Obj = GameObject.Find("GameManager");

            yield return null;
        }

        Debug.LogWarning("connected");

        GameManager gm = Obj.GetComponent<GameManager>();

        gm.Spawn();
    }

    public override void OnLeftRoom()
    {
        Player.players.Clear();
        Player.Local = null;
        state = 0;

        LoadingController.LoadScene("Lobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Player.players.Clear();
        Player.Local = null;
        state = 0;

        LoadingController.LoadScene("StartScene");
    }
}
