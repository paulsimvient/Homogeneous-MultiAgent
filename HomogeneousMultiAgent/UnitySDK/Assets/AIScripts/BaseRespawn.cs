using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRespawn : MonoBehaviour
{
    public GameObject blueTeam, redTeam;

    public void InitiateRespawn(GameObject newBase)
    {
        StartCoroutine(RespawnBase(newBase));
    }

    IEnumerator RespawnBase(GameObject Base)
    {
       // Debug.Log("dead base");
        Base.SetActive(false);
        SetGunsDestroyed(Base, true);
        yield return new WaitForSeconds(200);
        var randPos = new Vector3(Random.Range(-500, 2000), 30, Random.Range(-1000, 1000));
        Base.GetComponent<BodyIntegrity>().health = 5;
        Base.SetActive(true);
        Base.transform.position = randPos;
        SetGunsDestroyed(Base, false);
    }

    public void SetGunsDestroyed(GameObject Team, bool State)
    {
        if (Team.GetComponent<BodyIntegrity>()._team == 1) //blue gun
        {
            foreach (FighterPlaneAgent agent in redTeam.GetComponentsInChildren<FighterPlaneAgent>())
            {
                agent.gunsDestroyed = State;
            }
        }
        else if (Team.GetComponent<BodyIntegrity>()._team == 2) //red gun
        {
            foreach (FighterPlaneAgent agent in blueTeam.GetComponentsInChildren<FighterPlaneAgent>())
            {
                agent.gunsDestroyed = State;
            }
        }
    }
}