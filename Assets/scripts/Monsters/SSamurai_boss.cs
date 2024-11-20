using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSamurai_boss : Monster
{
    public override float stateY => 0.16f;

    public override string Name => "sSamurai_boss";

    int phase, sk, sk_;
    float tick = 0, tick2;
    List<Player> targets = new();
    bool dashing, isHealing;

    public void Start() {
        UIManager.Instance.bossbar.title.text = "기억의 환영";
        state.gameObject.SetActive(false);

        action = "waiting";
    }

    public override void MobUpdate()
    {
        targets = Search(20);

        tick += Time.deltaTime;

        if (phase == 0) {
            Phase1Update();
        } else if (phase == 1) {
            Phase2Update();
        }

        if (action == "waiting") {
            isMoving = false;
        } else if (action == "idle") {
            isMoving = false;
            if (targets.Count > 0) {
                action = "chase";

                target = targets[0];
            }
        } else if (action == "chase") {
            if (targets.Count <= 0) {
                action = "idle";
            } else {

                if (range > 3) {
                    Chase(target);
                } else {
                    isMoving = false;

                    if (atkCool <= 0) {
                        FaceTo(target);
                        IdleAttack();

                        atkCool = 1;
                    }
                }
            }
        }

        if (UIManager.Instance != null) {
            UIManager.Instance.bossbar.gameObject.SetActive(Vector2.Distance(transform.position, Player.Local.transform.position) <= 20 && !isDeath);
            UIManager.Instance.bossbar.bar.value = (float)health / maxHealth;
        }
    }

    public override void OnDeath(Player attacker)
    {
        UIManager.Instance.bossbar.gameObject.SetActive(false);
    }

    #region phase Control
    void Phase1Update() {
        if (sk == 0) {
            action = "waiting";
            if (tick >= 2) {

                Sk1();
                tick = -1;
                sk = 1;
            }
        }
        else if (sk == 1) {

            if (tick >= 4) {
                tick = 0;
                sk = 2;
            }

            if ((float)health / maxHealth <= 0.4f && phase == 0) {
                sk = -1;
                tick = 0;

                StartCoroutine(Healing());
            }
        }
        else if (sk == 2) {

            if (tick >= 2) {
                tick = Random.Range(0f, 1f);
                sk = Random.Range(0, 2);

                Sk2();
            }
        }
    }
    void Phase2Update() {
        if (sk_ == 0) {
            targets = Search(3);

            if (targets.Count > 0) {
                if (tick >= 0.5f) {
                    target = targets[0];

                    P2Sk1();

                    tick = -1;
                }
            } else {
                tick = 0;
            }
        } else if (sk_ == 1) {
            tick += Time.deltaTime;
            tick2 += Time.deltaTime;

            if (tick2 >= 6) {
                tick = 0;
                tick2 = 0;
                action = "waiting";
                sk_ = 0;
            }

            Phase1Update();
        }
    }
    #endregion

    #region phase1 Skill Functions
    public void Sk1() {
        StartCoroutine(sk1());
    }

    IEnumerator Healing() {
        action = "waiting";
        animator.SetBool("healing", true);
        isHealing = true;
        isMoving = false;

        stopMove = 5;

        for (int i = 0; i <= 20; i++) {

            Heal(maxHealth / 60);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.4f);

        animator.SetBool("healing", false);

        phase = 1;
        tick = 0;
        sk = 0;
        isHealing = false;

        CamManager.main.Shake(8, 0.2f);
        UIManager.Instance.bossbar.title.text = "<color=\"red\">" + UIManager.Instance.bossbar.title.text + "</color>";
    }
    IEnumerator sk1() {
        float vel = 6 * facing;
        if (targets.Count > 0) {
            FaceTo(targets[0]);
            vel = targets[0].transform.position.x - transform.position.x + 2 * facing;
        }

        isMoving = false;
        dashing = true;

        RpcAnimateTrigger("dash");

        rb.linearVelocityX = vel * 3;

        yield return new WaitForSeconds(0.3f);

        rb.linearVelocityX = 0;

        yield return new WaitForSeconds(0.2f);

        facing *= -1;
        render.flipX = !render.flipX;

        dashing = false;

        Sk1Attack();

        action = "idle";
        atkCool = 1;
    }

    public void Sk2() {
        StartCoroutine(sk2());
    }
    IEnumerator sk2() {
        Vector3 vel = new Vector3(4 * facing, 8);
        if (targets.Count > 0) {
            Chase(targets[0]);
            vel = new Vector3((targets[0].transform.position.x - transform.position.x)/2 + 2 * facing, 8);
        }

        isMoving = false;
        dashing = true;

        RpcAnimateTrigger("jump");

        rb.linearVelocityY = vel.y * 3;
        rb.linearVelocityX = vel.x * 3 * facing;

        yield return new WaitForSeconds(0.1f);

        rb.linearVelocityX = 0.5f;

        yield return new WaitForSeconds(0.3f);

        if (targets.Count > 0) {
            Chase(targets[0]);
            vel = new Vector3(targets[0].transform.position.x - transform.position.x + 2 * facing, 8);
        }

        rb.linearVelocityX = vel.x * 3;
        rb.linearVelocityY = -30;

        dashing = false;

        while (!onGround) {
            yield return null;
        }

        Sk2Attack();

        yield return new WaitForSeconds(0.2f);

        rb.linearVelocityX = 0;

        action = "waiting";
    }

    void Sk2Attack() {
        RpcAnimateTrigger("attack2");

        CamManager.main.Shake(6, 0.1f);

        SoundManager.Instance.PlayToDist("shinobi_super", transform.position, 8);
        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + groundOffset + new Vector3(0, 0.5f), new Vector2(3.5f, 2), 0, Vector2.right * facing, 0.9f), null);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            target.DamageByMob(attackDamage * 5, uniqueId);
            target.Knockback(Vector2.right * facing * 8);
        }
    }

    void Sk1Attack() {
        RpcAnimateTrigger("attack3");

        SoundManager.Instance.PlayToDist("samurai_ja", transform.position, 8);

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + groundOffset + new Vector3(0, 0.5f), new Vector2(3.5f, 2), 0, Vector2.right * facing, 0.9f), null);

        

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            target.DamageByMob(attackDamage * 3, uniqueId);
            target.Knockback(Vector2.right * facing * 4 + Vector2.up * 2);
        }
    }

    void IdleAttack() {
        RpcAnimateTrigger("attack1");

        SoundManager.Instance.PlayToDist("default_attack", transform.position, 8);

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + groundOffset + new Vector3(0, 0.5f), new Vector2(4f, 2), 0, Vector2.right * facing, 0.9f), null);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            target.DamageByMob(attackDamage, uniqueId);
            target.Knockback(Vector2.right * facing * 4 + Vector2.up * 2);
        }
    }
    #endregion

    #region phase2 Skill Functions
    void P2Sk1() {
        StartCoroutine(p2sk1());
    }
    IEnumerator p2sk1(){
        SoundManager.Instance.PlayToDist("shinobi_super", transform.position, 8, 1.2f);
        RpcAnimateTrigger("spAtk");

        stopMove = 0.5f;

        target.Knockback(Vector2.up * 10);
        target.DamageByMob((int)(attackDamage * 1.5f), uniqueId);

        yield return new WaitForSeconds(0.3f);

        target.Knockback(Vector2.up * 20);
        target.DamageByMob(attackDamage * 2, uniqueId);

        yield return new WaitForSeconds(0.5f);

        if (Random.Range(0, 100) <= 50) {
            StartCoroutine(p2Sk1_1());
        } else {
            sk_ = 1;
            tick = 0;
            tick2 = 0;
        }
    }

    IEnumerator p2Sk1_1() {
        tick = -1;
        tick2 = 0;

        float vel = 8;
        if (targets.Count > 0) {
            Chase(targets[0]);
            vel = targets[0].transform.position.y - transform.position.y;
        }

        isMoving = false;
        dashing = true;

        RpcAnimateTrigger("jump");

        rb.linearVelocityY = vel * 3;
        rb.linearVelocityX = 0.5f * facing;

        yield return new WaitForSeconds(0.5f);

        rb.linearVelocityY = -0.5f;

        dashing = false;

        AirAttack();

        sk_ = 1;
    }

    void AirAttack() {
        RpcAnimateTrigger("airAtk");

        CamManager.main.Shake(5, 0.1f);

        SoundManager.Instance.PlayToDist("samurai_ja", transform.position, 8, 0.8f);
        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + groundOffset + new Vector3(0, 0.5f), new Vector2(4f, 4), 0, Vector2.right * facing, 0.9f), null);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            target.DamageByMob(attackDamage * 4, uniqueId);
            target.Knockback(Vector2.right * facing * 8 + Vector2.down * 20);
        }
    }
    #endregion
    public override void OnKnockback(Vector2 force, ref bool cancel)
    {
        if (isHealing) {
            cancel = true;
        }
    }
    public override void OnHurt(Player attacker, int damage, ref bool cancel)
    {
        if (atkCool <= 0.3f) {
            atkCool = 1f;
        }

        if (dashing) {
            cancel = true;
        }

        if (action == "idle" || action == "chase") {
            if (Random.Range(0f, 1f) <= 0.1f) {
                StartCoroutine(shield(attacker));

                cancel = true;
            }
        }

        if (action == "waiting") {
            if (Random.Range(0f, 1f) <= 0.4f) {
                StartCoroutine(shield(attacker));

                cancel = true;
            }
        }

        if (!cancel) {
            RpcAnimateTrigger("hurt");
        }
    }

    IEnumerator shield(Player attacker) {
        FaceTo(attacker);
        
        attacker.ch.CANCEL();
        attacker.Knockback(Vector2.right * facing * 10 + Vector2.up * 5);
        attacker.ch.atkCool = 0.5f;

        RpcAnimateTrigger("shield");

        SoundManager.Instance.PlayToDist("samurai_shield", transform.position, 15, 0.8f);
        
        Knockback(Vector2.right * -facing * 2);

        CamManager.main.CloseUp(3.2f, -10 * facing, 0.01f);
        CamManager.main.Shake(3, 0.5f);

        stopMove = 0.5f;

        yield return new WaitForSeconds(0.2f);
        attacker.rb.linearVelocity /= 2;

        yield return new WaitForSeconds(0.3f);

        tick += 0.2f;

        CamManager.main.CloseOut(0.1f);
    }
}