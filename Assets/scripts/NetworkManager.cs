using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance = null;
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
        PhotonNetwork.NickName = "test" + UnityEngine.Random.Range(1, 1000);
        PhotonNetwork.JoinOrCreateRoom("test", new RoomOptions { MaxPlayers = 10 }, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("connected");
        PhotonNetwork.Instantiate("Player", new Vector3(-0.554f, -3.37f), quaternion.identity);
    }
}