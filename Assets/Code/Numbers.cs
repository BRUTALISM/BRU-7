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

	#endregion
}
