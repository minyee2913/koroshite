using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutChat : MonoBehaviour
{
    float lifetime;
    const float MaxLifeTime = 3;
    public ChatManager chat;

    // Update is called once per frame
    void Update()
    {
        lifetime += Time.deltaTime;

        if (lifetime > MaxLifeTime) {
            chat.outs.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}
