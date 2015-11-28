using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Helper class for producing Unity's Mesh instances procedurally.
/// </summary>
public class MeshBuilder
{
	#region Constants

	// The flags used for skipping face generation when packing hexahedrons
	public const int OmitForwardFace = 1;
	public const int OmitBackFace = 2;
	public const int OmitLeftFace = 4;
	public const int OmitRightFace = 8;
	public const int OmitTopFace = 16;
	public const int OmitBottomFace = 32;

	#endregion

	#region Public data

	public List<Vector3> Vertices { get; private set; }
	public List<int> Indices { get; private set; }
	public List<Vector3> Normals { get; private set; }
	public List<Color> Colors { get; private set; }
	public List<Vector4> Tangents { get; private set; }
	public List<Vector2> UVs { get; private set; }

	public IEnumerable<PartialMesh> PartialMeshes { get { return partials; } }

	#endregion

	#region Private data

	private List<PartialMesh> partials = new List<PartialMesh>();

	#endregion

	#region Constructor

	public MeshBuilder()
	{
		Vertices = new List<Vector3>();
		Indices = new List<int>();
		Normals = new List<Vector3>();
		Colors = new List<Color>();
		Tangents = new List<Vector4>();
		UVs = new List<Vector2>();
	}

	#endregion

	#region Building & clearing

	/// <summary>
	/// Builds meshes based on geometry packed so far.
	/// </summary>
	public List<Mesh> Build()
	{
		// Build a partial mesh for any leftover verts, indices, etc.
		PreparePartials();

		return PackPartials();
	}

	/// <summary>
	/// Transfers any remaining vertices, indices, normals, and colors into a new PartialMesh that is appended to the end of
	/// <c>PartialMeshes</c>.
	/// </summary>
	public void PreparePartials()
	{
		if (Vertices.Count > 0)
		{
			var partialMesh = new PartialMesh();
			
			partialMesh.Vertices = Vertices;
			partialMesh.Indices = Indices;
			partialMesh.Normals = Normals;
			partialMesh.Colors = Colors;
			partialMesh.Tangents = Tangents;
			partialMesh.UVs = UVs;
			
			partials.Add(partialMesh);
		}

		Clear();
	}

	/// <summary>
	/// Wraps mesh builder's geometry arrays into a new PartialMesh instance. Does not take into account any previously built partial
	/// meshes, nor does it clear mesh builder's internal lists. Use this only in cases where you need a local mesh builder to pack you some
	/// geometry and then use the produced partial mesh to pack it into some other mesh builder.
	/// </summary>
	/// <returns>The partial.</returns>
	public PartialMesh BuildPartial()
	{
		// TODO: Add splitting support for large partial meshes (>64k verts).

		PartialMesh partialMesh = new PartialMesh();
		partialMesh.Vertices = new List<Vector3>(Vertices);
		partialMesh.Indices = new List<int>(Indices);
		partialMesh.Normals = new List<Vector3>(Normals);
		partialMesh.Colors = new List<Color>(Colors);
		partialMesh.Tangents = new List<Vector4>(Tangents);
		partialMesh.UVs = new List<Vector2>(UVs);

		return partialMesh;
	}

	/// <summary>
	/// Clears mesh builder's internal geometry lists. Does not clear partial meshes.
	/// </summary>
	public void Clear()
	{
		if (Vertices.Count > 0) Vertices = new List<Vector3>();
		if (Indices.Count > 0) Indices = new List<int>();
		if (Normals.Count > 0) Normals = new List<Vector3>();
		if (Colors.Count > 0) Colors = new List<Color>();
		if (Tangents.Count > 0) Tangents = new List<Vector4>();
		if (UVs.Count > 0) UVs = new List<Vector2>();
	}

	#endregion

	#region Packing methods

