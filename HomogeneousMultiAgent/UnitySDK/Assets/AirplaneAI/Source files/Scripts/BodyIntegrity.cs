using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

public class BodyIntegrity : MonoBehaviour
{
    List<state> states = new List<state>();
    public int health = 10;
    public bool IsBase;
    public int _team; 
    public void TakeDamage(int damage)
    {
        if (health > 0)
        { 
           health -= damage;
            if (health <= 0)
            {
                //SendMessage("Death"); //call the Death function on the airplane script
                if (IsBase)
                {
                    Debug.Log("spawn!");
                    GetComponentInParent<BaseRespawn>().InitiateRespawn(this.gameObject);
                }
            }

          
        }

  
    }


 
}