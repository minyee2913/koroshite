using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillInfo : MonoBehaviour
{
    public static SkillInfo Instance {get; private set;}
    public GameObject panel;
    [SerializeField] 
    TMP_Text characterName;
    [SerializeField] TMP_Text characterHp;
    [SerializeField] TMP_Text atkInfo;
    [SerializeField] TMP_Text atk2Info;
    [SerializeField] TMP_Text skill1Info;
    [SerializeField] TMP_Text skill2Info;
    [SerializeField] Image profile;
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    public void Open(Character ch) {
        characterName.text = ch.id.ToUpper();
        characterHp.text = "HP 800";
        atkInfo.text = ch.atkInfo;
        atk2Info.text = ch.atk2Info;
        skill1Info.text = ch.skill1Info;
        skill2Info.text = ch.skill2Info;

        profile.sprite = ch.icon;

        panel.SetActive(true);
    }

    public void Close() {

        panel.SetActive(false);
    }
}
