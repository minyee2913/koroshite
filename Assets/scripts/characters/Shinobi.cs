using System.Collections;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.UI;
using System.Collections.Generic;

public class Shinobi : Character
{
    bool shielding;
    bool invisible;
    bool hurted;
    float hurtTime;
    float invTime;
    Cooldown jumpAtkCool = new(1.5f);
    Cooldown invCool = new(3.5f);
    Cooldown superCool = new(4.2f);
    [SerializeField]
    ParticleSystem slash1;
    [SerializeField]
    GameObject slash2;
    [SerializeField]
    GameObject sparks;
    [SerializeField]
    GameObject mist;
    public override int maxHealth => 650;

    public override string atkInfo => "1) 전방으로 이동하면서 적에게 <color=\"red\">30</color>의 피해를 입힙니다.\n2) 후방으로 이동하면서 적에게 <color=\"red\">70</color>의 피해를 입힙니다.";

    public override string atk2Info => "점프 후 일반 공격 발동시 은신 후 전방으로 빠르게 돌진한 후 도달한 위치에서 검기를 휘둘러 <color=\"red\">80</color>의 피해를 입히고 HP를 10 회복합니다. (쿨타임 1.5s)";

    public override string skill1Info => "피해를 입은 후 즉시 해당 스킬 사용시 2s 동안 무적 상태가 됩니다.";

    public override string skill2Info => "쌍절곤을 휘두르며 넓은 범위의 적들에게 <color=\"red\">120</color>의 피해를 4번 입힙니다.";

    public override string id => "shinobi";

    public override void Callfunc(string method)
    {
        if (method == "_inv") {
            SetInv();
        } else if (method == "inv_") {
            UnSetInv();
        } else if (method == "atk1") {
            atk1Effect();
        } else if (method == "atk2") {
            atk2Effect();
        } else if (method == "JA") {
            JAEffect();
        } else if (method == "sp") {
            SPEffect();
        }
    }
    void SetInv() {
        invisible = true;
    }
    void UnSetInv() {
        invisible = false;
    }

    public void atk1Effect() {
        var slash = Instantiate(slash1, pl.transform);
        var main = slash.main;
        main.startColor = Color.red;
        slash.gameObject.AddComponent<SetLayer>().sortingLayer = "particle";

        slash.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

        slash.transform.position = pl.transform.position + new Vector3(0.5f * pl.facing, 1);
        slash.transform.rotation = Quaternion.Euler(15, 0, (pl.facing == 1) ? 0 : 180);

        slash.transform.Find("Hit").gameObject.SetActive(false);
        slash.transform.Find("Sparks").gameObject.SetActive(false);

        Destroy(slash.gameObject, 1);
    }

    public void atk2Effect() {
        var slash = Instantiate(slash1, pl.transform);
        var main = slash.main;
        main.startColor = Color.red;
        slash.gameObject.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(0.25f * pl.facing, 1);
        slash.transform.rotation = Quaternion.Euler(0, 0, (pl.facing == -1) ? 0 : 180);

        slash.transform.Find("Hit").gameObject.SetActive(false);
        slash.transform.Find("Sparks").gameObject.SetActive(false);

        Destroy(slash.gameObject, 1);
    }

    public void JAEffect() {
        var slash = Instantiate(slash2, pl.transform);
        slash.gameObject.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(0.5f * pl.facing, 0.5f);
        slash.transform.rotation = Quaternion.Euler(5, 0, (pl.facing == -1) ? 0 : 180);

        Destroy(slash, 1);
    }

    public void SPEffect() {
        var slash = Instantiate(mist, pl.transform);
        slash.gameObject.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        slash.transform.position = pl.transform.position + new Vector3(1f * pl.facing, 1.5f);

        SP2Effect();

        Destroy(slash.gameObject, 1);
    }

    public void SP2Effect() {
        var slash = Instantiate(slash2, pl.transform);
        slash.gameObject.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(0.5f * pl.facing, 1f);
        slash.transform.rotation = Quaternion.Euler(0, (pl.facing == -1) ? 270 : 90, 90);

        Destroy(slash, 1);
    }

    public override void Attack()
    {
        routine = _attack();
        StartCoroutine(routine);
    }

    public override void Skill1()
    {
        if (hurted && !invCool.IsIn()) {
            invCool.Start();

            pl.Dash();
            pl.rb.linearVelocity = new Vector2(-12 * pl.facing, pl.rb.linearVelocity.y);

            pl.CallChFunc("_inv");

            SoundManager.Instance.PlayToDist("shinobi_hide", transform.position, 15);

            pl.RpcAnimateTrigger("shield1");
        } else {
            shielding = true;
        }
    }

    public override void Skill1Up()
    {
        shielding = false;
    }

    public override void OnCancel()
    {
        if (state != "super") {
            pl.SetChScale(defaultScale);

            CamManager.main.CloseOut(0.1f);
        }
    }

    public override void OnHurt(ref int damage, Transform attacker, ref bool cancel)
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

        if (pl.jumpCount > 0 && !jumpAtkCool.IsIn()) {
            jumpAtkCool.Start();

            routine = jumpAtk();
            StartCoroutine(routine);

            yield break;
        }

        List<Player> targets;

        if (atkType == 0) {
            SoundManager.Instance.PlayToDist("shinobi_atk", transform.position, 15);
            pl.RpcAnimateTrigger("attack1");

            pl.stopMove = 0.34f;
            atkCool = 0.3f;

            atkType++;

            pl.CallChFunc("atk1");

            pl.rb.linearVelocity = new Vector2(8 * pl.facing, pl.rb.linearVelocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.linearVelocity = new Vector2(0, pl.rb.linearVelocity.y);

            targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(1, 2), 0, Vector2.right * pl.facing, 0.5f), pl);

