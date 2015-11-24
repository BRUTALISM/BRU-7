using UnityEngine;
using System.Collections.Generic;

public struct HSLColor
{
	#region Public properties

	public float H { get; set; }
	public float S { get; set; }
	public float L { get; set; }
	public float A { get; set; }

	#endregion

	#region Private fields
	#endregion

	#region UnityEngine.Color conversion operators

	public static implicit operator Color(HSLColor hsl)
	{
		return ToUnityColor(hsl);
	}

	public static implicit operator HSLColor(Color color)
	{
		return From(color);
	}

	#endregion

	#region Constructors

	public HSLColor(float h, float s, float l, float a)
	{
		H = h;
		S = s;
		L = l;
		A = a;
	}

	public HSLColor(float h, float s, float l) : this(h, s, l, 1f) {}

	public HSLColor(Color color)
	{
		var hsl = HSLColor.From(color);
		H = hsl.H;
		S = hsl.S;
		L = hsl.L;
		A = hsl.A;
	}

	public HSLColor(HSLColor other) : this(other.H, other.S, other.L, other.A) {}

	#endregion

	#region RGB to HSL conversion

	private static HSLColor From(float r, float g, float b, float a = 1f)
	{
		// Conversion algorithm adapted from http://www.easyrgb.com/index.php?X=MATH&H=19#text19
		float h = 0f;
		float s = 0f;
		float l = 0f;

		float rgbMin = Mathf.Min(r, g, b);
		float rgbMax = Mathf.Max(r, g, b);
		float rgbDelta = rgbMax - rgbMin;

		l = (rgbMax + rgbMin) / 2;

		if (rgbDelta != 0f)
		{
			// Chromatic
			if (l < 0.5f) s = rgbDelta / (rgbMax + rgbMin);
			else s = rgbDelta / (2 - rgbMax - rgbMin);

			float deltaR = (((rgbMax - r) / 6) + (rgbDelta / 2)) / rgbDelta;
			float deltaG = (((rgbMax - g) / 6) + (rgbDelta / 2)) / rgbDelta;
			float deltaB = (((rgbMax - b) / 6) + (rgbDelta / 2)) / rgbDelta;

			if (r == rgbMax) h = deltaB - deltaG;
			else if (g == rgbMax) h = (1f / 3) + deltaR - deltaB;
			else if (b == rgbMax) h = (2f / 3) + deltaG - deltaR;

			if (h < 0) h += 1f;
			if (h > 1) h -= 1f;
		}

		return new HSLColor(h, s, l, a);
	}

	private static HSLColor From(Color color)
	{
		return From(color.r, color.g, color.b, color.a);
	}

	#endregion

	#region HSL to RGB conversion

	private static Color ToUnityColor(HSLColor hsl)
	{
		// Conversion algorithm adapted from http://www.easyrgb.com/index.php?X=MATH&H=19#text19
		float r, g, b;
		float h = hsl.H;
		float s = hsl.S;
		float l = hsl.L;

		if (s == 0f)
		{
			r = g = b = l;
		}
		else
		{
			float f1 = l < 0.5f ? l * (1 + s) : (l + s) - (s * l);
			float f2 = 2 * l - f1;

			r = HueToRGB(f2, f1, h + (1f / 3));
			g = HueToRGB(f2, f1, h);
			b = HueToRGB(f2, f1, h - (1f / 3));
		}

		return new Color(r, g, b, hsl.A);
	}

	private static float HueToRGB(float v1, float v2, float vH)
	{
		// Also taken from http://www.easyrgb.com/index.php?X=MATH&H=19#text19
		if (vH < 0 ) vH += 1f;
		if (vH > 1 ) vH -= 1f;
		if ((6 * vH) < 1f) return (v1 + (v2 - v1) * 6 * vH);
		if ((2 * vH ) < 1f) return v2;
		if ((3 * vH ) < 2f) return (v1 + (v2 - v1) * ((2f / 3) - vH) * 6);
		return v1;
	}

	#endregion
}
