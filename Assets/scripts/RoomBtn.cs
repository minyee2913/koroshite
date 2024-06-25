using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomBtn : MonoBehaviour
{
    public TMP_Text nameShadow, mainName;
    public TMP_Text owner;
    public TMP_Text playerCount, countShadow;

    public void CallJoinRoom() {
        if (NetworkManager.instance.SetNickName()) {
            NetworkManager.instance.savedRoom = mainName.text;

            LoadingController.LoadScene("GameScene");
        }
    }
}
