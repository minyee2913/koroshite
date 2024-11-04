using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterState : MonoBehaviour
{
    public Image background;
    public Text nametag;
    public Slider hprate;
    public string Name;

    // Update is called once per frame
    void Update()
    {
        nametag.text = Name;
        background.transform.localScale = new Vector3(Name.Length + 0.4f, 1);
    }
}
