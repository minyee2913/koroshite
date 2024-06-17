using System.Collections;
using System.Collections.Generic;
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

    public static UIManager Instance {get; private set;}
    void Awake() {
        Instance = this;
    }

    void Update() {
        if (Player.Local != null) {
            roomUI.SetActive(Player.Local.state == "room");

            hp.value = (float)Player.Local.health / Player.Local.maxHealth;
        }
    }
}
