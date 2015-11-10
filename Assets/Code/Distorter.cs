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

	private Subject<List<Point>> groupedPoints = new Subject<List<Point>>();

	private List<Point> currentGroup = new List<Point>();

	#endregion

	#region Unity methods

	void Start()
	{
		GetComponent<CloudGenerator>().Points.Subscribe(AddToGroup).AddTo(this);
	}

	#endregion

	#region Grouping and distorting

	private void AddToGroup(Point newPoint)
	{

		// TODO: Implement smarter grouping (add to nearest group?)

		if (currentGroup.Count == 8)
		{
			// TODO: Implement distortion and add distorted points to the group.

			groupedPoints.OnNext(currentGroup);
			currentGroup = new List<Point>();
		}

		currentGroup.Add(newPoint);
	}

	#endregion
}
