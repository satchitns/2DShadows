using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicArcLighting : DynamicLighting
{
	[Range(0, 1)]
	public float arcPercent = 0.2f;
	public float offsetValue = 0.001f;
	Vector3 unitVectorTop, unitVectorBottom;

	new public void Start()
	{
		base.Start();
		if (!doBorderChecking)
		{
			Debug.LogWarning("Border checking turned off on " + gameObject.name);
		}
	}

	public override void CreateBoundaryTrigger()
	{
		EdgeCollider2D triggerBoundary = gameObject.AddComponent<EdgeCollider2D>();
		triggerBoundary.isTrigger = true;
		Vector2[] array = new Vector2[borderPolySides + 1];
		array[0] = transform.right * offsetValue * -1;
		float subtendAmt = 360 * arcPercent;
		for (int i = 1; i < borderPolySides; i++)
		{
			array[i] = Vector3.Normalize(Quaternion.AngleAxis((i - borderPolySides / 2) * (subtendAmt) / borderPolySides, transform.forward) * (new Vector2(1, 0))) * raycastDistance;
		}
		array[borderPolySides] = array[0];
		unitVectorTop = Vector3.Normalize(array[1]);
		unitVectorBottom = Vector3.Normalize(array[borderPolySides - 1]);
		triggerBoundary.points = array;
	}

	public override void DetectColliderCorners()
	{
		base.DetectColliderCorners();
		boundPoints.RemoveAll(point => !pointIsInSubtendedAngle(point));
		staggeredBoundPoints.RemoveAll(point => !pointIsInSubtendedAngle(point));
		lightBoundary.RemoveAll(point => point == transform.position);
	}
	bool pointIsInSubtendedAngle(Vector3 point)
	{
		Vector2 normalVector = (Vector2)(point - (transform.position));
		normalVector.Normalize();
		normalVector = transform.InverseTransformDirection(normalVector);
		if (normalVector.x >= unitVectorTop.x && normalVector.x <= 1 && normalVector.y <= unitVectorBottom.y && normalVector.y >= unitVectorTop.y)
		{
			return true;
		}
		return false;
	}
}
