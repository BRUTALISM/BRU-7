using UnityEngine;
using System.Collections.Generic;

public class DestroyAfterDelay : MonoBehaviour
{
	#region Editor public fields

	public float DelaySeconds = 1f;

	#endregion

	#region Public properties
	#endregion

	#region Private fields
	#endregion

	#region Unity methods

	void Start()
	{
		StartCoroutine(DestroyLater());
	}

	#endregion

	#region Private methods

	private System.Collections.IEnumerator DestroyLater()
	{
		yield return new WaitForSeconds(DelaySeconds);
		Destroy(gameObject);
	}

	#endregion
}
