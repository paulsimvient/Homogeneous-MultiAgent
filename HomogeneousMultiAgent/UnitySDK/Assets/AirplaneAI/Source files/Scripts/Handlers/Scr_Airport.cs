using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_Airport : MonoBehaviour {

    public int team;
    public Transform[] landingSpots;

    [System.Serializable]
    public class ParkingSpot {
        public PlaneBase plane;
        public Transform spot;
    }
    public List<ParkingSpot> parkingSpots = new List<ParkingSpot>();

    public ParkingSpot GetFreeSpot(PlaneBase p) {
        foreach (ParkingSpot s in parkingSpots) {
            if (s.plane == null) {
                s.plane = p;
                return s;
            }
        }
        return null;
    }

    void Start () {
        Scr_AirportHandler.instance.airports.Add(this);
	}
	

    public Vector3 GetLandingPosition() {
        return landingSpots[0].position;
    }

    public void ClearLandingSpot(PlaneBase testPlane) {
        foreach (ParkingSpot s in parkingSpots) {
            if(s.plane == testPlane) {
                s.plane = null;
            }
        }
    }
}
