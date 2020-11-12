using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo_SimpleDogFight : MonoBehaviour {

    public Transform cameraTarget;

    public bool active;
    public Text endText;
    public List<DogFighter> planes = new List<DogFighter>();
	// Use this for initialization
	void Start () {
        active = true;
        foreach(object o in GameObject.FindObjectsOfType(typeof(DogFighter))) {
            planes.Add((DogFighter)o);
        }
        InvokeRepeating("Check", 0, 2f);
    }
    void Check() {
        DogFighter targetPlane = planes[UnityEngine.Random.Range(0, planes.Count)];
        if(targetPlane!=null)
        cameraTarget = targetPlane.transform; // set camera target


        if (active) {
            int team1Count = 0;
            int team2Count = 0;


            foreach (DogFighter d in planes) {
                if (d != null && d.currentBaseState != PlaneBase.BaseState.Crashing) { // count alive planes
                    if (d.team == 1) {
                        team1Count++;
                    } else if (d.team == 2) {
                        team2Count++;
                    }

                }
            }
            if(team1Count == 0 && team2Count == 0) { // all planes are crashed
                endText.gameObject.SetActive(true);
                endText.color = Color.yellow;
                endText.text = "Draw!";
            } else if(team1Count == 0) { // all red planes are gone
                endText.gameObject.SetActive(true);
                endText.color = Color.blue;
                endText.text = "Blue team wins!";

            } else if (team2Count == 0) {// all blue planes are gone
                endText.gameObject.SetActive(true);
                endText.color = Color.red;
                endText.text = "Red team wins!";

            }
        }

    }

    void LateUpdate () {
        if (cameraTarget != null) {
            Camera.main.transform.position = cameraTarget.transform.position + new Vector3(50, 25, 0);
            Camera.main.transform.LookAt(cameraTarget);
        }
	}
}
