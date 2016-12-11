using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
	/// The affinity for choosing the next primary color to continue the iteration on.
	/// </summary>
	public enum DerivationSourceAffinity
	{
		None,
		LessSaturation,		// prefer lower saturation
		MoreSaturation,	// prefer higher saturation
		LessLightness,		// prefer lower lightness
		MoreLightness		// prefer higher lightness
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
		public int PrimaryColorCount = 5;

		/// <summary>
		/// The angle increment to be used by primary color methods which use the hue offset technique (Analogous, Complementary).
		/// </summary>
		public float AnalogousHueAngleIncrement = 10f;

		/// <summary>
		/// The mean value for the Gaussian distribution when generating a random lightness value with the LightnessAffinity.Dark affinity.
		/// </summary>
		public float LightnessDarkAffinityMean = 0.2f;

		/// <summary>
		/// The mean value for the Gaussian distribution when generating a random lightness value with the LightnessAffinity.Light affinity.
		/// </summary>
		public float LightnessLightAffinityMean = 0.6f;

		/// <summary>
		/// The standard deviation for the Gaussian distribution when generating a random lightness value.
		/// </summary>
		public float LightnessAffinityStandardDeviation = 0.1f;

		#endregion

		#region Derived color parameters

		/// <summary>
		/// When the generation algorithm iterates over primary columns, this is the affinity it will use to choose the next column.
		/// </summary>
		public DerivationSourceAffinity DerivationSourceAffinity;

		/// <summary>
		/// How much the column affinity is taken into account when selecting columns, in the [0, 1] range. A zero value means that the
		/// affinity will be ignored (effectively reducing column selection to a random choice), while a value of 1 will always select the
		/// column with the highest preference, ignoring other columns. The Truth is somewhere between 0 and 1.
		/// </summary>
		[Range(0f, 1f)]
		public float DerivationSourceAffinityIntensity = 0.9f;

		/// <summary>
		/// The maximum distance a derived color's hue can be away from its source primary point.
		/// </summary>
		[Range(0f, 1f)]
		public float HueWanderMax = 0.1f;

		/// <summary>
		/// The maximum distance a derived color's saturation can be away from its source primary point.
		/// </summary>
		[Range(0f, 1f)]
		public float SaturationWanderMax = 0.5f;

		/// <summary>
		/// The maximum distance a derived color's lightness can be away from its source primary point.
		/// </summary>
		[Range(0f, 1f)]
		public float LightnessWanderMax = 0.1f;

		// The three values below control the magnitude of the offset vector (in cylindrical space) from the current color to the color
		// about to be generated next.

		/// <summary>
		/// The maximum value a hue component of the offset vector can have.
		/// </summary>
		[Range(0f, 1f)]
		public float HueMaxStep = 0.05f;

		/// <summary>
		/// The maximum value a saturation component of the offset vector can have.
		/// </summary>
		[Range(0f, 1f)]
		public float SaturationMaxStep = 0.05f;

		/// <summary>
		/// The maximum value a lightness component of the offset vector can have.
		/// </summary>
		[Range(0f, 1f)]
		public float LightnessMaxStep = 0.05f;

		#endregion

		#region Global parameters

		[Range(0f, 1f)]
		public float SaturationMinimum = 0f;

		[Range(0f, 1f)]
		public float SaturationMaximum = 1f;

		#endregion
	}

	#region Constants

	public const float HueMinimum = 0f;
	public const float HueMaximum = 1f;
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

		var previousColor = new HSLColor(RandomHue(), Random.Range(parameters.SaturationMinimum, parameters.SaturationMaximum), 0f, 1f);
		if (primaries.Count != 0) previousColor = primaries[primaries.Count - 1];

		// Generate hue and saturation based on PrimaryColorMethod
		switch (parameters.PrimaryColorMethod)
		{
			case PrimaryColorMethod.None:
				newColor.H = Nasum.Range(HueMinimum, HueMaximum);
				newColor.S = Random.Range(parameters.SaturationMinimum, parameters.SaturationMaximum);
				break;
			case PrimaryColorMethod.Analogous:
				newColor.H = (previousColor.H + parameters.AnalogousHueAngleIncrement / 360f).Wrap01();
				newColor.S = previousColor.S;
				break;
			case PrimaryColorMethod.Complementary:
				newColor.H = (previousColor.H + 0.5f).Wrap01();
				newColor.S = Random.Range(parameters.SaturationMinimum, parameters.SaturationMaximum);
				break;
			case PrimaryColorMethod.Compound:
				var compoundOffset = primaries.Count > 0 ? compoundColorOffsets[(primaries.Count - 1) % compoundColorOffsets.Count] : 0f;
				newColor.H = (previousColor.H + compoundOffset).Wrap01();
				newColor.S = Random.Range(parameters.SaturationMinimum, parameters.SaturationMaximum);
				break;
			case PrimaryColorMethod.Monochromatic:
				newColor.H = previousColor.H;
				newColor.S = Random.Range(parameters.SaturationMinimum, parameters.SaturationMaximum);
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
				newColor.L = Nasum.GaussianInRange(LightnessMaximumColor, parameters.LightnessAffinityStandardDeviation,
					LightnessBlack, LightnessWhite);
				break;
		}

		// Done
		return newColor;
	}

	#endregion

	#region Derived color generation

	private HSLColor GenerateDerivedColor()
	{
		var sourcePrimaryColor = primaries[sourceColumnIndex];
		var currentColumnColor = currentColorsByColumn[sourceColumnIndex];

		// Generate a random step vector
		var hueStep = Nasum.Range(-parameters.HueMaxStep, parameters.HueMaxStep);
		var saturationStep = Nasum.Range(-parameters.SaturationMaxStep, parameters.SaturationMaxStep);
		var lightnessStep = Nasum.Range(-parameters.LightnessMaxStep, parameters.LightnessMaxStep);
		var step = new Vector3(hueStep, saturationStep, lightnessStep);

		// Clamp the result color to be no more than parameters.WanderMax per-component from sourcePrimaryColor
		var hueMin = sourcePrimaryColor.H - parameters.HueWanderMax;
		var hueMax = sourcePrimaryColor.H + parameters.HueWanderMax;
		var saturationMin = Mathf.Max(0f, sourcePrimaryColor.S - parameters.SaturationWanderMax);
		var saturationMax = Mathf.Min(1f, sourcePrimaryColor.S + parameters.SaturationWanderMax);
		var lightnessMin = Mathf.Max(0f, sourcePrimaryColor.L - parameters.LightnessWanderMax);
		var lightnessMax = Mathf.Min(1f, sourcePrimaryColor.L + parameters.LightnessWanderMax);

		var hue = Mathf.Clamp(currentColumnColor.H + step.x, hueMin, hueMax).Wrap01();
		var saturation = Mathf.Clamp(currentColumnColor.S + step.y, saturationMin, saturationMax);
		var lightness = Mathf.Clamp(currentColumnColor.L + step.z, lightnessMin, lightnessMax);

		return new HSLColor(hue, saturation, lightness);
	}

	private void SelectNextColumn()
	{
		switch (parameters.DerivationSourceAffinity)
		{
			case DerivationSourceAffinity.None:
				sourceColumnIndex = Nasum.Range(0, parameters.PrimaryColorCount);
				break;
			case DerivationSourceAffinity.LessLightness:
				sourceColumnIndex = PickWithAffinity(primaries.OrderBy(c => c.L));
				break;
			case DerivationSourceAffinity.MoreLightness:
				sourceColumnIndex = PickWithAffinity(primaries.OrderByDescending(c => c.L));
				break;
			case DerivationSourceAffinity.LessSaturation:
				sourceColumnIndex = PickWithAffinity(primaries.OrderBy(c => c.S));
				break;
			case DerivationSourceAffinity.MoreSaturation:
				sourceColumnIndex = PickWithAffinity(primaries.OrderByDescending(c => c.S));
				break;
		}
	}

	private int PickWithAffinity(IEnumerable<HSLColor> orderedColors)
	{
		var intensity = parameters.DerivationSourceAffinityIntensity;
		var deviation = Mathf.Max(0.0001f, 1f - intensity);
		var randomIndex = Mathf.RoundToInt(Nasum.GaussianInRange(0f, deviation, 0f, 1f) * (orderedColors.Count() - 1));

		return primaries.IndexOf(orderedColors.ElementAt(randomIndex));
	}

	#endregion

	#region Component randomization methods

	private float RandomHue()
	{
		// TODO: Parameterize this as well?
		return Nasum.Range(HueMinimum, HueMaximum);
	}

	#endregion
}
