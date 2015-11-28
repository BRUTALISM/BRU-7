using UnityEngine;
using System.Collections.Generic;

public class Tutorial : MonoBehaviour
{
	#region Editor public fields

	public float FadeAnimationDuration = 0.5f;
	public Sidekick Sidekick;

	#endregion

	#region Public properties
	#endregion

	#region Private fields

	private readonly string KeyboardTutorialShown = "KeyboardTutorialShown";

	#endregion

	#region Unity methods

	void Start()
	{
		if (PlayerPrefs.HasKey(KeyboardTutorialShown)) Destroy(gameObject);
		else
		{
			StartCoroutine(Fade(1f));
		}
	}

	void Update()
	{
		if (Sidekick.IsFullyShown)
		{
			// Fade out and destroy when the keyboard has been pulled out
			StartCoroutine(Fade(0f, () =>
			{
				PlayerPrefs.SetInt(KeyboardTutorialShown, 1);
				PlayerPrefs.Save();

				Destroy(gameObject);
			}));
		}
	}

	#endregion

	#region Animation

	private System.Collections.IEnumerator Fade(float targetAlpha, System.Action onComplete = null)
	{
		var canvasGroup = GetComponent<CanvasGroup>();
		float startAlpha = canvasGroup.alpha;
		float startTime = Time.time;

		while (Time.time - startTime < FadeAnimationDuration)
		{
			var t = (Time.time - startTime) / FadeAnimationDuration;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
			yield return null;
		}

		canvasGroup.alpha = targetAlpha;

		if (onComplete != null) onComplete();
	}

	#endregion
}
