using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Sidekick : MonoBehaviour, IDragHandler, IEndDragHandler
{
	#region Editor public fields

	public ScreenSide Side;
	public CanvasGroup Content;
	public float EdgeOffset = 20f;
	public float DragThreshold;
	public float DragAnimationDuration;

	#endregion

	#region Public properties

	public enum ScreenSide { Left, Right };

	#endregion

	#region Private fields

	private RectTransform rectTransform;
	private ScreenOrientation currentOrientation;

	private Vector2 hiddenPosition;

	private ScreenSide swipeDirection;

	#endregion

	#region Unity methods

	void OnEnable()
	{
		rectTransform = GetComponent<RectTransform>();
		currentOrientation = Screen.orientation;

		if (Side == ScreenSide.Left) hiddenPosition = new Vector2(-Camera.main.pixelWidth + EdgeOffset, 0f);
		else hiddenPosition = new Vector2(Camera.main.pixelWidth - EdgeOffset, 0f);

		RepositionOnTheSide();
		ResetContentAlpha();
	}

	void Update()
	{
		if (Screen.orientation != currentOrientation)
		{
			currentOrientation = Screen.orientation;
			RepositionOnTheSide();
		}
	}

	#endregion

	#region Positioning

	private void RepositionOnTheSide()
	{
		rectTransform.anchoredPosition = hiddenPosition;
	}

	private System.Collections.IEnumerator AnimateToX(float x)
	{
		float startTime = Time.time;
		Vector2 startPosition = rectTransform.anchoredPosition;
		Vector2 endPosition = new Vector2(x, startPosition.y);

		while (Time.time < startTime + DragAnimationDuration)
		{
			float t = Mathf.SmoothStep(0f, 1f, (Time.time - startTime) / DragAnimationDuration);
			rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
			ResetContentAlpha();
			yield return null;
		}

		rectTransform.anchoredPosition = endPosition;
		ResetContentAlpha();
	}

	private void ResetContentAlpha()
	{
		var hiddenX = hiddenPosition.x;
		var shownX = 0f;
		var t = Mathf.InverseLerp(hiddenX, shownX, rectTransform.anchoredPosition.x);

		Content.alpha = t;
	}

	#endregion

	#region Event handling

	public void OnDrag(PointerEventData eventData)
	{
		float draggedX = eventData.delta.x;

		var newX = rectTransform.anchoredPosition.x + draggedX;
		if (Side == ScreenSide.Left) newX = Mathf.Clamp(newX, -Camera.main.pixelWidth, 0f);
		else newX = Mathf.Clamp(newX, 0f, Camera.main.pixelWidth);

		rectTransform.anchoredPosition = new Vector2(newX, rectTransform.anchoredPosition.y);

		swipeDirection = draggedX > 0f ? ScreenSide.Right : ScreenSide.Left;

		ResetContentAlpha();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		// Animate to opposite edge
		StartCoroutine(AnimateToX(swipeDirection == Side ? hiddenPosition.x : 0f));
	}

	#endregion
}
