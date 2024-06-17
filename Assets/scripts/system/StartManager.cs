using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartManager : MonoBehaviour
{
    [SerializeField]
    GameObject roomPanel;
    [SerializeField]
    GameObject loading;

    void Awake() {
        loading.SetActive(true);
    }

    public void Connected() {
        loading.SetActive(false);
    }
}
