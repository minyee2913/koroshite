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

    Cooldown superKickCool = new(0.5f);
    Cooldown superCool = new(1f);

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
        slash.transform.position = pl.transform.position + new Vector3(-2f * pl.facing, -1);
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

        public override void OnHurt(ref int damage, Player attacker, ref bool cancel)
    {
        if (shielding) {
            damage -= (int)Mathf.Round(damage * 0.4f);
            shieldTime += 0.2f;
        }
    }

    IEnumerator kick() {
        pl.RpcAnimateTrigger("attack3");

        yield return new WaitForSeconds(0.2f);

        CamManager.main.CloseUp(5.8f - 2.2f * shieldTime, 10 * shieldTime * pl.facing, 0.01f);
        CamManager.main.Shake(4 * shieldTime);

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(3.6f, 3), 0, Vector2.right * pl.facing, 1.2f), pl);

        if (shieldTime >= 0.5f) pl.CallChFunc("sk");

        if (targets.Count > 0) {
            pl.Heal(30);
            pl.energy += (int)(10 * shieldTime);
        }

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            target.ch.CANCEL();

            target.Damage((int)(200 * shieldTime), pl.name_);
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

        pl.RpcAnimateTrigger("attack3");
        pl.stopMove = 0.3f;

        yield return new WaitForSeconds(0.2f);

        CamManager.main.CloseUp(5.8f - 2.2f, 10 * pl.facing, 0.01f);
        CamManager.main.Shake(4);

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(4.2f, 4), 0, Vector2.right * pl.facing, 1.5f), pl);

        pl.CallChFunc("sk");
        pl.CallChFunc("sk2");

        if (targets.Count > 0) pl.Heal(15);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            target.ch.CANCEL();
            target.ch.atkCool = 0.8f;

            target.Damage(160, pl.name_);
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
                    stack.value = superCount / 6f;

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

        pl.Heal(10);

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            if (i == 0) {
                pl.energy += 5;
            }

            target.Damage(80, pl.name_);
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

            target.Damage(210, pl.name_);
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
        if (superCool.IsIn() || !EnergySystem.CheckNWarn(pl, 60)) {
            yield break;
        }

        pl.energy -= 60;

        superCool.Start();
        CamManager.main.CloseUp(3.4f, 0, 0.1f);

        pl.RpcAnimateTrigger("charge");
        pl.CallChFunc("aura1");

        yield return new WaitForSeconds(0.8f);

        pl.CallChFunc("sp");

        CamManager.main.CloseOut(0.1f);

        inSuper = true;
        superCount = 6;
    }
}
