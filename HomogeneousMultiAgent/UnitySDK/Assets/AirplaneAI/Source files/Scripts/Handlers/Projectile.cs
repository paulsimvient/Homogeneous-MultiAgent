using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public GameObject ownerGameObject;
    public BodyIntegrity owner;
    public int team;
    public float speed;


    private void OnTriggerEnter(Collider collision) {

  

        if (collision.GetComponentInParent<BodyIntegrity>() != null && collision.GetComponentInParent<BodyIntegrity>().Equals(owner) == false) {
            if (collision.gameObject.GetComponentInParent<BodyIntegrity>().IsBase && collision.gameObject.GetComponentInParent<BodyIntegrity>()._team != team)
            {
                collision.GetComponentInParent<BodyIntegrity>().TakeDamage(1);// make the target take damage
                ownerGameObject.GetComponent<FighterPlaneAgent>().AddReward(5f);
            }
            //if (collision.gameObject.GetComponentInParent<PlaneBase>() == null || collision.gameObject.GetComponentInParent<PlaneBase>().team != team) {//prevent friendly fire
            else if (collision.gameObject.GetComponentInParent<FighterPlaneAgent>() == null || collision.gameObject.GetComponentInParent<FighterPlaneAgent>()._team != team || !collision.gameObject.GetComponentInParent<BodyIntegrity>().IsBase) {//prevent friendly fire

                collision.GetComponentInParent<BodyIntegrity>().TakeDamage(1);// make the target take damage
                //collision.GetComponentInParent<FighterPlaneAgent>().PrintHit();
                ownerGameObject.GetComponent<FighterPlaneAgent>().AddReward(2f);
                GFXHandler.instance.CreateGFX(0, transform.position);
                GameObject.Destroy(gameObject); // destroy the projectile
            }
        }
    }

    void Update () {

        transform.position += transform.forward * speed * Time.deltaTime;
	}
}
