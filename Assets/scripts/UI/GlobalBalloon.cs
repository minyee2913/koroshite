using UnityEngine;
using UnityEngine.UI;

public class GlobalBalloon : MonoBehaviour
{
    [SerializeField] GameObject TextBalloon;
    [SerializeField] Canvas canvas;
    public GameObject ShowBalloon(string text, Vector2 pos, float lifeTime) {
        var balloon = Instantiate(TextBalloon, canvas.transform);
        balloon.SetActive(true);
        balloon.transform.localScale = Vector2.one * 100;
        Text tx = balloon.GetComponentInChildren<Text>();
        tx.text = text;

        balloon.transform.position = new Vector3(pos.x, pos.y, balloon.transform.position.z);

        Destroy(balloon, lifeTime);

        return balloon;
    }
}
