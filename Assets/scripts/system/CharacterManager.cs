using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance = null;
    public List<Character> list;

    private void Awake() {
        instance = this;
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
}
