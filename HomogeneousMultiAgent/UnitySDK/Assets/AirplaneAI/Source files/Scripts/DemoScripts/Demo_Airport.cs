using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo_Airport : MonoBehaviour {

    public List<Airliner> planes = new List<Airliner>();

    // Use this for initialization
    void Start () {
        foreach (object o in GameObject.FindObjectsOfType(typeof(Airliner))) {
            planes.Add((Airliner)o);
        }
        
    }
	
    private void OnGUI() {

        if (GUI.Button(new Rect(0, 0, 100, 25), "Speed 1x")) {
            Time.timeScale = 1;
        }
        if (GUI.Button(new Rect(100, 0, 100, 25), "Speed 5x")) {
            Time.timeScale = 5;
        }
        if (GUI.Button(new Rect(200, 0, 100, 25), "Speed 10x")) {
            Time.timeScale = 10;
        }
        if (GUI.Button(new Rect(300, 0, 100, 25), "Save"))
        {
            state.done = true;
        }


        foreach (Airliner p in planes) {
            if (p != null) {
                Vector2 pos = Camera.main.WorldToScreenPoint(p.transform.position);
                if (p.currentBaseState == PlaneBase.BaseState.Parked) {

                    if (GUI.Button(new Rect(pos.x, Screen.height - pos.y + 25, 60, 25), "Take off")) {
                        p.StartTakeoff();
                    }
                }
            }
        }
    }
}