	/// <summary>
	/// Pack the specified vertices and indices into this builder. The indices are expected to be relative to the
	/// <paramref name="vertices"/> array, not the already packed stuff in the builder (otherwise there'd be no point in
	/// having this class).
	/// </summary>
	public void Pack(List<Vector3> vertices, List<int> indices, List<Vector3> normals = null, List<Color> colors = null,
		List<Vector4> tangents = null, List<Vector2> uvs = null)
	{
		if (vertices.Count > 65534) throw new System.ArgumentException("vertices.Count must be less than 65534");
		if (this.Vertices.Count + vertices.Count > 65534) PreparePartials();

		// Copy indices
		int indexStart = this.Vertices.Count;
		foreach (int index in indices) this.Indices.Add(indexStart + index);

		// Copy vertices
		this.Vertices.AddRange(vertices);

		// Copy colors
		if (colors != null) this.Colors.AddRange(colors);

		// Copy normals
		if (normals != null) this.Normals.AddRange(normals);

		if (tangents != null) this.Tangents.AddRange(tangents);

		if (uvs != null) this.UVs.AddRange(uvs);
	}

	/// <summary>
	/// Pack the specified partialMesh into the builder. See the overloaded Pack method for details.
	/// </summary>
	public void Pack(PartialMesh partialMesh)
	{
		Pack(partialMesh.Vertices, partialMesh.Indices, partialMesh.Normals, partialMesh.Colors, partialMesh.Tangents, partialMesh.UVs);
	}

	#endregion

	#region Specific packing methods

	// TODO: Refactor this entire region into a separate class, no need for MeshBuilder to know anything about quads, cuboids, etc.

	/// <summary>
	/// Packs the cuboid (AABB) with the given <param name="bounds"> and <paramref name="color"/> into the builder.
	/// </summary>
	public void PackCuboid(Bounds bounds, Color? color = null, int? omitFaceFlags = null, float bevel = 0f)
	{
		PackCuboid(bounds.center, bounds.size, color, omitFaceFlags, bevel);
	}

	/// <summary>
	/// Packs the cuboid given with <paramref name="center"/> and <paramref name="size"/> into the builder, using the specified
	/// <paramref name="color"/> and <paramref name="omitFaceFlags"/> (see the PackHexahedron method for flag explanation).
	/// </summary>
	public void PackCuboid(Vector3 center, Vector3 size, Color? color = null, int? omitFaceFlags = null, float bevel = 0f)
	{
		Vector3 v0 = center + new Vector3(-size.x / 2, size.y / 2, size.z / 2);
		Vector3 v1 = center + new Vector3(size.x / 2, size.y / 2, size.z / 2);
		Vector3 v2 = center + new Vector3(size.x / 2, -size.y / 2, size.z / 2);
		Vector3 v3 = center + new Vector3(-size.x / 2, -size.y / 2, size.z / 2);
		Vector3 v4 = center + new Vector3(-size.x / 2, size.y / 2, -size.z / 2);
		Vector3 v5 = center + new Vector3(size.x / 2, size.y / 2, -size.z / 2);
		Vector3 v6 = center + new Vector3(size.x / 2, -size.y / 2, -size.z / 2);
		Vector3 v7 = center + new Vector3(-size.x / 2, -size.y / 2, -size.z / 2);

		PackHexahedron(new List<Vector3>() { v0, v1, v2, v3, v4, v5, v6, v7 }, color, omitFaceFlags, bevel);
	}

	/// <summary>
	/// Packs the quad given by the <paramref name="vertices"/> list into the builder. The triangles will be calculated by traversing the
	/// vertices in the order given in the list, and normals will be calculated accordingly. If <paramref name="vertices"/> has more than
	/// 4 elements, they are ignored. If <paramref name="color"/> is set, each vertex's color is set to that color.
	/// </summary>
	public void PackQuad(List<Vector3> vertices, Color? color = null)
	{
		if (vertices == null || vertices.Count != 4) throw new System.ArgumentException("vertices list needs to have exactly 4 elements");

		List<int> indices = new List<int>(6) { 2, 1, 0, 0, 3, 2};

		var normals = GetQuadNormals(vertices);

		List<Color> colors = null;
		if (color.HasValue) colors = new List<Color>(4) { color.Value, color.Value, color.Value, color.Value };

		Pack(vertices, indices, normals, colors);
	}

