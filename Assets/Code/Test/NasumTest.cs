using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class NasumTest : MonoBehaviour
{
	#region Editor public fields

	public int SamplesPerTest = 100;
	public float YScale = 1f;

	#endregion

	#region Public properties
	#endregion

	#region Private fields

	private List<Dictionary<int, int>> testResults = new List<Dictionary<int, int>>();
	private List<Color> gizmoColors = new List<Color>();

	#endregion

	#region Unity methods

	void OnEnable()
	{
		// Generate some test data
		GenerateTest(() => Nasum.Gaussian(0f, 1f));
		GenerateTest(() => Nasum.Gaussian(2f, 1f));
		GenerateTest(() => Nasum.Gaussian(0f, 5f));
		GenerateTest(() => Nasum.GaussianInRange(0f, 5f, -5f, 5f));
		GenerateTest(() => Nasum.GaussianInRange(0f, 2f, -5f, 5f));
		GenerateTest(() => Nasum.GaussianInRange(0f, 1f, -5f, 5f));
	}

	void OnDisable()
	{
		testResults.Clear();
		gizmoColors.Clear();
	}

	void OnDrawGizmos()
	{
		for (int testNumber = 0; testNumber < testResults.Count; testNumber++)
		{
			Vector3 origin = transform.position + Vector3.forward * testNumber * 10f;
			var testResult = testResults[testNumber];

			Gizmos.color = gizmoColors[testNumber];
			foreach (var keyValuePair in testResult)
			{
				var index = keyValuePair.Key;
				var occurences = keyValuePair.Value;
				Gizmos.DrawWireCube(origin + new Vector3(index, (YScale * occurences) / 2, 0f),
					new Vector3(0.1f, YScale * occurences, 0.1f));
			}
		}
	}

	#endregion

	#region Test generation

	private void GenerateTest(System.Func<float> randomGenerator)
	{
		var resultsDictionary = new Dictionary<int, int>();
		for (int i = 0; i < SamplesPerTest; i++)
		{
			var randomValue = randomGenerator();
			var flooredRandom = Mathf.FloorToInt(randomValue);
			if (resultsDictionary.ContainsKey(flooredRandom)) resultsDictionary[flooredRandom]++;
			else resultsDictionary[flooredRandom] = 1;
		}

		testResults.Add(resultsDictionary);

		gizmoColors.Add(new HSLColor(Random.Range(0f, 1f), 1f, 0.5f));
	}

	#endregion
}
