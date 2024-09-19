using System.Collections;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.UI;

public class Fighter : Character
{
    bool shielding;
    bool inSuper;
    int superCount = 0;
    float shieldTime = 0;
    float time;
    [SerializeField]
    Slider stack;

    [SerializeField]
    GameObject impact;
    [SerializeField]
    GameObject impact2;
    [SerializeField]
    GameObject strike;
    [SerializeField]
    GameObject strike2;
    [SerializeField]
    ParticleSystem spike;
    [SerializeField]
    GameObject buffAura;
    [SerializeField]
    GameObject lightningAura;
    [SerializeField]
    Color skillCol;
    [SerializeField]
    Color superCol;
    [SerializeField]
    Image fill;

    Cooldown superKickCool = new(0.3f);
    Cooldown superCool = new(1f);

    public override int maxHealth => 1000;

    public override string atkInfo => "전방으로 빠르게 이동하면서 적에게 <color=\"red\">10</color>의 피해를 입힙니다.";

    public override string atk2Info => "X";

    public override string skill1Info => "홀드시 자세를 잡습니다. 자세를 잡은 상태에서는 입는 피해가 <color=\"lightblue\">40%</color> 감소하며 발차기 게이지를 계속 충전합니다.\n\n홀드를 멈추거나 발차기 게이지가 가득차면 축적된 발차기 게이지에 따라서 적에게 최대 <color=\"red\">120</color>의 피해를 입히고 HP를 최대 30 회복하며 밀쳐냅니다.";

    public override string skill2Info => "<color=\"yellow\">특수 스택</color>을 얻습니다. 발차기를 가할 때마다 <color=\"yellow\">특수 스택</color>을 소모하여 최대 게이지의 발차기를 즉시 발동합니다. <color=\"yellow\">특수 스택</color>으로 발동한 발차기는 입히는 피해가 <color=\"red\">150</color>, 회복하는 HP가 15로 제한됩니다. <color=\"yellow\">특수 스택</color>을 모두 소모하면 오의 상태가 종료됩니다.";

    public override void Callfunc(string method)
    {
        if (method == "atk1") {
            Atk1Effect();
        } else if (method == "atk2") {
            Atk2Effect();
        } else if (method == "sk") {
            SkEffect();
        } else if (method == "sk2") {
            Sk2Effect();
        } else if (method == "JA") {
            JAEffect();
        } else if (method == "aura1") {
            AuraEffect();
        } else if (method == "b1") {
            bufOn();
        } else if (method == "b0") {
            bufOff();
        } else if (method == "sp") {
            SpEffect();
        }
    }

    void Atk1Effect() {
        var par = Instantiate(impact, pl.transform);
        par.AddComponent<SetLayer>().sortingLayer = "particle";
        par.transform.position = pl.transform.position + new Vector3(0.5f * pl.facing, 0.9f);
        par.transform.localScale = new Vector3(0.8f, 0.8f);

        Destroy(par, 1);
    }
    void Atk2Effect() {
        var par = Instantiate(impact2, pl.transform);
        par.AddComponent<SetLayer>().sortingLayer = "particle";
        par.transform.position = pl.transform.position + new Vector3(0.6f * pl.facing, 1.1f);
        par.transform.localScale = new Vector3(0.8f, 0.8f);

        Destroy(par, 1);
    }

    public void SkEffect() {
        var slash = Instantiate(strike, pl.transform);
        slash.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(2f * pl.facing, 1);
        slash.transform.rotation = Quaternion.Euler((pl.facing == -1) ? 210 : -30, 90, 90);

        Destroy(slash, 1);
    }

    public void Sk2Effect() {
        var slash = Instantiate(strike2, pl.transform);
        slash.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(-1f * pl.facing, 0);
        slash.transform.rotation = Quaternion.Euler((pl.facing == -1) ? 210 : -30, 90, 90);

        Destroy(slash, 1);
    }

    public void JAEffect() {
        var slash = Instantiate(spike);
        var main = slash.main;
        main.startColor = new Color(255, 174, 105);
        slash.gameObject.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(0, -1);

        Destroy(slash.gameObject, 2);
    }

    public void AuraEffect() {
        var slash = Instantiate(lightningAura, pl.transform);
        slash.gameObject.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(0, 0);

        Destroy(slash.gameObject, 1);
    }

    public void SpEffect() {
        var slash = Instantiate(strike, pl.transform);
        slash.AddComponent<SetLayer>().sortingLayer = "particle";
        slash.transform.position = pl.transform.position + new Vector3(0, 3);
        slash.transform.rotation = Quaternion.Euler(90, 90, 90);
        slash.transform.localScale = new Vector3(3, 0.5f, 0.5f);

        Destroy(slash, 1);
    }

    public void bufOn() {
        buffAura.SetActive(true);
    }
    public void bufOff() {
        buffAura.SetActive(false);
    }

    public override void Attack()
    {
        routine = _attack();
        StartCoroutine(routine);
    }

