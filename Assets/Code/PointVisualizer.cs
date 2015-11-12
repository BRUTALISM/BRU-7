using UnityEngine;
using System;
using System.Collections.Generic;
using UniRx;

public class PointVisualizer : MonoBehaviour
{
	#region Editor public fields

	public Material PointMaterial;

	#endregion

	#region Public properties
	#endregion

	#region Private fields

	private CompositeDisposable disposables = new CompositeDisposable();
	private List<GameObject> visualizations = new List<GameObject>();

	private Dictionary<float, Material> materialsByWeight = new Dictionary<float, Material>();

	#endregion

	#region Unity methods

	void OnEnable()
	{
		GetComponent<Mirror>().MirroredPoints.Subscribe(points =>
		{
			foreach (var point in points)
			{
				var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
				cube.transform.position = point.Position;
				cube.GetComponent<MeshRenderer>().sharedMaterial = MaterialForWeight(point.Weight);
				cube.transform.localScale = Vector3.one * 0.2f;
				
				cube.transform.SetParent(this.transform);
				
				visualizations.Add(cube);
			}
		}).AddTo(disposables);
	}

	void OnDisable()
	{
		disposables.Dispose();
		foreach (var vis in visualizations) Destroy(vis);
		visualizations.Clear();
	}

	#endregion

	#region Private methods

	private Material MaterialForWeight(float weight)
	{
		if (materialsByWeight.ContainsKey(weight)) return materialsByWeight[weight];
		var material = new Material(PointMaterial);
		material.color = new Color(1f, 1f - weight, 1f - weight);
		materialsByWeight[weight] = material;
		return material;
	}

	#endregion
}
