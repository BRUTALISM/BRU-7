using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class SaveScreenshot : MonoBehaviour, IPointerUpHandler
{
	#region Editor public fields

	public string FilenamePrefix = "BRU-7-";
	public int ScreenshotScale = 2;

	#endregion

	#region Public properties
	#endregion

	#region Private fields
	#endregion

	#region Native hooks

	[DllImport("__Internal")]
	private static extern bool CheckPermissions();

	[DllImport("__Internal")]
	private static extern void AskPermissions();

	[DllImport("__Internal")]
	private static extern void SaveScreenshotToAlbum(string path);

	#endregion

	#region Unity methods

	void Start()
	{}

	void Update()
	{}

	#endregion

	#region Event system interface implementations

	public void OnPointerUp(PointerEventData eventData)
	{
		#if UNITY_IOS && !UNITY_EDITOR
		if (!eventData.dragging)
		{
			if (CheckPermissions())
			{
				CaptureScreenshot();
			}
			else
			{
				// Present a friendly dialog letting the user know he's about to surrender the rights for writing to the Photo Stream
				// FIXME: Implement.
				AskPermissions();
			}
		}
		#endif
	}

	#endregion

	#region Screenshot capture

	private void CaptureScreenshot()
	{
		var filename = string.Format("{0}/{1}{2}.png", Application.persistentDataPath, FilenamePrefix, GetTimestamp());

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

		Debug.Log(string.Format("Took screenshot to: {0}", filename));

		SaveScreenshotToAlbum(filename);
	}

	private string GetTimestamp()
	{
		return System.DateTime.Now.Ticks.ToString();
	}

	#endregion
}
