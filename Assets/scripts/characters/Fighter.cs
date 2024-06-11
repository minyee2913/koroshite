using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : Character
{
    bool shielding;
    float time;

    public override void Attack()
    {
        routine = _attack();
        StartCoroutine(routine);
    }

    public override void Skill1()
    {
        shielding = true;
        CamManager.main.CloseUp(5.2f, 0, 0.1f);
    }

    public override void Skill1Up()
    {
        shielding = false;
        CamManager.main.CloseOut(0.1f);
    }

    private void Update() {
        if (pl != null) {
            if (pl.pv.IsMine) pl.RpcAnimateBool("shielding", shielding);

            if (shielding) {
                pl.stopMove = 0.2f;
                atkCool = 0.2f;
            }
        }

        time += Time.deltaTime;

        if (time > 1) {
            time = 0;
            if (pl) Attack();
        }
    }

    IEnumerator _attack() {
        if (atkCool > 0) {
            routine = null;

            yield break;
        }

        if (atkType == 0) {
            pl.RpcAnimateTrigger("attack1");

            pl.stopMove = 0.4f;
            atkCool = 0.3f;

            atkType++;

            // pl.rb.velocity = new Vector2(6 * pl.facing, pl.rb.velocity.y);

            // yield return new WaitForSeconds(0.2f);
            // pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);
        } else {
            pl.RpcAnimateTrigger("attack2");

            pl.stopMove = 0.5f;
            atkCool = 0.35f;

            atkType = 0;

            // pl.rb.velocity = new Vector2(10 * pl.facing, pl.rb.velocity.y);

            // yield return new WaitForSeconds(0.2f);
            // pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);
        }

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(2, 2), 0, Vector2.right * pl.facing, 1), pl);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            target.Damage(50, pl.name_);
        }

        routine = null;
    }
}
