using UnityEngine;
using System.Collections.Generic;

public class Orbit : MonoBehaviour
{
	#region Editor public fields

	public float RotationLerpFactor = 0.5f;

	#endregion

	#region Public properties
	#endregion

	#region Private fields

	private Quaternion rotationOffset;
	private float distanceFromOrigin;

	#endregion

	#region Unity methods

	void Start()
	{
		rotationOffset = Quaternion.Euler(90f, 0f, 0f);
		distanceFromOrigin = transform.position.magnitude;
	}

	void Update()
	{
		var gyroRotation = rotationOffset * ConvertRotation(Input.gyro.attitude);

		transform.position = Vector3.zero;
		transform.rotation = Quaternion.Lerp(transform.rotation, gyroRotation, RotationLerpFactor);
		transform.position = -transform.forward * distanceFromOrigin;
	}

	#endregion

	#region Private methods

	private static Quaternion ConvertRotation(Quaternion q)
	{
		return new Quaternion(q.x, q.y, -q.z, -q.w);
	}

	#endregion
}
