﻿using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public class Hull : MonoBehaviour
{
	#region Editor public fields

	public bool FlatShadedVertices = false;
	public bool RunOnSeparateThread = false;

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

	private class TrianglePoint : Point
	{
		private HashSet<Triangle> triangles = new HashSet<Triangle>();
		public IEnumerable<Triangle> Triangles { get { return triangles; } }

		public TrianglePoint(Vector3 position, float weight) : base(position, weight) { }
		public TrianglePoint(Point point) : base(point.Position, point.Weight) { }

		public void AddToTriangle(Triangle triangle)
		{
			if (triangle == null) throw new System.ArgumentException("Triangle can't be null when assigning to a point");
			triangles.Add(triangle);
		}

		public void RemoveFromTriangle(Triangle triangle)
		{
			if (triangle == null) throw new System.ArgumentException("Triangle can't be null when removing a point from it");
			if (!triangles.Contains(triangle)) Debug.LogWarning("Point being removed from triangle, but triangle not associated");
			triangles.Remove(triangle);
		}

		public List<TrianglePoint> GetSinglyLinkedNeighbours()
		{
			var neighbourLinkCounts = new Dictionary<TrianglePoint, int>();
			foreach (var triangle in triangles)
			{
				foreach (var neighbourPoint in triangle.Points)
				{
					if (neighbourPoint != this)
					{
						if (neighbourLinkCounts.ContainsKey(neighbourPoint)) neighbourLinkCounts[neighbourPoint]++;
						else neighbourLinkCounts[neighbourPoint] = 1;
					}
				}
			}

			return neighbourLinkCounts.Where(kvp => kvp.Value == 1).Select(kvp => kvp.Key).ToList();
		}
	}

	private class Triangle
	{
		List<TrianglePoint> points;
		public IEnumerable<TrianglePoint> Points { get { return points; } }

		public Vector3 Normal { get; private set; }

		public Triangle(IEnumerable<TrianglePoint> points)
		{
			this.points = new List<TrianglePoint>(points);
			foreach (var point in this.points) point.AddToTriangle(this);
			RecalculateNormal();
		}

		public bool IsFacingPoint(Point point)
		{
			return Vector3.Angle(point.Position - points[0].Position, Normal) < 90f;
		}

		public float DistanceTo(Point point)
		{
			return new Plane(Normal, points[0].Position).GetDistanceToPoint(point.Position);
		}

		public void Reverse()
		{
			PointCheck();
			var temp = points[0];
			points[0] = points[2];
			points[2] = temp;
			RecalculateNormal();
		}

		public void DetachPoints()
		{
			foreach (var point in points) point.RemoveFromTriangle(this);
			this.points = null;
		}

		public static bool PointsValid(Point p1, Point p2, Point p3)
		{
			return p1 != null && p2 != null && p3 != null && p1 != p2 && p1 != p3 && p2 != p3;
		}

		private void PointCheck()
		{
			if (points == null) throw new System.InvalidOperationException("points is null");
			if (points.Count != 3) throw new System.InvalidOperationException("points.Count is not 3");
		}

		private void RecalculateNormal()
		{
			Normal = Vector3.Cross(points[1].Position - points[0].Position, points[2].Position - points[0].Position).normalized;
		}
	}

	private void HullCalculation(List<Point> points)
	{
		if (RunOnSeparateThread)
		{
			// Perform QuickHull on a separate thread, then return back on main with the results
			Observable.Start(() => CalculateConvexHull(points)).ObserveOnMainThread().Subscribe(hulled.OnNext).AddTo(this);
		}
		else
		{
			hulled.OnNext(CalculateConvexHull(points));
		}
	}

	private PartialMesh CalculateConvexHull(List<Point> points)
	{
		var hullTriangles = new List<Triangle>();
		var remainingPoints = new HashSet<TrianglePoint>(points.Select(p => new TrianglePoint(p)));

		// TODO: Support for cases when points.Count <= 4

		// Find points with minimum and maximum Y and Z coordinates (could have also been X, doesn't matter, we need two)
		TrianglePoint yMin = null;
		TrianglePoint yMax = null;
		TrianglePoint zMin = null;
		TrianglePoint zMax = null;
		var iterationCopy = new List<TrianglePoint>(remainingPoints);
		foreach (var point in iterationCopy)
		{
			if (yMin == null || point.Position.y < yMin.Position.y) yMin = point;
			else if (yMax == null || point.Position.y > yMax.Position.y) yMax = point;

			if (zMin == null || point.Position.z < zMin.Position.z) zMin = point;
			else if (zMax == null || point.Position.z > zMax.Position.z) zMax = point;
		}

		if (yMin != null) remainingPoints.Remove(yMin);
		if (yMax != null) remainingPoints.Remove(yMax);
		if (zMin != null) remainingPoints.Remove(zMin);
		if (zMax != null) remainingPoints.Remove(zMax);

		// Create the triangles for the base tetrahedron
		Triangle baseTriangle = null;
		if (Triangle.PointsValid(yMax, yMin, zMax))
		{
			baseTriangle = new Triangle(new List<TrianglePoint>() { yMax, yMin, zMax });
			if (baseTriangle.IsFacingPoint(zMin)) baseTriangle.Reverse();
			hullTriangles.Add(baseTriangle);
		}

		if (Triangle.PointsValid(yMax, zMin, yMin))
		{
			baseTriangle = new Triangle(new List<TrianglePoint>() { yMax, zMin, yMin });
			if (baseTriangle.IsFacingPoint(zMax)) baseTriangle.Reverse();
			hullTriangles.Add(baseTriangle);
		}

		if (Triangle.PointsValid(yMax, zMax, zMin))
		{
			baseTriangle = new Triangle(new List<TrianglePoint>() { yMax, zMax, zMin });
			if (baseTriangle.IsFacingPoint(yMin)) baseTriangle.Reverse();
			hullTriangles.Add(baseTriangle);
		}

		if (Triangle.PointsValid(zMin, zMax, yMin))
		{
			baseTriangle = new Triangle(new List<TrianglePoint>() { zMin, zMax, yMin });
			if (baseTriangle.IsFacingPoint(yMax)) baseTriangle.Reverse();
			hullTriangles.Add(baseTriangle);
		}

		var trianglesToProcess = new Queue<Triangle>(hullTriangles);
		while (remainingPoints.Count > 0 && trianglesToProcess.Count > 0)
		{
			var currentTriangle = trianglesToProcess.Dequeue();

			// Find the point furthest away from the triangle
			TrianglePoint furthestPoint = null;
			float maxDistance = float.MinValue;
			var pointsFacingCurrentTriangle = new List<TrianglePoint>();
			foreach (var point in remainingPoints)
			{
				if (currentTriangle.IsFacingPoint(point))
				{
					pointsFacingCurrentTriangle.Add(point);
					if (furthestPoint == null || currentTriangle.DistanceTo(point) > maxDistance) furthestPoint = point;
				}
			}
			
			if (furthestPoint != null)
			{
				// Commence the triangle adding algo
				remainingPoints.Remove(furthestPoint);
				
				// Remove all triangles which are facing this point
				var seers = new List<Triangle>();
				foreach (var triangle in hullTriangles) if (triangle.IsFacingPoint(furthestPoint)) seers.Add(triangle);
				Assert.IsTrue(seers.Count >= 1);
				var pointsFromRemovedTriangles = new HashSet<TrianglePoint>();
				foreach (var triangleToBeRemoved in seers)
				{
					hullTriangles.Remove(triangleToBeRemoved);
					pointsFromRemovedTriangles.UnionWith(triangleToBeRemoved.Points);
					triangleToBeRemoved.DetachPoints();
				}

				// Find points from deleted triangles which are left without any triangle and delete them, as they will be inside the hull
				// after the new triangles are added
				// TODO: Maybe just add those points to pointsFacingCurrentTriangle, since they also get culled later in the algorithm?
				var pointsInsideHull = new List<TrianglePoint>();
				foreach (var point in pointsFromRemovedTriangles) if (point.Triangles.Count() == 0) pointsInsideHull.Add(point);
				pointsFromRemovedTriangles.ExceptWith(pointsInsideHull);

				// Sew the hole that has come up when removing the triangle(s) by attaching furthestPoint to hole edges
				var newTriangles = SewPointToHoleEdges(furthestPoint, pointsFromRemovedTriangles);

				// Check orientation of added triangles (if the triangle can see any hull point, reverse it)
				var hullPoint = hullTriangles[0].Points.ElementAt(0);
				foreach (var newTriangle in newTriangles) if (newTriangle.IsFacingPoint(hullPoint)) newTriangle.Reverse();

				hullTriangles.AddRange(newTriangles);
				foreach (var newTriangle in newTriangles) trianglesToProcess.Enqueue(newTriangle);

				// Cull the points which are on the inside
				foreach (var point in pointsFacingCurrentTriangle)
				{
					bool outside = false;
					foreach (var triangle in hullTriangles)
					{
						if (triangle.IsFacingPoint(point))
						{
							outside = true;
							break;
						}
					}
					
					if (!outside) remainingPoints.Remove(point);
				}
			}
		}

		Assert.IsTrue(remainingPoints.Count == 0);

		return TrianglesToPartialMesh(hullTriangles);
	}

	private List<Triangle> SewPointToHoleEdges(Point point, IEnumerable<TrianglePoint> edgePoints)
	{
		var triangles = new List<Triangle>();

		var topPoint = new TrianglePoint(point);

		var startPoint = edgePoints.ElementAt(0);
		var currentPoint = startPoint;
		TrianglePoint previousPoint = null;
		TrianglePoint nextPoint = null;
		do
		{
			var currentPointNeighbours = currentPoint.GetSinglyLinkedNeighbours();
			Assert.IsTrue(currentPointNeighbours.Count == 2);

			// Check that we're always moving in the same direction
			if (currentPointNeighbours[0] == previousPoint) nextPoint = currentPointNeighbours[1];
			else nextPoint = currentPointNeighbours[0];

			triangles.Add(new Triangle(new List<TrianglePoint>() { currentPoint, nextPoint, topPoint }));

			previousPoint = currentPoint;
			currentPoint = nextPoint;
		}
		while (nextPoint != startPoint);

		return triangles;
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
