using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageInfo : MonoBehaviour
{
    public Color color;
    public string txt;
    TMP_Text tmp;

    public float lifetime = 0;
    void Start()
    {
        tmp = GetComponentInChildren<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        tmp.color = color;
        tmp.text = txt;

        transform.Translate(Vector2.up * Time.deltaTime * 0.25f);
        lifetime += Time.deltaTime;

        if (lifetime > 1.5f) {
            Destroy(gameObject);
        }
    }
}
