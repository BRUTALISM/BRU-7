using UnityEngine;
using System.Collections.Generic;

public class Point : IPosition
{
	#region Public properties

	// IPosition impl
	public Vector3 Position { get; set; }

	public float Weight { get; private set; }

	public float CreationTime { get; protected set; }

	#endregion

	#region Private fields
	#endregion

	public Point(Vector3 position, float weight)
	{
		Position = position;
		Weight = Mathf.Clamp01(weight);
		CreationTime = Time.time;
	}
}
