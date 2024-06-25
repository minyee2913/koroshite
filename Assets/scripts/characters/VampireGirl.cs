using System.Collections;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.UI;

public class VampireGirl : Character
{
    bool shielding;

    public override void Callfunc(string method)
    {
    }

    public override void Attack()
    {
        routine = _attack();
        StartCoroutine(routine);
    }

    public override void Skill1()
    {
    }

    public override void Skill1Up()
    {
    }

    public override void OnHurt(ref int damage, Player attacker, ref bool cancel)
    {
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

            pl.stopMove = 0.22f;
            atkCool = 0.2f;

            atkType++;

            pl.CallChFunc("atk1");

            pl.rb.velocity = new Vector2(6 * pl.facing, pl.rb.velocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);
        } else {
            pl.RpcAnimateTrigger("attack2");

            pl.stopMove = 0.22f;
            atkCool = 0.2f;

            atkType = 0;

            pl.CallChFunc("atk2");

            pl.rb.velocity = new Vector2(12 * pl.facing, pl.rb.velocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);
        }

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(1, 2), 0, Vector2.right * pl.facing, 0.5f), pl);

        CamManager.main.Shake();

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            pl.energy += 5;

            target.Damage(30, pl.name_);
            target.Knockback(Vector2.right * pl.facing * 10 + Vector2.up * 2);
        }

        routine = null;
    }

    IEnumerator jumpAtk() {

        pl.RpcAnimateTrigger("down");

        pl.stopMove = 0.6f;
        atkCool = 0.3f;

        atkType = 0;

        var p1 = pl.transform.position;

        CamManager.main.CloseUp(4.2f, -pl.facing * 2, 0.1f);

        pl.rb.velocity = new Vector2(10 * pl.facing, -30);

        while (!pl.onGround) {
            yield return new WaitForSeconds(0.05f);
        }

        pl.CallChFunc("JA");

        pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);

        var targets = Player.Convert(Physics2D.BoxCastAll((p1 + transform.position) / 2, new Vector2(Mathf.Abs(transform.position.x - p1.x), Mathf.Abs(transform.position.y - p1.y)), 0, Vector2.zero), pl);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            pl.energy += 7;

            target.Damage(90, pl.name_);
        }

        yield return new WaitForSeconds(0.3f);

        CamManager.main.CloseOut(0.1f);

        routine = null;
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
