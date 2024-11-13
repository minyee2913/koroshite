using UnityEngine;
using UnityEngine.UI;

public class CharacterListBtn : MonoBehaviour
{
    public LobbyManager manager;
    public WanderingNpc target;
    public Image image;
    public Text nametag;

    public void Start() {
        image.sprite = target.ch.icon;
        nametag.text = target.ch.id.ToUpper();
    }
    public void OnClick() {
        manager.OnSelectNpc(target.transform);

        SoundManager.Instance.Play("typing");
    }

    void Update() {
        if (manager.selected == target.transform) {
            image.color = Color.black;
        } else {
            image.color = new Color(200, 200, 200, 255);
        }
    }
}
