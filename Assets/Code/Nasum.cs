using System;
using System.Collections.Generic;

public static class Nasum
{
	private static Random random = null;

	private static int seed;
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

	public static float Value
	{
		get
		{
			if (random == null) Init();
			return (float) random.NextDouble();
		}
	}

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

	public static float Range(float min, float max)
	{
		return min + Value * (max - min);
	}
}
