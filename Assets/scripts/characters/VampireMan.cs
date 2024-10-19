using System.Collections;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.UI;
using System.Collections.Generic;

public class VampireMan : Character
{
    bool running;
    public GameObject swordStrike, speedAura, crit, speedEnd;
    List<Player> targets = new();
    List<Monster> targetMob = new();
    List<Player> tt;
    List<Monster> tm;
    Cooldown skillCool = new(1f);
    Cooldown superCool = new(3f);
    bool inSuper;

    public override int maxHealth => 600;

    public override string atkInfo => "전방으로 이동하면서 적에게 <color=\"red\">25</color>의 피해를 입히고 보호막을 20 획득합니다.";

    public override string atk2Info => "X";

    public override string skill1Info => "홀드시 보호막을 보유하고 있다면 보호막을 소모하며\n질주합니다.\n질주 도중 스친 적들에게 표식을 남기며 홀드를 끝내거나 보호막을 모두 소모하면\n 표식이 남겨진 적들에게 거리에 따라 최대 <color=\"red\">120</color>의 피해를 가합니다.";

    public override string skill2Info => "더욱 빠르게 질주하며 스친 적들에게 표식을 남기고 표식이 남겨진 적들을 계속 끌어당깁니다.\n당겨진 적들에게 <color=\"red\">20</color>의 피해를 입히고\n자신의 체력을 80, 보호막을 100 회복합니다. 이것을 7회 반복하고 질주를 종료합니다.";

    public override void Callfunc(string method)
    {
        if (method == "atk") {
            AtkEffect();
        } else if (method == "bufOn") {
            bufOn();
        } else if (method == "bufOff") {
            bufOff();
        } else if (method.StartsWith("crit-")) {
            CritEf(method);
        } else if (method == "speedEnd") {
            SpeedEnd();
        }
    }

    void CritEf(string method) {
        var poses = method.Replace("crit-", "").Split("|");
        Vector3 pos = new Vector3(float.Parse(poses[0]), float.Parse(poses[1]));

        var slash = Instantiate(crit, pl.transform);
        slash.AddComponent<SetLayer>().sortingLayer = "particle";

        slash.transform.position = pos + new Vector3(0, 0.5f);

        Destroy(slash, 1);
    }
    void SendCrit(Vector2 pos) {
        pl.CallChFunc("crit-" + pos.x + "|" + pos.y);
    }

    void SpeedEnd() {
        var slash = Instantiate(swordStrike, pl.transform);
        slash.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.rotation = Quaternion.Euler(37, 0, 0);
        slash.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        Destroy(slash, 1);
    }

    void bufOn() {
        speedAura.SetActive(true);
    }

    void bufOff() {
        speedAura.SetActive(false);
    }

    public void AtkEffect() {
        var slash = Instantiate(swordStrike, pl.transform);
        slash.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(1f * pl.facing, 1);
        slash.transform.rotation = Quaternion.Euler(1, (pl.facing == -1) ? 280 : 100, 175);

        Destroy(slash, 1);
    }

    public override void Attack()
    {
        routine = _attack();
        StartCoroutine(routine);
    }

    public override void Skill1()
    {
        if (inSuper) {
            return;
        }

        if (pl.shield > 0 && !skillCool.IsIn()) {
            foreach (Player p in targets) {
                p.vman_mark.SetActive(false);
            }

            targets.Clear();

            running = true;

            skillCool.Start();

            SoundManager.Instance.PlayToAll("vampire_man_run");
        }
    }

    public override void Skill1Up()
    {
        SkillEnd();
    }

    public override void OnRevive()
    {
        pl.shield = 100;
    }

    public override void OnHurt(ref int damage, Transform attacker, ref bool cancel)
    {
    }

    public override void OnForceCancel()
    {
        inSuper = false;

        if (state == "super") {
            CamManager.main.CloseOut(0.1f);

            state = "";

            running = false;

            targets.Clear();
        }
    }

    public override void OnDash() {
        if (running) {
            pl.rb.linearVelocity = Vector2.zero;
        }

        SkillEnd();
    }

