using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance = null;
    public Vector2 roomPos;
    private void Awake() {
        if (instance != null) {
            Destroy(this.gameObject);
        }
        instance = this;

        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        DontDestroyOnLoad(this.gameObject);

        Lobby();
    }

    public void Lobby() {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() {
        GameObject startObj = GameObject.Find("StartManager");

        if (startObj != null) {
            StartManager start = startObj.GetComponent<StartManager>();

            start.Connected();
        } else {
            PhotonNetwork.NickName = "test" + UnityEngine.Random.Range(1, 1000);
            PhotonNetwork.JoinOrCreateRoom("그녀는 그녀를 그녀했어", new RoomOptions { MaxPlayers = 8 }, null);
        }
        // PhotonNetwork.NickName = "test" + UnityEngine.Random.Range(1, 1000);
        // PhotonNetwork.JoinOrCreateRoom("그녀는 그녀를 그녀했어", new RoomOptions { MaxPlayers = 8 }, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("connected");
        //new Vector3(-0.554f, -3.37f)
        PhotonNetwork.Instantiate("Player", roomPos, quaternion.identity);
    }
}