    public override void Skill1()
    {
        if (inSuper) {
            routine = kick2();
            StartCoroutine(routine);

            return;
        }

        shielding = true;
        CamManager.main.CloseUp(5.2f, 0, 0.1f);

        shieldTime = 0;
    }

    public override void Skill1Up()
    {
        if (shielding) {
            shielding = false;

            routine = kick();
            StartCoroutine(routine);
        }
    }

        public override void OnHurt(ref int damage, Transform attacker, ref bool cancel)
    {
        if (shielding) {
            damage -= (int)Mathf.Round(damage * 0.4f);
            shieldTime += 0.2f;
        }
    }

    public override void OnCancel()
    {
        if (state != "super") {
            CamManager.main.CloseOut(0.1f);
        }
    }

    IEnumerator kick() {
        if (shieldTime <= 0.5f) yield break;
        pl.RpcAnimateTrigger("attack3");

        SoundManager.Instance.PlayToDist("fighter_kick", transform.position, 15);

        yield return new WaitForSeconds(0.2f);

        CamManager.main.CloseUp(5.8f - 2.2f * shieldTime, 10 * shieldTime * pl.facing, 0.01f);
        CamManager.main.Shake(4 * shieldTime);

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(3.6f, 3), 0, Vector2.right * pl.facing, 1.2f), pl);

        if (shieldTime >= 0.5f) pl.CallChFunc("sk");

        if (targets.Count > 0) {
            pl.Heal((int)(30 * shieldTime));
            pl.energy += (int)(10 * shieldTime);
        }

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            target.ch.CANCEL();

            target.Damage((int)(180 * shieldTime), pl.name_);
            target.Knockback(Vector2.right * pl.facing * 16 * shieldTime + Vector2.up * 4);
        }

        yield return new WaitForSeconds(0.3f);

        CamManager.main.CloseOut(0.1f);
    }

    IEnumerator kick2() {
        if (superKickCool.IsIn()) {
            yield break;
        }
        superKickCool.Start();

        SoundManager.Instance.PlayToDist("fighter_kick2", transform.position, 15);

        pl.RpcAnimateTrigger("attack3");
        pl.stopMove = 0.3f;

        yield return new WaitForSeconds(0.2f);

        CamManager.main.CloseUp(5.8f - 2.2f, 10 * pl.facing, 0.01f);
        CamManager.main.Shake(4);

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(4.2f, 4), 0, Vector2.right * pl.facing, 1.5f), pl);

        pl.CallChFunc("sk");
        pl.CallChFunc("sk2");

        if (targets.Count > 0) pl.Heal(60);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            target.ch.CANCEL();
            target.ch.atkCool = 0.8f;

            target.Damage(150, pl.name_);
            target.Knockback(Vector2.right * pl.facing * 4 + Vector2.up * 4);
        }

        superCount--;

        yield return new WaitForSeconds(0.3f);

        CamManager.main.CloseOut(0.1f);
    }

    private void Update() {
        if (pl != null) {
            if (pl.pv.IsMine) {
                animator.SetBool("shielding", shielding);

                stack.gameObject.SetActive(shielding || inSuper);

                if (inSuper) {
                    pl.CallChFunc("b1");
                    fill.color = superCol;
                    stack.value = superCount / 8f;

                    if (superCount <= 0) {
                        inSuper = false;
                    }
                } else {
                    pl.CallChFunc("b0");
                    fill.color = skillCol;
                }
            }

            if (shielding) {
                pl.stopMove = 0.2f;
                atkCool = 0.2f;

                shieldTime += Time.deltaTime;

                stack.value = shieldTime;

                if (shieldTime > 1f) {
                    Skill1Up();
                }
            }
        }
    }

    IEnumerator _attack() {
        if (atkCool > 0) {
            routine = null;

            yield break;
        }

        SoundManager.Instance.PlayToDist("fighter_atk", transform.position, 15);

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

            if (i == 0) {
                pl.energy += 8;
            }

            target.Damage(10, pl.name_);
            target.Knockback(Vector2.right * pl.facing * 10 + Vector2.up * 2);
        }

        routine = null;
    }

    public override void Skill2()
    {
        routine = super();
        StartCoroutine(routine);
    }

    public override void OnForceCancel()
    {
        if (inSuper || state == "super") {
            CamManager.main.CloseOut(0.1f);

            inSuper = false;
            superCount = 0;
        }
    }

    IEnumerator super() {
        if (superCool.IsIn() || !EnergySystem.CheckNWarn(pl, 80)) {
            yield break;
        }

        state = "super";

        pl.energy -= 80;

        superCool.Start();
        CamManager.main.CloseUp(3.4f, 0, 0.1f);

        pl.RpcAnimateTrigger("charge");
        pl.CallChFunc("aura1");

        yield return new WaitForSeconds(0.8f);

        pl.CallChFunc("sp");

        SoundManager.Instance.PlayToDist("fighter_super", transform.position, 15);

        CamManager.main.CloseOut(0.1f);

        inSuper = true;
        superCount = 8;

        state = null;
    }
}
