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

        UpdateText();
    }

    void UpdateText() {
        if (GameManager.Instance.mode == GameMode.FreeAllKill) {
            display.text = "> DEATH MATCH <";
        } else if (GameManager.Instance.mode == GameMode.BattleRoyal) {
            display.text = "> BATTLE ROYALE <";
        } else if (GameManager.Instance.mode == GameMode.Dual) {
            display.text = "> DUAL <";
        }
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
                GameManager.Instance.SetMode(GameMode.FreeAllKill);
                
                break;
            case "royale":
                GameManager.Instance.SetMode(GameMode.BattleRoyal);
                
                break;
            case "dual":
                GameManager.Instance.SetMode(GameMode.Dual);
                
                break;


        }

        UpdateText();

        Close();
    }
}
