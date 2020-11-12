using UnityEngine;

public class FPS: MonoBehaviour {
    float deltaTime;

    void Update () {
        deltaTime += (Time.deltaTime - deltaTime) / 10;
    }

    void OnGUI () {
        Rect r = new Rect (Screen.width - 50, Screen.height - 25, 50, 50);
        GUI.Label (r, Mathf.Round (1f / deltaTime) + "fps");
    }
}
