using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class HSLTest : MonoBehaviour
{
	#region Editor public fields

	public int Resolution = 16;
	public float CylinderHeight = 20f;
	public float CylinderRadius = 5f;
	public float GizmoCubeDimension = 1f;

	#endregion

	#region Public properties
	#endregion

	#region Private fields

	private HSLColor[,,] colors;

	#endregion

	#region Unity methods

	void OnEnable()
	{
		if (colors == null)
		{
			colors = new HSLColor[Resolution, Resolution, Resolution];
			for (int h = 0; h < Resolution; h++)
			{
				for (int s = 0; s < Resolution; s++)
				{
					for (int l = 0; l < Resolution; l++)
					{
						colors[h, s, l] = new HSLColor(((float)h) / Resolution, ((float)s) / Resolution, ((float)l) / Resolution);
					}
				}
			}
		}
	}

	void OnDrawGizmos()
	{
		for (int h = 0; h < Resolution; h++)
		{
			for (int s = 0; s < Resolution; s++)
			{
				for (int l = 0; l < Resolution; l++)
				{
					var color = colors[h, s, l];
					var x = Mathf.Cos(color.H * 2 * Mathf.PI) * color.S * CylinderRadius;
					var z = Mathf.Sin(color.H * 2 * Mathf.PI) * color.S * CylinderRadius;
					Gizmos.color = color;
					Gizmos.DrawWireCube(new Vector3(x, color.L * CylinderHeight, z), Vector3.one * GizmoCubeDimension);
				}
			}
		}
	}

	#endregion
}
