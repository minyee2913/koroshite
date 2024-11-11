using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DialogueDirection {
    Right,
    Left
}
public struct DialogueContent {
    public string name, msg;
    public Sprite face;
    public DialogueDirection direction;
    public float typing;

}
public class DialogueController : MonoBehaviour
{
    [SerializeField] Text rightText, leftText, msg;
    [SerializeField] Image rightCh, leftCh;
    List<DialogueContent> contents = new();
    int now = 0;
    public GameObject body;
    public DialogueDirection direction = DialogueDirection.Right;

    public string message;
    public string displayName;
    public Sprite face;
    public bool opened;
    public bool typing;
    float typingTime;

    IEnumerator lastRoutine = null;

    public static DialogueController Instance {get; private set;}

    void Start()
    {
        Instance = this;
    }

    public static void Add(string name, string msg, Sprite face, DialogueDirection direction = DialogueDirection.Right, float typing = 1.5f) {
        if (Instance != null) {
            Instance.contents.Add(new DialogueContent{
                name = name,
                direction = direction,
                msg = msg,
                face = face,
                typing = typing,
            });
        }
    }

    public static void Show(int now = 0) {
        if (Instance != null) {
            Instance.opened = true;
            Instance.now = now;

            var content = Instance.contents[now];

            Instance.body.SetActive(true);
            Instance.displayName = content.name;
            Instance.direction = content.direction;
            Instance.message = content.msg;
            Instance.msg.text = "";
            Instance.typing = true;
            Instance.typingTime = content.typing;
            Instance.face = content.face;

            if (Instance.lastRoutine != null) {
                Instance.StopCoroutine(Instance.lastRoutine);
            }

            Instance.lastRoutine = Instance.typingMsg();

            Instance.StartCoroutine(Instance.lastRoutine);
        }
    }

    IEnumerator typingMsg() {
        string msg_ = "";
        for (int i = 0; i < message.Length; i++) {
            msg_ += message[i];
            msg.text = msg_;
            SoundManager.Instance.Play("typing", true);
            
            yield return new WaitForSeconds(typingTime / message.Length);
        }

        msg.text = message;

        typing = false;
        lastRoutine = null;
    }

    public static void Hide() {
        if (Instance != null) {
            Instance.opened = false;
            Instance.body.SetActive(false);
            Instance.contents.Clear();
        }
    }
    
    void Update()
    {
        if (opened) {
            if (direction == DialogueDirection.Right) {
                rightCh.gameObject.SetActive(true);
                leftCh.gameObject.SetActive(false);
            } else {
                leftCh.gameObject.SetActive(true);
                rightCh.gameObject.SetActive(false);
            }

            rightText.text = leftText.text = displayName;
            rightCh.sprite = leftCh.sprite = face;

            if (typing) {
                if (Input.GetMouseButtonDown(0)) {
                    if (Instance.lastRoutine != null) {
                        Instance.StopCoroutine(Instance.lastRoutine);

                        Instance.lastRoutine = null;
                    }

                    msg.text = message;
                    typing = false;
                }
            } else {
                if (Input.GetMouseButtonDown(0)) {
                    if (now+1 < contents.Count) {
                        Show(now+1);
                    } else {
                        Hide();
                    }
                }
            }

            
        }
    }
}
