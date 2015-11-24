using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class RGBToHSLTest : MonoBehaviour
{
	#region Editor public fields

	public float CubeSize = 5f;

	#endregion

	#region Public properties
	#endregion

	#region Private fields
	#endregion

	#region Unity methods

	void Start()
	{}

	void OnDrawGizmos()
	{
		var hsl1 = Color.red;
		var hsl2 = Color.blue;
		var hsl3 = Color.green;
		var hsl4 = Color.magenta;
		var hsl5 = Color.cyan;
		var hsl6 = Color.yellow;
		var hsl7 = Color.black;

		var hslColors = new List<HSLColor>() { hsl1, hsl2, hsl3, hsl4, hsl5, hsl6, hsl7 };
		var xOffset = 0f;
		foreach (var hslColor in hslColors)
		{
			Gizmos.color = hslColor;
			Gizmos.DrawCube(transform.position + new Vector3(xOffset, 0f, 0f), Vector3.one * CubeSize);
			xOffset += CubeSize * 1.5f;
		}
	}

	#endregion
}
