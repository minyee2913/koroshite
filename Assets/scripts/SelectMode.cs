using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectMode : MonoBehaviour
{
    public static SelectMode Instance {get; private set;}

    public GameObject panel;
    public GameObject btn;
    public TMP_Text display;

    void Start() {
        Instance = this;
    }

    void Update() {
        btn.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void Open() {
        panel.SetActive(true);
    }
    public void Close() {
        panel.SetActive(false);
    }
    public void Select(string mode) {
        switch (mode)
        {
            case "fak":
                GameManager.Instance.mode = GameMode.FreeAllKill;
                
                break;
            case "royale":
                GameManager.Instance.mode = GameMode.BattleRoyal;
                
                break;


        }
    }
}
