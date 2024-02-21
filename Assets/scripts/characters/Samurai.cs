using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Samurai : Character
{
    bool shielding;

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
        animator.SetBool("shielding", shielding);

        if (shielding) {
            pl.stopMove = 0.2f;
            atkCool = 0.2f;
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
            animator.SetTrigger("attack1");

            pl.stopMove = 0.4f;
            atkCool = 0.3f;

            atkType++;

            pl.rb.velocity = new Vector2(6 * pl.facing, pl.rb.velocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);
        } else {
            animator.SetTrigger("attack2");

            pl.stopMove = 0.5f;
            atkCool = 0.35f;

            atkType = 0;

            pl.rb.velocity = new Vector2(10 * pl.facing, pl.rb.velocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);
        }

        routine = null;
    }

    IEnumerator jumpAtk() {
        animator.SetTrigger("attack3");

        pl.stopMove = 0.6f;
        atkCool = 0.3f;

        atkType = 0;

        CamManager.main.CloseUp(4.2f, -pl.facing * 2, 0.1f);

        pl.rb.velocity = new Vector2(30 * pl.facing, -20);

        yield return new WaitForSeconds(0.2f);
        pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);

        yield return new WaitForSeconds(0.3f);

        CamManager.main.CloseOut(0.1f);

        routine = null;
    }
}
