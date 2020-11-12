using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSTank : MonoBehaviour {

    void Death() { // called from attached BodyIntegrity script
        GFXHandler.instance.CreateGFX(1, transform.position);
        GameObject.Destroy(gameObject);
    }
	
}