            for (int i = 0; i < targets.Count; i++) {
                var target = targets[i];

                if (i == 0) pl.energy += 5;

                target.Damage(30, pl.name_);
                target.Knockback(Vector2.right * pl.facing * -4 + Vector2.up * 6);
            }

            var targetMobs = Monster.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(1, 2), 0, Vector2.right * pl.facing, 0.5f));

            for (int i = 0; i < targetMobs.Count; i++) {
                var target = targetMobs[i];

                target.Damage(30, pl.name_);
                target.Knockback(Vector2.right * pl.facing * -4 + Vector2.up * 6);
            }
        } else {
            SoundManager.Instance.PlayToDist("shinobi_atk2", transform.position, 15);
            pl.RpcAnimateTrigger("attack2");

            pl.stopMove = 0.34f;
            atkCool = 0.32f;

            atkType = 0;

            pl.CallChFunc("atk2");

            pl.rb.linearVelocity = new Vector2(-16 * pl.facing, pl.rb.linearVelocity.y);

            yield return new WaitForSeconds(0.2f);
            pl.rb.linearVelocity = new Vector2(0, pl.rb.linearVelocity.y);

            CamManager.main.Shake(2);

            targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(2, 2), 0, Vector2.right * pl.facing, 0f), pl);

            for (int i = 0; i < targets.Count; i++) {
                var target = targets[i];

                if (i == 0) pl.energy += 8;

                target.Damage(70, pl.name_);
                target.Knockback(Vector2.right * pl.facing * -3 + Vector2.up * 8);
            }

            var targetMobs = Monster.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(2, 2), 0, Vector2.right * pl.facing, 0f));

            for (int i = 0; i < targetMobs.Count; i++) {
                var target = targetMobs[i];

                target.Damage(70, pl.name_);
                target.Knockback(Vector2.right * pl.facing * -3 + Vector2.up * 8);
            }
        }

        routine = null;
    }

    IEnumerator jumpAtk() {

        pl.stopMove = 0.6f;
        atkCool = 0.3f;

        atkType = 0;

        pl.rb.linearVelocity = new Vector2(20 * pl.facing, -10);

        pl.SetChScale(Vector3.zero);

        SoundManager.Instance.PlayToDist("shinobi_ja", transform.position, 15);

        yield return new WaitForSeconds(0.26f);

        pl.RpcAnimateTrigger("attack3");

        pl.rb.linearVelocity = new Vector2(40 * pl.facing, pl.rb.linearVelocity.y);

        CamManager.main.CloseUp(4.2f, -pl.facing * 2, 0.1f);

        yield return new WaitForSeconds(0.1f);

        pl.SetChScale(defaultScale);

        pl.CallChFunc("JA");

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(10f, 3), 0, Vector2.right * pl.facing, 0f), pl);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            
            pl.energy += 6;
            pl.Heal(30);

            target.Damage(80, pl.name_);
            target.Knockback(Vector2.right * pl.facing * -3 + Vector2.up * 8);
        }

        var targetMobs = Monster.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(10f, 3), 0, Vector2.right * pl.facing, 0f));

        for (int i = 0; i < targetMobs.Count; i++) {
            var target = targetMobs[i];

            target.Damage(80, pl.name_);
            target.Knockback(Vector2.right * pl.facing * -3 + Vector2.up * 8);
        }

        pl.rb.linearVelocity = new Vector2(0, pl.rb.linearVelocity.y);

        yield return new WaitForSeconds(0.4f);

        CamManager.main.CloseOut(0.2f);

        routine = null;
    }

    public override void Skill2()
    {
        routine = super();
        StartCoroutine(routine);
    }

    public override void OnForceCancel()
    {
        if (state == "super") {
            CamManager.main.CloseOut(0.1f);
        }
    }

    IEnumerator super() {
        if (superCool.IsIn() || !EnergySystem.CheckNWarn(pl, 80)) {
            yield break;
        }

        state = "super";

        superCool.Start();

        pl.energy -= 80;
        pl.RpcAnimateTrigger("shield1");

        pl.stopMove = 1f;

        CamManager.main.CloseUp(3.4f, 0, 0.1f);

        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < 4; i++) {
            pl.RpcAnimateTrigger("spin");

            SoundManager.Instance.PlayToDist("shinobi_super", transform.position, 15);

            yield return new WaitForSeconds(0.2f);
            if (i > 0) CamManager.main.CloseUp(4.8f, 0, 0.1f);

            yield return new WaitForSeconds(0.4f);

            CamManager.main.Shake(8);
            CamManager.main.CloseUp(3.4f, 10 * pl.facing, 0.05f);

            var targets = Player.Convert(Physics2D.CircleCastAll(transform.position + new Vector3(0, 1f), 5, Vector2.right * pl.facing, 0.25f), pl);

            for (int j = 0; j < targets.Count; j++) {
                var target = targets[j];

                if (i == 0) pl.Heal(60);

                target.Damage(120, pl.name_);
                target.Knockback(Vector2.up * 15);
            }

            var targetMobs = Monster.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(10f, 3), 0, Vector2.right * pl.facing, 0f));

            for (int j = 0; j < targetMobs.Count; j++) {
                var target = targetMobs[j];

                target.Damage(120, pl.name_);
                target.Knockback(Vector2.up * 15);
            }

            pl.CallChFunc("sp");

            pl.stopMove = 0.4f;
        }

        CamManager.main.CloseOut(0.2f);

        state = null;
    }
}
