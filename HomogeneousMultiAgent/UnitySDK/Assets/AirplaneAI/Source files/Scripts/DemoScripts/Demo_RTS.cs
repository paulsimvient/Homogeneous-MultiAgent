using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo_RTS : MonoBehaviour {

    public RTSPlane selectedPlane;


    public Texture2D healthbar;
    public Texture2D flag;
    public Texture2D crosshair;

    public List<RTSTank> tanks = new List<RTSTank>();
    // Use this for initialization
    void Start() {
        foreach (object o in GameObject.FindObjectsOfType(typeof(RTSTank))) {
            tanks.Add((RTSTank)o);
        }

    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.gameObject.GetComponent<BodyIntegrity>()!=null) { // found a target, attack
                    selectedPlane.RTSAttack(hit.collider.GetComponent<BodyIntegrity>());
                } else {
                    selectedPlane.currentState = RTSPlane.State.Move;//clicked on terrain, move
                    selectedPlane.RTSMove(hit.point);
                }
            }
        }
    }

    private void OnGUI() {
        if (selectedPlane.currentState == RTSPlane.State.Move) {
            Vector2 pos = Camera.main.WorldToScreenPoint(selectedPlane.targetPos);
            GUI.DrawTexture(new Rect(pos.x, Screen.height - pos.y + 25, 16, 16), flag); //flag
        } else if(selectedPlane.currentState == RTSPlane.State.Attack && selectedPlane.target!=null){

            Vector2 pos = Camera.main.WorldToScreenPoint(selectedPlane.target.transform.position);
            GUI.DrawTexture(new Rect(pos.x, Screen.height - pos.y + 25, 16, 16), crosshair); // attacking crosshair
        }
        foreach(RTSTank tank in tanks){ // tank healthbars
            if (tank != null) {
                Vector2 pos = Camera.main.WorldToScreenPoint(tank.transform.position);
                GUI.color = Color.red;
                GUI.DrawTexture(new Rect(pos.x, Screen.height - pos.y-25, 20, 4), healthbar);
                GUI.color = Color.green;
                GUI.DrawTexture(new Rect(pos.x, Screen.height - pos.y - 25, tank.GetComponent<BodyIntegrity>().health*2, 4), healthbar);
                GUI.color = Color.white;
            }
        }
    }


}
