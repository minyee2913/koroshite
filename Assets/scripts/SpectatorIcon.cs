using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpectatorIcon : MonoBehaviour
{
    public TMP_Text nameTag;
    public Image icon;
    public Player target;

    public void OnSelect() {
        CamManager.main.spectatorTarget = target;
    }

    void Update() {
        if (CamManager.main.spectatorTarget != null && target != null) {
            if (CamManager.main.spectatorTarget == target) {
                nameTag.color = Color.yellow;
            } else {
                nameTag.color = Color.white;
            }
        }
    }
}
