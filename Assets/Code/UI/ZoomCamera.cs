using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ZoomCamera : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	#region Editor public fields

	public Orbit Orbiter;

	#endregion

	#region Public properties
	#endregion

	#region Private fields

	private List<int> currentTouches = new List<int>();
	private Dictionary<int, Vector2> touchPositions = new Dictionary<int, Vector2>();

	private float lastTouchDistance = 0f;

	#endregion

	#region Unity methods
	#endregion

	#region Event system interface implementations

	public void OnPointerDown(PointerEventData eventData)
	{
		currentTouches.Add(eventData.pointerId);
		touchPositions[eventData.pointerId] = eventData.position;

		if (currentTouches.Count >= 2)
		{
			lastTouchDistance = Vector2.Distance(touchPositions[currentTouches[0]], touchPositions[currentTouches[1]]);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		currentTouches.Remove(eventData.pointerId);
		touchPositions.Remove(eventData.pointerId);
		lastTouchDistance = 0f;
	}

	public void OnDrag(PointerEventData eventData)
	{
		touchPositions[eventData.pointerId] = eventData.position;

		if (currentTouches.Count >= 2)
		{
			// Find the distance between the first two touches (ignore the rest)
			var distance = Vector2.Distance(touchPositions[currentTouches[0]], touchPositions[currentTouches[1]]);

			Orbiter.DistanceFromOrigin *= lastTouchDistance / distance;

			lastTouchDistance = distance;
		}
	}

	#endregion
}
