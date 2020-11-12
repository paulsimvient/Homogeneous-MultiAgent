using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogFighter : PlaneBase {
    private PlaneBase target;

    public float attackDistance = 50;
    public float attackTime=0.1f; // time between each bullet fired

    private float attackTimer;
    private float cantReachTargetTimer; // add some randomization to movement if we're chasing the target too long without reaching it


	void Start () {
        base.Initialize();
        InvokeRepeating("CheckTarget", 0, 0.5f);
    }

    void CheckTarget() {
        if (target == null || target.currentBaseState == BaseState.Crashing) { // select new target if our current target doesn't exist or is already going down
            NextTarget();
        }
    }
    void NextTarget() {
        if (currentBaseState == BaseState.Moving || currentBaseState == BaseState.Idle) {
            target = Scr_PlaneHandler.instance.GetRandomEnemyPlane(this);
            if(target!=null && target.currentBaseState != BaseState.Crashing && target.currentBaseState!= BaseState.Landed) {
                SetState(BaseState.Moving);
            } else {
                target = null;
            }
        }
    }

    void Update() {
        base.UpdateTick();
        if (currentBaseState == BaseState.Moving) {
            if (target != null) {
                cantReachTargetTimer -= 1 * Time.deltaTime;
                if (cantReachTargetTimer <= 0) { // spent 15 seconds chasing the same target without being able to fire a single bullet, so let's drop it
                    cantReachTargetTimer = 15f;
                    target = null;
                    SetState(BaseState.Idle);
                    return;
                }

                SetTargetPosition(target.transform.position + target.transform.forward * 20);

                if (attackTimer <= 0) {
                    if (Vector3.Angle(transform.forward, target.transform.position - transform.position) < 20) { // check that we're facing the target
                        if (Vector3.Distance(transform.position, target.transform.position) < attackDistance) { // and we're close enough
                            //ProjectileHandler.instance.CreateBulletProjectile(transform.position, transform.position + transform.forward, bodyIntegrity, team);
                            attackTimer = attackTime;
                            cantReachTargetTimer = 15; // we are in range to fire so reset this timer
                        }
                    }
                } else {
                    attackTimer -= 1 * Time.deltaTime;
                }
            }
        }
    }
}
