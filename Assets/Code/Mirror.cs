using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class Mirror : MonoBehaviour
{
	public enum ImperfectionSide { Left, Right, Both }

	#region Editor public fields

	public ImperfectionSide imperfectionSide = ImperfectionSide.Left;
	public float imperfectionMagnitude = 0;

	#endregion

	#region Public properties

	public IObservable<List<Point>> MirroredPoints { get { return mirroredPoints; } }

	#endregion

	#region Private fields

	private Subject<List<Point>> mirroredPoints = new Subject<List<Point>>();

	#endregion

	#region Unity methods

	void Start()
	{
		GetComponent<Distorter>().GroupedPoints.Subscribe(MirrorPoints).AddTo(this);
	}

	#endregion

	#region Mirrorring

	private void MirrorPoints(List<Point> points)
	{
		var mirrorAxis = GetComponent<CloudGenerator>().AxisOfSymmetry;

		// Create a new list of mirrored points
		var mirrored = new List<Point>(points.Count);
		foreach (var point in points)
		{
			var position = point.Position;
			if (mirrorAxis == CloudGenerator.Axis.XY) position.z *= -1;
			else if (mirrorAxis == CloudGenerator.Axis.XZ) position.y *= -1;
			else if (mirrorAxis == CloudGenerator.Axis.YZ) position.x *= -1;

			mirrored.Add(new Point(position, point.Weight));
		}

		// Apply imperfection offset
		for (int i = 0; i < points.Count; i++) {
			var offset = new Vector3(Random.value * imperfectionMagnitude,
			                         Random.value * imperfectionMagnitude,
			                         Random.value * imperfectionMagnitude);
			switch (imperfectionSide)
			{
				case ImperfectionSide.Left:
				mirrored[i].Position += offset;
				break;

				case ImperfectionSide.Right:
				points[i].Position += offset;
				break;

				case ImperfectionSide.Both:
				var pointToOffset = Random.value < 0.5 ? points[i] : mirrored[i];
				pointToOffset.Position += offset;
				break;
			}
		}

		// Publish both
		mirroredPoints.OnNext(points);
		mirroredPoints.OnNext(mirrored);
	}

	#endregion
}
