using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class csDemoScene : MonoBehaviour {
	
	public Text txt;
	public string[] st_ModelName;
	public Transform[] obj_Model;
	public Transform MakePoint;
	Transform MakedObject;

	int i;

	void Start()
	{
		i = 1;
		MakeModel();
	}

	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.X))
		{
			if((i-1) <= obj_Model.Length-2)
				i++;
			else
				i=1;
			MakeModel();
		}
		else if(Input.GetKeyDown(KeyCode.Z))
		{
			if((i-1) > 0)
				i--;
			else
				i = obj_Model.Length;
			MakeModel();
		}
		else if(Input.GetKeyDown(KeyCode.C))
			MakeModel();

		
	}

	void MakeModel()
	{
		if(MakedObject)
			Destroy(MakedObject.gameObject);
		
		MakedObject = Instantiate(obj_Model[i-1], MakePoint.transform.position, MakePoint.transform.rotation) as Transform;
		txt.text = i + " : " + st_ModelName[i-1];
	}
}
