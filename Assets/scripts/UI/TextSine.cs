using UnityEngine;
using UnityEngine.UI;

public class TextSine : MonoBehaviour
{
    Text text;
    float time;
    public float speed;
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        Color col = Color.white;

        col.a = Mathf.Sin(time * speed);

        text.color = col;
    }
}