	public void PackQuad(Vector3 position, Vector2 size, Vector3 normal, Vector3 upTangent, Color? color = null)
	{
		Vector3 left = Vector3.Cross(upTangent, normal);
		upTangent = upTangent.normalized;

		PackQuad(new List<Vector3>()
		{
			position + left * size.x - upTangent * size.y,
			position - left * size.x - upTangent * size.y,
			position - left * size.x + upTangent * size.y,
			position + left * size.x + upTangent * size.y
		}, color);
	}

	/// <summary>
	/// Packs hexahedron given by <paramref name="vertices"/> into the builder. The <paramref name="vertices"/> list needs to have exactly 8
	/// elements. The given <paramref name="color"/> is set as vertex color. If <paramref name="omitFace"/> is set, the flags are read from
	/// it and the faces mentioned there are not added to the indices array (the vertices still are). If <paramref name="bevel"/> is set,
	/// the hexahedron sides are shrunk by the given amount and an additional face is generated between the main faces, to create a beveled
	/// edge.
	/// 
	/// The <paramref name="vertices"/> need to be specified in the following order:
	/// 
	///                 top
	///                  |
	///                  v
	///            0-----------1
	///           /.          /|
	///          / .    fwd  / |
	///         4-----------5  |
	/// left -> |  3 . . . .|. 2 <- right
	///         | .  back   | /
	///         |.          |/
	///         7-----------6
	///               ^
	///               |
	///             bottom
	/// 
	/// </summary>
	public void PackHexahedron(List<Vector3> vertices, Color? color = null, int? omitFace = null, float bevel = 0f)
	{
		if (vertices == null || vertices.Count != 8) throw new System.ArgumentException("vertices list needs to have exactly 8 elements");

		bool hasTopFace = true;
		bool hasBottomFace = true;
		bool hasLeftFace = true;
		bool hasRightFace = true;
		bool hasForwardFace = true;
		bool hasBackFace = true;
		if (omitFace.HasValue)
		{
			hasTopFace = (omitFace.Value & OmitTopFace) == 0;
			hasBottomFace = (omitFace.Value & OmitBottomFace) == 0;
			hasLeftFace = (omitFace.Value & OmitLeftFace) == 0;
			hasRightFace = (omitFace.Value & OmitRightFace) == 0;
			hasForwardFace = (omitFace.Value & OmitForwardFace) == 0;
			hasBackFace = (omitFace.Value & OmitBackFace) == 0;
		}

		if (bevel > 0f)
		{
			// --------------------------------------------------------------------
			// Modify each side's vertices to account for the bevel where necessary
			// --------------------------------------------------------------------

			Vector3 forwardBevelOffset = (hasForwardFace ? Vector3.back * bevel : Vector3.zero);
			Vector3 backBevelOffset = (hasBackFace ? Vector3.forward * bevel : Vector3.zero);
			Vector3 leftBevelOffset = (hasLeftFace ? Vector3.right * bevel : Vector3.zero);
			Vector3 rightBevelOffset = (hasRightFace ? Vector3.left * bevel : Vector3.zero);
			Vector3 topBevelOffset = (hasTopFace ? Vector3.down * bevel : Vector3.zero);
			Vector3 bottomBevelOffset = (hasBottomFace ? Vector3.up * bevel : Vector3.zero);

			// Top
			Vector3 top0 = vertices[0] + forwardBevelOffset + leftBevelOffset;		// 0
			Vector3 top4 = vertices[4] + backBevelOffset + leftBevelOffset;			// 1
			Vector3 top5 = vertices[5] + rightBevelOffset + backBevelOffset;		// 2
			Vector3 top1 = vertices[1] + forwardBevelOffset + rightBevelOffset;		// 3

			// Bottom
			Vector3 bottom3 = vertices[3] + forwardBevelOffset + leftBevelOffset;	// 4
			Vector3 bottom2 = vertices[2] + forwardBevelOffset + rightBevelOffset;	// 5
			Vector3 bottom6 = vertices[6] + rightBevelOffset + backBevelOffset;		// 6
			Vector3 bottom7 = vertices[7] + backBevelOffset + leftBevelOffset;		// 7

			// Left
			Vector3 left0 = vertices[0] + topBevelOffset + forwardBevelOffset;		// 8
			Vector3 left3 = vertices[3] + bottomBevelOffset + forwardBevelOffset;	// 9
			Vector3 left7 = vertices[7] + bottomBevelOffset + backBevelOffset;		// 10
			Vector3 left4 = vertices[4] + backBevelOffset + topBevelOffset;			// 11

			// Right
			Vector3 right1 = vertices[1] + topBevelOffset + forwardBevelOffset;		// 12
			Vector3 right5 = vertices[5] + backBevelOffset + topBevelOffset;		// 13
			Vector3 right6 = vertices[6] + bottomBevelOffset + backBevelOffset;		// 14
			Vector3 right2 = vertices[2] + bottomBevelOffset + forwardBevelOffset;	// 15

			// Forward
			Vector3 forward0 = vertices[0] + topBevelOffset + leftBevelOffset;		// 16
			Vector3 forward1 = vertices[1] + topBevelOffset + rightBevelOffset;		// 17
			Vector3 forward2 = vertices[2] + rightBevelOffset + bottomBevelOffset;	// 18
			Vector3 forward3 = vertices[3] + leftBevelOffset + bottomBevelOffset;	// 19

			// Back
			Vector3 back4 = vertices[4] + topBevelOffset + leftBevelOffset;			// 20
			Vector3 back7 = vertices[7] + leftBevelOffset + bottomBevelOffset;		// 21
			Vector3 back6 = vertices[6] + rightBevelOffset + bottomBevelOffset;		// 22
			Vector3 back5 = vertices[5] + topBevelOffset + rightBevelOffset;		// 23

			// ------------------
			// Now pack each face
			// ------------------
			List<Vector3> vs = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<int> indices = new List<int>();
			List<Color> colors = null;

			// Top
			List<Vector3> quadVertices = new List<Vector3>(4) { top0, top4, top5, top1 };
			vs.AddRange(quadVertices);
			normals.AddRange(GetQuadNormals(quadVertices));
			if (hasTopFace)
			{
				indices.Add(3); indices.Add(2); indices.Add(1);
				indices.Add(1); indices.Add(0); indices.Add(3);
			}

			// Bottom
			quadVertices = new List<Vector3>(4) { bottom3, bottom2, bottom6, bottom7 };
			vs.AddRange(quadVertices);
			normals.AddRange(GetQuadNormals(quadVertices));
			if (hasBottomFace)
			{
				indices.Add(7); indices.Add(6); indices.Add(5);
				indices.Add(5); indices.Add(4); indices.Add(7);
			}

			// Left
			quadVertices = new List<Vector3>(4) { left0, left3, left7, left4 };
			vs.AddRange(quadVertices);
			normals.AddRange(GetQuadNormals(quadVertices));
			if (hasLeftFace)
			{
				indices.Add(11); indices.Add(10); indices.Add(9);
				indices.Add(9); indices.Add(8); indices.Add(11);
			}

			// Right
			quadVertices = new List<Vector3>(4) { right1, right5, right6, right2 };
			vs.AddRange(quadVertices);
			normals.AddRange(GetQuadNormals(quadVertices));
			if (hasRightFace)
			{
				indices.Add(15); indices.Add(14); indices.Add(13);
				indices.Add(13); indices.Add(12); indices.Add(15);
			}

			// Forward
			quadVertices = new List<Vector3>(4) { forward0, forward1, forward2, forward3 };
			vs.AddRange(quadVertices);
			normals.AddRange(GetQuadNormals(quadVertices));
			if (hasForwardFace)
			{
				indices.Add(19); indices.Add(18); indices.Add(17);
				indices.Add(17); indices.Add(16); indices.Add(19);
			}

			// Back
			quadVertices = new List<Vector3>(4) { back4, back7, back6, back5 };
			vs.AddRange(quadVertices);
			normals.AddRange(GetQuadNormals(quadVertices));
			if (hasBackFace)
			{
				indices.Add(23); indices.Add(22); indices.Add(21);
				indices.Add(21); indices.Add(20); indices.Add(23);
			}

			if (color.HasValue)
			{
				colors = new List<Color>(vs.Count);
				for (int i = 0; i < vs.Count; i++) colors.Add(color.Value);
			}

			// Pack bevel side triangles
			if (hasTopFace && hasForwardFace) indices.AddRange(new List<int>() { 3, 0, 16, 16, 17, 3 });		// Top/forward
			if (hasTopFace && hasRightFace) indices.AddRange(new List<int>() { 2, 3, 12, 12, 13, 2 });			// Top/right
			if (hasTopFace && hasBackFace) indices.AddRange(new List<int>() { 1, 2, 23, 23, 20, 1 });			// Top/back
			if (hasTopFace && hasLeftFace) indices.AddRange(new List<int>() { 0, 1, 11, 11, 8, 0 });			// Top/left
			if (hasBottomFace && hasForwardFace) indices.AddRange(new List<int>() { 18, 19, 4, 4, 5, 18 });		// Bottom/forward
			if (hasBottomFace && hasRightFace) indices.AddRange(new List<int>() { 14, 15, 5, 5, 6, 14 });		// Bottom/right
			if (hasBottomFace && hasBackFace) indices.AddRange(new List<int>() { 21, 22, 6, 6, 7, 21 });		// Bottom/back
			if (hasBottomFace && hasLeftFace) indices.AddRange(new List<int>() { 9, 10, 7, 7, 4, 9 });			// Bottom/left
			if (hasLeftFace && hasBackFace) indices.AddRange(new List<int>() { 20, 21, 10, 10, 11, 20 });		// Left/back
			if (hasLeftFace && hasForwardFace) indices.AddRange(new List<int>() { 8, 9, 19, 19, 16, 8 });		// Left/forward
			if (hasRightFace && hasBackFace) indices.AddRange(new List<int>() { 22, 23, 13, 13, 14, 22 });		// Right/back
			if (hasRightFace && hasForwardFace) indices.AddRange(new List<int>() { 15, 12, 17, 17, 18, 15 });	// Right/forward

			// Pack bevel corners triangles
			if (hasTopFace && hasForwardFace && hasLeftFace) indices.AddRange(new List<int>() { 0, 8, 16 });		// 0
			if (hasTopFace && hasForwardFace && hasRightFace) indices.AddRange(new List<int>() { 3, 17, 12 });		// 1
			if (hasForwardFace && hasRightFace && hasBottomFace) indices.AddRange(new List<int>() { 15, 18, 5 });	// 2
			if (hasForwardFace && hasLeftFace && hasBottomFace) indices.AddRange(new List<int>() { 19, 9, 4 });		// 3
			if (hasTopFace && hasLeftFace && hasBackFace) indices.AddRange(new List<int>() { 1, 20, 11 });			// 4
			if (hasTopFace && hasRightFace && hasBackFace) indices.AddRange(new List<int>() { 2, 13, 23 });			// 5
			if (hasBackFace && hasBottomFace && hasRightFace) indices.AddRange(new List<int>() { 22, 14, 6 });		// 6
			if (hasLeftFace && hasBackFace && hasBottomFace) indices.AddRange(new List<int>() { 10, 21, 7 });		// 7

			// Finish up
			Pack(vs, indices, normals, colors);
		}
		else
		{
			if (hasForwardFace)
			{
				var faceVertices = new List<Vector3>() { vertices[0], vertices[1], vertices[2], vertices[3] };
				PackQuad(faceVertices, color);
			}
			if (hasBackFace)
			{
				var faceVertices = new List<Vector3>() { vertices[7], vertices[6], vertices[5], vertices[4] };
				PackQuad(faceVertices, color);
			}
			if (hasLeftFace)
			{
				var faceVertices = new List<Vector3>() { vertices[4], vertices[0], vertices[3], vertices[7] };
				PackQuad(faceVertices, color);
			}
			if (hasRightFace)
			{
				var faceVertices = new List<Vector3>() { vertices[1], vertices[5], vertices[6], vertices[2] };
				PackQuad(faceVertices, color);
			}
			if (hasTopFace)
			{
				var faceVertices = new List<Vector3>() { vertices[4], vertices[5], vertices[1], vertices[0] };
				PackQuad(faceVertices, color);
			}
			if (hasBottomFace)
			{
				var faceVertices = new List<Vector3>() { vertices[2], vertices[6], vertices[7], vertices[3] };
				PackQuad(faceVertices, color);
			}
		}
	}

