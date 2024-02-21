using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviourPun
{
    string name_;
    public PhotonView pv;
    public float moveSpeed;
    public Rigidbody2D rb;
    public BoxCollider2D col;
    public Character ch = null;
    public Slider hprate;
    public Image background;
    public Text nametag;
    [SerializeField]
    Vector2 center;
    [SerializeField]
    Vector2 mapSize;

    [SerializeField]
    float cameraMoveSpeed;
    float height;
    float width;

    public bool isMoving, onGround, jumping, dashing, running;
    public int jumpCount = 0;
    public float jumpTime = 0, dashTime = 0, runTime = 0, stopMove = 0;
    float dashData = 0;
    public int facing = 1;

    public int maxHealth = 1000, health;

    public string GetName() {
        return name_;
    }
    public void SetName(string str) {
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
    }

    private void Update() {
        if (ch != null) {
            if (pv.IsMine) {
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

                        ch.animator.SetBool("isRunning", false);
                    }
                }

                if (stopMove > 0) {
                    stopMove -= Time.deltaTime;
                }
            }

            hprate.value = health / maxHealth;
            nametag.text = name_;

            ch.animator.SetBool("isMoving", isMoving);
            if (running) {
                ch.animator.SetBool("isRunning", isMoving);
            }
            ch.animator.SetBool("onGround", onGround);

            if (dashing) {
                float dashD = Mathf.Floor(dashTime * 100) / 100;
                if (dashD != dashData) {
                    if (dashD % 0.02f == 0 && dashD != 0) {
                        var obj = Instantiate(ch, ch.transform.position, quaternion.identity);

                        obj.animator.SetBool("isRunning", true);

                        obj.animator.speed = 0.1f;
                        Color col = obj.render.color;
                        col.a = 0.2f;

                        obj.render.color = col;

                        Destroy(obj.gameObject, 0.4f);
                    } 
                    dashData = dashD;
                }
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

            ch.animator.SetTrigger("jump");

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

    public void SetCharacter(Character c) {
        object[] param = {
            c
        };
        pv.RPC("_setCh", RpcTarget.All, param);
    }

    [PunRPC]
    void _setCh(Character c) {
        if (ch != null) {
            Destroy(c);
        }

        ch = Instantiate(c);

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
    }
}
