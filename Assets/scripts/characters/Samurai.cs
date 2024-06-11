using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Samurai : Character
{
    bool shielding, shieldSuccess;
    float shieldTime;

    public override void Attack()
    {
        routine = _attack();
        StartCoroutine(routine);
    }

    public override void Skill1()
    {
        shielding = true;
        CamManager.main.CloseUp(5.2f, 0, 0.1f);

        shieldTime = 0;
    }

    public override void Skill1Up()
    {
        if (!shieldSuccess) {
            shielding = false;
            CamManager.main.CloseOut(0.1f);

            shieldTime = 0;
        }
    }

    void Shielded(Player attacker) {
        attacker.ch.CANCEL();
        attacker.Knockback(Vector2.right * pl.facing * 8);

        CamManager.main.CloseUp(3.2f, -15, 0.1f);
        CamManager.main.Shake(1, 0.8f);

        pl.stopMove = 0.8f;

        shieldSuccess = true;

        Invoke("ShieldEnd", 0.8f);
    }

    void ShieldEnd() {
        CamManager.main.CloseOut(0.1f);

        shielding = false;

        shieldSuccess = false;
    }

    public override void OnHurt(int damage, Player attacker)
    {
        if (shielding) {
            if (shieldTime < 0.3f) {
                if (pl.facing > 0) {
                    if (pl.transform.position.x < attacker.transform.position.x) {
                        Shielded(attacker);
                    }
                } else {
                    if (pl.transform.position.x > attacker.transform.position.x) {
                        Shielded(attacker);
                    }
                }
            }
        }
    }

    private void Update() {
        if (pl != null) {
            if (pl.pv.IsMine) pl.RpcAnimateBool("shielding", shielding);

            if (shielding) {
                pl.stopMove = 0.2f;
                atkCool = 0.2f;

                shieldTime += Time.deltaTime;
            }
        }
    }

    IEnumerator _attack() {
        if (atkCool > 0) {
            routine = null;

            yield break;
        }

        if (pl.jumpCount >= 2) {
            routine = jumpAtk();
            StartCoroutine(routine);

            yield break;
        }

        if (atkType == 0) {
            pl.RpcAnimateTrigger("attack1");

            pl.stopMove = 0.4f;
            atkCool = 0.3f;

            atkType++;

            pl.rb.velocity = new Vector2(6 * pl.facing, pl.rb.velocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);
        } else {
            pl.RpcAnimateTrigger("attack2");

            pl.stopMove = 0.5f;
            atkCool = 0.35f;

            atkType = 0;

            pl.rb.velocity = new Vector2(10 * pl.facing, pl.rb.velocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);
        }

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(2, 2), 0, Vector2.right * pl.facing, 1), pl);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            target.Damage(50, pl.name_);
            target.Knockback(Vector2.right * pl.facing * 6 + Vector2.up * 2);
        }

        routine = null;
    }

    IEnumerator jumpAtk() {
        pl.RpcAnimateTrigger("attack3");

        pl.stopMove = 0.6f;
        atkCool = 0.3f;

        atkType = 0;

        var p1 = pl.transform.position;

        CamManager.main.CloseUp(4.2f, -pl.facing * 2, 0.1f);

        pl.rb.velocity = new Vector2(30 * pl.facing, -20);

        yield return new WaitForSeconds(0.2f);
        pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);

        var targets = Player.Convert(Physics2D.BoxCastAll((p1 + transform.position) / 2, new Vector2(Mathf.Abs(transform.position.x - p1.x), Mathf.Abs(transform.position.y - p1.y)), 0, Vector2.zero), pl);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            target.Damage(80, pl.name_);
        }

        yield return new WaitForSeconds(0.3f);

        CamManager.main.CloseOut(0.1f);

        routine = null;
    }
}