    private void Update() {
        if (pl != null) {
            if (pl.pv.IsMine) {
                animator.SetBool("skillrun", running);
            }

            if (inSuper) {
                running = true;
            }

            if (running) {
                pl.CallChFunc("bufOn");
                atkCool = 0.2f;

                if (!inSuper) {
                    pl.shield -= 30 * Time.deltaTime;
                }

                pl.moveSpeed = pl.moveSpeedDef * 3f;

                tt = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(1.5f, 2.2f), 0, Vector2.right, 0f), pl);
                tm = Monster.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(1.5f, 2.2f), 0, Vector2.right, 0f));

                foreach (Player p in tt) {
                    p.vman_mark.SetActive(true);

                    if (!targets.Contains(p)) {
                        p.vman_mark.transform.rotation = Quaternion.Euler(0, 0, 0);
                        p.vman_mark.transform.DOLocalRotate(new Vector3(0, 0, 45), 0.3f).SetEase(Ease.OutCubic);

                        SoundManager.Instance.Play("vampire_man_mark");

                        targets.Add(p);
                    }
                }

                foreach (Monster p in tm) {
                    p.vman_mark.SetActive(true);

                    if (!targetMob.Contains(p)) {
                        p.vman_mark.transform.rotation = Quaternion.Euler(0, 0, 0);
                        p.vman_mark.transform.DOLocalRotate(new Vector3(0, 0, 45), 0.3f).SetEase(Ease.OutCubic);

                        SoundManager.Instance.Play("vampire_man_mark");

                        targetMob.Add(p);
                    }
                }

                if (pl.shield < 0) {
                    pl.shield = 0;
                    
                    SkillEnd();
                }
            } else {
                pl.CallChFunc("bufOff");
                pl.moveSpeed = pl.moveSpeedDef;
            }
        }
    }

    public override void OnJump(ref bool cancel)
    {
        if (inSuper) {
            cancel = true;
        }
    }

    void SkillEnd() {
        if (inSuper) {
            return;
        }

        if (!running) {
            return;
        }

        running = false;

        if (targets.Count > 0) {
            pl.RpcAnimateTrigger("attack1");
            pl.energy += 2;
            pl.shield += 2;
        }

        pl.CallChFunc("speedEnd");

        SoundManager.Instance.PlayToAll("vampire_man_stop");

        foreach (Player p in targets) {
            p.vman_mark.SetActive(false);

            float dist = Vector2.Distance(p.transform.position, pl.transform.position);

            if (dist > 10) dist = 10f;

            p.Damage(60 + (int)(60f * (dist / 10)), pl.name_);
            p.Knockback(Vector2.up * 2);

            SendCrit(p.transform.position);
        }

        foreach (Monster p in targetMob) {
            p.vman_mark.SetActive(false);

            float dist = Vector2.Distance(p.transform.position, pl.transform.position);

            if (dist > 10) dist = 10f;

            p.Damage(60 + (int)(60f * (dist / 10)), pl.name_);
            p.Knockback(Vector2.up * 2);

            SendCrit(p.transform.position);
        }

        targets.Clear();
        targetMob.Clear();
    }

    IEnumerator _attack() {
        if (atkCool > 0) {
            routine = null;

            yield break;
        }

        SoundManager.Instance.PlayToDist("vampire_man_atk", transform.position, 15);

        pl.RpcAnimateTrigger("attack2");

        pl.stopMove = 0.22f;
        atkCool = 0.2f;

        atkType = 0;

        pl.CallChFunc("atk");

        pl.rb.linearVelocity = new Vector2(4 * pl.facing, pl.rb.linearVelocity.y);

        yield return new WaitForSeconds(0.2f);
        pl.rb.linearVelocity = new Vector2(0, pl.rb.linearVelocity.y);
        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(4, 2), 0, Vector2.right * pl.facing, 2f), pl);
        var targetMob = Monster.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(4, 2), 0, Vector2.right * pl.facing, 2f));

        CamManager.main.Shake();

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            if (i == 0) {
                pl.energy += 2;
                pl.shield += 20;
            }

            target.Damage(25, pl.name_);
            target.Knockback(Vector2.right * pl.facing * 4 + Vector2.up * 2);
        }

        for (int i = 0; i < targetMob.Count; i++) {
            var target = targetMob[i];

            if (i == 0) {
                pl.shield += 20;
            }

            target.Damage(25, pl.name_);
            target.Knockback(Vector2.right * pl.facing * 4 + Vector2.up * 2);
        }

        routine = null;
    }

    public override void Skill2()
    {
        routine = super();
        StartCoroutine(routine);
    }

    IEnumerator super() {
        if (superCool.IsIn() || !EnergySystem.CheckNWarn(pl, 85)) {
            yield break;
        }

        state = "super";

        superCool.Start();

        pl.energy -= 80;
        pl.RpcAnimateTrigger("shield");

        pl.stopMove = 0.4f;

        CamManager.main.CloseUp(3.4f, 0, 0.1f);

        yield return new WaitForSeconds(0.4f);

        if (state != "super") {
            ForceCANCEL();
            yield break;
        }

        CamManager.main.CloseUp(4f, 0, 0.1f);

        inSuper = true;

        SoundManager.Instance.PlayToAll("vampire_man_run");

        targets.Clear();
        targetMob.Clear();

        Vector2 lastPos = pl.transform.position;

        for (int i = 0; i < 7; i++) {
            SendCrit(pl.transform.position + new Vector3(0, 1f));

            SendCrit(pl.transform.position + new Vector3(0.8f, 0.5f));
            SendCrit(pl.transform.position + new Vector3(-0.8f, 0.5f));

            CamManager.main.Shake(2);

            foreach (Player target in targets) {
                target.Damage(20, pl.name_);

                target.SetPos(lastPos);

                target.CallCancel();
            }

            foreach (Monster target in targetMob) {
                target.Damage(20, pl.name_);
            }

            if (tt.Count > 0) {
                pl.Heal(80);
                pl.shield += 100;
            }

            SoundManager.Instance.PlayToAll("vampire_man_super");

            lastPos = pl.transform.position;

            yield return new WaitForSeconds(0.3f);

            if (state != "super") {
                ForceCANCEL();
                yield break;
            }
        }

        CamManager.main.CloseOut(0.1f);

        yield return new WaitForSeconds(0.8f);

        inSuper = false;

        CamManager.main.Shake(10);

        SkillEnd();

        state = "";
    }
}
