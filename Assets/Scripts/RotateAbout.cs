using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAbout : MonoBehaviour
{
	public int clockwise = 1;
	public float vel = 10;
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		transform.Rotate(Vector3.forward * Time.deltaTime * vel * clockwise);
	}
}
