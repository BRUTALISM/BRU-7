using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class SaveScreenshot : MonoBehaviour, IPointerUpHandler
{
	#region Editor public fields

	public int ScreenshotScale = 2;
	public GameObject AskPermissionsOverlayPrefab;
	public GameObject SaveOverlayPrefab;

	#endregion

	#region Public properties
	#endregion

	#region Private fields

	private StringInput stringInputComponent;
	private PhotosPermissionDialog photosPermissionDialog;

	#endregion

	#region Native hooks

	[DllImport("__Internal")]
	private static extern bool CheckPermissions();

	[DllImport("__Internal")]
	private static extern void AskPermissions();

	[DllImport("__Internal")]
	private static extern void SaveScreenshotToAlbum(string path);

	private bool saving = false;
	private GameObject saveOverlay;

	#endregion

	#region Unity methods

	void Start()
	{
		stringInputComponent = FindObjectOfType<StringInput>();
	}

	#endregion

	#region Event system interface implementations

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!saving && !eventData.dragging)
		{
			#if UNITY_IOS && !UNITY_EDITOR
			if (CheckPermissions())
			{
				StartCoroutine(CaptureScreenshot());
			}
			else
			{
				// Present a friendly dialog letting the user know he's about to surrender the rights for writing to the Photo Stream
				AskForPermissions();
			}
			#elif UNITY_EDITOR
			StartCoroutine(CaptureScreenshot());
			#endif
		}
	}

	#endregion

	#region Ask for permissions

	private void AskForPermissions()
	{
		var askPermissionsOverlay = Instantiate(AskPermissionsOverlayPrefab);
		askPermissionsOverlay.transform.SetParent(FindObjectOfType<Canvas>().transform, false);

		photosPermissionDialog = askPermissionsOverlay.GetComponentInChildren<PhotosPermissionDialog>();
		photosPermissionDialog.OnConfirm += PhotosPermissionDialogOnConfirm;
	}

	void PhotosPermissionDialogOnConfirm()
	{
		AskPermissions();
		photosPermissionDialog.OnConfirm -= PhotosPermissionDialogOnConfirm;
	}

	#endregion

	#region Screenshot capture

	private System.Collections.IEnumerator CaptureScreenshot()
	{
		saving = true;

		saveOverlay = Instantiate(SaveOverlayPrefab);

		yield return new WaitForSeconds(0.1f);

		var filename = string.Format("{0}/AE1-{1}-{2}.png", Application.persistentDataPath, stringInputComponent.CurrentInputString,
			GetTimestamp());

		var targetWidth = Camera.main.pixelWidth * ScreenshotScale;
		var targetHeight = Camera.main.pixelHeight * ScreenshotScale;

		RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 24);
		Camera.main.targetTexture = rt;

		var screenShot = new Texture2D(targetWidth, targetHeight, TextureFormat.ARGB32, false);
		Camera.main.Render();
		RenderTexture.active = rt;
		screenShot.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);

		Camera.main.targetTexture = null;
		RenderTexture.active = null; 
		Destroy(rt);

		byte[] bytes = screenShot.EncodeToPNG();
		System.IO.File.WriteAllBytes(filename, bytes);

		#if UNITY_IOS && !UNITY_EDITOR
		SaveScreenshotToAlbum(filename);
		#endif

		Debug.LogFormat("Saved screenshot to: {0}", filename);

		#if UNITY_EDITOR
		NativeCodeFinishedSave("");
		#endif
	}

	private string GetTimestamp()
	{
		return System.DateTime.Now.Ticks.ToString();
	}

	public void NativeCodeFinishedSave(string message)
	{
		Debug.Log("Save finished.");

		Destroy(saveOverlay);
		saving = false;
	}

	#endregion
}
