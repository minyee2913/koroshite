using System.Collections;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.UI;

public class Shinobi : Character
{
    bool shielding;
    bool invisible;
    bool hurted;
    float hurtTime;
    float invTime;
    Cooldown jumpAtkCool = new(1.5f);

    public override void Callfunc(string method)
    {
        if (method == "_inv") {
            SetInv();
        } else if (method == "inv_") {
            UnSetInv();
        }
    }
    void SetInv() {
        invisible = true;
    }
    void UnSetInv() {
        invisible = false;
    }

    public override void Attack()
    {
        routine = _attack();
        StartCoroutine(routine);
    }

    public override void Skill1()
    {
        if (hurted) {
            pl.Dash();
            pl.rb.velocity = new Vector2(-12 * pl.facing, pl.rb.velocity.y);

            pl.CallChFunc("_inv");

            pl.RpcAnimateTrigger("shield1");
        } else {
            shielding = true;
        }
    }

    public override void Skill1Up()
    {
        shielding = false;
    }

    public override void OnHurt(ref int damage, Player attacker, ref bool cancel)
    {
        if (invisible) {
            cancel = true;
        } else {
            hurted = true;
            hurtTime = 0;
        }
    }

    private void Update() {
        if (pl != null) {
            if (pl.pv.IsMine) {
                animator.SetBool("shielding", shielding);
            }

            if (shielding) {
                pl.stopMove = 0.2f;
                atkCool = 0.2f;
            }

            Color col = Color.white;
            if (invisible) {
                invTime += Time.deltaTime;

                col.a = 0.5f;

                if (invTime > 2f) {
                    invisible = false;
                    pl.CallChFunc("inv_");
                    invTime = 0;
                }
            } else {
                col.a = 1;
            }

            render.color = col;
        }

        if (hurted) {
            hurtTime += Time.deltaTime;

            if (hurtTime > 2f) {
                hurted = false;
                hurtTime = 0;
            }
        }

    }

    IEnumerator _attack() {
        if (atkCool > 0) {
            routine = null;

            yield break;
        }

        if (!pl.onGround && !jumpAtkCool.IsIn()) {
            jumpAtkCool.Start();

            routine = jumpAtk();
            StartCoroutine(routine);

            yield break;
        }

        if (atkType == 0) {
            pl.RpcAnimateTrigger("attack1");

            pl.stopMove = 0.34f;
            atkCool = 0.3f;

            atkType++;

            pl.CallChFunc("atk1");

            pl.rb.velocity = new Vector2(8 * pl.facing, pl.rb.velocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);
        } else {
            pl.RpcAnimateTrigger("attack2");

            pl.stopMove = 0.34f;
            atkCool = 0.32f;

            atkType = 0;

            pl.CallChFunc("atk2");

            pl.rb.velocity = new Vector2(-16 * pl.facing, pl.rb.velocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);
        }

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(1, 2), 0, Vector2.right * pl.facing, 0.5f), pl);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            pl.energy += 5;

            target.Damage(30, pl.name_);
            target.Knockback(Vector2.right * pl.facing * 3 + Vector2.up * 6);
        }

        routine = null;
    }

    IEnumerator jumpAtk() {

        pl.stopMove = 0.6f;
        atkCool = 0.3f;

        atkType = 0;

        pl.rb.velocity = new Vector2(20 * pl.facing, -10);

        transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(0.26f);

        pl.RpcAnimateTrigger("attack3");

        pl.rb.velocity = new Vector2(40 * pl.facing, pl.rb.velocity.y);

        CamManager.main.CloseUp(4.2f, -pl.facing * 2, 0.1f);

        yield return new WaitForSeconds(0.1f);

        transform.localScale = defaultScale;

        pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);

        yield return new WaitForSeconds(0.4f);

        CamManager.main.CloseOut(0.2f);
    }

    public override void Skill2()
    {
        routine = super();
        StartCoroutine(routine);
    }

    IEnumerator super() {
        yield return null;
    }
}
