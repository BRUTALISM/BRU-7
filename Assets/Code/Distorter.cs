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

	CompositeDisposable disposables = new CompositeDisposable();
	ReplaySubject<List<Point>> groupedPoints = new ReplaySubject<List<Point>>();

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
		Debug.Log(newPoint);
	}

	#endregion
}
