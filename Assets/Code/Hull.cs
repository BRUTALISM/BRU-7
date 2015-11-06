using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public class Hull : MonoBehaviour
{
	#region Editor public fields
	#endregion

	#region Public properties

	public IObservable<PartialMesh> Hulled { get { return hulled; } }

	#endregion

	#region Private fields

	private CompositeDisposable disposables = new CompositeDisposable();

	private ReplaySubject<PartialMesh> hulled = new ReplaySubject<PartialMesh>();

	#endregion

	#region Unity methods

	void OnEnable()
	{
		GetComponent<Mirror>().MirroredPoints.Subscribe(CalculateConvexHull).AddTo(disposables);
	}

	void OnDisable()
	{
		disposables.Dispose();
	}

	#endregion

	#region Convex hull calculation

	private void CalculateConvexHull(List<Point> points)
	{
		// FIXME: Implement convex hull algo.

		var partialMesh = new PartialMesh();
		partialMesh.Vertices = points.Select(p => p.Position).ToList();
		partialMesh.Colors = points.Select(p => new Color(p.Weight, p.Weight, p.Weight)).ToList();
		partialMesh.Indices = new List<int>();
		for (int i = 0; i < points.Count; i++)
		{
			partialMesh.Indices.Add(i);
			partialMesh.Indices.Add((i + 1) % points.Count);
			partialMesh.Indices.Add((i + 2) % points.Count);

			// Double sided, bitches
			// FIXME: Nope.
			partialMesh.Indices.Add((i + 2) % points.Count);
			partialMesh.Indices.Add((i + 1) % points.Count);
			partialMesh.Indices.Add(i);
		}

		hulled.OnNext(partialMesh);
	}

	#endregion
}
