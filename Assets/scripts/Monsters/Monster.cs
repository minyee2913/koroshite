using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public abstract class Monster : MonoBehaviour, IPunObservable
{
    public static List<Monster> monsters = new();
    [SerializeField]
    public MonsterState state;
    public PhotonView pv;
    public Animator animator;
    public Rigidbody2D rb;
    public BoxCollider2D col;
    public SpriteRenderer render;
    public int uniqueId;
    public float speed;
    public int attackDamage;
    public abstract float stateY {get;}
    public abstract string Name {get;}
    public bool onGround, isMoving, isDeath;
    public string action, actionInfo;
    float actionTime;
    float stepTime;
    public float range, atkCool;
    public int facing, health, maxHealth;
    public Player target;
    Color hpColor;
    public bool HideName;
    public GameObject vman_mark;
    public Vector3 groundOffset;
    public float stopMove;

    public static List<Monster> Convert(RaycastHit2D[] casts, Monster not = null) {
        List<Monster> result = new();

        for (int i = 0; i < casts.Length; i++) {
            var cast = casts[i];

            var pl = cast.transform.GetComponent<Monster>();
            if (pl != null) {
                if (not != null && not == pl) {
                    continue;
                }

                result.Add(pl);
            }
        }

        return result;
    }

    void Awake()
    {
        monsters.Add(this);

        pv = GetComponent<PhotonView>();
        col = GetComponent<BoxCollider2D>();
        render = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        hpColor = state.hprate.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color;

        action = "idle";
        actionInfo = "stay";

        state = Instantiate(state, transform);
        state.transform.localPosition = new Vector3(0, stateY);

        if (HideName) {
            state.nametag.gameObject.SetActive(false);
        }

        state.Name = Name;
        health = maxHealth;

        uniqueId = Random.Range(-9999, 9999) * 100 + monsters.Count;
    }

    void Update()
    {
        if (pv.IsMine) {
            onGround = OnGround(transform.position + groundOffset);

            MobUpdate();

            if (atkCool > 0) {
                atkCool -= Time.deltaTime;
            }

            animator.SetBool("death", isDeath);

            if (action == "idle") {
                idleState();
            }

            animator.SetBool("isMoving", isMoving);

            if (target != null) {
                range = Vector2.Distance(new Vector2(target.transform.position.x, 0), new Vector2(transform.position.x, 0));
            }

            if (stopMove > 0) {
                stopMove -= Time.deltaTime;
            }
        }

        state.hprate.value = (float)health / maxHealth;
    }

    public virtual void MobUpdate() {}

    void idleState() {
        actionTime += Time.deltaTime;
        
        if (actionInfo == "stay" || actionInfo == "") {
            if (actionTime > stepTime) {
                int rd = Random.Range(0, 100);

                if (rd <= 40) {
                    actionInfo = "left";
                } else if (rd <= 80) {
                    actionInfo = "right";
                } else {
                    actionInfo = "stay";
                }

                actionTime = 0;
                stepTime = Random.Range(1f, 4f);
            }
        } else {
            isMoving = true;

            if (actionInfo == "left") {
                if (HasObstacle(Vector2.left) || !OnGround(transform.position + Vector3.left)) {
                    actionInfo = "right";
                } else Move(Vector2.left);
            } else if (actionInfo == "right") {
                if (HasObstacle(Vector2.right) || !OnGround(transform.position + Vector3.right)) {
                    actionInfo = "left";
                } else Move(Vector2.right);
            }
            if (actionTime > stepTime) {
                actionInfo = "stay";

                actionTime = 0;
                stepTime = Random.Range(1f, 2f);

                isMoving = false;
            }
        }
    }

    public List<Player> Search(float distance) {
        return Player.Convert(Physics2D.BoxCastAll(transform.position + groundOffset + new Vector3(0, 0.5f), new Vector2(distance * 2, 2), 0, Vector2.zero, 0));
    }

    public void FaceTo(Player target) {
        if (transform.position.x < target.transform.position.x) {
            render.flipX = false;

            facing = 1;
        } else {
            render.flipX = true;

            facing = -1;
        }
    }

    public void Chase(Player target) {
        if (stopMove > 0) {
            return;
        }

        isMoving = false;
        
        if (transform.position.x > target.transform.position.x) {
            if (!HasObstacle(Vector2.left) && OnGround(transform.position + groundOffset + Vector3.left * speed * Time.deltaTime)) {
                Move(Vector2.left);

                isMoving = true;
            }
        } else if (transform.position.x < target.transform.position.x) {
            if (!HasObstacle(Vector2.right) && OnGround(transform.position + groundOffset + Vector3.right * speed * Time.deltaTime)) {
                Move(Vector2.right);

                isMoving = true;
            }
        }
    }

    public void RpcAnimateTrigger(string name) {
        object[] param = {
            name,
        };

        if (UtilManager.CheckPhoton()) {
            pv.RpcSecure("_animTrigg", RpcTarget.All, true, param);
        } else {
            _animTrigg(name);
        }
    }

    [PunRPC]
    public void _animTrigg(string name) {
        animator.SetTrigger(name);
    }

    public bool OnGround(Vector3 point) {
        RaycastHit2D[] center = Physics2D.RaycastAll(point + new Vector3(0, -1.8f), Vector2.down, 0.2f);
        RaycastHit2D[] left = Physics2D.RaycastAll(point + new Vector3(-0.3f, -1.8f), Vector2.down, 0.2f);
        RaycastHit2D[] right = Physics2D.RaycastAll(point + new Vector3(0.3f, -1.8f), Vector2.down, 0.2f);

        bool l = CheckGround(left), c = CheckGround(center), r = CheckGround(right);

        if (l && c && r) {
            return true;
        }

        if (l && c) {
            return true;
        }

        if (c && r) {
            return true;
        }

        if (!l && c && !r) {
            return true;
        }

        return false;
    }

    public void FaceTarget() {
        if (target != null) {
            if (target.transform.position.x > transform.position.x) {
                facing = 1;
                render.flipX = false;
            } else {
                facing = -1;
                render.flipX = true;
            }
        }
    }

    public void Move(Vector3 direction) {
        //if (!onGround) return;

        if (direction.x >= 0) {
            render.flipX = false;

            facing = 1;
        } else {
            render.flipX = true;

            facing = -1;
        }

        if (!HasObstacle(direction)) {
            transform.Translate(direction * Time.deltaTime * speed);
        } else {
            Debug.Log("obstacle!");
        }
    }

    bool HasObstacle(Vector2 direction) {
        var casts = Physics2D.BoxCastAll(transform.position + (Vector3)col.offset, new Vector3(col.size.x / 2, col.size.y), 0, direction, 0.3f, LayerMask.GetMask("ground"));

        if (casts.Length > 0) {
            Debug.Log(casts[0].transform.name);
        }

        return casts.Length > 0;
    }

    bool CheckGround(RaycastHit2D[] casts) {
        for (int i = 0; i < casts.Length; i++) {
            RaycastHit2D cast = casts[i];

            if ((LayerMask.GetMask("ground") & (1 << cast.transform.gameObject.layer)) != 0) {
                return true;
            }
        }

        return false;
    }

    public void Damage(int damage, string plName = null, bool display = true) {
        if (UtilManager.CheckPhoton()) {
            pv.RPC("hurt", RpcTarget.All, new object[]{damage, plName, display});
        } else {
            hurt(damage, plName, display);
        }
    }

    

    [PunRPC]
    public void hurt(int damage, string plName, bool display) {
        Player attacker = Player.players.Find((p)=>p.name_ == plName);
        bool cancel = false;

        OnHurt(attacker, damage, ref cancel);

        if (cancel)
            return;

        health -= damage;

        float y = transform.position.y + 1;

        if (attacker) {
            y = attacker.transform.position.y + 1;
        }


        if (display) {
            UtilManager.DisplayDamage(new Vector3(transform.position.x + Random.Range(-1f, 1f), Random.Range(0, 0.25f) + y), damage, Color.white);
        }

        if (health <= 0 && !isDeath) {
            OnDeath(attacker);
            StartCoroutine(Death());
        } else StartCoroutine(hurtEff());
    }

    public void Heal(int val) {
        if (UtilManager.CheckPhoton()) {
            pv.RPC("heal", RpcTarget.All, new object[]{val});
        } else {
            heal(val);
        }
    }

    

    [PunRPC]
    public void heal(int val) {
        health += val;

        UtilManager.DisplayDamage(transform.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), 1 + UnityEngine.Random.Range(0, 1.25f)), val, Color.green);

        if (health > maxHealth)
            health = maxHealth;
    }

    public virtual void OnHurt(Player attacker, int damage, ref bool cancel){}
    public virtual void OnDeath(Player attacker){}

    IEnumerator Death() {
        isDeath = true;
        animator.SetBool("death", true);

        action = "death";

        yield return new WaitForSeconds(1);

        if (UtilManager.CheckPhoton()) {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Destroy(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void OnDestroy() {
        monsters.Remove(this);
    }

    IEnumerator hurtEff() {
        var img = state.hprate.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        
        img.color = Color.white;

        yield return new WaitForSeconds(0.2f);

        img.color = hpColor;
    }

    public void Knockback(Vector2 force) {
        object[] param = {
            force,
        };

        if (UtilManager.CheckPhoton()) {
            pv.RPC("_knockback", RpcTarget.All, param);
        } else {
            _knockback(force);
        }
    }

    [PunRPC]
    public void _knockback(Vector2 force) {
        bool cancel = false;

        OnKnockback(force, ref cancel);
        if (cancel) {
            return;
        }
        
        rb.linearVelocity += force;

        Invoke("afterKnock", 0.3f);
    }

    public virtual void OnKnockback(Vector2 force, ref bool cancel) {}

    void afterKnock() {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x / 2, rb.linearVelocity.y / 2);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position + groundOffset + new Vector3(0, -1.8f), transform.position + groundOffset + new Vector3(0, -2f));

        Gizmos.DrawLine(transform.position + groundOffset + new Vector3(0.3f, -1.8f), transform.position + groundOffset + new Vector3(0.3f, -2f));
        Gizmos.DrawLine(transform.position + groundOffset + new Vector3(-0.3f, -1.8f), transform.position + groundOffset + new Vector3(-0.3f, -2f));
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(health);
            stream.SendNext(facing);
            stream.SendNext(isMoving);
            stream.SendNext(onGround);
        }
        else
        {
            health = (int)stream.ReceiveNext();
            facing = (int)stream.ReceiveNext();
            isMoving = (bool)stream.ReceiveNext();
            onGround = (bool)stream.ReceiveNext();
        }
    }
}
