using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HoverText : MonoBehaviour
{
    Text text;
    Color def;
    public Color hover;
    void Awake() {
        text = GetComponent<Text>();
        def = text.color;
    }
    public void OnEnter(){
        text.color = hover;
    }
    public void OnExit(){
        text.color = def;
    }
}
