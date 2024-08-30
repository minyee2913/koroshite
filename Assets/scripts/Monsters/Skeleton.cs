using UnityEngine;

public class Skeleton : Monster
{
    public override float stateY => 0.16f;

    public override string Name => "skeleton";

    public override void MobUpdate()
    {
        var targets = Search(8);

        if (action == "idle") {
            if (targets.Count > 0) {
                action = "chase";

                target = targets[0];
            }
        } else if (action == "chase") {
            if (targets.Count <= 0) {
                action = "idle";
            } else {

                if (range > 2) {
                    Chase(target);
                } else {
                    isMoving = false;

                    if (atkCool <= 0) {
                        Attack();

                        atkCool = 1;
                    }
                }
            }
        }
    }

    void Attack() {
        RpcAnimateTrigger("attack1");

        SoundManager.Instance.PlayToDist("default_attack", transform.position, 8);

        var targets = Player.Convert(Physics2D.BoxCastAll(transform.position + new Vector3(0, 0.5f), new Vector2(2.5f, 2), 0, Vector2.right * facing, 0.9f), null);

        

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];

            target.DamageByMob(100, uniqueId);
            target.Knockback(Vector2.right * facing * 4 + Vector2.up * 2);
        }
    }
}