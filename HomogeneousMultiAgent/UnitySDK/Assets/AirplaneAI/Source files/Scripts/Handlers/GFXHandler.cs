using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GFXHandler : MonoBehaviour {
    public static GFXHandler instance;
    private void Awake() {
        instance = this;
    }
    public List<GameObject> gfxList = new List<GameObject>();


    public void CreateGFX(int id, Vector3 pos) {
        CreateGFX(id, pos, null);
    }
    public void CreateGFX(int id, Vector3 pos, Transform parent) {

        GameObject newgfx = GameObject.Instantiate(gfxList[id]);
        if (parent != null) {
            newgfx.transform.SetParent(parent);
            newgfx.transform.localPosition = Vector3.zero;
        }
        newgfx.transform.position = pos;
        Destroy(newgfx, 10f);
    }
}
