using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class CloudGenerator : MonoBehaviour
{
	#region Editor public fields

	public Axis AxisOfSymmetry = Axis.YZ;
	public int InitialBatches = 1;
	public int PointsPerBatch = 5;
	public float NewBatchInterval = 2f;
	public float Extent = 100f;
	public float MaxDistanceBetweenGeneratedPoints = 5f;
	public float StartingPointWeight = 1f;

	#endregion

	#region Public properties

	public enum Axis { XY, XZ, YZ }

	public IObservable<List<Point>> PointBatches { get { return pointBatches; } }

	#endregion

	#region Private fields

	private Subject<List<Point>> pointBatches = new Subject<List<Point>>();

	#endregion

	#region Unity methods

	void Start()
	{
		InitialBatches.Times(_ => pointBatches.OnNext(NewBatch()));

		Observable.Interval(System.TimeSpan.FromSeconds(NewBatchInterval)).Subscribe(_ => pointBatches.OnNext(NewBatch())).AddTo(this);
	}

//	#if UNITY_EDITOR
//	void OnDrawGizmos()
//	{
//		var gizmoDimensions = Vector3.zero;
//		switch (AxisOfSymmetry)
//		{
//			case Axis.XY:
//				gizmoDimensions = new Vector3(Extent, Extent, 0f);
//				break;
//			case Axis.XZ:
//				gizmoDimensions = new Vector3(Extent, 0f, Extent);
//				break;
//			case Axis.YZ:
//				gizmoDimensions = new Vector3(0f, Extent, Extent);
//				break;
//		}
//		Gizmos.color = Color.green;
//		Gizmos.DrawWireCube(transform.position, gizmoDimensions);
//	}
//	#endif

	#endregion

	#region Generation

	private List<Point> NewBatch()
	{
		var points = new List<Point>();

		var x = AxisOfSymmetry == Axis.YZ ? Random.Range(0f, Extent) : Random.Range(-Extent, Extent);
		var y = AxisOfSymmetry == Axis.XZ ? Random.Range(0f, Extent) : Random.Range(-Extent, Extent);
		var z = AxisOfSymmetry == Axis.XY ? Random.Range(0f, Extent) : Random.Range(-Extent, Extent);
		var lastGeneratedPosition = new Vector3(x, y, z);
		points.Add(new Point(lastGeneratedPosition, StartingPointWeight));

		for (int numberOfPoints = 1; numberOfPoints < PointsPerBatch; numberOfPoints++)
		{
			var offsetLength = Random.Range(0f, MaxDistanceBetweenGeneratedPoints);
			var randomPosition = lastGeneratedPosition + (Random.rotation * Vector3.forward) * offsetLength;

			x = AxisOfSymmetry == Axis.YZ ? Mathf.Clamp(randomPosition.x, 0f, Extent) : Mathf.Clamp(randomPosition.x, -Extent, Extent);
			y = AxisOfSymmetry == Axis.XZ ? Mathf.Clamp(randomPosition.y, 0f, Extent) : Mathf.Clamp(randomPosition.y, -Extent, Extent);
			z = AxisOfSymmetry == Axis.XY ? Mathf.Clamp(randomPosition.z, 0f, Extent) : Mathf.Clamp(randomPosition.z, -Extent, Extent);
			lastGeneratedPosition = new Vector3(x, y, z);

			points.Add(new Point(lastGeneratedPosition, StartingPointWeight));
		}
			
		return points;
	}

	#endregion
}
