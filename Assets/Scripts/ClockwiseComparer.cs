using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     ClockwiseComparer provides functionality for sorting a collection of Vector3s such
///     that they are ordered clockwise about a given origin.
/// </summary>
public class ClockwiseComparer : IComparer<Vector3>
{
	private Vector3 m_Origin;

	#region Properties

	/// <summary>
	///     Gets or sets the origin.
	/// </summary>
	/// <value>The origin.</value>
	public Vector3 origin { get { return m_Origin; } set { m_Origin = value; } }

	#endregion

	/// <summary>
	///     Initializes a new instance of the ClockwiseComparer class.
	/// </summary>
	/// <param name="origin">Origin.</param>
	public ClockwiseComparer(Vector3 origin)
	{
		m_Origin = origin;
	}

	#region IComparer Methods

	/// <summary>
	///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
	/// </summary>
	/// <param name="first">First.</param>
	/// <param name="second">Second.</param>
	public int Compare(Vector3 first, Vector3 second)
	{
		return IsClockwise(first, second, m_Origin);
	}

	#endregion

	/// <summary>
	///     Returns 1 if first comes before second in clockwise order.
	///     Returns -1 if second comes before first.
	///     Returns 0 if the points are identical.
	/// </summary>
	/// <param name="first">First.</param>
	/// <param name="second">Second.</param>
	/// <param name="origin">Origin.</param>
	public static int IsClockwise(Vector3 first, Vector3 second, Vector3 origin)
	{
		///* Non-Atan Non-Square implementation  (achieved 0.2ms on average vs 0.3 seconds for atan implementation for same scene)
		if (first.x - origin.x >= 0 && second.x - origin.x < 0)
			return -1;
		if (first.x - origin.x < 0 && second.x - origin.x >= 0)
			return 1;
		if (first.x - origin.x == 0 && second.x - origin.x == 0)
		{
			if (first.y - origin.y >= 0 || second.y - origin.y >= 0)
				return (first.y > second.y) ? -1 : 1;
			return (second.y > first.y) ? -1 : 1;
		}
		// compute the cross product of vectors (center -> first) x (center -> second)
		float det = (first.x - origin.x) * (second.y - origin.y) - (second.x - origin.x) * (first.y - origin.y);
		if (det < 0)
			return -1;
		if (det > 0)
			return 1;

		// points first and second are on the same line from the center
		// check which point is closer to the center
		float d1 = (first.x - origin.x) * (first.x - origin.x) + (first.y - origin.y) * (first.y - origin.y);
		float d2 = (second.x - origin.x) * (second.x - origin.x) + (second.y - origin.y) * (second.y - origin.y);
		return (d1 > d2) ? -1 : 1;
		//*/

		/*//Atan Square implementation

        Vector3 firstOffset = first - origin;
        Vector3 secondOffset = second - origin;
        
        float angle1 = Mathf.Atan2(firstOffset.x, firstOffset.y);
        float angle2 = Mathf.Atan2(secondOffset.x, secondOffset.y);

        if (angle1 < angle2)
            return -1;

        if (angle1 > angle2)
            return 1;

        // Check to see which point is closest
        return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? -1 : 1; */
	}
}