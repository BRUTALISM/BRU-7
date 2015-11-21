using UnityEngine;
using System.Collections.Generic;

public static class Numbers
{
	#region Float extensions

	/// <summary>
	/// Returns the fractional part of the given float (the part after the decimal point).
	/// </summary>
	public static float Fract(this float self)
	{
		return self - Mathf.Floor(self);
	}

	#endregion
}
