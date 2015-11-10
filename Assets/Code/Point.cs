using UnityEngine;
using System.Collections.Generic;

public class Point : IPosition
{
	#region Public properties

	public float Weight { get; private set; }

	// IPosition impl
	public Vector3 Position { get; set; }

	#endregion

	#region Private fields
	#endregion

	public Point(Vector3 position, float weight)
	{
		Position = position;
		this.Weight = Mathf.Clamp01(weight);
	}
}
