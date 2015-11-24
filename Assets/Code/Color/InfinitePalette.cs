using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// An infinite color palette which can be polled for new colors ad infinitum. The color generation algorithm is controlled by the
/// parameters supplied at creation time.
/// </summary>
public class InfinitePalette
{
	#region Generation parameters

	/// <summary>
	/// The method used to generate initial (primary) colors which are used as starting points for generating an infinite palette.
	/// </summary>
	public enum PrimaryColorMethod
	{
		None,
		Random,
		Analogous,
		Monochromatic,
		Complementary,
		Compound
	}

	/// <summary>
	/// The affinity of the algorithm towards selecting light and/or dark colors (with high and low Lightness values, respectively).
	/// </summary>
	public enum LightnessAffinity
	{
		None,
		Light,
		Dark,
		LightAndDark
	}

	/// <summary>
	/// The affinity for choosing the next primary color "column" to continue the iteration on.
	/// </summary>
	public enum ColumnIterationAffinity
	{
		None,
		Muted,		// prefer lower saturation
		Vibrant,	// prefer higher saturation
		Dark,		// prefer lower lightness
		Bright		// prefer higher lightness
		// ... ?
	}

	/// <summary>
	/// The parameters controlling the color generation algorithm of the <see cref="InfinitePalette"/> class.
	/// </summary>
	public class Parameters
	{
		// +--------------------------+
		// | Primary color parameters |
		// +--------------------------+

		/// <summary>
		/// Which method is used to select the primary colors.
		/// </summary>
		public PrimaryColorMethod PrimaryColorMethod;

		/// <summary>
		/// The affinity of the generation algorithm to pick very light or very dark colors (or both) as primary colors.
		/// </summary>
		public LightnessAffinity LightnessAffinity;

		/// <summary>
		/// The number of primary colors to generate.
		/// </summary>
		public int PrimaryColorCount;

		// +--------------------------+
		// | Derived color parameters |
		// +--------------------------+

		/// <summary>
		/// When the generation algorithm iterates over primary columns, this is the affinity it will use to choose the next column.
		/// </summary>
		public ColumnIterationAffinity ColumnIterationAffinity;

		/// <summary>
		/// How much the column affinity is taken into account when selecting columns, in the [0, 1] range. A zero value means that the
		/// affinity will be ignored (effectively reducing column selection to a random choice), while a value of 1 will always select the
		/// column with the highest preference, ignoring other columns. The Truth is somewhere between 0 and 1.
		/// </summary>
		public float ColumnAffinityIntensity;

		/// <summary>
		/// The maximum distance (in normalized HSL cylindrical coordinates) a derived point can be away from its source primary point.
		/// </summary>
		public float WanderMax;

		// The three values below control the magnitude of the offset vector (in cylindrical space) from the current color to the color
		// about to be generated next.

		/// <summary>
		/// The maximum value a hue component of the offset vector can have.
		/// </summary>
		public float HueMaxStep;

		/// <summary>
		/// The maximum value a saturation component of the offset vector can have.
		/// </summary>
		public float SaturationMaxStep;

		/// <summary>
		/// The maximum value a lightness component of the offset vector can have.
		/// </summary>
		public float LightnessMaxStep;
	}

	#endregion

	#region Public properties

	/// <summary>
	/// An infinite sequence of colors, generated on demand.
	/// </summary>
	public IEnumerable<HSLColor> Colors
	{
		get
		{
			if (primaries.Count < parameters.PrimaryColorCount)
			{
				var newColor = GeneratePrimaryColor();
				primaries.Add(newColor);
				currentColorsByColumn.Add(newColor);

				if (primaries.Count == parameters.PrimaryColorCount) SelectNextColumn();

				yield return newColor;
			}
			else
			{
				var newColor = GenerateDerivedColor();
				currentColorsByColumn[sourceColumnIndex] = newColor;

				SelectNextColumn();

				yield return newColor;
			}
		}
	}

	#endregion

	#region Private fields

	private Parameters parameters;

	private List<HSLColor> primaries;

	private List<HSLColor> currentColorsByColumn;
	private int sourceColumnIndex;

	#endregion

	#region Initialization

	public InfinitePalette(Parameters parameters)
	{
		if (parameters == null) throw new System.ArgumentException("parameters are null, can't generate palette without them");
		this.parameters = parameters;
		this.primaries = new List<HSLColor>(parameters.PrimaryColorCount);
		this.currentColorsByColumn = new List<HSLColor>(parameters.PrimaryColorCount);
	}

	#endregion

	#region Primary color generation

	private HSLColor GeneratePrimaryColor()
	{
		// FIXME: Implement.
		throw new System.NotImplementedException();
	}

	#endregion

	#region Derived color generation

	private HSLColor GenerateDerivedColor()
	{
		// FIXME: Implement.
		throw new System.NotImplementedException();
	}

	private void SelectNextColumn()
	{
		// FIXME: Implement.
		throw new System.NotImplementedException();
	}

	#endregion
}
