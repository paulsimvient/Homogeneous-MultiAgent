using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTile : MonoBehaviour
{
    public Vector3 initPos;
    public bool start = false;
    public int range = 10;

	void Start () {
        initPos = transform.localPosition;
        float xr = Random.Range(-range, range);
        float yr = Random.Range(-range, 0);
        float zr = Random.Range(-range, range);
        transform.localPosition = new Vector3(xr, yr, zr);
        start = true;
	}
	
    void Update () {
        if (start == true)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, initPos, Time.deltaTime * 5f);
            if (Mathf.Abs(transform.localPosition.magnitude - initPos.magnitude) < 0.0000001f)
            {
                start = false;
            }
        }
	}
}
