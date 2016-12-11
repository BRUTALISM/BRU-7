using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public class Colorizer : MonoBehaviour
{
	#region Editor public fields

	public Material MeshMaterial;

	public string MaterialColorProperty = "_Color";

	[Range(0f, 1f)]
	public float ColorAlpha = 1f;

	#endregion

	#region Public properties
	#endregion

	#region Private fields
	private InfinitePalette currentPalette;
	#endregion

	#region Unity methods

	private void Start()
	{
		Farb.Scenestance.Palettes.Subscribe((palette) => currentPalette = palette).AddTo(this);

		var meshPacker = GetComponent<MeshPacker>();
		meshPacker.GeneratedChildren.Subscribe(Colorize).AddTo(this);
	}

	#endregion

	#region Coloring

	private void Colorize(List<GameObject> objects)
	{
		var colorEnumerator = currentPalette.Colors.GetEnumerator();
		foreach (var renderer in objects.Select(o => o.GetComponent<MeshRenderer>()))
		{
			colorEnumerator.MoveNext();
			var color = colorEnumerator.Current;
			color.A = ColorAlpha;

			var meshMaterial = new Material(MeshMaterial);
			meshMaterial.SetColor(MaterialColorProperty, color);

			renderer.sharedMaterial = meshMaterial;
		}
	}

	#endregion
}
