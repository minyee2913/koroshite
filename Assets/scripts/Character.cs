using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Character : MonoBehaviour
{
    public string id;
    public Animator animator;
    public SpriteRenderer render;
    [SerializeField]
    public Player pl = null;
    public IEnumerator routine = null;
    public int atkType = 0;
    public float atkCool = 0;
    public bool atkCooling = false;
    public Vector3 defaultScale;
    void Awake()
    {
        animator = GetComponent<Animator>();
        render = GetComponent<SpriteRenderer>();

        defaultScale = transform.localScale;
    }
    public virtual void Attack() {}
    public virtual void Skill1() {}
    public virtual void Skill1Up() {}
    public virtual void Skill2() {}
    public virtual void Skill2Up() {}
    public virtual void OnHurt(ref int damage, Player attacker, ref bool cancel) {}

    public virtual void Callfunc(string method) {}

    private void FixedUpdate() {
        if (atkCool > 0) {
            atkCool -= Time.fixedDeltaTime;

            if (!atkCooling) atkCooling = true;
        } else if (atkCooling) {
            atkCool -= Time.fixedDeltaTime;

            if (atkCool < -0.4f) {
                atkType = 0;
                atkCooling = false;
            }
        }
    }

    public void CANCEL() {
        if (routine != null) {
            StopCoroutine(routine);

            routine = null;
        }
    }
}
