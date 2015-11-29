using UnityEngine;
using System.Collections.Generic;

public class PhotosPermissionDialog : MonoBehaviour
{
	#region Public properties

	public event System.Action OnConfirm;

	#endregion

	#region Event handling

	public void ConfirmPressed()
	{
		if (OnConfirm != null) OnConfirm();
		Destroy(gameObject);
	}

	#endregion
}
