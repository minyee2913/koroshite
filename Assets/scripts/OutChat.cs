using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutChat : MonoBehaviour
{
    public float lifetime;
    const float MaxLifeTime = 3;
    const float FadeOutTime = 1;
    public ChatManager chat;
    Image img;
    TMP_Text txt;
    float defaultAlpha;

    float alpha;

    // Update is called once per frame
    void Update()
    {
        if (img == null) {
            img = GetComponent<Image>();
            txt = GetComponentInChildren<TMP_Text>();
            defaultAlpha = img.color.a;
        }

        lifetime += Time.deltaTime;

        if (lifetime > MaxLifeTime - FadeOutTime) {
            Color col = Color.black;
            col.a = defaultAlpha * ((FadeOutTime - alpha)/FadeOutTime);

            img.color = col;

            Color colText = txt.color;
            colText.a = 1 * (FadeOutTime - alpha)/FadeOutTime;
            Debug.Log((FadeOutTime - alpha)/FadeOutTime);

            txt.color = colText;

            alpha += Time.deltaTime;
        }

        if (lifetime > MaxLifeTime) {
            chat.outs.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}
