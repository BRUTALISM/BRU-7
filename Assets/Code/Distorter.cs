using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public class Distorter : MonoBehaviour
{
	#region Editor public fields

	[Range(0f, 20f)]
	public float FieldIntensity = 10f;

	[Range(0.0001f, 0.1f)]
	public float FieldScale = 0.0001f;

	public int FirstTierPoints = 4;
	public int TierPointsIncrement = -1;
	public int MaxTiers = 5;

	public bool DrawGizmos = false;

	#endregion

	#region Public properties

	public IObservable<List<Point>> GroupedPoints { get { return groupedPoints; } }

	#endregion

	#region Private fields

	private Subject<List<Point>> groupedPoints = new Subject<List<Point>>();

	private List<Point> currentGroup = new List<Point>();

	private IVectorField vectorField;

	private float extent = 50f;
	private CloudGenerator.Axis axisOfSymmetry = CloudGenerator.Axis.YZ;

	#endregion

	#region Unity methods

	void Start()
	{
		var cloudGenerator = GetComponent<CloudGenerator>();
		if (cloudGenerator != null)
		{
			cloudGenerator.Points.Subscribe(AddToGroup).AddTo(this);
			extent = cloudGenerator.Extent * 2;
			axisOfSymmetry = cloudGenerator.AxisOfSymmetry;
		}

		vectorField = new RepeatedCubeVectorField();
	}

	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (Application.isPlaying && DrawGizmos)
		{
			Gizmos.color = Color.green;

			const float SamplingStep = 5f;
			int totalSteps = Mathf.CeilToInt(extent / SamplingStep);
			var samplingStartPosition = -extent / 2;
			for (int i = 0; i < totalSteps; i++)
			{
				float x = samplingStartPosition + i * SamplingStep;

				for (int j = 0; j < totalSteps; j++)
				{
					float y = samplingStartPosition + j * SamplingStep;

					for (int k = 0; k < totalSteps; k++)
					{
						float z = samplingStartPosition + k * SamplingStep;
						var rootPosition = new Vector3(x, y, z);
						var targetPosition = rootPosition + FieldAt(rootPosition);
						Draw.ArrowLine(rootPosition, targetPosition);
					}
				}
			}
		}
	}
	#endif

	#endregion

	#region Grouping and distorting

	private void AddToGroup(Point newPoint)
	{
		if (currentGroup.Count == FirstTierPoints)
		{
			var nextBatch = new List<Point>(currentGroup);
			var nextTierPoints = FirstTierPoints + TierPointsIncrement;
			var tierCount = 1;
			IEnumerable<Point> previousTier = currentGroup;
			while (tierCount < MaxTiers && nextTierPoints > 0)
			{
				var currentTier = previousTier
					.OrderBy(p => Random.value)
					.Take(nextTierPoints)
					.Select(p => new Point(p.Position + FieldAt(p.Position), p.Weight));
				
				nextBatch.AddRange(currentTier);

				nextTierPoints += TierPointsIncrement;
				previousTier = currentTier;
				tierCount++;
			}

			groupedPoints.OnNext(nextBatch);
			currentGroup = new List<Point>();
		}

		currentGroup.Add(newPoint);
	}

	private Vector3 FieldAt(Vector3 position)
	{
		var mirror = Vector3.one;
		switch (axisOfSymmetry)
		{
			case CloudGenerator.Axis.XY:
				mirror.z = Mathf.Sign(position.z);
				position.z = Mathf.Abs(position.z);
				break;
			case CloudGenerator.Axis.XZ:
				mirror.y = Mathf.Sign(position.y);
				position.y = Mathf.Abs(position.y);
				break;
			case CloudGenerator.Axis.YZ:
				mirror.x = Mathf.Sign(position.x);
				position.x = Mathf.Abs(position.x);
				break;
		}

		return Vector3.Scale(vectorField.VectorAt(position * FieldScale).normalized * FieldIntensity, mirror);
	}

	#endregion
}
