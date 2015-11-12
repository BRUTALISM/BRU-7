using UnityEngine;
using System.Collections.Generic;

public class PartialMesh
{
	public List<Vector3> Vertices = new List<Vector3>();
	public List<int> Indices = new List<int>();
	public List<Vector3> Normals = new List<Vector3>();
	public List<Color> Colors = new List<Color>();
	public List<Vector4> Tangents = new List<Vector4>();

	public PartialMesh() {}
	public PartialMesh(PartialMesh original)
	{
		this.Vertices.AddRange(original.Vertices);
		this.Indices.AddRange(original.Indices);
		this.Normals.AddRange(original.Normals);
		this.Colors.AddRange(original.Colors);
		this.Tangents.AddRange(original.Tangents);
	}
}
