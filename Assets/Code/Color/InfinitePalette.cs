using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A palette.
/// </summary>
public class InfinitePalette
{
	#region Public

	public const int DominantColorIndex = 0;
	public const int BackgroundColorIndex = 1;

	public Color DominantColor { get { return colors.Count > 0 ? colors[DominantColorIndex] : default(Color); } }
	public Color BackgroundColor { get { return colors.Count > 1 ? colors[BackgroundColorIndex] : default(Color); } }
	public IEnumerable<Color> DetailColors
	{
		get
		{
			for (int i = BackgroundColorIndex + 1; i < colors.Count; i++)
			{
				yield return colors[i];
			}
		}
	}

	/// <summary>
	/// Gets the color at the given index, which is automatically wrapped around if it's pointing past the end of the colors list.
	/// </summary>
	/// <param name="index">Index.</param>
	public Color this[int index]
	{
		get
		{
			return colors[index % colors.Count];
		}
	}

	#endregion

	#region Private fields

	private List<Color> colors;

	#endregion

	#region Initialization

	public InfinitePalette(IEnumerable<Color> colors)
	{
		this.colors = new List<Color>(colors);
	}

	#endregion

	#region Public methods

	/// <summary>
	/// Gets the detail color at the given index, which is automatically wrapped past the end of the detail color list (so you don't have to
	/// worry about it, chum).
	/// </summary>
	/// <returns>The detail color.</returns>
	/// <param name="index">Index.</param>
	public Color GetDetailColor(int index)
	{
		return colors[2 + index % (colors.Count - 2)];
	}

	#endregion
}
