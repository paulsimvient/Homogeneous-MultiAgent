using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_AirportHandler : MonoBehaviour {
    public static Scr_AirportHandler instance;
    public List<Scr_Airport> airports = new List<Scr_Airport>();


    private void Awake() {
        instance = this;
    }

    public Scr_Airport GetClosestAirport(Vector3 pos) {
        Scr_Airport currentClosest = null;
        float currentDistance = -1;
        foreach (Scr_Airport a in airports) {
            float dist = Vector3.Distance(a.transform.position, pos);
            if (currentDistance == -1 || dist < currentDistance) {
                currentClosest = a;
                currentDistance = dist;
            }
        }
        return currentClosest;
    }

    public Scr_Airport GetClosestNeutralOrFriendlyAirport(Vector3 pos, int team) {
        Scr_Airport currentClosest = null;
        float currentDistance = -1;
        foreach (Scr_Airport a in airports) {
            if (a.team == 0 || a.team == team) { // neutral or same team
                float dist = Vector3.Distance(a.transform.position, pos);
                if (currentDistance == -1 || dist < currentDistance) {
                    currentClosest = a;
                    currentDistance = dist;
                }
            }
        }
        return currentClosest;
    }

    public Scr_Airport GetRandomOtherNeutralOrFriendlyAirport(int team, Vector3 currentHomeAirportPos) {
        List<Scr_Airport> ports = new List<Scr_Airport>();

        foreach(Scr_Airport a in airports) {
            if (a.team == team && a.transform.position!=currentHomeAirportPos) {
                ports.Add(a);
            }
        }
        return ports[UnityEngine.Random.Range(0, ports.Count)];
    }
}
