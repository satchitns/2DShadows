using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class DynamicLighting : MonoBehaviour
{
	protected List<Vector3> boundPoints; //stores the corner points of each collider
	protected List<Vector3> staggeredBoundPoints; //stores two extra points offset a bit from each collider-corner
	protected List<Vector3> lightBoundary; //stores the light's trigger collider's points
	protected List<Collider2D> colliders;
	protected GameObject meshObject;
	protected Mesh mesh;
	protected int borderCount;
	public bool doBorderChecking = true;
	public float staggerThreshold = 0.001f;
	public int borderPolySides = 8;
	public float raycastDistance = 3f;
	[Range(0.1f, 5.0f)]
	public float size = 1.0f;
	public Color color = Color.white;
	public void Start()
	{
		mesh = new Mesh();
		borderCount = 0;
		gameObject.layer = LayerMask.NameToLayer("Lights");
		colliders = new List<Collider2D>();
		CreateBoundaryTrigger();
		InitialColliderSweep();
		DetectColliderCorners();
		if (doBorderChecking)
			DetectBorderIntrusions();
		Rigidbody2D rgdBody = GetComponent<Rigidbody2D>();
		rgdBody.bodyType = RigidbodyType2D.Kinematic;
		rgdBody.simulated = true;
		rgdBody.useFullKinematicContacts = true;
		meshObject = new GameObject();
		meshObject.transform.position = new Vector3(meshObject.transform.position.x, meshObject.transform.position.y, transform.position.z);
		meshObject.AddComponent<MeshFilter>();
		MeshRenderer renderer = meshObject.AddComponent<MeshRenderer>();
		renderer.material = LightMaskClass.Instance.lightMaterial;
		renderer.material.SetFloat("_Offset", renderer.material.GetFloat("_Offset") / size);
		renderer.material.SetColor("_Color", color);
		DrawMesh();
	}

	public void Update()
	{
		//if any collider in this object's collider list has moved, update the whole mesh
		if (transform.hasChanged || colliders.Capacity > 0)
		{
			DetectColliderCorners();
			if (doBorderChecking)
				DetectBorderIntrusions();
			DrawMesh();
			transform.hasChanged = false;
		}
	}

	public virtual void DrawMesh()
	{
		List<Vector3> meshPoints = new List<Vector3>();
		mesh.Clear();
		int[] newTriangles;
		Vector2[] newUVs;
		int counter = 0;

		DetectCornerPoints(ref meshPoints);
		ClockwiseComparer cc = new ClockwiseComparer(transform.position);
		meshPoints.Sort(cc);    //sorting the points found clockwise

		meshPoints.Insert(0, transform.position); //inserting center point at zero index, this will be a vertex of every triangle

		newUVs = new Vector2[meshPoints.Count];
		CreateUVs(ref newUVs, meshPoints);

		//triangle vertices array has to be a multiple of 3, each set of 3 elements forms one triangle
		newTriangles = new int[(meshPoints.Count - 1) * 3];

		for (int i = 1; i < meshPoints.Count - 1; i++)
		{
			newTriangles[counter++] = 0;
			newTriangles[counter++] = i;
			newTriangles[counter++] = i + 1;
		}

		//forming a final triangle using the last vertex and the first
		newTriangles[counter++] = 0;
		newTriangles[counter++] = meshPoints.Count - 1;
		newTriangles[counter] = 1;

		mesh.vertices = meshPoints.ToArray();
		mesh.triangles = newTriangles;
		mesh.uv = newUVs;
		meshObject.GetComponent<MeshFilter>().mesh = mesh;
	}

	public virtual void DetectCornerPoints(ref List<Vector3> points)
	{
		//raycasting to collider corners
		foreach (Vector2 point in boundPoints)
		{
			RaycastHit2D hit;
			hit = Physics2D.Raycast(transform.position, (Vector3)point - transform.position, raycastDistance, LightMaskClass.Instance.mask);
			if (hit)
			{
				points.Add(hit.point);
			}
		}
		//raycasting to staggered collider corners
		foreach (Vector2 point in staggeredBoundPoints)
		{
			RaycastHit2D hit;
			hit = Physics2D.Raycast(transform.position, (Vector3)point - transform.position, raycastDistance, LightMaskClass.Instance.mask);

			if (hit)
			{
				points.Add(hit.point);
			}
			else
			{
				//if the staggered point didn't hit anything nice, let's see if it hits our own boundary
				RaycastHit2D[] hitArray;
				hitArray = Physics2D.RaycastAll(transform.position, (Vector3)point - transform.position, raycastDistance, ~LightMaskClass.Instance.mask);
				foreach (RaycastHit2D hitObj in hitArray)
				{
					if (hitObj.transform == transform)
					{
						points.Add(hitObj.point);
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
			RaycastHit2D hit;
			hit = Physics2D.Linecast(transform.position, (Vector3)point, LightMaskClass.Instance.mask);
			if (!hit)
			{
				RaycastHit2D[] hitArray;
				hitArray = Physics2D.LinecastAll(transform.position, (Vector3)point, ~LightMaskClass.Instance.mask);
				bool flag = false;
				foreach (RaycastHit2D hitObj in hitArray)
				{
					if (hitObj.transform == transform)
					{
						flag = true;
						points.Add(hitObj.point);
						break;
					}
				}
				if (flag == false)
					points.Add(point);
			}
			else
			{
				points.Add(hit.point);
			}
		}
	}

	public virtual void CreateUVs(ref Vector2[] uvs, List<Vector3> points)
	{
		for(int i = 0; i < uvs.Length; ++i)
		{
			uvs[i] = points[i] - transform.position;
		}
	}
	public void DetectBorderIntrusions()
	{
		if (borderPolySides <= 20)
		{
			if (borderCount > 0)
			{
				List<Vector3> tempArray = new List<Vector3>();
				for (int i = 0; i < lightBoundary.Count - 1; i++)
				{
					RaycastHit2D[] hitArray = Physics2D.LinecastAll(lightBoundary[i], lightBoundary[i + 1], LightMaskClass.Instance.mask);
					foreach (RaycastHit2D hit in hitArray)
					{

						tempArray.Add((Vector3)hit.point + (lightBoundary[i + 1] - lightBoundary[i]).normalized * staggerThreshold);
					}
				}
				for (int i = lightBoundary.Count - 1; i > 0; i--)
				{
					RaycastHit2D[] hitArray = Physics2D.LinecastAll(lightBoundary[i], lightBoundary[i - 1], LightMaskClass.Instance.mask);
					foreach (RaycastHit2D hit in hitArray)
					{
						if (!tempArray.Contains(hit.point))
							tempArray.Add((Vector3)hit.point + (lightBoundary[i - 1] - lightBoundary[i]).normalized * staggerThreshold);
					}
				}

				foreach (Vector3 point in tempArray)
				{
					lightBoundary.Add(point);
				}
			}
		}
		else
		{
			Debug.LogWarning("Too many border sides, not going to detect border intrusion - capped at 20");
		}
	}

	public virtual void DetectColliderCorners()
	{
		boundPoints = new List<Vector3>();
		staggeredBoundPoints = new List<Vector3>();
		lightBoundary = new List<Vector3>();
		foreach (Collider2D coll in colliders)
		{
			if (coll.GetType() == typeof(BoxCollider2D))
			{
				BoxCollider2D boxColl = (BoxCollider2D)coll;
				float top = boxColl.offset.y + (boxColl.size.y / 2f);
				float btm = boxColl.offset.y - (boxColl.size.y / 2f);
				float left = boxColl.offset.x - (boxColl.size.x / 2f);
				float right = boxColl.offset.x + (boxColl.size.x / 2f);
				Vector3 topLeft = boxColl.transform.TransformPoint(new Vector3(left, top, 0f));
				Vector3 topRight = boxColl.transform.TransformPoint(new Vector3(right, top, 0f));
				Vector3 bottomLeft = boxColl.transform.TransformPoint(new Vector3(left, btm, 0f));
				Vector3 bottomRight = boxColl.transform.TransformPoint(new Vector3(right, btm, 0f));
				boundPoints.Add(topLeft);
				boundPoints.Add(topRight);
				boundPoints.Add(bottomLeft);
				boundPoints.Add(bottomRight);
				//finding the cross product of each directional vector and the forward vector to get a perpendicular to the
				//directional on the 2D plane, used for offsetting
				staggeredBoundPoints.Add(topLeft + Vector3.Cross(transform.forward, (topLeft - transform.position).normalized) * staggerThreshold);
				staggeredBoundPoints.Add(topRight + Vector3.Cross(transform.forward, (topRight - transform.position).normalized) * staggerThreshold);
				staggeredBoundPoints.Add(bottomLeft + Vector3.Cross(transform.forward, (bottomLeft - transform.position).normalized) * staggerThreshold);
				staggeredBoundPoints.Add(bottomRight + Vector3.Cross(transform.forward, (bottomRight - transform.position).normalized) * staggerThreshold);
				staggeredBoundPoints.Add(topLeft - Vector3.Cross(transform.forward, (topLeft - transform.position).normalized) * staggerThreshold);
				staggeredBoundPoints.Add(topRight - Vector3.Cross(transform.forward, (topRight - transform.position).normalized) * staggerThreshold);
				staggeredBoundPoints.Add(bottomLeft - Vector3.Cross(transform.forward, (bottomLeft - transform.position).normalized) * staggerThreshold);
				staggeredBoundPoints.Add(bottomRight - Vector3.Cross(transform.forward, (bottomRight - transform.position).normalized) * staggerThreshold);
			}

			else if (coll.GetType() == typeof(EdgeCollider2D))
			{
				EdgeCollider2D edgeColl = (EdgeCollider2D)coll;
				foreach (Vector2 point in edgeColl.points)
				{
					Vector3 transformedPoint = edgeColl.transform.TransformPoint(point);
					boundPoints.Add(transformedPoint);
					staggeredBoundPoints.Add(transformedPoint - Vector3.Cross(transform.forward, (transformedPoint - transform.position).normalized) * staggerThreshold);
					staggeredBoundPoints.Add(transformedPoint + Vector3.Cross(transform.forward, (transformedPoint - transform.position).normalized) * staggerThreshold);
				}
			}

			else if (coll.GetType() == typeof(PolygonCollider2D))
			{
				PolygonCollider2D polyColl = (PolygonCollider2D)coll;
				foreach (Vector2 point in polyColl.points)
				{
					Vector3 transformedPoint = polyColl.transform.TransformPoint(point);
					boundPoints.Add(transformedPoint);
					staggeredBoundPoints.Add(transformedPoint - Vector3.Cross(transform.forward, (transformedPoint - transform.position).normalized) * staggerThreshold);
					staggeredBoundPoints.Add(transformedPoint + Vector3.Cross(transform.forward, (transformedPoint - transform.position).normalized) * staggerThreshold);
				}
			}

			//TO ADD : Circle collider if we need one

		}

		//adding the border constraints of this light
		foreach (Vector2 point in GetComponent<EdgeCollider2D>().points)
		{
			lightBoundary.Add(transform.TransformPoint(point));
		}
	}
	public virtual void CreateBoundaryTrigger()
	{

		EdgeCollider2D triggerBoundary = gameObject.AddComponent<EdgeCollider2D>();
		triggerBoundary.isTrigger = true;
		Vector2[] array = new Vector2[borderPolySides + 1];
		for (int i = 0; i < borderPolySides; i++)
		{
			array[i] = Vector3.Normalize(Quaternion.AngleAxis(i * (360) / borderPolySides, transform.forward) * (new Vector2(1, 0))) * raycastDistance;
		}
		array[borderPolySides] = array[0]; //last point has to coincide with first for closed shape
		triggerBoundary.points = array;
	}

	public void InitialColliderSweep()
	{
		foreach (Collider2D coll in Physics2D.OverlapCircleAll(transform.position, raycastDistance, LightMaskClass.Instance.mask))
		{
			colliders.Add(coll);
		}
	}

	public void OnTriggerEnter2D(Collider2D coll)
	{

		if (CheckMask(coll.gameObject.layer))
		{
			if (!colliders.Contains(coll))
			{
				colliders.Add(coll);
			}
			borderCount++;
		}
	}

	public void OnTriggerExit2D(Collider2D coll)
	{
		if (colliders.Contains(coll))
		{
			borderCount--;
		}
		if (System.Array.IndexOf(Physics2D.OverlapCircleAll(transform.position, raycastDistance, LightMaskClass.Instance.mask), coll) == -1)
		{
			if (colliders.Contains(coll))
			{
				colliders.Remove(coll);
				DetectColliderCorners();
				if (doBorderChecking)
					DetectBorderIntrusions();
				DrawMesh();
			}
		}
	}
	public bool CheckMask(int n)
	{
		return ((1 << n) & LightMaskClass.Instance.mask) > 0;
	}
}