using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHandler : MonoBehaviour {
    public static ProjectileHandler instance;
    private void Awake() {
        instance = this;
    }
    public GameObject bulletProjectile;

	public void CreateBulletProjectile (Vector3 pos, Vector3 targetPos, BodyIntegrity owner, GameObject ownerGameObject, int team){

        Projectile projectile = GameObject.Instantiate(bulletProjectile).GetComponent<Projectile>();
        projectile.owner = owner;// not to hit self
        projectile.ownerGameObject = ownerGameObject;
        projectile.team = team; // not to hit friendly aircrafts

        projectile.transform.position = pos;
        projectile.transform.LookAt(targetPos);

        float inaccuracy = 1.5f; //slight randomization to bullets, set to 0 for 100% accuracy
        projectile.transform.rotation = Quaternion.Euler(projectile.transform.eulerAngles.x + UnityEngine.Random.Range(-inaccuracy, inaccuracy), projectile.transform.eulerAngles.y + UnityEngine.Random.Range(-inaccuracy, inaccuracy), projectile.transform.eulerAngles.z + UnityEngine.Random.Range(-inaccuracy, inaccuracy));
        GameObject.Destroy(projectile.gameObject, 2f);
    }
}
