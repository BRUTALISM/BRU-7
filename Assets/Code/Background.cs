using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class Background : MonoBehaviour
{
	#region Editor public fields

	public MeshRenderer BackgroundRenderer;

	#endregion

	#region Public properties
	#endregion

	#region Private fields
	#endregion

	#region Unity methods

	void Start()
	{
		BackgroundRenderer.material.SetInt("_ZWrite", 0);

		Farb.Scenestance.Palettes.Subscribe(Paint).AddTo(this);
	}

	#endregion

	#region Color logic

	private void Paint(InfinitePalette palette)
	{
		var enumerator = palette.Colors.GetEnumerator();
		enumerator.MoveNext();
		var color = enumerator.Current;

		BackgroundRenderer.material.color = color;
	}

	#endregion
}
