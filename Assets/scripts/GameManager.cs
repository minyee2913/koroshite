using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject CharacterPanel;
    void Start()
    {
        CharacterPanel.SetActive(true);

        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) {
            CharacterPanel.SetActive(!CharacterPanel.activeSelf);
        }
    }
}
