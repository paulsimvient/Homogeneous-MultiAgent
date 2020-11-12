using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airliner : PlaneBase {


    public Scr_Airport airport;


    void Start() {
        base.Initialize();
        InvokeRepeating("Check", 1f, 1f);
        SetState(BaseState.Idle);
    }


    void Check() {
        if (currentBaseState == BaseState.Idle) { // when the basestate is set to idle, it means the plane has successfully taken off
            if (airport != null) {
                airport = Scr_AirportHandler.instance.GetRandomOtherNeutralOrFriendlyAirport(0, airport.transform.position); // find another airport
            } else {
                airport = Scr_AirportHandler.instance.GetClosestAirport(transform.position);
            }
            StartLanding(airport);// and land there
        }
    }

    void Update() {
        base.UpdateTick();
    }
}