	#endregion

	#region Helper methods

	public static List<Vector3> Corners(Bounds bounds)
	{
		Vector3 center = bounds.center;
		Vector3 size = bounds.size;

		Vector3 v0 = center + new Vector3(-size.x / 2, size.y / 2, size.z / 2);
		Vector3 v1 = center + new Vector3(size.x / 2, size.y / 2, size.z / 2);
		Vector3 v2 = center + new Vector3(size.x / 2, -size.y / 2, size.z / 2);
		Vector3 v3 = center + new Vector3(-size.x / 2, -size.y / 2, size.z / 2);
		Vector3 v4 = center + new Vector3(-size.x / 2, size.y / 2, -size.z / 2);
		Vector3 v5 = center + new Vector3(size.x / 2, size.y / 2, -size.z / 2);
		Vector3 v6 = center + new Vector3(size.x / 2, -size.y / 2, -size.z / 2);
		Vector3 v7 = center + new Vector3(-size.x / 2, -size.y / 2, -size.z / 2);

		return new List<Vector3>() { v0, v1, v2, v3, v4, v5, v6, v7 };
	}

	#endregion

	#region Private methods

	private List<Vector3> GetQuadNormals(List<Vector3> quadVertices)
	{
		Vector3 normal1 = Vector3.Cross(quadVertices[2] - quadVertices[0], quadVertices[1] - quadVertices[0]).normalized;
		Vector3 normal2 = Vector3.Cross(quadVertices[0] - quadVertices[2], quadVertices[3] - quadVertices[2]).normalized;
		return new List<Vector3>(4) { normal1, normal1, normal2, normal2 };
	}

	private List<Mesh> PackPartials()
	{
		var meshes = new List<Mesh>();
		foreach (var partialMesh in partials)
		{
			Mesh mesh = new Mesh();
			
			mesh.vertices = partialMesh.Vertices.ToArray();
			mesh.triangles = partialMesh.Indices.ToArray();
			
			if (partialMesh.Normals != null && partialMesh.Normals.Count > 0) mesh.normals = partialMesh.Normals.ToArray();
			if (partialMesh.Colors != null && partialMesh.Colors.Count > 0) mesh.colors = partialMesh.Colors.ToArray();
			if (partialMesh.Tangents != null && partialMesh.Tangents.Count > 0) mesh.tangents = partialMesh.Tangents.ToArray();
			if (partialMesh.UVs != null && partialMesh.UVs.Count > 0) mesh.uv = partialMesh.UVs.ToArray();

			meshes.Add(mesh);
		}

		return meshes;
	}

	#endregion
}