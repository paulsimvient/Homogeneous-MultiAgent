using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;


//ID    Target=0 gun 1 fighter  Gun Destroyed   Vector (< 0 = -1 ) (< .7 = 1 ) (>.7 = 2 )    Shoot  Die
public class state
{
    public static bool done = false;

    public state()
    {
        time = 0.0f;
        target = 0;
        gun_active = 0;
        vector = -1;
        shoot = 0;
        die = 0;
 
    }

    void Save()
    {

    }

    public float time;
    public int target;
    public int gun_active;
    public float vector;
    public int shoot;
    public int die;
  

}

public class CsvReadWrite
{

    private List<string[]> rowData = new List<string[]>();
    static int num_file = 0;
    static int size = 6;


    public void Save(List<state> states)
    {

        // Crdieing First row of titles manually..
        string[] rowDataTemp = new string[size];

        rowDataTemp[0] = "Time";
        rowDataTemp[1] = "target 1:gun 2:fighter";
        rowDataTemp[2] = "gun_active";
        rowDataTemp[3] = "vector";
        rowDataTemp[4] = "shoot";
        rowDataTemp[5] = "die"; 


        rowData.Add(rowDataTemp);

        // You can add up the values in as many cells as you want.
        for (int i = 0; i < states.Count; i++)
        {
            state s = states[i];
            rowDataTemp = new string[size];
            rowDataTemp[0] = s.time.ToString();
            rowDataTemp[1] = s.target.ToString();
            rowDataTemp[2] = s.gun_active.ToString();
            rowDataTemp[3] = s.vector.ToString();
            rowDataTemp[4] = s.shoot.ToString();
            rowDataTemp[5] = s.die.ToString(); 

            rowData.Add(rowDataTemp);
        }

        string[][] output = new string[rowData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int indegun_destroyed = 0; indegun_destroyed < length; indegun_destroyed++)
            sb.AppendLine(string.Join(delimiter, output[indegun_destroyed]));


        string filePath = getPath();

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
    }

    // Following method is used to retrive the relative path as device platform
    private string getPath()
    {
        num_file++;
        return "Saved_flight_data" + num_file + ".csv";
    }
}


