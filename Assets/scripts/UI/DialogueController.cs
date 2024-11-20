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

    public void SetChColor(Color col) {
        rightCh.color = col;
        leftCh.color = col;
    }

    public static DialogueController Init(string name, string msg, Sprite face, DialogueDirection direction = DialogueDirection.Right, float typing = 0.1f) {
        if (Instance != null) {
            Instance.contents.Add(new DialogueContent{
                name = name,
                direction = direction,
                msg = msg,
                face = face,
                typing = typing,
            });

        }

        return Instance;
    }

    public DialogueController Add(string name, string msg, Sprite face, DialogueDirection direction = DialogueDirection.Right, float typing = 0.1f) {
        contents.Add(new DialogueContent{
            name = name,
            direction = direction,
            msg = msg,
            face = face,
            typing = typing,
        });

        return Instance;
    }

    public void Show(int now = 0) {
        opened = true;
        this.now = now;

        var content = contents[now];

        body.SetActive(true);
        displayName = content.name;
        direction = content.direction;
        message = content.msg;
        msg.text = "";
        typing = true;
        typingTime = content.typing;
        face = content.face;

        if (lastRoutine != null) {
            StopCoroutine(lastRoutine);
        }

        lastRoutine = typingMsg();

        StartCoroutine(lastRoutine);
    }

    IEnumerator typingMsg() {
        string msg_ = "";
        for (int i = 0; i < message.Length; i++) {
            msg_ += message[i];
            msg.text = msg_;
            if (typingTime / message.Length > 0.05) SoundManager.Instance.Play("typing", true);

            if (i < message.Length-1 && (message[i] == '.' || message[i] == '?' || message[i] == '!')) {
                yield return new WaitForSeconds(typingTime * 2);
            }
            
            yield return new WaitForSeconds(typingTime);
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
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) {
                    if (Instance.lastRoutine != null) {
                        Instance.StopCoroutine(Instance.lastRoutine);

                        Instance.lastRoutine = null;
                    }

                    msg.text = message;
                    typing = false;
                }
            } else {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) {
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
