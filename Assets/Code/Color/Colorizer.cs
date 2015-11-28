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
	#endregion

	#region Unity methods

	private void Start()
	{
		var meshPacker = GetComponent<MeshPacker>();
		Farb.Scenestance.Palettes
			.Zip(meshPacker.GeneratedChildren, (p, cs) => new { Palette = p, MeshChildren = cs })
			.Subscribe(x => Colorize(x.Palette, x.MeshChildren))
			.AddTo(this);
	}

	#endregion

	#region Coloring

	private void Colorize(InfinitePalette palette, List<GameObject> objects)
	{
		var colorEnumerator = palette.Colors.GetEnumerator();
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
