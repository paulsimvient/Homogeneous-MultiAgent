using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;


/*
  public float time;
    public int target;
    public int gun_destroyed;
    public int vector;
    public int shoot;
    public int die;
 **/

public class DataCollection : MonoBehaviour
{
    List<state> states = new List<state>();

  
    bool bSaved = false;

    public void Start()
    {
        print(name);
    }
    public void Update()
    {
        SaveState();
    }

    public void SaveState()
    {
 
        state st = new state(); 
  
        st.shoot = System.Convert.ToInt32(0);

        states.Add(st);

        if (state.done == true && bSaved == false)
        { 
            CsvReadWrite writer = new CsvReadWrite();
            writer.Save(states);
            states.Clear();
            bSaved = true;

        }
    }


}