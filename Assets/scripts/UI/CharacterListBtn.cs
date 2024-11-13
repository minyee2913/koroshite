using UnityEngine;
using UnityEngine.UI;

public class CharacterListBtn : MonoBehaviour
{
    public LobbyManager manager;
    public WanderingNpc target;
    public Image image;

    public void Start() {
        image.sprite = target.ch.render.sprite;
    }
    public void OnClick() {
        manager.OnSelectNpc(target.transform);
    }

    void Update() {
        if (manager.selected == target.transform) {
            image.color = Color.black;
        } else {
            image.color = Color.white;
        }
    }
}
