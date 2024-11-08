using UnityEngine;

public class WonderingNpc : MonoBehaviour {
    public Character ch;
    public string character;
    public BoxCollider2D col;
    public int facing = 1;
    public float speed;
    public string action, actionInfo;
    public float actionTime, stepTime;
    void Awake() {
        col = GetComponent<BoxCollider2D>();
        ch = CharacterManager.instance.Get(character);
    }
    public bool isMoving, onGround;
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
            ch.render.flipX = false;

            facing = 1;
        } else {
            ch.render.flipX = true;

            facing = -1;
        }

        if (!HasObstacle(direction)) {
            transform.Translate(direction * Time.deltaTime * speed);
        } else {
            Debug.Log("obstacle!");
        }
    }
    public void Chase(Player target) {
        isMoving = false;
        
        if (transform.position.x > target.transform.position.x) {
            if (!HasObstacle(Vector2.left) && OnGround(transform.position + Vector3.left * speed * Time.deltaTime)) {
                Move(Vector2.left);

                isMoving = true;
            }
        } else if (transform.position.x < target.transform.position.x) {
            if (!HasObstacle(Vector2.right) && OnGround(transform.position + Vector3.right * speed * Time.deltaTime)) {
                Move(Vector2.right);

                isMoving = true;
            }
        }
    }

    public void Update()
    {
            if (action == "idle") {
                idleState();
            }

            ch.animator.SetBool("isMoving", isMoving);
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
}