using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility class for handling various color manipulations.
/// </summary>
public static class Colors
{
	public struct HSV
	{
		public float h;
		public float s;
		public float v;
	}

	#region Predefined colors and palettes

	// "Kelly's 22 colors of maximum contrast", google it.
	public static List<Color> Kellys22 = new List<Color>()
	{
		HexToColor("F2F3F4"), //  0 white
		HexToColor("222222"), //  1 black
		HexToColor("F3C300"), //  2 yellow
		HexToColor("875692"), //  3 purple
		HexToColor("F38400"), //  4 orange
		HexToColor("A1CAF1"), //  5 light blue
		HexToColor("BE0032"), //  6 red
		HexToColor("C2B280"), //  7 buff
		HexToColor("848482"), //  8 grey
		HexToColor("008856"), //  9 green
		HexToColor("E68FAC"), // 10 purplish pink
		HexToColor("0067A5"), // 11 blue
		HexToColor("F99379"), // 12 yellowish pink
		HexToColor("604E97"), // 13 violet
		HexToColor("F6A600"), // 14 orange yellow
		HexToColor("B3446C"), // 15 purplish red
		HexToColor("DCD300"), // 16 greenish yellow
		HexToColor("882D17"), // 17 reddish brown
		HexToColor("8DB600"), // 18 yellow green
		HexToColor("654522"), // 19 yellowish brown
		HexToColor("E25822"), // 20 reddish orange
		HexToColor("2B3D26")  // 21 olive green
	};

	public static List<Color> CuratedKellys22 = new List<Color>()
	{
		HexToColor("F3C300"), //  2 yellow
		HexToColor("875692"), //  3 purple
		HexToColor("F38400"), //  4 orange
		HexToColor("A1CAF1"), //  5 light blue
		HexToColor("BE0032"), //  6 red
		HexToColor("C2B280"), //  7 buff
		HexToColor("008856"), //  9 green
		HexToColor("E68FAC"), // 10 purplish pink
		HexToColor("0067A5"), // 11 blue
		HexToColor("F99379"), // 12 yellowish pink
		HexToColor("604E97"), // 13 violet
		HexToColor("F6A600"), // 14 orange yellow
		HexToColor("B3446C"), // 15 purplish red
		HexToColor("DCD300"), // 16 greenish yellow
		HexToColor("882D17"), // 17 reddish brown
		HexToColor("8DB600"), // 18 yellow green
		HexToColor("E25822"), // 20 reddish orange
	};

	public static List<Color> Neon = new List<Color>()
	{
		HexToColor("AAFF00"),
		HexToColor("FFAA00"),
		HexToColor("FF00AA"),
		HexToColor("AA00FF"),
		HexToColor("00AAFF")
	};

	public static List<Color> Noir = new List<Color>()
	{
		HexToColor("000000"),
		HexToColor("111111"),
		HexToColor("222222"),
		HexToColor("333333"),
		HexToColor("444444"),
		HexToColor("555555"),
		HexToColor("666666"),
		HexToColor("777777"),
//		HexToColor("888888"),
//		HexToColor("999999"),
//		HexToColor("AAAAAA"),
//		HexToColor("BBBBBB"),
//		HexToColor("CCCCCC"),
//		HexToColor("DDDDDD"),
//		HexToColor("EEEEEE"),
//		HexToColor("FFFFFF")
	};

	#endregion

	#region Conversions

	public static string ColorToHex(Color color, bool alpha = false)
	{
		Color32 color32 = (Color32) color;
		string hex = color32.r.ToString("X2") + color32.g.ToString("X2") + color32.b.ToString("X2") +
			(alpha ? color32.a.ToString("X2") : "");
		return hex;
	}

	public static Color HexToColor(string hex)
	{
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		return new Color32(r, g, b, 255);
	}

	public static Color IntToColor(int value, int hueSubdivision, int saturationSubdivision)
	{
		int hueIndex = (value * (hueSubdivision / 2 + 1)) % hueSubdivision;
		int saturationIndex = value / hueSubdivision;
		
		return FromHSV(((float)hueIndex) / hueSubdivision, 0.8f * (1f - ((float)saturationIndex) / saturationSubdivision), 1f);
	}

	#endregion

	#region HSV conversion

	public static Color FromHSV(HSV hsv)
	{
		return FromHSV(hsv.h, hsv.s, hsv.v);
	}

