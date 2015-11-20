using UnityEngine;
using System.Collections.Generic;

public class Point : IPosition
{
	#region Public properties

	// IPosition impl
	public Vector3 Position { get; set; }

	public float Weight { get; private set; }

	public float CreationTime { get; protected set; }

	public Color Color { get; set; }

	#endregion

	#region Private fields
	#endregion

	public Point(Vector3 position, float weight, Color color = default(Color))
	{
		Position = position;
		Weight = Mathf.Clamp01(weight);
		CreationTime = SafeTime.Instance.Time;
		Color = color;
	}
}
