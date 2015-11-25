using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// An infinite color palette which can be polled for new colors ad infinitum. The color generation algorithm is controlled by the
/// parameters supplied at creation time.
/// </summary>
public class InfinitePalette
{
	#region Primary color parameters
	/// <summary>
	/// The method used to generate initial (primary) colors which are used as starting points for generating an infinite palette.
	/// </summary>
	public enum PrimaryColorMethod
	{
		None,
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

	#endregion

	#region Derived color parameters

	/// <summary>
	/// The affinity influencing derivative colors' direction relative to their parent primary color.
	/// </summary>
	public enum ColorWanderingAffinity
	{
		None,					// the derivatives start from their primary color but wander randomly from then on
		VicinityOfPrimaryColor,	// the derivatives will stay in the vicinity of their corresponding primary color
		WithinPrimaryColors		// the derivatives will tend to stay within the n-gon defined by all primary colors
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

	#endregion

	/// <summary>
	/// The parameters controlling the color generation algorithm of the <see cref="InfinitePalette"/> class.
	/// </summary>
	[System.Serializable]
	public class Parameters
	{
		#region Primary color parameters

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

		/// <summary>
		/// The angle increment to be used by primary color methods which use the hue offset technique (Analogous, Complementary).
		/// </summary>
		public float HueAngleIncrement;

		/// <summary>
		/// The mean value for the Gaussian distribution when generating a random lightness value with the LightnessAffinity.Dark affinity.
		/// </summary>
		public float LightnessDarkAffinityMean = 0.2f;

		/// <summary>
		/// The mean value for the Gaussian distribution when generating a random lightness value with the LightnessAffinity.Light affinity.
		/// </summary>
		public float LightnessLightAffinityMean = 0.8f;

		/// <summary>
		/// The standard deviation for the Gaussian distribution when generating a random lightness value.
		/// </summary>
		public float LightnessAffinityStandardDeviation = 0.2f;

		#endregion

		#region Derived color parameters

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
		/// The affinity influencing derivative colors' direction relative to their parent primary color.
		/// </summary>
		public ColorWanderingAffinity ColorWanderingAffinity;

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

		#endregion
	}

	#region Constants

	public const float HueMinimum = 0f;
	public const float HueMaximum = 1f;
	public const float SaturationMinimum = 0f;
	public const float SaturationMaximum = 1f;
	public const float LightnessBlack = 0f;
	public const float LightnessMaximumColor = 0.5f;
	public const float LightnessWhite = 1f;

	#endregion

	#region Public properties

	/// <summary>
	/// An infinite sequence of colors, generated on demand.
	/// </summary>
	public IEnumerable<HSLColor> Colors
	{
		get
		{
			while (true)
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
	}

	/// <summary>
	/// Gets the base color of this palette. The base color is the first color that is generated.
	/// </summary>
	public HSLColor BaseColor
	{
		get
		{
			if (primaries.Count == 0) throw new System.InvalidOperationException("base color not set, no colors have been generated");
			return primaries[0];
		}
	}

	#endregion

	#region Private fields

	private Parameters parameters;

	private List<HSLColor> primaries;

	private List<HSLColor> currentColorsByColumn;
	private int sourceColumnIndex;

	// Hue offsets with relation to the previous color when using the PrimaryColorMethod.Compound method
	private static List<float> compoundColorOffsets = new List<float>()
	{
		30f / 360f,
		0f,
		150f / 360f,
		345f / 360f,
		195f / 360f
	};

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
		var newColor = new HSLColor();
		newColor.A = 1f;

		if (primaries.Count == 0)
		{
			// First color
			newColor.H = Nasum.Range(0f, 1f);
			newColor.S = SaturationMaximum;
			newColor.L = LightnessMaximumColor;
		}
		else
		{
			var previousColor = primaries[primaries.Count - 1];

			// Generate hue and saturation based on PrimaryColorMethod
			switch (parameters.PrimaryColorMethod)
			{
				case PrimaryColorMethod.None:
					newColor.H = Nasum.Range(HueMinimum, HueMaximum);
					newColor.S = RandomSaturation();
					break;
				case PrimaryColorMethod.Analogous:
					newColor.H = (previousColor.H + parameters.HueAngleIncrement / 360f).PositiveFract();
					newColor.S = previousColor.S;
					break;
				case PrimaryColorMethod.Complementary:
					newColor.H = (previousColor.H + 0.5f).PositiveFract();
					newColor.S = RandomSaturation();
					break;
				case PrimaryColorMethod.Compound:
					var compoundOffset = compoundColorOffsets[(primaries.Count - 1) % compoundColorOffsets.Count];
					newColor.H = (previousColor.H + compoundOffset).PositiveFract();
					newColor.S = RandomSaturation();
					break;
				case PrimaryColorMethod.Monochromatic:
					newColor.H = previousColor.H;
					newColor.S = RandomSaturation();
					break;
			}
			
			// Generate lightness based on LightnessAffinity
			switch (parameters.LightnessAffinity)
			{
				case LightnessAffinity.None:
					newColor.L = LightnessMaximumColor;
					break;
				case LightnessAffinity.Dark:
					newColor.L = Nasum.GaussianInRange(parameters.LightnessDarkAffinityMean, parameters.LightnessAffinityStandardDeviation,
						LightnessBlack, LightnessMaximumColor);
					break;
				case LightnessAffinity.Light:
					newColor.L = Nasum.GaussianInRange(parameters.LightnessLightAffinityMean, parameters.LightnessAffinityStandardDeviation,
						LightnessMaximumColor, LightnessWhite);
					break;
				case LightnessAffinity.LightAndDark:
					// TODO: Gaussian, perhaps? Uniform might be all over the place.
					newColor.L = Nasum.GaussianInRange(LightnessMaximumColor, parameters.LightnessAffinityStandardDeviation,
						LightnessMaximumColor, LightnessWhite);
					break;
			}
		}

		// Done
		return newColor;
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
//		throw new System.NotImplementedException();
	}

	#endregion

	#region Component randomization methods

	private float RandomSaturation()
	{
		// TODO: Parameterize this as well?
		return Nasum.Range(SaturationMinimum, SaturationMaximum);
	}

	#endregion
}
