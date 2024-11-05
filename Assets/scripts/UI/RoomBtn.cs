using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class RoomBtn : MonoBehaviour
{
    public TMP_Text nameShadow, mainName;
    public TMP_Text owner;
    public TMP_Text playerCount, countShadow;

    public void CallJoinRoom() {
        PhotonNetwork.NickName = NetworkManager.instance.playerName;
        NetworkManager.instance.savedRoom = mainName.text;

        LoadingController.LoadScene("GameScene");
    }
}
