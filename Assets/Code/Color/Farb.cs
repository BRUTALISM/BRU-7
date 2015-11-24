using UnityEngine;
using System.Collections.Generic;
using UniRx;

/// <summary>
/// Palette generator.
/// </summary>
public class Farb : MonoBehaviour
{
	#region Singleton

	private static Farb scenestance;
	public static Farb Scenestance
	{
		get
		{
			if (scenestance == null)
			{
				scenestance = FindObjectOfType<Farb>();
			}

			return scenestance;
		}
	}

	#endregion

	#region Editor public fields

	public RandomSeedGenerator SeedGenerator;
	public float SaturationForAllColors = 1f;
	public float ValueForAllColors = 0.82f;
	public int NumberOfHuesMinimum = 1;
	public int NumberOfHuesMaximum = 5;
	public int TotalColorsMin = 5;
	public int TotalColorsMax = 10;
	public float HueAngleIncrement = 15f;

	#endregion

	#region Public properties

	public IObservable<InfinitePalette> Palettes { get { return palettes; } }

	#endregion

	#region Private data

	private Subject<InfinitePalette> palettes = new Subject<InfinitePalette>();

	#endregion

	#region Unity methods

	void Start()
	{
		SeedGenerator.Seeds.Subscribe(GenerateNewPalette).AddTo(this);
	}

	#endregion

	#region Palete generation

	private void GenerateNewPalette(int seed)
	{
		var seedColor = SeedToColor(seed);

		int totalColorCount = Nasum.Range(TotalColorsMin, TotalColorsMax + 1);

		int AngleStepsMax = Mathf.RoundToInt(360f / HueAngleIncrement);
		float hueAngle = HueAngleIncrement * Nasum.Range(1, AngleStepsMax);

		int numberOfHues = Nasum.Range(NumberOfHuesMinimum, NumberOfHuesMaximum + 1);

//		Debug.LogFormat("Palette[ {3} ]: hues={1}, angle={0}, colors={2}", hueAngle, numberOfHues, totalColorCount, seed);

//		palettes.OnNext(new InfinitePalette(HueAngleVariation(seedColor, hueAngle, numberOfHues, totalColorCount)));
	}

	private IEnumerable<Color> HueOffsetVariation(Color seed, float hueStep, int numberOfHues, int totalColorCount)
	{
		var colors = new List<Color>(totalColorCount);

		float hueOffset = 0f;
		float originalHue = seed.ToHSV().h;

		Color currentColor = seed;
		for (int i = 0; i < totalColorCount; i++)
		{
			if (i % numberOfHues == 0) hueOffset = 0f;

			currentColor = currentColor
				.SetHue((originalHue + hueOffset).Fract())
				.SetSaturation(SaturationForAllColors)
				.SetValue(ValueForAllColors);
			
			colors.Add(currentColor);

			hueOffset += hueStep;
		}

		return colors;
	}

	private IEnumerable<Color> HueAngleVariation(Color seed, float angle, int numberOfHues, int count)
	{
		return HueOffsetVariation(seed, angle / 360f, numberOfHues, count);
	}

	#endregion

	#region Helpers

	private Color SeedToColor(int seed)
	{
		// Take the absolute value of the seed
		seed = seed == int.MinValue ? int.MaxValue : Mathf.Abs(seed);

		// Randomly chosen constants to be used for mod operations
		const int RFactor = 34752;
		const int GFactor = 382;
		const int BFactor = 20244;

		// Make a color, component-wise
		Color seedColor = new Color(((float)(seed % RFactor)) / RFactor, ((float)(seed % GFactor)) / GFactor,
			((float)(seed % BFactor)) / BFactor);

		// Crank up value to 1
		Colors.HSV seedHSV = seedColor.ToHSV();
		seedHSV.v = 1f;
		seedColor = Colors.FromHSV(seedHSV);

		return seedColor;
	}

	#endregion
}
