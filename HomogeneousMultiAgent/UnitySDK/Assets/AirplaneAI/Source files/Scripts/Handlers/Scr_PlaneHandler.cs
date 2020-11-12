using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Scr_PlaneHandler : MonoBehaviour {
    public static Scr_PlaneHandler instance;
    private void Awake() {
        instance = this;
    }
    public List<PlaneBase> planes = new List<PlaneBase>();

    public PlaneBase GetRandomOtherPlane(PlaneBase plane) {
        List<PlaneBase> newList = new List<PlaneBase>(planes);

        newList.Remove(plane);
        if (newList.Count == 0) {
            return null;
        }
        return newList[UnityEngine.Random.Range(0, newList.Count)];
    }
    public PlaneBase GetOtherClosestPlane(PlaneBase plane) {

        List<PlaneBase> newList = new List<PlaneBase>(planes);
        newList.Remove(plane);
        PlaneBase closest = newList.OrderBy(t => (t.transform.position - plane.transform.position).sqrMagnitude).FirstOrDefault(); 
        return closest;
    }
    public PlaneBase GetRandomEnemyPlane(PlaneBase plane) {
        List<PlaneBase> newList = new List<PlaneBase>();
        foreach (PlaneBase p in planes) {
            if (p.team != plane.team) {
                newList.Add(p);
            }
        }
        if (newList.Count == 0) {
            return null;
        }
        return newList[UnityEngine.Random.Range(0, newList.Count)];
    }
    // Update is called once per frame
    void Update () {
		
	}
}
