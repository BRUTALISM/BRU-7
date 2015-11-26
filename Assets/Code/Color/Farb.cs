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
	public InfinitePalette.Parameters PaletteGeneratorParameters;

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
		SeedGenerator.Seeds.Subscribe(_ => palettes.OnNext(new InfinitePalette(PaletteGeneratorParameters))).AddTo(this);
	}

	#endregion
}
