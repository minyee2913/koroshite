using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public static List<Monster> monsters = new();
    public PhotonView pv;
    public BoxCollider2D col;
    public SpriteRenderer render;
    public float speed;
    public bool onGround;
    public string action, actionInfo;
    float actionTime;
    float stepTime;
    int facing;

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

        action = "idle";
        actionInfo = "stay";
    }

    void Update()
    {
        if (pv.IsMine) {
            onGround = OnGround(transform.position);

            if (action == "idle") {
                idleState();
            }
        }
    }

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
                stepTime = Random.Range(1f, 1.5f);
            }
        } else {
            if (actionInfo == "left") {
                if (HasObstacle(Vector2.left) || !OnGround(transform.position + Vector3.left * speed)) {
                    actionInfo = "right";
                } else Move(Vector2.left);
            } else if (actionInfo == "right") {
                if (HasObstacle(Vector2.right) || !OnGround(transform.position + Vector3.right * speed)) {
                    actionInfo = "left";
                } else Move(Vector2.right);
            }
            if (actionTime > stepTime) {
                actionInfo = "stay";

                actionTime = 0;
                stepTime = Random.Range(1f, 2f);
            }
        }
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
        }
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
}
