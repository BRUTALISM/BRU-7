using System;
using System.Collections.Generic;
using UnityEngine;

public class CompositeVectorField : IVectorField
{
	private HashSet<IVectorField> vectorFields;

	public CompositeVectorField()
	{
		this.vectorFields = new HashSet<IVectorField>();
	}

	public void Add(IVectorField field)
	{
		vectorFields.Add(field);
	}

	public void Remove(IVectorField field)
	{
		vectorFields.Remove(field);
	}

	public Vector3 VectorAt(Vector3 worldPos)
	{
		Vector3 sum = Vector3.zero;
		foreach (IVectorField vectorField in vectorFields)
		{
			sum += vectorField.VectorAt(worldPos);
		}

		return sum / vectorFields.Count;
	}
}