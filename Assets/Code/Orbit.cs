using UnityEngine;
using System.Collections.Generic;

public class Orbit : MonoBehaviour
{
	#region Editor public fields

	public float RotationLerpFactor = 0.5f;
	public bool XRotation = true;
	public bool YRotation = true;
	public bool ZRotation = true;
	public float MinimumDistance = 10f;
	public float MaximumDistance = 150f;
	public float RotationOriginResetDuration = 3f;

	#endregion

	#region Public properties

	private float distanceFromOrigin;
	public float DistanceFromOrigin
	{
		get { return distanceFromOrigin; }
		set { distanceFromOrigin = Mathf.Clamp(value, MinimumDistance, MaximumDistance); }
	}

	public Vector3 RotationOrigin
	{
		get { return rotationOrigin; }
		set { lastSetRotationOrigin = rotationOrigin = value; lastRotationOriginSetTime = Time.time; }
	}

	public bool LerpRotationOriginToZero
	{
		get { return lerpRotationOrigin; }
		set
		{
			lerpRotationOrigin = value;
			if (lerpRotationOrigin)
			{
				// Invoke the rotation origin setter again, to reset the animation timer and stuff
				RotationOrigin = rotationOrigin;
			}
		}
	}

	#endregion

	#region Private fields

	private Quaternion rotationOffset;

	private Vector3 rotationOrigin;
	private Vector3 lastSetRotationOrigin;
	private float lastRotationOriginSetTime;

	private bool lerpRotationOrigin;

	#endregion

	#region Unity methods

	void Start()
	{
		rotationOffset = Quaternion.Euler(90f, 0f, 0f);
		DistanceFromOrigin = transform.position.magnitude;
	}

	void Update()
	{
		const float KeyboardRotationPerFrame = 2f;
		const float DistanceZoomPerFrame = 2f;
		if (Input.GetKey(KeyCode.LeftArrow)) rotationOffset *= Quaternion.Euler(0f, -KeyboardRotationPerFrame, 0f);
		if (Input.GetKey(KeyCode.RightArrow)) rotationOffset *= Quaternion.Euler(0f, KeyboardRotationPerFrame, 0f);
		if (Input.GetKey(KeyCode.UpArrow)) rotationOffset *= Quaternion.Euler(-KeyboardRotationPerFrame, 0f, 0f);
		if (Input.GetKey(KeyCode.DownArrow)) rotationOffset *= Quaternion.Euler(KeyboardRotationPerFrame, 0f, 0f);
		if (Input.GetKey(KeyCode.Equals)) DistanceFromOrigin -= DistanceZoomPerFrame;
		if (Input.GetKey(KeyCode.Minus)) DistanceFromOrigin += DistanceZoomPerFrame;

		var gyroRotation = rotationOffset * ConvertRotation(Input.gyro.attitude);

		transform.position = Vector3.zero;

		var rotation = Quaternion.Lerp(transform.rotation, gyroRotation, RotationLerpFactor);
		if (!XRotation || !YRotation || !ZRotation)
		{
			rotation = Quaternion.Euler(XRotation ? rotation.eulerAngles.x : 0f, YRotation ? rotation.eulerAngles.y : 0f,
				ZRotation ? rotation.eulerAngles.z : 0f);
		}
		transform.rotation = rotation;

		transform.position = RotationOrigin - transform.forward * DistanceFromOrigin;

		if (LerpRotationOriginToZero)
		{
			// Slowly reset RotationOrigin back to zero
			float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((Time.time - lastRotationOriginSetTime) / RotationOriginResetDuration));
			rotationOrigin = Vector3.Lerp(lastSetRotationOrigin, Vector3.zero, t);
		}
	}

	#endregion

	#region Private methods

	private static Quaternion ConvertRotation(Quaternion q)
	{
		return new Quaternion(q.x, q.y, -q.z, -q.w);
	}

	#endregion
}
