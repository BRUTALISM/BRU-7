using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public class Hull : MonoBehaviour
{
	#region Editor public fields

	public bool FlatShadedVertices = false;

	#endregion

	#region Public properties

	public IObservable<PartialMesh> HulledPartialMeshes { get { return hulled; } }

	#endregion

	#region Private fields

	private Subject<PartialMesh> hulled = new Subject<PartialMesh>();

	#endregion

	#region Unity methods

	void Start()
	{
		GetComponent<Mirror>().MirroredPoints.Subscribe(points => HullCalculation(points)).AddTo(this);
	}

	#endregion

	#region Convex hull calculation

	private class Triangle
	{
		List<Point> points;
		public IEnumerable<Point> Points { get { return points; } }

		public Vector3 Normal
		{
			get
			{
				PointCheck();
				return Vector3.Cross(points[1].Position - points[0].Position, points[2].Position - points[0].Position).normalized;
			}
		}

		public Triangle(IEnumerable<Point> points)
		{
			this.points = new List<Point>(points);
		}

		public bool CanSeePoint(Point point)
		{
			return Vector3.Angle(point.Position - points[0].Position, Normal) < 90f;
		}

		public void Reverse()
		{
			PointCheck();
			this.points = new List<Point>(3) { points[2], points[1], points[0] };
		}

		private void PointCheck()
		{
			if (points == null) throw new System.InvalidOperationException("points is null");
			if (points.Count != 3) throw new System.InvalidOperationException("points.Count is not 3");
		}
	}

	private void HullCalculation(List<Point> points)
	{
		// Perform QuickHull on a separate thread, then return back on main with the results
		Observable.Start(() => CalculateConvexHull(points)).ObserveOnMainThread().Subscribe(hulled.OnNext).AddTo(this);
	}

	private PartialMesh CalculateConvexHull(List<Point> points)
	{
		var triangles = new List<Triangle>();
		var remainingPoints = new HashSet<Point>(points);

		// TODO: Support for cases when points.Count <= 4

		// Find points with minimum and maximum Y and Z coordinates (could have also been X, doesn't matter, we need two)
		Point yMin, yMax, zMin, zMax;
		yMin = yMax = zMin = zMax = points[0];
		foreach (var point in points)
		{
			if (yMin == null || point.Position.y < yMin.Position.y)
			{
				yMin = point;
				remainingPoints.Remove(point);
			}
			else if (yMax == null || point.Position.y > yMax.Position.y)
			{
				yMax = point;
				remainingPoints.Remove(point);
			}

			if (zMin == null || point.Position.z < zMin.Position.z)
			{
				zMin = point;
				remainingPoints.Remove(point);
			}
			else if (zMax == null || point.Position.z > zMax.Position.z)
			{
				zMax = point;
				remainingPoints.Remove(point);
			}
		}

		// Create the triangles for the base tetrahedron
		var triangle = new Triangle(new List<Point>() { yMax, yMin, zMax });
		if (triangle.CanSeePoint(zMin)) triangle.Reverse();
		triangles.Add(triangle);

		triangle = new Triangle(new List<Point>() { yMax, zMin, yMin });
		if (triangle.CanSeePoint(zMax)) triangle.Reverse();
		triangles.Add(triangle);

		triangle = new Triangle(new List<Point>() { yMax, zMax, zMin });
		if (triangle.CanSeePoint(yMin)) triangle.Reverse();
		triangles.Add(triangle);

		triangle = new Triangle(new List<Point>() { zMin, zMax, yMin });
		if (triangle.CanSeePoint(yMax)) triangle.Reverse();
		triangles.Add(triangle);

//		while (remainingPoints.Count > 0)
//		{
			// TODO: Cull points
//		}

		return TrianglesToPartialMesh(triangles);
	}

	private PartialMesh TrianglesToPartialMesh(List<Triangle> triangles)
	{
		// Create a mesh from all the remaining triangles
		var partialMesh = new PartialMesh();
		if (FlatShadedVertices)
		{
			foreach (var triangle in triangles)
			{
				foreach (var point in triangle.Points)
				{
					// Duplicate each vertex so that each face is flat shaded
					partialMesh.Indices.Add(partialMesh.Vertices.Count);
					partialMesh.Vertices.Add(point.Position);
				}
			}
		}
		else
		{
			var pointIndices = new Dictionary<Point, int>();
			foreach (var triangle in triangles)
			{
				foreach (var point in triangle.Points)
				{
					if (pointIndices.ContainsKey(point))
					{
						// This point has already been added to the vertices list, just add its index again
						partialMesh.Indices.Add(pointIndices[point]);
					}
					else
					{
						var index = partialMesh.Vertices.Count;
						partialMesh.Indices.Add(index);
						pointIndices[point] = index;

						partialMesh.Vertices.Add(point.Position);
					}
				}
			}
		}

		return partialMesh;
	}

	#endregion
}
