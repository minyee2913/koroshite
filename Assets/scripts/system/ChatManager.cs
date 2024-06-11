using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    [SerializeField]
    GameObject panel;
    [SerializeField]
    TMP_InputField inp;
    [SerializeField]
    ScrollRect view;
    [SerializeField]
    TMP_Text textData;
    [SerializeField]
    Canvas canvas;
    [SerializeField]
    Transform outChat;

    bool inChat;
    public bool scrollTo;

    private List<string> messages = new();
    public List<GameObject> outs = new();

    void Start() {
        panel.SetActive(false);

        scrollTo = true;
    }

    void UpdateMsg(bool forceScroll = false) {
        foreach(Transform child in view.content.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < messages.Count; i++) {
            var msg = messages[i];

            GameObject txt_obj = Instantiate(textData.gameObject, canvas.transform);
            TMP_Text txt = txt_obj.GetComponent<TMP_Text>();
            txt.text = msg;

            txt_obj.transform.localPosition = Vector2.zero;

            txt_obj.transform.SetParent(view.content, true);
            txt_obj.transform.localScale = Vector2.one;
        }

        if (forceScroll || scrollTo) {
            view.verticalNormalizedPosition = 0;
        }
    }

    public void ChatOpen() {
        panel.SetActive(true);
        outChat.gameObject.SetActive(false);

        inp.text = "";
        inp.ActivateInputField();
        inChat = true;
        
        GameManager.Instance.applyPlayerInput = false;

        UpdateMsg(true);
    }

    public void SendMessage() {
        messages.Add(inp.text);
        SendOutChat(inp.text);
        
        
        inp.text = "";

        inp.ActivateInputField();

        UpdateMsg(true);
    }

    public void SendOutChat(string text) {
        GameObject txt_obj = Instantiate(textData.gameObject, canvas.transform);
        TMP_Text txt = txt_obj.GetComponent<TMP_Text>();
        txt.text = text;

        txt_obj.transform.localPosition = Vector2.zero;

        txt_obj.transform.SetParent(outChat, true);

        var oc = txt_obj.AddComponent<OutChat>();
        oc.chat = this;
        
        outs.Add(txt_obj);
    }

    public void ChatClose() {
        panel.SetActive(false);
        outChat.gameObject.SetActive(true);

        inp.DeactivateInputField(true);
        inChat = false;
        
        GameManager.Instance.applyPlayerInput = true;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.T)) {
            if (inChat) {
                ChatClose();
            } else {
                ChatOpen();
            }
        }

        if (inChat) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                SendMessage();
            }
            if (Input.GetKeyDown(KeyCode.Escape)) {
                ChatClose();
            }
        } else {
            if (Input.GetKeyDown(KeyCode.T) && Input.GetKeyDown(KeyCode.Return)) {
                ChatOpen();
            }
        }

        if (view.verticalNormalizedPosition > 0.1) {
            scrollTo = false;
        } else {
            scrollTo = true;
        }
    }
}
