using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSPlane : PlaneBase {
    public enum State {
        Move,
        Attack
    }
    [HideInInspector]
    public State currentState;

    public float attackDistance = 50;

    [HideInInspector]
    public BodyIntegrity target;

    public float attackTime;
    private float attackTimer;

    void Start () {
        base.Initialize();
        transform.position = new Vector3(transform.position.x, 30, transform.position.z);
        SetTargetPosition(transform.position);
        currentMovementSpeed = movementSpeed;
    }


    public void RTSAttack(BodyIntegrity t) {
        if (t != null) {
            Debug.Log("found2 "+t.name);
            currentState = State.Attack;
            SetState(BaseState.Moving);
            target = t;
        } else { Debug.Log("nulL"); }
    }
    public void RTSMove(Vector3 pos) {
        currentState = State.Move;
        SetState(BaseState.Moving);
        SetTargetPosition(pos);
    }
    void Update () {
        base.UpdateTick();
        if (currentState == State.Move) {
            gettingDistanceFromTarget = false; // we don't need this for the RTS plane
        } else if(currentState == State.Attack){ // attacking
            if (currentBaseState == BaseState.Moving) {
                if (target != null) {

                    SetTargetPosition(target.transform.position); // move towards target

                    if (attackTimer <= 0) {
                        if (Vector3.Angle(transform.forward, target.transform.position - transform.position) < 45) { // shoot if we're facing target
                            if (gettingDistanceFromTarget == false) {
                                if (Vector3.Distance(transform.position, target.transform.position) < attackDistance) {
                                    //ProjectileHandler.instance.CreateBulletProjectile(transform.position, target.transform.position, bodyIntegrity, team);
                                    attackTimer = attackTime;
                                }
                            }
                        }
                    } else {
                        attackTimer -= 1 * Time.deltaTime;
                    }
                } else {
                    RTSMove(transform.position);
                }
            }

            if (gettingDistanceTimer > 2) gettingDistanceTimer = 2; // RTS plane turns really fast so we don't need much time for reapproach
        }
        targetPos = new Vector3(targetPos.x, 30, targetPos.z); // limit height to 30 for RTS
        transform.position = new Vector3(transform.position.x, 30, transform.position.z);
    }
}
