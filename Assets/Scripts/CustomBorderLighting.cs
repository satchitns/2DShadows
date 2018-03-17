using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
public class CustomBorderLighting : DynamicLighting
{
	Vector2[] array;
	EdgeCollider2D edgeColl;
	new public void Start()
	{
		edgeColl = GetComponent<EdgeCollider2D>();
		edgeColl.isTrigger = true;
		raycastDistance = Vector3.Magnitude(edgeColl.bounds.size);
		base.Start();
		if (!doBorderChecking)
		{
			Debug.LogWarning("Border checking turned off on " + gameObject.name);
		}
	}
	public override void CreateBoundaryTrigger()
	{
		array = new Vector2[borderPolySides];
		for (int i = 0; i < borderPolySides; i++)
		{
			array[i] = Vector3.Normalize(Quaternion.AngleAxis(i * (360) / borderPolySides, transform.forward) * (new Vector2(1, 0))) * raycastDistance;
		}
	}

	public override void DetectColliderCorners()
	{
		base.DetectColliderCorners();
		boundPoints.RemoveAll(point => !edgeColl.bounds.Contains((Vector2)point));
		staggeredBoundPoints.RemoveAll(point => !edgeColl.bounds.Contains((Vector2)point));
		foreach (Vector2 point in array)
		{
			lightBoundary.Add(transform.TransformPoint(point));
		}
	}
	public override void DetectCornerPoints(ref List<Vector3> points)
	{
		//raycasting to collider corners
		foreach (Vector2 point in boundPoints)
		{
			RaycastHit2D[] hitArr;
			hitArr = Physics2D.RaycastAll(transform.position, (Vector3)point - transform.position, raycastDistance);
			for (int i = 0; i < hitArr.Length; i++)
			{
				if (hitArr[i].transform == transform)
					break;
				if (CheckMask(hitArr[i].transform.gameObject.layer))
				{
					points.Add(hitArr[i].point);

					break;
				}
			}
		}
		//raycasting to staggered collider corners
		foreach (Vector2 point in staggeredBoundPoints)
		{
			RaycastHit2D[] hitArr;
			hitArr = Physics2D.RaycastAll(transform.position, (Vector3)point - transform.position, raycastDistance);
			bool flag = false;
			for (int i = 0; i < hitArr.Length; i++)
			{
				if (hitArr[i].transform == transform)
					break;
				if (CheckMask(hitArr[i].transform.gameObject.layer))
				{
					points.Add(hitArr[i].point);

					flag = true;
					break;
				}
			}

			if (!flag)
			{
				for (int i = 0; i < hitArr.Length; i++)
				{
					if (hitArr[i].transform == transform)
					{
						points.Add(hitArr[i].point);

						break;
					}
				}
			}
		}
		//linecasting to light border constraints (this is so that two lights can tread each other's boundaries -
		//a raycast would hit either both the triggers or neither of the triggers depending on the mask value,
		//which would make it useless either way
		foreach (Vector2 point in lightBoundary)
		{
			RaycastHit2D[] hitArr;
			hitArr = Physics2D.RaycastAll(transform.position, (Vector3)point - transform.position, raycastDistance);
			bool flag = false;
			int i;
			for (i = 0; i < hitArr.Length; i++)
			{
				if (hitArr[i].transform == transform)
				{
					flag = true;
					break;
				}
				if (CheckMask(hitArr[i].transform.gameObject.layer))
				{
					points.Add(hitArr[i].point);
					break;
				}
			}
			if (flag)
			{
				points.Add(hitArr[i].point);
			}
		}
	}
	public override void CreateUVs(ref Vector2[] uvs, List<Vector3> points)
	{
		for (int i = 0; i < uvs.Length; ++i)
		{
			uvs[i] = points[i] - edgeColl.bounds.center;
		}
	}
}