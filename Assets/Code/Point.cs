using UnityEngine;
using System.Collections.Generic;

public class Point : IPosition
{
	#region Public properties

	public float Weight { get; private set; }

	// IPosition impl
	public Vector3 Position
	{
		get
		{
			return position;
		}
	}

	#endregion

	#region Private fields

	private Vector3 position;

	#endregion

	public Point(Vector3 position, float weight)
	{
		this.position = position;
		this.Weight = Mathf.Clamp01(weight);
	}
}
