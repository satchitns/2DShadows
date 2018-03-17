using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUp : MonoBehaviour
{

	// Use this for initialization
	public float maxUp = 20;
	public float speed = 20;
	Vector3 dest;
	bool up;
	void Start()
	{
		up = true;
		dest = transform.position + Vector3.up * maxUp;
	}

	// Update is called once per frame
	void Update()
	{
		transform.position = transform.position + (Vector3)(Vector2.up * Time.deltaTime * speed  * (up ? 1 : -1));
		if (up && transform.position.y >= dest.y)
		{
				up = false;
				dest = transform.position - Vector3.up * maxUp;
		}
		else if(!up && transform.position.y <= dest.y)
		{
			up = true;
			dest = transform.position + Vector3.up * maxUp;
		}
	}
}
