using UnityEngine;
using System.Collections.Generic;

public class ConstantVectorField : IVectorField
{
	public Vector3 Direction { get; set; }

	public ConstantVectorField(Vector3 direction)
	{
		Direction = direction;
	}

	public Vector3 VectorAt(Vector3 worldPos)
	{
		return Direction;
	}
}
