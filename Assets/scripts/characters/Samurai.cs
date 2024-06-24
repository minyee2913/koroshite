using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Samurai : Character
{
    bool shielding, shieldSuccess;
    float shieldTime;
    Cooldown jumpAtkCool = new(0.6f);
    Cooldown superCool = new(3f);
    public GameObject swordSlash;
    public GameObject swordStrike;
    public GameObject spatial;

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
        StartCoroutine(_shield(attacker));
    }

    IEnumerator _shield(Player attacker) {
        attacker.ch.CANCEL();
        attacker.Knockback(Vector2.right * pl.facing * 10 + Vector2.up * 5);
        attacker.ch.atkCool = 0.5f;
        
        pl.Knockback(Vector2.right * -pl.facing * 4);
        pl.Heal(30);
        pl.energy += 10;

        CamManager.main.CloseUp(3.2f, -10 * pl.facing, 0.01f);
        CamManager.main.Shake(3, 0.5f);

        pl.stopMove = 0.5f;

        shieldSuccess = true;

        yield return new WaitForSeconds(0.2f);
        attacker.rb.velocity /= 2;

        yield return new WaitForSeconds(0.3f);

        CamManager.main.CloseOut(0.1f);

        shieldSuccess = false;
        shielding = false;
    }

    public override void OnHurt(ref int damage, Player attacker, ref bool cancel)
    {
        if (shielding) {
            if (shieldTime < 0.5f) {
                if (pl.facing > 0) {
                    if (pl.transform.position.x < attacker.transform.position.x) {
                        Shielded(attacker);

                        cancel = true;
                    }
                } else {
                    if (pl.transform.position.x > attacker.transform.position.x) {
                        Shielded(attacker);

                        cancel = true;
                    }
                }
            } else {
                damage -= (int)Mathf.Round(damage * 0.3f);
            }
        }
    }

    private void Update() {
        if (pl != null) {
            if (pl.pv.IsMine) animator.SetBool("shielding", shielding);

            if (shielding) {
                pl.stopMove = 0.2f;
                atkCool = 0.2f;

                shieldTime += Time.deltaTime;
            }
        }
    }

    public override void Callfunc(string method)
    {
        if (method == "atk1") {
            atk1Effect();
        } else if (method == "atk2") {
            atk2Effect();
        } else if (method == "JA") {
            JAEffect();
        } else if (method == "sp") {
            SPEffect();
        } else if (method == "atkSP") {
            atkSPEffect();
        }
    }

    public void atk1Effect() {
        var slash = Instantiate(swordSlash, pl.transform);
        slash.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(0.5f * pl.facing, 1);
        slash.transform.rotation = Quaternion.Euler(5, 0, (pl.facing == -1) ? 0 : 180);

        Destroy(slash, 1);
    }

    public void atk2Effect() {
        var slash = Instantiate(swordSlash, pl.transform);
        slash.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(0.5f * pl.facing, 1);
        slash.transform.rotation = Quaternion.Euler(8, 0, (pl.facing == -1) ? 180 : 0);

        Destroy(slash, 1);
    }

    public void JAEffect() {
        var slash = Instantiate(swordStrike, pl.transform);
        slash.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(-2f * pl.facing, 1);
        slash.transform.rotation = Quaternion.Euler((pl.facing == -1) ? 170 : 10, 90, 90);

        Destroy(slash, 1);
    }

    public void SPEffect() {
        var slash = Instantiate(spatial, pl.transform);
        slash.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(0, -1);

        Destroy(slash, 2);
    }

    public void atkSPEffect() {
        var slash = Instantiate(swordSlash, pl.transform);
        slash.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(0, 1);
        slash.transform.localScale = Vector3.one * 2.5f;
        slash.transform.rotation = Quaternion.Euler(0, 0, (pl.facing == -1) ? 180 : 0);

        Destroy(slash, 1);
    }

    public override void Skill2()
    {
        StartCoroutine(super());
    }

    IEnumerator super() {
        if (!EnergySystem.CheckNWarn(pl, 80) || superCool.IsIn()) yield break;

        pl.energy -= 80;
        superCool.Start();

        pl.stopMove = 0.5f;
        pl.RpcAnimateTrigger("charge");

        CamManager.main.CloseUp(3f, 0, 0.1f);

        pl.CallChFunc("sp");

        yield return new WaitForSeconds(0.5f);

        transform.localScale = Vector2.zero;

        CamManager.main.CloseUp(5f, 0, 0.1f);

        yield return new WaitForSeconds(0.3f);

        pl.stopMove = 1f;

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 2f), new Vector2(14, 5), 0, Vector2.right, 0), pl);

        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < targets.Count; j++) {
                var target = targets[j];

                pl.energy += 2;

                target.Damage(80, pl.name_);
            }

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.3f);

        transform.localScale = defaultScale;

        pl.RpcAnimateTrigger("attack2");

        CamManager.main.CloseUp(4.9f, 0, 0.1f);

        pl.rb.velocity = new Vector2(-16 * pl.facing, pl.rb.velocity.y);
        pl.SetFacing(-pl.facing);

        pl.CallChFunc("atkSP");

        yield return new WaitForSeconds(0.2f);

        pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);

        targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 1f), new Vector2(14, 3), 0, Vector2.right, 0), pl);

        for (int j = 0; j < targets.Count; j++) {
            var target = targets[j];

            pl.energy += 4;

            target.Damage(120, pl.name_);
        }

        yield return new WaitForSeconds(0.5f);

        CamManager.main.CloseOut(0.1f);
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

            pl.CallChFunc("atk1");

            pl.rb.velocity = new Vector2(6 * pl.facing, pl.rb.velocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);
        } else {
            pl.RpcAnimateTrigger("attack2");

            pl.stopMove = 0.5f;
            atkCool = 0.35f;

            atkType = 0;

            pl.CallChFunc("atk2");

            pl.rb.velocity = new Vector2(10 * pl.facing, pl.rb.velocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);
        }

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(2, 2), 0, Vector2.right * pl.facing, 1), pl);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            pl.energy += 5;

            target.Damage(50, pl.name_);
            target.Knockback(Vector2.right * pl.facing * 6 + Vector2.up * 2);
        }

        routine = null;
    }

    IEnumerator jumpAtk() {
        if (jumpAtkCool.IsIn()) yield break;
        jumpAtkCool.Start();

        pl.RpcAnimateTrigger("attack3");

        pl.stopMove = 0.6f;
        atkCool = 0.3f;

        atkType = 0;

        var p1 = pl.transform.position;

        CamManager.main.CloseUp(4.2f, -pl.facing * 2, 0.1f);
        
        pl.CallChFunc("JA");

        pl.rb.velocity = new Vector2(30 * pl.facing, -20);

        yield return new WaitForSeconds(0.2f);
        pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);

        var targets = Player.Convert(Physics2D.BoxCastAll((p1 + transform.position) / 2, new Vector2(Mathf.Abs(transform.position.x - p1.x), Mathf.Abs(transform.position.y - p1.y)), 0, Vector2.zero), pl);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            pl.energy += 7;

            target.Damage(80, pl.name_);
        }

        yield return new WaitForSeconds(0.3f);

        CamManager.main.CloseOut(0.1f);

        routine = null;
    }
}
