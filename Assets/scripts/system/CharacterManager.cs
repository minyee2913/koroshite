using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance = null;
    public List<Character> list;
    public static Dictionary<string, bool>owns = new();

    public static bool ownChecked;

    private void Awake() {
        instance = this;

        CheckOwns();
    }

    public void CheckOwns() {
        ownChecked = false;
        
        if (!PlayFabManager.login) {

            owns["samurai"] = true;
            owns["fighter"] = true;

            ownChecked = true;

            return;
        }

        PlayFabManager.GetHasCharacter((success)=>{
            foreach (Character ch in list) {
                owns[ch.id] = success.Data.ContainsKey("has_" + ch.id);
            }

            owns["samurai"] = true;
            owns["fighter"] = true;

            ownChecked = true;
        }, fail => {
            owns["samurai"] = true;
            owns["fighter"] = true;

            ownChecked = true;
        });
    }

    public bool IsOwn(Character ch) {
        if (!owns.ContainsKey(ch.id)) {
            return false;
        }
        return owns[ch.id];
    }
    public bool IsOwn(string id) {
        return owns[id];
    }

    public void Init(Character ch) {
        list.Add(ch);
    }

    public Character Get(string id) {
        for (int i = 0; i < list.Count; i++) {
            Character ch = list[i];

            if (ch.id == id) {
                return ch;
            }
        }

        return null;
    }

    public Sprite GetIcon(string id) {
        var ch = Get(id);
        if (!ch) {
            return null;
        }

        if (ch.icon == null) {
            SpriteRenderer render = ch.GetComponent<SpriteRenderer>();
            if (render != null) {
                return render.sprite;
            }
        }

        return ch.icon;
    }
}
