using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class Mirror : MonoBehaviour
{
	#region Editor public fields
	#endregion

	#region Public properties

	public IObservable<List<Point>> MirroredPoints { get { return mirroredPoints; } }

	#endregion

	#region Private fields

	private CompositeDisposable disposables = new CompositeDisposable();
	private ReplaySubject<List<Point>> mirroredPoints = new ReplaySubject<List<Point>>();

	#endregion

	#region Unity methods

	void OnEnable()
	{
		GetComponent<Distorter>().GroupedPoints.Subscribe(MirrorPoints).AddTo(disposables);
	}

	void OnDisable()
	{
		disposables.Dispose();
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

		// Publish both
		mirroredPoints.OnNext(points);
		mirroredPoints.OnNext(mirrored);
	}

	#endregion
}
