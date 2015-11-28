using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Pan : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	#region Editor public fields
	#endregion

	#region Public properties
	#endregion

	#region Private fields

	private Orbit orbit;

	private List<int> currentTouches = new List<int>();

	#endregion

	#region Unity methods

	void Start()
	{
		orbit = FindObjectOfType<Orbit>();
	}

	#endregion

	#region Touch handling

	public void OnPointerDown(PointerEventData eventData)
	{
		currentTouches.Add(eventData.pointerId);
		orbit.LerpRotationOriginToZero = false;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		currentTouches.Remove(eventData.pointerId);

		if (currentTouches.Count == 0) orbit.LerpRotationOriginToZero = true;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (currentTouches.Count == 1)
		{
			// Figure out the world-space offset from the screen delta
			var camera = Camera.main;
			var screenDelta = eventData.delta;
			var screenCenterX = camera.pixelWidth / 2;
			var screenCenterY = camera.pixelHeight / 2;
			var startPoint = camera.ScreenToWorldPoint(new Vector3(screenCenterX, screenCenterY, orbit.DistanceFromOrigin));
			var endPoint = camera.ScreenToWorldPoint(new Vector3(screenCenterX + screenDelta.x, screenCenterY + screenDelta.y,
				orbit.DistanceFromOrigin));
			var worldDelta = endPoint - startPoint;

			camera.transform.position -= worldDelta;
			orbit.RotationOrigin -= worldDelta;
		}
	}

	#endregion
}
