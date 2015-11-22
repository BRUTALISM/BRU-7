using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public class RandomSeedGenerator : MonoBehaviour
{
	#region Editor public fields

	public int SeedWhenEmpty = 7;

	#endregion

	#region Public properties

	public IObservable<int> Seeds { get { return seedsSubject; } }

	#endregion

	#region Private fields

	private Subject<int> seedsSubject = new Subject<int>();

	#endregion

	#region Unity methods

	void Start()
	{
		FindObjectOfType<StringInput>().InputStrings.Subscribe(ProcessInput).AddTo(this);

		// Fire out an initial seed, to start the pipeline
		Nasum.Seed = SeedWhenEmpty;
		seedsSubject.OnNext(SeedWhenEmpty);
	}

	#endregion

	#region Seed generation

	private void ProcessInput(string currentInput)
	{
		var seed = SeedWhenEmpty;
		if (currentInput.Length > 0)
		{
			seed = currentInput.AsSafeEnumerable().Aggregate(SeedWhenEmpty, (acc, c) => acc + (SeedWhenEmpty * (int)c));
		}

		Debug.LogFormat("[ {0} ] => [ {1} ]", currentInput.Length > 0 ? currentInput : "EMPTY", seed);

		// Set the global seed and fire it off
		Nasum.Seed = seed;
		seedsSubject.OnNext(seed);
	}

	#endregion
}
