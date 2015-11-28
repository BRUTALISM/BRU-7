using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class CloudGenerator : MonoBehaviour
{
	#region Editor public fields

	public RandomSeedGenerator SeedGenerator;
	public Axis AxisOfSymmetry = Axis.YZ;
	public int InitialBatches = 1;
	public int PointsPerBatch = 5;
	public float NewBatchInterval = 2f;
	public float Extent = 100f;
	public float MaxDistanceToPreviousBatch = 5f;
	public Vector3 DistanceToPreviousBatchScale = new Vector3(1f, 0.5f, 1f);
	public float MaxDistanceToPreviousPoint = 5f;
	public float StartingPointWeight = 1f;
	public Vector3 PointPositionAffinity = Vector3.zero;

	#endregion

	#region Public properties

	public enum Axis { XY, XZ, YZ }

	public IObservable<List<Point>> PointBatches { get { return pointBatches; } }

	#endregion

	#region Private fields

	private Subject<List<Point>> pointBatches = new Subject<List<Point>>();
	private Vector3 lastBatchCenter = Vector3.zero;

	#endregion

	#region Unity methods

	void Start()
	{
		SeedGenerator.Seeds.Subscribe(seed =>
		{
			lastBatchCenter = Vector3.zero;
			InitialBatches.Times(_ => pointBatches.OnNext(NewBatch()));
		}).AddTo(this);

//		Observable.Interval(System.TimeSpan.FromSeconds(NewBatchInterval)).Subscribe(_ => pointBatches.OnNext(NewBatch())).AddTo(this);
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

		var randomOffset = RandomPointOffset(MaxDistanceToPreviousBatch);

		var lastGeneratedPosition = lastBatchCenter + randomOffset;
		points.Add(new Point(lastGeneratedPosition, StartingPointWeight));

		lastBatchCenter = Vector3.zero;
		for (int numberOfPoints = 1; numberOfPoints < PointsPerBatch; numberOfPoints++)
		{
			randomOffset = RandomPointOffset(MaxDistanceToPreviousPoint);

			var randomPosition = lastGeneratedPosition + randomOffset;
			lastGeneratedPosition = ClampToExtent(randomPosition);

			points.Add(new Point(lastGeneratedPosition, StartingPointWeight));

			lastBatchCenter += lastGeneratedPosition;
		}

		lastBatchCenter /= points.Count;

		return points;
	}

	private Vector3 RandomPointOffset(float distance)
	{
		var randomOffset = (Nasum.Rotation * Vector3.forward + PointPositionAffinity).normalized;
		randomOffset *= Nasum.Range(0f, distance);
		randomOffset.Scale(DistanceToPreviousBatchScale);
		return randomOffset;
	}

	private Vector3 ClampToExtent(Vector3 vector)
	{
		var x = AxisOfSymmetry == Axis.YZ ? Mathf.Clamp(vector.x, 0f, Extent) : Mathf.Clamp(vector.x, -Extent, Extent);
		var y = AxisOfSymmetry == Axis.XZ ? Mathf.Clamp(vector.y, 0f, Extent) : Mathf.Clamp(vector.y, -Extent, Extent);
		var z = AxisOfSymmetry == Axis.XY ? Mathf.Clamp(vector.z, 0f, Extent) : Mathf.Clamp(vector.z, -Extent, Extent);
		return new Vector3(x, y, z);
	}

	#endregion
}
