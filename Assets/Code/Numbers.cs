using UnityEngine;
using System.Collections.Generic;

public static class Numbers
{
	#region Float extensions

	/// <summary>
	/// Returns the fractional part of the given float (the part after the decimal point). For negative numbers, it returns a positive
	/// number calculated by subtracting the fractional part from 1.
	/// </summary>
	public static float PositiveFract(this float self)
	{
		return self - Mathf.Floor(self);
	}

	/// <summary>
	/// Wraps the given value so that it's always in the [0f, 1f] range.
	/// </summary>
	public static float Wrap01(this float self)
	{
		return self >= 0f && self <= 1f ? self : self - Mathf.Floor(self);
	}

	#endregion
}
