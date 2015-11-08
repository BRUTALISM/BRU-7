using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class CloudGenerator : MonoBehaviour
{
	#region Editor public fields

	public int NumberOfPoints = 100;
	public Axis AxisOfSymmetry = Axis.YZ;
	public float Extent = 100f;
	public float StartingPointWeight = 1f;
	public float NewPointInterval = 0.1f;

	#endregion

	#region Public properties

	public enum Axis { XY, XZ, YZ }

	public IObservable<Point> Points { get { return points; } }

	#endregion

	#region Private fields

	private Subject<Point> points = new Subject<Point>();

	#endregion

	#region Unity methods

	void Start()
	{
		GenerateCloud();

		Observable.Interval(System.TimeSpan.FromSeconds(NewPointInterval)).Subscribe((_) =>
		{
			points.OnNext(NewRandomPoint());
		}).AddTo(this);
	}

	#endregion

	#region Generation

	public void GenerateCloud()
	{
		for (int i = 0; i < NumberOfPoints; i++) points.OnNext(NewRandomPoint());
	}

	private Point NewRandomPoint()
	{
		float x = AxisOfSymmetry == Axis.YZ ? Random.Range(0f, Extent) : Random.Range(-Extent, Extent);
		float y = AxisOfSymmetry == Axis.XZ ? Random.Range(0f, Extent) : Random.Range(-Extent, Extent);
		float z = AxisOfSymmetry == Axis.XY ? Random.Range(0f, Extent) : Random.Range(-Extent, Extent);
		return new Point(new Vector3(x, y, z), StartingPointWeight);
	}

	#endregion
}
