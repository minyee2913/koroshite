using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    GameObject roomUI;
    [SerializeField]
    TMP_Text coin;
    [SerializeField]
    Slider hp;
    [SerializeField]
    Slider energy;
    public Image face;
    public TMP_Text sectionTitle;

    public static UIManager Instance {get; private set;}
    void Awake() {
        Instance = this;
    }

    void Update() {
        if (Player.Local != null) {
            roomUI.SetActive(Player.Local.state == "room");

            if (Player.Local.state == "room") {
                sectionTitle.text = "대기중 - " + PhotonNetwork.CurrentRoom.Name + "\n(" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + ")";
            }

            hp.value = (float)Player.Local.health / Player.Local.maxHealth;
            coin.text = Player.Local.coin.ToString() + "G";
        }
    }
}