	// Decompiled from UnityEditor.EditorGUIUtilities.HSVToRGB
	public static Color FromHSV(float h, float s, float v)
	{
		h = Mathf.Clamp01(h);
		s = Mathf.Clamp01(s);
		v = Mathf.Clamp01(v);

		Color white = Color.white;
		if (s == 0f)
		{
			white.r = v;
			white.g = v;
			white.b = v;
		}
		else
		{
			if (v == 0f)
			{
				white.r = 0f;
				white.g = 0f;
				white.b = 0f;
			}
			else
			{
				white.r = 0f;
				white.g = 0f;
				white.b = 0f;
				float num = h * 6f;
				int num2 = (int)Mathf.Floor (num);
				float num3 = num - (float)num2;
				float num4 = v * (1f - s);
				float num5 = v * (1f - s * num3);
				float num6 = v * (1f - s * (1f - num3));
				int num7 = num2;
				switch (num7 + 1)
				{
					case 0:
						white.r = v;
						white.g = num4;
						white.b = num5;
						break;
					case 1:
						white.r = v;
						white.g = num6;
						white.b = num4;
						break;
					case 2:
						white.r = num5;
						white.g = v;
						white.b = num4;
						break;
					case 3:
						white.r = num4;
						white.g = v;
						white.b = num6;
						break;
					case 4:
						white.r = num4;
						white.g = num5;
						white.b = v;
						break;
					case 5:
						white.r = num6;
						white.g = num4;
						white.b = v;
						break;
					case 6:
						white.r = v;
						white.g = num4;
						white.b = num5;
						break;
					case 7:
						white.r = v;
						white.g = num6;
						white.b = num4;
						break;
				}
				white.r = Mathf.Clamp (white.r, 0f, 1f);
				white.g = Mathf.Clamp (white.g, 0f, 1f);
				white.b = Mathf.Clamp (white.b, 0f, 1f);
			}
		}
		return white;
	}

	// From: http://www.easyrgb.com/index.php?X=MATH&H=20#text20
	public static HSV RGBToHSV(float r, float g, float b)
	{
		float min = Mathf.Min(r, g, b);
		float max = Mathf.Max(r, g, b);
		float delta = max - min;

		float h = 0f;
		float s = 0f;
		float v = max;

		if (delta > 0f)
		{
			s = delta / max;

			float deltaR = (((max - r) / 6) + (delta / 2)) / delta;
			float deltaG = (((max - g) / 6) + (delta / 2)) / delta;
			float deltaB = (((max - b) / 6) + (delta / 2)) / delta;

			if (r == max) h = deltaB - deltaG;
			else if (g == max) h = (1f / 3) + deltaR - deltaB;
			else if (b == max) h = (2f / 3) + deltaG - deltaR;

			if (h < 0f) h += 1f;
			if (h > 1f) h -= 1f;
		}

		return new HSV() { h = h, s = s, v = v };
	}

	#endregion

	#region UnityEngine.Color HSV extensions

	public static HSV ToHSV(this Color color)
	{
		return RGBToHSV(color.r, color.g, color.b);
	}

	public static Color SetHue(this Color self, float hue)
	{
		HSV hsv = self.ToHSV();
		hsv.h = hue;
		return FromHSV(hsv);
	}

	public static Color SetSaturation(this Color self, float saturation)
	{
		HSV hsv = self.ToHSV();
		hsv.s = saturation;
		return FromHSV(hsv);
	}

	public static Color SetValue(this Color self, float value)
	{
		HSV hsv = self.ToHSV();
		hsv.v = value;
		return FromHSV(hsv);
	}

	public static Color MultiplyHue(this Color self, float factor)
	{
		HSV hsv = self.ToHSV();
		hsv.h *= factor;
		return FromHSV(hsv);
	}

	public static Color MultiplySaturation(this Color self, float factor)
	{
		HSV hsv = self.ToHSV();
		hsv.s *= factor;
		return FromHSV(hsv);
	}

	public static Color MultiplyValue(this Color self, float factor)
	{
		HSV hsv = self.ToHSV();
		hsv.v *= factor;
		return FromHSV(hsv);
	}

	public static Color RandomizeHue(this Color self, float maxHueDeviation)
	{
		float hueDeviation = Nasum.Range(-maxHueDeviation, maxHueDeviation).PositiveFract();
		HSV hsv = self.ToHSV();
		hsv.h += hueDeviation;
		return FromHSV(hsv);
	}

	public static Color RandomizeSaturation(this Color self, float maxSaturationDeviation)
	{
		float saturationDeviation = Nasum.Range(-maxSaturationDeviation, maxSaturationDeviation).PositiveFract();
		HSV hsv = self.ToHSV();
		hsv.s += saturationDeviation;
		return FromHSV(hsv);
	}

	public static Color RandomizeValue(this Color self, float maxValueDeviation)
	{
		float valueDeviation = Nasum.Range(-maxValueDeviation, maxValueDeviation).PositiveFract();
		HSV hsv = self.ToHSV();
		hsv.v += valueDeviation;
		return FromHSV(hsv);
	}

	#endregion
}