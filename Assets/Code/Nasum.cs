using System;
using System.Collections.Generic;

/// <summary>
/// A random number generator class which uses a single internal seed to generate values. Unlike Unity's Random class, this class'
/// generated values are guaranteed to be the same across invokations if both the seeds and the method invokation orders are the same.
/// </summary>
public static class Nasum
{
	private static Random random = null;

	private static int seed;

	/// <summary>
	/// The seed used by all methods of this class.
	/// </summary>
	public static int Seed
	{
		get { return seed; }
		set
		{
			seed = value;
			if (seed == int.MinValue) seed++;
			Init();
		}
	}

	/// <summary>
	/// A random value in the [0, 1] range.
	/// </summary>
	public static float Value
	{
		get
		{
			if (random == null) Init();
			return (float) random.NextDouble();
		}
	}

	/// <summary>
	/// A random rotation.
	/// </summary>
	public static UnityEngine.Quaternion Rotation
	{
		get
		{
			return UnityEngine.Quaternion.Euler(Range(0f, 360f), Range(0f, 360f), Range(0f, 360f));
		}
	}

	private static void Init()
	{
		random = new Random(seed);
	}

	/// <summary>
	/// Returns a random float in the [min, max] range.
	/// </summary>
	public static float Range(float min, float max)
	{
		return min + Value * (max - min);
	}

	/// <summary>
	/// Returns a random integer in the [min, max - 1] range.
	/// </summary>
	public static int Range(int min, int max)
	{
		return min + (int)(Value * (max - min - 1));
	}

	/// <summary>
	/// Returns a random number generated using the standard distribution with the given <paramref name="mean"/> and
	/// <paramref name="standardDeviation"/>.
	/// </summary>
	public static float Gaussian(float mean, float standardDeviation)
	{
		// Code taken from: http://stackoverflow.com/questions/5817490/implementing-box-mueller-random-number-generator-in-c-sharp
		float u, v, S;

		do
		{
			u = 2f * Value - 1f;
			v = 2f * Value - 1f;
			S = u * u + v * v;
		}
		while (S >= 1f);

		var fac = UnityEngine.Mathf.Sqrt(-2f * UnityEngine.Mathf.Log(S) / S);

		var normalRandom = u * fac;

		return normalRandom * standardDeviation + mean;
	}

	/// <summary>
	/// Returns a random number generated using the standard distribution with the given <paramref name="mean"/> and
	/// <paramref name="standardDeviation"/>, in the range given by <paramref name="rangeMin"/> and <paramref name="rangeMax"/>. The
	/// algorithm first obtains a gaussian random variable in a standard way, and then checks if it's within the given range. If it's not,
	/// the random roll is performed again using a uniform distribution in the [rangeMin, rangeMax] range.
	/// </summary>
	public static float GaussianInRange(float mean, float standardDeviation, float rangeMin, float rangeMax)
	{
		var randomValue = Gaussian(mean, standardDeviation);
		if (randomValue < rangeMin || randomValue > rangeMax) return Range(rangeMin, rangeMax);
		return randomValue;
	}
}
