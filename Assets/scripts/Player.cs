using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public static List<Player> players = new();
    public static Player Local;
    public string name_;
    public PhotonView pv;
    public float moveSpeed;
    public Rigidbody2D rb;
    public BoxCollider2D col;
    public string character;
    public Character ch = null;
    public Slider hprate;
    public Image background;
    public Text nametag;
    private Vector3 curPos;
    [SerializeField]
    private Vector2 center;
    [SerializeField]
    Vector2 mapSize;

    [SerializeField]
    float cameraMoveSpeed;
    float height;
    float width;

    public bool isMoving, onGround, jumping, dashing, running, isDeath;
    public int jumpCount = 0;
    public float jumpTime = 0, dashTime = 0, runTime = 0, stopMove = 0;
    float dashData = 0;
    public int facing = 1;

    public int maxHealth = 1000, health;

    public static List<Player> Convert(RaycastHit2D[] casts, Player not = null) {
        List<Player> result = new();

        for (int i = 0; i < casts.Length; i++) {
            var cast = casts[i];

            var pl = cast.transform.GetComponent<Player>();
            if (pl != null) {
                if (not != null && not == pl) {
                    continue;
                }

                result.Add(pl);
            }
        }

        return result;
    }

    public string GetName() {
        return name_;
    }
    public void SetName(string str) {
        object[] obj = {
            str
        };
        pv.RPC("setName", RpcTarget.All, obj);
    }
    [PunRPC]
    public void setName(string str) {
        name_ = str;
    }
    void Start()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        
        height = Camera.main.orthographicSize;
        width = height * Screen.width / Screen.height;

        health = maxHealth;

        players.Add(this);

        if (pv.IsMine) {
            Local = this;

            SetCharacter("samurai");
            SetName(PhotonNetwork.LocalPlayer.NickName);
        }
    }

    public void RpcAnimateBool(string name, bool val) {
        object[] param = {
            name, val,
        };

        pv.RPC("_animateBool", RpcTarget.All, param);
    }

    [PunRPC]
    void _animateBool(string name, bool val) {
        if (ch != null) {
            ch.animator.SetBool(name, val);
        }
    }
    public void RpcAnimateTrigger(string name) {
        object[] param = {
            name,
        };

        pv.RPC("_animateTrigger", RpcTarget.All, param);
    }

    [PunRPC]
    void _animateTrigger(string name) {
        if (ch != null) {
            ch.animator.SetTrigger(name);
        }
    }

    public void Knockback(Vector2 force) {
        object[] param = {
            force,
        };

        pv.RPC("_knockback", RpcTarget.All, param);
    }

    [PunRPC]
    void _knockback(Vector2 force) {
        rb.velocity += force;

        Invoke("afterKnock", 0.3f);
    }

    void afterKnock() {
        rb.velocity = new Vector2(rb.velocity.x / 2, rb.velocity.y / 2);
    }

    private void Update() {
        if (ch != null) {
            if (pv.IsMine && !isDeath && GameManager.Instance.applyPlayerInput) {
                isMoving = false;

                LimitedCamera();

                bool left = Input.GetKey(KeyCode.A);
                bool right = Input.GetKey(KeyCode.D);

                float speed = moveSpeed;

                if (running) {
                    speed *= 1.6f;
                }

                if (left) {
                    if (!right) {
                        isMoving = true;
                        facing = -1;

                        ch.render.flipX = true;

                        if (stopMove <= 0 && !HasObstacle(Vector2.left)) transform.Translate(new Vector3(-speed * Time.deltaTime, 0));
                    }
                } else if (right) {
                    isMoving = true;
                    facing = 1;

                    ch.render.flipX = false;

                    if (stopMove <= 0 && !HasObstacle(Vector2.right)) transform.Translate(new Vector3(speed * Time.deltaTime, 0));
                }

                onGround = OnGround();

                if (jumping) {
                    jumpTime += Time.deltaTime;

                    if (onGround && jumpTime >= 0.2f) {
                        jumpCount = 0;
                        jumping = false;

                        jumpTime = 0;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Space)) {
                    Jump();
                }

                if (Input.GetKeyDown(KeyCode.LeftShift)) {
                    Dash();
                }

                if (Input.GetKeyDown(KeyCode.J)) {
                    ch.Attack();
                }

                if (Input.GetKeyDown(KeyCode.K)) {
                    ch.Skill1();
                }

                if (Input.GetKeyUp(KeyCode.K)) {
                    ch.Skill1Up();
                }

                if (Input.GetKeyDown(KeyCode.L)) {
                    ch.Skill2();
                }

                if (Input.GetKeyUp(KeyCode.L)) {
                    ch.Skill2Up();
                }

                if (dashing) {
                    dashTime -= Time.deltaTime;

                    if (dashTime <= 0) {
                        dashing = false;

                        rb.velocity = new Vector2(0, rb.velocity.y);
                    }
                }

                if (running) {
                    runTime -= Time.deltaTime;

                    if (runTime <= 0) {
                        running = false;

                        RpcAnimateBool("isRunning", false);
                    }
                }

                if (stopMove > 0) {
                    stopMove -= Time.deltaTime;
                }
            }else {
                rb.gravityScale = 0;
                rb.velocity = Vector2.zero;
            }

            hprate.value = (float)health / maxHealth;
            nametag.text = name_;
            background.transform.localScale = new Vector3(name_.Length + 0.4f, 1);

            ch.animator.SetBool("isMoving", isMoving);
            if (running) {
                ch.animator.SetBool("isRunning", isMoving);
            }

            ch.animator.SetBool("onGround", onGround);
            ch.animator.SetBool("isDead", isDeath);

            if (dashing) {
                float dashD = Mathf.Floor(dashTime * 100) / 100;
                if (dashD != dashData) {
                    if (dashD % 0.02f == 0 && dashD != 0) {
                        pv.RPC("dashEffect", RpcTarget.All, null);
                    } 
                    dashData = dashD;
                }
            }
        }
    }

    public void Damage(int damage, string plName = null) {
        pv.RPC("hurt", RpcTarget.All, new object[]{damage, plName});
    }

    

    [PunRPC]
    void hurt(int damage, string plName) {
        health -= damage;

        Player attacker = players.Find((p)=>p.name_ == plName);

        if (ch != null) {
            ch.OnHurt(damage, attacker);
        }

        if (health <= 0) {
            isDeath = true;
        } else StartCoroutine(hurtEff());
    }

    IEnumerator hurtEff() {
        var img = hprate.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        
        img.color = Color.white;

        yield return new WaitForSeconds(0.2f);

        img.color = Color.red;
    }

    [PunRPC]
    void dashEffect() {
        var obj = Instantiate(ch, ch.transform.position, quaternion.identity);

        obj.animator.SetBool("isRunning", true);

        obj.animator.speed = 0.1f;
        Color col = obj.render.color;
        col.a = 0.2f;

        obj.render.color = col;

        Destroy(obj.gameObject, 0.4f);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
            if (stream.IsWriting)
            {
                stream.SendNext(name_);
                stream.SendNext(health);
                stream.SendNext(isMoving);
                stream.SendNext(running);
                stream.SendNext(onGround);
                stream.SendNext(jumping);
                stream.SendNext(dashing);
                stream.SendNext(isDeath);
                stream.SendNext(ch != null ? ch.render.flipX : false);
                stream.SendNext(character);
            }
            else
            {
                name_ = (string)stream.ReceiveNext();
                health = (int)stream.ReceiveNext();
                isMoving = (bool)stream.ReceiveNext();
                running = (bool)stream.ReceiveNext();
                onGround = (bool)stream.ReceiveNext();
                jumping = (bool)stream.ReceiveNext();
                dashing = (bool)stream.ReceiveNext();
                isDeath = (bool)stream.ReceiveNext();

                var flip = (bool)stream.ReceiveNext();
                if (ch != null) {
                    ch.render.flipX = flip;
                }

                string chId = (string)stream.ReceiveNext();
                Debug.Log(chId);
                if (chId != character) {
                    _setCh(chId);
                }
            }
        
    }

    public void Dash() {
        dashing = true;
        dashTime = 0.25f;

        running = true;
        runTime = 1;

        ch.animator.SetBool("isRunning", true);

        rb.velocity = new Vector2(facing * 14, 0);
    }

    public void Jump() {
        jumpCount++;
        
        if (jumpCount <= 2) {
            if (jumpCount == 1) {
                rb.velocity = new Vector2(rb.velocity.x, 8);
            } else if (jumpCount == 2) {
                rb.velocity = new Vector2(rb.velocity.x, 10);
            }

            RpcAnimateTrigger("jump");

            jumping = true;
            jumpTime = 0;
        }
    }

    public bool OnGround() {

        RaycastHit2D[] center = Physics2D.RaycastAll(transform.position + new Vector3(0, -0.5f), Vector2.down, 0.2f);
        RaycastHit2D[] left = Physics2D.RaycastAll(transform.position + new Vector3(-0.3f, -0.5f), Vector2.down, 0.2f);
        RaycastHit2D[] right = Physics2D.RaycastAll(transform.position + new Vector3(0.3f, -0.5f), Vector2.down, 0.2f);

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

    bool HasObstacle(Vector2 direction) {
        var casts = Physics2D.BoxCastAll(transform.position + (Vector3)col.offset, new Vector3(col.size.x / 2, col.size.y), 0, direction, 0.3f, LayerMask.GetMask("ground"));

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

    public void SetCharacter(string id) {
        object[] param = {
            id
        };
        pv.RPC("_setCh", RpcTarget.All, param);
    }

    [PunRPC]
    void _setCh(string id) {
        if (ch != null) {
            Destroy(ch.gameObject);
        }

        character = id;

        ch = Instantiate(CharacterManager.instance.Get(id));

        ch.transform.SetParent(transform);
        ch.transform.localPosition = new Vector3(0, 1.36f);
        
        ch.pl = this;
    }

    void LimitedCamera() {
        float lx = mapSize.x - width;
        float clampX = Mathf.Clamp(transform.position.x, -lx + center.x, lx + center.x);

        float ly = mapSize.y - height;
        float clampY = Mathf.Clamp(transform.position.y, -ly + center.y, ly + center.y);

        CamManager.main.transform.position = new Vector3(clampX, clampY, -10f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position + new Vector3(0, -0.5f), transform.position + new Vector3(0, -0.6f));

        Gizmos.DrawLine(transform.position + new Vector3(0.3f, -0.5f), transform.position + new Vector3(0.3f, -0.6f));
        Gizmos.DrawLine(transform.position + new Vector3(-0.3f, -0.5f), transform.position + new Vector3(-0.3f, -0.6f));

        Gizmos.DrawWireCube(transform.position + new Vector3(1 * facing, 0.5f), new Vector2(2, 2));
    }
}
