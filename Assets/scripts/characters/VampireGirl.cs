using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class VampireGirl : Character
{
    bool shielding, skillOn;
    public GameObject slash1;
    public GameObject slash2;
    public GameObject crit;
    public GameObject explode;

    Cooldown superCool = new(4.2f);
    public override int maxHealth => 1200;

    public override string atkInfo => "전방으로 이동하면서 적에게 <color=\"red\">20</color>의 피해를 입힙니다.\n피격된 적이 존재하면 에너지가 5, 체력이 10 감소합니다.";

    public override string atk2Info => "일반 공격 2연타 사용시 고유 스킬이 강화됩니다. 일반 공격을 다시 사용하면 강화가 종료됩니다.";

    public override string skill1Info => "가드를 올려 모든 넉백 효과를 무시합니다.\n\n- 강화 스킬\n전방에 있는 적들을 할퀴어서 <color=\"red\">60</color>의 피해를 입힙니다.\n피격된 적이 존재하면 에너지를 35, 체력을 50 회복합니다.\n피격된 적이 존재하지 않으면 에너지가 20, 체력이 50 감소합니다.";

    public override string skill2Info => "전방에 있는 적 1명을 집어 삼켜서 행동 불능 상태로 만들고 <color=\"red\">100</color>의 피해를 7회 입히고 적을 뱉어내면서 추가로 <color=\"red\">100</color>의 피해를 입힙니다.";

    public override void Callfunc(string method)
    {
        if (method == "atk1") {
            atk1Effect();
        } else if (method == "atk2") {
            atk2Effect();
        } else if (method == "sk") {
            skEffect();
        } else if (method == "hit") {
            hitEffect();
        } else if (method == "explode") {
            explodeEffect();
        }
    }

    public void atk1Effect() {
        var slash = Instantiate(slash1, pl.transform);
        slash.gameObject.AddComponent<SetLayer>().sortingLayer = "particle";

        slash.transform.localScale = new Vector3(0.5f, 1, 1);

        slash.transform.position = pl.transform.position + new Vector3(0.6f * pl.facing, 0.6f);
        slash.transform.rotation = Quaternion.Euler(0, 0, (pl.facing == -1) ? 205 : -25);

        var slash_1 = Instantiate(slash, pl.transform);
        var slash_2 = Instantiate(slash, pl.transform);

        slash_1.transform.position += new Vector3(0, 0.5f);
        slash_2.transform.position += new Vector3(0, -0.5f);

        Destroy(slash.gameObject, 1);
        Destroy(slash_1.gameObject, 1);
        Destroy(slash_2.gameObject, 1);
    }

    public void atk2Effect() {
        var slash = Instantiate(slash1, pl.transform);
        slash.gameObject.AddComponent<SetLayer>().sortingLayer = "particle";

        slash.transform.localScale = new Vector3(0.5f, 1, 1);

        slash.transform.position = pl.transform.position + new Vector3(0.6f * pl.facing, 0.8f);
        slash.transform.rotation = Quaternion.Euler(0, 0, (pl.facing == -1) ? 155 : 25);

        var slash_1 = Instantiate(slash, pl.transform);
        var slash_2 = Instantiate(slash, pl.transform);

        slash_1.transform.position += new Vector3(0, 0.3f);
        slash_2.transform.position += new Vector3(0, -0.3f);

        Destroy(slash.gameObject, 1);
        Destroy(slash_1.gameObject, 1);
        Destroy(slash_2.gameObject, 1);
    }

    public void skEffect() {
        var slash = Instantiate(slash2, pl.transform);
        slash.gameObject.AddComponent<SetLayer>().sortingLayer = "particle";

        slash.transform.localScale = new Vector3(0.5f, 1, 1);

        slash.transform.position = pl.transform.position + new Vector3(1f * pl.facing, 0.8f);
        slash.transform.rotation = Quaternion.Euler(0, 0, (pl.facing == -1) ? 250 : -70);

        var slash_1 = Instantiate(slash, pl.transform);
        var slash_2 = Instantiate(slash, pl.transform);

        slash_1.transform.position += new Vector3(0.8f, 0.3f);
        slash_2.transform.position += new Vector3(-0.8f, -0.3f);

        Destroy(slash.gameObject, 1);
        Destroy(slash_1.gameObject, 1);
        Destroy(slash_2.gameObject, 1);
    }

    public void hitEffect() {
        var slash = Instantiate(crit, pl.transform);
        var layer = slash.gameObject.AddComponent<SetLayer>();
        layer.sortingLayer = "tile";
        layer.sortingOrder = 5;

        slash.transform.position = pl.transform.position + new Vector3(0.5f * pl.facing, 1f);

        Destroy(slash.gameObject, 1);
    }

    public void explodeEffect() {
        var slash = Instantiate(explode, pl.transform);
        var layer = slash.gameObject.AddComponent<SetLayer>();
        layer.sortingLayer = "particle";

        slash.transform.position = pl.transform.position + new Vector3(1f * pl.facing, 1f);

        Destroy(slash.gameObject, 1);
    }

    public override void Attack()
    {
        routine = _attack();
        StartCoroutine(routine);
    }

    public override void OnCancel()
    {
        if (state != "super") {
            CamManager.main.Offset(Vector2.zero, 0.1f);
            CamManager.main.CloseOut(0.1f);

            pl.SetChScale(defaultScale);
        } else {
            CamManager.main.Offset(Vector2.zero, 0f);
            CamManager.main.CloseOut(0f);

            animator.SetBool("inSuper", false);
        }
    }

    public override void Skill1()
    {
        if (skillOn) {
            routine = cackk();
            StartCoroutine(routine);
        } else {
            shielding = true;
        }
    }

    IEnumerator cackk() {
        skillOn = false;
        shielding = false;

        pl.preventInput = 0.5f;
        pl.stopMove = 0.5f;

        pl.RpcAnimateTrigger("attack3");
        CamManager.main.Shake(10);

        SoundManager.Instance.PlayToDist("vampire_girl_skill", transform.position, 15);

        yield return new WaitForSeconds(0.1f);

        pl.CallChFunc("sk");

        pl.SetChScale(new Vector3(defaultScale.x * 1.25f, defaultScale.y));

        CamManager.main.Shake(3, 0.3f);
        CamManager.main.CloseUp(4.2f, 8 * -pl.facing, 0.1f);
        CamManager.main.Offset(new Vector3(pl.facing * 2, -0.5f), 0.1f);

        List<Player> targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(2.5f, 2.2f), 0, Vector2.right * pl.facing, 0.9f), pl);
        List<Monster> targetMob = Monster.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(2.5f, 2.2f), 0, Vector2.right * pl.facing, 0.9f));

        if (targets.Count > 0) {
            pl.energy += 35;
            pl.Heal(50);
        } else if (targetMob.Count > 0) {
            pl.Heal(50);
        } else {
            pl.energy -= 10;
            pl.Damage(50, null, false);
        }

        for (int j = 0; j < 4; j++) {
            targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(2.5f, 2.2f), 0, Vector2.right * pl.facing, 0.9f), pl);
            targetMob = Monster.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(2.5f, 2.2f), 0, Vector2.right * pl.facing, 0.9f));

            for (int i = 0; i < targets.Count; i++) {
                var target = targets[i];

                target.Damage(60, pl.name_);
                target.CallCancel();
            }

            for (int i = 0; i < targetMob.Count; i++) {
                var target = targetMob[i];

                target.Damage(60, pl.name_);
            }

            yield return new WaitForSeconds(0.1f);
        }

        pl.SetChScale(defaultScale);

        CamManager.main.Offset(Vector3.zero, 0.2f);
        CamManager.main.CloseOut(0.2f);
    }

    public override void Skill1Up()
    {
        shielding = false;
    }

    public override void OnHurt(ref int damage, Transform attacker, ref bool cancel)
    {
        if (state == "super") {
            cancel = true;
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

                float y = pl.rb.velocity.y;

                if (y > 1f) y = 1f;

                pl.rb.velocity = new Vector2(0, y);
            }
        }
    }

    IEnumerator _attack() {
        if (atkCool > 0) {
            routine = null;

            yield break;
        }

        SoundManager.Instance.PlayToDist("vampire_girl_atk", transform.position, 15);

        if (atkType == 0) {
            skillOn = false;

            pl.RpcAnimateTrigger("attack1");

            pl.stopMove = 0.4f;
            atkCool = 0.3f;

            atkType++;

            pl.CallChFunc("atk1");

            pl.rb.velocity = new Vector2(2 * pl.facing, pl.rb.velocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);
        } else {
            pl.RpcAnimateTrigger("attack2");

            pl.stopMove = 0.5f;
            atkCool = 0.4f;

            atkType = 0;

            pl.CallChFunc("atk2");

            pl.rb.velocity = new Vector2(4 * pl.facing, pl.rb.velocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);

            skillOn = true;

            CamManager.main.Shake();
        }

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(2.5f, 2), 0, Vector2.right * pl.facing, 0.9f), pl);
        var targetMob = Monster.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(2.5f, 2), 0, Vector2.right * pl.facing, 0.9f));

        

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            if (i == 0) {
                pl.Damage(10, null, false);
            }

            target.Damage(20, pl.name_);
            target.Knockback(Vector2.right * pl.facing * 4 + Vector2.up * 2);
        }

        for (int i = 0; i < targetMob.Count; i++) {
            var target = targetMob[i];

            if (i == 0) {
                pl.Damage(10, null, false);
            }

            target.Damage(20, pl.name_);
            target.Knockback(Vector2.right * pl.facing * 4 + Vector2.up * 2);
        }

        if (pl.energy < 0) pl.energy = 0;

        routine = null;
    }

    public override void Skill2()
    {
        routine = super();
        StartCoroutine(routine);
    }

    IEnumerator super() {
        if (superCool.IsIn() || !EnergySystem.CheckNWarn(pl, 100)) {
            yield break;
        }

        CamManager.main.CloseUp(3.4f, 0, 0.1f);

        state = "super";
        shielding = true;

        superCool.Start();

        pl.energy -= 100;

        pl.preventInput = 1f;

        pl.rb.velocity = new Vector2(10 * pl.facing, pl.rb.velocity.y);

        yield return new WaitForSeconds(0.2f);
        pl.rb.velocity = new Vector2(0, pl.rb.velocity.y);

        yield return new WaitForSeconds(0.4f);

        shielding = false;
        pl.stopMove = 1f;

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(5f, 3), 0, Vector2.right * pl.facing, 0.9f), pl);
        var targetMob = Monster.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(5f, 3), 0, Vector2.right * pl.facing, 0.9f));

        animator.SetBool("inSuper", true);

        SoundManager.Instance.PlayToDist("vampire_girl_eat", transform.position, 15);

        foreach (Monster target in targetMob) {
            target.Damage(120, pl.name_);
        }

        if (targets.Count > 0) {
            CamManager.main.CloseUp(3f, 0, 0.1f);

            targets[0].CallCancelF();
            targets[0].SetPrevent(0.5f);
            targets[0].SetStopMove(0.5f);
            targets[0].SetPos(transform.position);
            targets[0].SetChScale(Vector2.zero);

            for (int i = 0; i < 7; i++) {
                pl.CallChFunc("hit");

                targets[0].Damage(100, pl.name_);
                pl.Heal(60);
                
                SoundManager.Instance.PlayToDist("vampire_girl_super_dot", transform.position, 15);

                yield return new WaitForSeconds(0.1f);
            }

            targets[0].Knockback(Vector2.right * pl.facing * 20 + Vector2.up * 5);

            yield return new WaitForSeconds(0.3f);

            CamManager.main.Shake(20);
            targets[0].Damage(120, pl.name_);

            SoundManager.Instance.PlayToDist("vampire_girl_super", transform.position, 15);

            pl.CallChFunc("explode");

            animator.SetBool("inSuper", false);
            targets[0].SetChScale(defaultScale);
        } else {
            yield return new WaitForSeconds(0.5f);

            animator.SetBool("inSuper", false);
        }

        CamManager.main.CloseOut(0.5f);

        pl.stopMove = 0.2f;
        pl.preventInput = 0.2f;

        state = null;
    }
}
