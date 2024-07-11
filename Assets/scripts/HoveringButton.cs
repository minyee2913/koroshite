using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoveringButton : MonoBehaviour
{
    Vector2 defaultvec;
    public Vector2 before;
    public Vector2 after;
    void Awake() {
        defaultvec = transform.localScale;
        before = defaultvec;
        after = defaultvec * 1.25f;
    }
    public void OnHover()
    {
        transform.DOKill();
        transform.localScale = before;
        transform.DOScale(after, 0.2f);
    }

    public void OutHover()
    {
        transform.DOKill();
        transform.localScale = after;
        transform.DOScale(before, 0.2f);
    }
}