using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public static List<Player> players = new();
    public static Player Local;
    public string name_;
    public int uniqueCode;
    public PhotonView pv;
    public float moveSpeed, moveSpeedDef;
    public Rigidbody2D rb;
    public BoxCollider2D col;
    public string character;
    public Character ch = null;
    public Slider hprate, shieldRate;
    public Image background;
    public Text nametag;
    private Vector3 curPos;

    [SerializeField]
    public float cameraMoveSpeed;
    float height;
    float width;
    public GameObject vman_mark;

    public bool isMoving, onGround, jumping, dashing, running, isDeath, isSpectator;
    public int jumpCount = 0;
    public float jumpTime = 0, dashTime = 0, runTime = 0, stopMove = 0, preventInput = 0;
    float dashData = 0;
    public int facing = 1;
    [SerializeField]
    private Image balloon;
    [SerializeField]
    private Text balloon_Text;
    [SerializeField]
    private float balloon_time;
    public bool blocked;

    public int maxHealth, health, coin, energy, kill, death;
    public float shield;
    public Color hpColor = Color.red;
    public string state = "room";
    float realGravity;
    Cooldown dashCool = new(1);
    public Canvas infos;
    bool flip;
    float lavaTime;
    public Material WhiteFlash;
    public bool ForceWhite;
    bool forceWhited;

    public static List<Player> Convert(RaycastHit2D[] casts, Player not = null) {
        List<Player> result = new();

        for (int i = 0; i < casts.Length; i++) {
            var cast = casts[i];

            var pl = cast.transform.GetComponent<Player>();
            if (pl != null) {
                if (not != null && not == pl) {
                    continue;
                }

                if (pl.isSpectator) {
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
    public void SetSpectator(bool val) {
        object[] obj = {
            val
        };
        pv.RPC("setSpectator", RpcTarget.All, obj);
    }
    [PunRPC]
    void setSpectator(bool val) {
        isSpectator = val;
    }
    public void SetPos(Vector2 pos) {
        object[] obj = {
            pos
        };
        if (UtilManager.CheckPhoton()) {
            pv.RPC("setPos", RpcTarget.All, obj);
        } else {
            setPos(pos);
        }
    }
    [PunRPC]
    void setPos(Vector2 pos) {
        transform.position = pos;
    }
    public void SetCoin(int val) {
        object[] obj = {
            val
        };
        pv.RPC("setCoin", RpcTarget.All, obj);
    }
    [PunRPC]
    void setCoin(int val) {
        coin = val;
    }
    public void AddCoin(int val) {
        object[] obj = {
            val
        };
        pv.RPC("addCoin", RpcTarget.All, obj);
    }
    [PunRPC]
    void addCoin(int val) {
        coin += val;
    }
    public void AddKill(int val) {
        object[] obj = {
            val
        };
        if (UtilManager.CheckPhoton()) {
            pv.RPC("addKill", RpcTarget.All, obj);
        } else {
            addKill(val);
        }
    }
    [PunRPC]
    void addKill(int val) {
        kill += val;
    }
    public void SetKill(int val) {
        object[] obj = {
            val
        };
        pv.RPC("setKill", RpcTarget.All, obj);
    }
    [PunRPC]
    void setKill(int val) {
        kill = val;
    }
    public void SetPrevent(float val) {
        object[] obj = {
            val
        };
        if (UtilManager.CheckPhoton()) {
            pv.RPC("setPrevent", RpcTarget.All, obj);
        } else {
            setPrevent(val);
        }
    }
    [PunRPC]
    void setPrevent(float val) {
        preventInput = val;
    }
    public void SetStopMove(float val) {
        object[] obj = {
            val
        };
        if (UtilManager.CheckPhoton()) {
            pv.RPC("setStopMove", RpcTarget.All, obj);
        } else {
            setStopMove(val);
        }
    }
    [PunRPC]
    void setStopMove(float val) {
        stopMove = val;
    }
    public void SetDeath(int val) {
        object[] obj = {
            val
        };
        pv.RPC("setDeath", RpcTarget.All, obj);
    }
    [PunRPC]
    void setDeath(int val) {
        death = val;
    }
    public void SetState(string str) {
        object[] obj = {
            str
        };
        pv.RPC("setState", RpcTarget.All, obj);
    }
    [PunRPC]
    void setState(string str) {
        state = str;
        vman_mark.SetActive(false);
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
    public void SetBalloon(string text) {
        object[] obj = {
            text
        };
        pv.RPC("_setBalloon", RpcTarget.All, obj);
    }
    [PunRPC]
    public void _setBalloon(string text) {
        balloon_time = 0f;
        balloon.gameObject.SetActive(true);
        balloon_Text.gameObject.SetActive(true);

        balloon.color = Color.white;
        balloon_Text.color = Color.black;

        balloon_Text.text = text;

        SoundManager.Instance.Play("typing");
    }
    public void SetChScale(Vector3 scale) {
        object[] obj = {
            scale.x, scale.y, scale.z,
        };
        if (UtilManager.CheckPhoton()) {
            pv.RpcSecure("setChScale", RpcTarget.All, true, obj);
        } else {
            setChScale(scale.x, scale.y, scale.z);
        }
    }
    [PunRPC]
    void setChScale(float x, float y, float z) {
        if (ch != null) {
            ch.transform.localScale = new Vector3(x, y, z);
        }
    }
    void Start()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        
        height = Camera.main.orthographicSize;
        width = height * Screen.width / Screen.height;

        health = maxHealth;

        realGravity = rb.gravityScale;

        if (pv.IsMine) {
            if (Local != null) {
                PhotonNetwork.Destroy(Local.gameObject);
            }

            Local = this;

            uniqueCode = UnityEngine.Random.Range(-9999, 9999);

            hprate.transform.Find("Fill Area").GetComponentInChildren<Image>().color = hpColor = new Color(85/256f, 255/256f, 11/256f);

            if (SceneManager.GetActiveScene().name == "GameScene") {
                SetName(PhotonNetwork.NickName);

                SetCharacter("samurai");
                ChatManager.Instance.SendComment("<color=\"green\">" + name_ + "님이 게임에 접속했습니다.</color>");
            }
        }

        moveSpeedDef = moveSpeed;

        players.Add(this);
    }

    // public void RpcAnimateBool(string name, bool val) {
    //     object[] param = {
    //         name, val,
    //     };

    //     pv.RpcSecure("_animBool", RpcTarget.All, true, param);
    // }

    // [PunRPC]
    // void _animBool(string name, bool val) {
    //     if (ch != null) {
    //         ch.animator.SetBool(name, val);
    //     }
    // }
    public void RpcAnimateTrigger(string name) {
        object[] param = {
            name,
        };

        if (UtilManager.CheckPhoton()) {
            pv.RpcSecure("_animTrig", RpcTarget.All, true, param);
        } else {
            _animTrig(name);
        }
    }

    [PunRPC]
    void _animTrig(string name) {
        if (ch != null) {
            ch.animator.SetTrigger(name);
        }
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
    void _knockback(Vector2 force) {
        rb.linearVelocity += force;

        Invoke("afterKnock", 0.3f);
    }

    void afterKnock() {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x / 2, rb.linearVelocity.y / 2);
    }

    public void CallChFunc(string method) {
        if (UtilManager.CheckPhoton()) {
            pv.RpcSecure("ccf", RpcTarget.All, true, new object[]{method});
        } else {
            ccf(method);
        }
    }

    [PunRPC]
    void ccf(string method) {
        if (ch != null) {
            ch.Callfunc(method);
        }
    }

    public void SetFacing(int facing) {
        if (UtilManager.CheckPhoton()) {
            pv.RpcSecure("setFac", RpcTarget.All, true, new object[]{facing});
        } else {
            setFac(facing);
        }
    }
    [PunRPC]
    void setFac(int facing) {
        this.facing = facing;
        ch.render.flipX = facing == -1;
    }

    public void SetEnergy(int val) {
        if (UtilManager.CheckPhoton()) {
            pv.RpcSecure("setEne", RpcTarget.All, true, new object[]{val});
        } else {
            setEne(val);
        }
    }
    [PunRPC]
    void setEne(int val) {
        energy = val;

        if (energy > 100) {
            energy = 100;
        }
    }
    public void SetShield(int val) {
        if (UtilManager.CheckPhoton()) {
            pv.RPC("setShi", RpcTarget.All, new object[]{val});
        } else {
            setShi(val);
        }
    }
    [PunRPC]
    void setShi(int val) {
        shield = val;
    }

    private void Update() {
        if (isSpectator) {
            transform.localScale = Vector3.zero;
        } else {
            transform.localScale = Vector3.one;
        }

        if (ch != null) {
            if (pv.IsMine && !isSpectator) {
                if (PhotonNetwork.LocalPlayer.NickName == name_ && players.Find((pl)=>pl.name_ == name_ && pl.uniqueCode != uniqueCode) != null) {
                    SetName("(" + uniqueCode.ToString() + ")" + PhotonNetwork.LocalPlayer.NickName);
                }
                isMoving = false;

                if (ForceWhite != forceWhited) {
                    if (ForceWhite) {
                        ch.render.material = WhiteFlash;
                    } else {
                        ch.render.material = ch.defaultMat;
                    }

                    forceWhited = ForceWhite;
                }

                if (state == "room")
                    energy = 100;

                if (state == "ingame" || state == "ready")
                    LimitedCamera();

                if (health <= 0 && !isDeath) {
                    isDeath = true;
                    running = false;
                    isMoving = false;
                    dashing = false;

                    if (SceneManager.GetActiveScene().name == "GameScene") {
                        UIManager.Instance.ShowDeathScreen();
                    }
                }

                bool left = Input.GetKey(KeyCode.A);
                bool right = Input.GetKey(KeyCode.D);

                float speed = moveSpeed;

                if (running) {
                    speed *= 1.6f;
                }

                if (GameManager.Instance != null && !GameManager.Instance.applyPlayerInput || isDeath || state == "ready") {
                    speed *= 0;
                }

                if (preventInput <= 0) {
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

                if ((GameManager.Instance == null || GameManager.Instance.applyPlayerInput) && !isDeath && state != "ready" && preventInput <= 0) {
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
                }

                if (dashing) {
                    dashTime -= Time.deltaTime;

                    if (dashTime <= 0) {
                        dashing = false;

                        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                        CamManager.main.CloseOut(0.25f);
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

                if (preventInput > 0) {
                    preventInput -= Time.deltaTime;
                }

                bool inLava = InLava();

                if (!inLava)
                    lavaTime = 0;

                if (inLava) {
                    if (jumping) {
                        rb.linearVelocityY = 0;
                    }
                    bool up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space);

                    if (up) 
                        transform.Translate(Vector2.up * 3 * Time.deltaTime);

                    lavaTime += Time.deltaTime;

                    if (!isDeath && lavaTime > 0.3f) {
                        hurt((int)(maxHealth * 0.05f), null, true);

                        lavaTime = 0;
                    }
                } else if (InLadder()) {
                    rb.linearVelocityY = 0;

                    bool up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space);
                    bool down = Input.GetKey(KeyCode.S);

                    if (up) 
                        transform.Translate(Vector2.up * 6 * Time.deltaTime);
                    else if (down) 
                        transform.Translate(-Vector2.up * 6 * Time.deltaTime);
                } else {
                    rb.gravityScale = realGravity;
                }

                ch.animator.SetBool("isMoving", isMoving);
                if (running) {
                    ch.animator.SetBool("isRunning", isMoving);
                } else {
                    ch.animator.SetBool("isRunning", false);
                }

                ch.animator.SetBool("onGround", onGround);
                ch.animator.SetBool("isDead", isDeath);
            }else {
                rb.gravityScale = 0;
                rb.linearVelocity = Vector2.zero;
            }

            hprate.value = (float)health / maxHealth;
            shieldRate.value = (float)shield / (maxHealth / 2);
            nametag.text = name_;
            background.transform.localScale = new Vector3(name_.Length + 0.4f, 1);

            if (shield > maxHealth / 2) {
                shield = maxHealth / 2;
            }

            if (dashing) {
                float dashD = Mathf.Floor(dashTime * 100) / 100;
                if (dashD != dashData) {
                    if (dashD % 0.02f == 0 && dashD != 0) {
                        if (UtilManager.CheckPhoton()) {
                            pv.RPC("dashEffect", RpcTarget.All, null);
                        } else {
                            dashEffect();
                        }
                    } 
                    dashData = dashD;
                }
            }
        }

        if (balloon.gameObject.activeSelf) {
            balloon_time += Time.deltaTime;

            if (balloon_time > 1.5f) {
                Color col = Color.white;
                col.a = 2.5f - balloon_time;

                balloon.color = col;
                balloon_Text.color = col;
            }

            if (balloon_time > 2.5f) {
                balloon.gameObject.SetActive(false);
                balloon_Text.gameObject.SetActive(false);

                balloon_time = 0;
            }
        }
    }

    public void SetMaxHp(int val) {
        if (UtilManager.CheckPhoton()) {
            pv.RPC("smhp", RpcTarget.All, new object[]{val});
        } else {
            smhp(val);
        }
    }

    [PunRPC]
    void smhp(int val) {
        maxHealth = val;
        health = val;
    }

    public void Heal(int val) {
        if (UtilManager.CheckPhoton()) {
            pv.RPC("heal", RpcTarget.All, new object[]{val});
        } else {
            heal(val);
        }
    }

    

    [PunRPC]
    void heal(int val) {
        health += val;

        UtilManager.DisplayDamage(transform.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), 1 + UnityEngine.Random.Range(0, 1.25f)), val, Color.green);

        if (health > maxHealth)
            health = maxHealth;
    }

    public void Damage(int damage, string plName = null, bool display = true) {
        if (UtilManager.CheckPhoton()) {
            pv.RPC("hurt", RpcTarget.All, new object[]{damage, plName, display});
        } else {
            hurt(damage, plName, display);
        }
    }

    public void DamageByMob(int damage, int uniqueId, bool display = true) {
        if (UtilManager.CheckPhoton()) {
            pv.RPC("hurtByMob", RpcTarget.All, new object[]{damage, uniqueId, display});
        } else {
            hurtByMob(damage, uniqueId, display);
        }
    }

        [PunRPC]
    void hurtByMob(int damage, int uniqueId, bool display = true) {
        Monster attacker = Monster.monsters.Find((v)=>v.uniqueId == uniqueId);
        bool cancel = false;

        if (ch != null) {
            ch.OnHurt(ref damage, attacker.transform, ref cancel);
        }

        if (cancel || state == "room" || isDeath)
            return;

        if (shield > 0) {
            shield -= damage;

            if (shield < 0) {
                health += (int)shield;
                shield = 0;
            }
        } else {
            health -= damage;
        }

        if (display) {
            if (this == Local) {
                UtilManager.DisplayDamage(transform.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), 1 + UnityEngine.Random.Range(0, 1.25f)), damage, Color.red);
            }
        }

        if (health <= 0 && !isDeath && this == Local) {
            death++;
            ch.ForceCANCEL();
            OnDeath();
        } else StartCoroutine(hurtEff());
    }

    [PunRPC]
    void hurt(int damage, string plName, bool display) {
        if (isSpectator) {
            return;
        }

        Player attacker = players.Find((p)=>p.name_ == plName);
        bool cancel = false;

        if (ch != null) {
            ch.OnHurt(ref damage, (attacker != null) ? attacker.transform : null, ref cancel);
        }

        if (cancel || state == "room" || isDeath)
            return;

        if (shield > 0) {
            shield -= damage;

            if (shield < 0) {
                health += (int)shield;
                shield = 0;
            }
        } else {
            health -= damage;
        }

        if (display) {
            if (this == Local) {
                UtilManager.DisplayDamage(transform.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), 1 + UnityEngine.Random.Range(0, 1.25f)), damage, Color.red);
            } else if (attacker == Local) {
                UtilManager.DisplayDamage(transform.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), 1 + UnityEngine.Random.Range(0, 1.25f)), damage, Color.white);
            } else {
                UtilManager.DisplayDamage(transform.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), 1 + UnityEngine.Random.Range(0, 1.25f)), damage, Color.gray);
            }
        }

        if (health <= 0 && !isDeath && this == Local) {
            death++;
            ch.ForceCANCEL();
            OnDeath();

            if (attacker != null) {
                attacker.AddCoin(10);
                attacker.AddKill(1);
            }
        } else StartCoroutine(hurtEff());
    }


    void OnDeath() {
        if (GameManager.Instance != null) {
            if (GameManager.Instance.mode == GameMode.Dual) {
                GameManager.Instance.ForcePhaseEnd();
            }
        }
    }

    IEnumerator hurtEff() {
        var img = hprate.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        
        img.color = Color.gray;
        if (ch != null && !ForceWhite) {
            ch.render.material = WhiteFlash;
        }

        img.transform.DOScale(new Vector3(1.1f, 1.1f), 0.2f);

        yield return new WaitForSeconds(0.2f);

        img.color = hpColor;

        img.transform.DOScale(Vector3.one, 0.2f);

        if (ch != null && !ForceWhite) {
            ch.render.material = ch.defaultMat;
        }
    }

    [PunRPC]
    void dashEffect() {
        if (ch == null) {
            return;
        }
        var obj = Instantiate(ch, ch.transform.position, quaternion.identity);

        obj.pl = null;

        obj.animator.SetBool("isRunning", true);

        obj.animator.speed = 0.1f;
        Color col = Color.black;
        col.a = 0.2f;

        obj.render.color = col;

        Destroy(obj.gameObject, 0.4f);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(name_);
            stream.SendNext(uniqueCode);
            stream.SendNext(health);
            stream.SendNext(shield);
            stream.SendNext(kill);
            stream.SendNext(death);
            stream.SendNext(facing);
            stream.SendNext(isMoving);
            stream.SendNext(running);
            stream.SendNext(onGround);
            stream.SendNext(jumping);
            stream.SendNext(dashing);
            stream.SendNext(isDeath);
            stream.SendNext(ch != null ? ch.render.flipX : false);
        }
        else
        {
            name_ = (string)stream.ReceiveNext();
            uniqueCode = (int)stream.ReceiveNext();
            health = (int)stream.ReceiveNext();
            shield = (float)stream.ReceiveNext();
            kill = (int)stream.ReceiveNext();
            death = (int)stream.ReceiveNext();
            facing = (int)stream.ReceiveNext();
            isMoving = (bool)stream.ReceiveNext();
            running = (bool)stream.ReceiveNext();
            onGround = (bool)stream.ReceiveNext();
            jumping = (bool)stream.ReceiveNext();
            dashing = (bool)stream.ReceiveNext();
            isDeath = (bool)stream.ReceiveNext();
            flip = (bool)stream.ReceiveNext();
            if (ch != null) {
                ch.render.flipX = flip;
            }
        }
        
    }

    public void Dash() {
        if (dashCool.IsIn()) return;

        dashCool.Start();

        CamManager.main.CloseUp(5.9f, 0, 0.25f);

        dashing = true;
        dashTime = 0.25f;

        running = true;
        runTime = 1;

        ch.animator.SetBool("isRunning", true);

        rb.linearVelocity = new Vector2(facing * 14, 0);

        SoundManager.Instance.Play("dash");

        ch.OnDash();
    }
    [PunRPC]
    public void Revive() {
        isDeath = false;
        health = maxHealth;

        if (ch != null) {
            ch.OnRevive();
        }
    }

    public void CallRevive() {
        if (UtilManager.CheckPhoton()) {
            pv.RPC("Revive", RpcTarget.All);
        } else {
            Revive();
        }
        
    }

    public void CallCancel() {
        if (UtilManager.CheckPhoton()) {
            pv.RPC("cancel", RpcTarget.All);
        } else {
            cancel();
        }
    }
    [PunRPC]
    void cancel() {
        if (ch != null) {
            ch.CANCEL();
        }
    }

    public void CallCancelF() {
        if (UtilManager.CheckPhoton()) {
            pv.RPC("cancelf", RpcTarget.All);
        } else {
            cancelf();
        }
    }

    [PunRPC]
    void cancelf() {
        if (ch != null) {
            ch.ForceCANCEL();
        }
    }

    public void Jump() {
        bool cancel = false;
        if (ch != null) {
            ch.OnJump(ref cancel);
        }

        if (cancel) {
            return;
        }

        jumpCount++;
        
        if (jumpCount <= 2) {
            if (jumpCount == 1) {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 8);
            } else if (jumpCount == 2) {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10);
            }

            RpcAnimateTrigger("jump");

            jumping = true;
            jumpTime = 0;
        }
    }

    public bool InLadder() {
        RaycastHit2D cast = Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.5f), 0, Vector2.down, 0.5f, LayerMask.GetMask("ladder"));

        return cast;
    }

    public bool InLava() {
        RaycastHit2D cast = Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.5f), 0, Vector2.down, 0.5f, LayerMask.GetMask("lava"));

        return cast;
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
        if (UtilManager.CheckPhoton()) {
            object[] data = {
                name_,
                pv.Owner.UserId,
            };
            PhotonNetwork.Instantiate("character/" + id, transform.position, transform.rotation, 0, data);
        } else {
            Character ch = Instantiate(Resources.Load<Character>("character/" + id), transform.position, transform.rotation);
            _setCh(ch);
        }
    }

    public void _setCh(Character chh) {
        if (ch != null && pv.IsMine) {
            PhotonNetwork.Destroy(ch.gameObject);
        }

        ch = chh;

        character = chh.id;

        chh.transform.SetParent(transform);
        chh.transform.localPosition = new Vector3(0, 1.36f);

        float rate = (float)health / maxHealth;
        maxHealth = chh.maxHealth;
        health = (int)(maxHealth * rate);

        UIManager.Instance.face.sprite = ch.icon;
        
        chh.pl = this;

        chh.OnStart();
    }

    void LimitedCamera() {
        if (MapManager.Instance == null) {
            return;
        }
        var map = MapManager.Instance.selectedMap;

        CamManager.main.transform.position = Vector3.Lerp(CamManager.main.transform.position, 
                                          transform.position, 
                                          Time.deltaTime * cameraMoveSpeed);

        float lx = map.mapSize.x - width;
        float clampX = Mathf.Clamp(CamManager.main.transform.position.x, -lx + map.center.x, lx + map.center.x);

        float ly = map.mapSize.y - height;
        float clampY = Mathf.Clamp(CamManager.main.transform.position.y, -ly + map.center.y, ly + map.center.y);

        CamManager.main.transform.position = new Vector3(clampX, clampY, -10f);
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position + new Vector3(0, -0.5f), transform.position + new Vector3(0, -0.6f));

        Gizmos.DrawLine(transform.position + new Vector3(0.3f, -0.5f), transform.position + new Vector3(0.3f, -0.6f));
        Gizmos.DrawLine(transform.position + new Vector3(-0.3f, -0.5f), transform.position + new Vector3(-0.3f, -0.6f));

        //samurai
        // Gizmos.DrawWireCube(transform.position + new Vector3(1 * facing, 0.5f), new Vector2(2, 2)); //attack
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireCube(transform.position + new Vector3(0, 2f), new Vector2(14, 5)); //super
        // Gizmos.DrawWireCube(transform.position + new Vector3(0, 1f), new Vector2(14, 3)); //super

        //fighter
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireCube(transform.position + new Vector3(0.5f * facing, 0.5f), new Vector2(1, 2)); //attack
        // Gizmos.DrawWireCube(transform.position + new Vector3(1.2f * facing, 0.5f), new Vector2(3.6f, 3)); //super kick
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireCube(transform.position + new Vector3(0, 0f), new Vector2(6, 2)); //jumpAtk
        // Gizmos.DrawWireCube(transform.position + new Vector3(0, 1f), new Vector2(14, 3)); //super

        //shinobi
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(transform.position + new Vector3(0.25f, 1f), 5); //attack
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireCube(transform.position + new Vector3(0f, 0.5f), new Vector2(8, 3)); //attack

        //vampire girl
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireCube(transform.position + new Vector3(1f * facing, 0.5f), new Vector2(2.5f, 2)); //attack

        //vampire man
        Gizmos.color = Color.red;
        //Gizmos.DrawWireCube(transform.position + new Vector3(2f * facing, 0.5f), new Vector2(4f, 2)); //attack
        Gizmos.DrawWireCube(transform.position + new Vector3(0, 0.5f), new Vector2(1.5f, 2.2f)); //attack
    }
}
