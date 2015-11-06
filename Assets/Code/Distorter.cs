using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class Distorter : MonoBehaviour
{
	#region Editor public fields
	#endregion

	#region Public properties

	public IObservable<List<Point>> GroupedPoints { get { return groupedPoints; } }

	#endregion

	#region Private fields

	private CompositeDisposable disposables = new CompositeDisposable();
	private ReplaySubject<List<Point>> groupedPoints = new ReplaySubject<List<Point>>();

	private List<Point> currentGroup = new List<Point>();

	#endregion

	#region Unity methods

	void OnEnable()
	{
		GetComponent<CloudGenerator>().Points.Subscribe(AddToGroup).AddTo(disposables);
	}

	void OnDisable()
	{
		disposables.Dispose();
	}

	#endregion

	#region Grouping and distorting

	private void AddToGroup(Point newPoint)
	{
		// TODO: Implement distortion.

		// TODO: Implement smarter grouping.

		if (currentGroup.Count == 4)
		{
			groupedPoints.OnNext(currentGroup);
			currentGroup = new List<Point>();
		}

		currentGroup.Add(newPoint);
	}

	#endregion
}
