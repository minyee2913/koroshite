using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectCharacter : MonoBehaviour
{
    public Character ch;

    public void Select() {
        Player.Local.SetCharacter(ch.id);
        Player.Local.SetShield(0);
        GameManager.Instance.CharacterPanel.SetActive(false);

        UIManager.Instance.face.sprite = ch.icon;
    }
}
