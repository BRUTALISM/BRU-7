using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class InfinitePaletteTest : MonoBehaviour
{
	#region Editor public fields

	public int ColorsToGenerate = 5;
	public Material MeshMaterial;
	public InfinitePalette.Parameters Parameters;

	#endregion

	#region Public properties
	#endregion

	#region Private fields

	private InfinitePalette palette;
	private List<Color> colors = new List<Color>();

	#endregion

	#region Unity methods

	void OnEnable()
	{
		Generate();
	}

	void OnDisable()
	{
		Clear();
	}

	#endregion

	#region Upkeep

	private void Generate()
	{
		palette = new InfinitePalette(Parameters);

		var colorEnumerator = palette.Colors.GetEnumerator();
		for (int i = 0; i < ColorsToGenerate; i++)
		{
			colorEnumerator.MoveNext();
			colors.Add(colorEnumerator.Current);
		}

		var renderer = GetComponent<MeshRenderer>();
		if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>();
		renderer.sharedMaterial = MeshMaterial;

		var filter = GetComponent<MeshFilter>();
		if (filter == null) filter = gameObject.AddComponent<MeshFilter>();
		filter.sharedMesh = GenerateMesh();
	}

	private void Clear()
	{
		palette = null;
		colors.Clear();

		if (!Application.isPlaying) DestroyImmediate(GetComponent<MeshFilter>().sharedMesh);
		else Destroy(GetComponent<MeshFilter>().sharedMesh);
	}

	#endregion

	#region Mesh generation

	private Mesh GenerateMesh()
	{
		var meshBuilder = new MeshBuilder();

		const float SingleColorCellSize = 0.8f;

		float horizontalOffset = 0f;
		float verticalOffset = SingleColorCellSize;
		for (int i = 0; i < colors.Count; i++)
		{
			if (i % Parameters.PrimaryColorCount == 0)
			{
				horizontalOffset = 0f;
				verticalOffset -= SingleColorCellSize;
			}

			var color = colors[i];
			var rootPosition = new Vector3(horizontalOffset, verticalOffset, 0f);
			horizontalOffset += SingleColorCellSize;

			meshBuilder.Pack(
				new List<Vector3>()
				{
					rootPosition,
					rootPosition + new Vector3(SingleColorCellSize, 0f, 0f),
					rootPosition + new Vector3(SingleColorCellSize, SingleColorCellSize, 0f),
					rootPosition + new Vector3(0f, SingleColorCellSize, 0f)
				},
				new List<int>()
				{
					2, 1, 0,
					0, 3, 2
				},
				null,
				new List<Color>() { color, color, color, color }
			);
		}

		return meshBuilder.Build()[0];
	}

	#endregion
}
