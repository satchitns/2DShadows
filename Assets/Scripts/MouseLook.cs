using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
	public float padFromCollider = 0.001f;
	public Transform head;
	float rootTwo;
	float collBounds;
	// Use this for initialization
	void Start()
	{
		rootTwo = Mathf.Sqrt(2);
		collBounds = GetComponentInParent<BoxCollider2D>().bounds.extents.x;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		Vector2 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 direction = mousePos - (Vector2)transform.parent.position;
		float rotAngle = Vector3.Angle(Vector3.right, direction.normalized);
		if (mousePos.y < transform.parent.position.y)
		{
			rotAngle = 360 - rotAngle;
		}
		transform.rotation = Quaternion.AngleAxis(rotAngle, Vector3.forward);
		head.rotation = transform.rotation;
		transform.localPosition = direction.normalized * (collBounds * rootTwo + padFromCollider); //placing it on a circle
																								   //whose radius is the diagonal of the square collider of our parent(i.e. the body collider of the character)
	}

}
