using System;
using UnityEngine;

public interface IVectorField
{
	Vector3 VectorAt(Vector3 worldPos);
}

