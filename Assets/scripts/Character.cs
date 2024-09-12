using System.Collections;
using Photon.Pun;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public string id;
    public Animator animator;
    public PhotonView pv;
    public SpriteRenderer render;
    [SerializeField]
    public Player pl = null;
    abstract public int maxHealth {get;}
    public IEnumerator routine = null;
    public int atkType = 0;
    public float atkCool = 0;
    public bool atkCooling = false;
    public Vector3 defaultScale;
    public Sprite icon;
    string ownerName = "";
    public string state = null;
    int ownerCode = 0;
    public abstract string atkInfo {get;}
    public abstract string atk2Info {get;}
    public abstract string skill1Info {get;}
    public abstract string skill2Info {get;}
    void Awake()
    {
        pv = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();
        render = GetComponent<SpriteRenderer>();

        defaultScale = transform.localScale;

        if (icon == null) {
            icon = render.sprite;
        }

        if (pv.InstantiationData == null) {
            return;
        }

        if (pv.InstantiationData.Length > 0) {
            ownerName = (string)pv.InstantiationData[0];

            if (pv.InstantiationData.Length > 1) {
                ownerCode = (int)pv.InstantiationData[1];
            }
        }
    }
    public virtual void OnStart() {}
    public virtual void OnJump(ref bool cancel) {}
    public virtual void OnDash() {}
    public virtual void Attack() {}
    public virtual void Skill1() {}
    public virtual void Skill1Up() {}
    public virtual void Skill2() {}
    public virtual void Skill2Up() {}
    public virtual void OnCancel() {}
    public virtual void OnForceCancel() {}
    public virtual void OnHurt(ref int damage, Transform attacker, ref bool cancel) {}

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

        if (ownerName.Length > 0 && pl == null) {
            Player pl = Player.players.Find((p)=>p.name_ == ownerName && p.uniqueCode == ownerCode);

            if (pl != null) {
                pl._setCh(this);
            }
        }
    }

    public void CANCEL() {
        if (routine != null) {
            StopCoroutine(routine);
            

            routine = null;
        }

        OnCancel();
    }

    public void ForceCANCEL() {
        CANCEL();

        StopAllCoroutines();

        OnForceCancel();
    }
}
