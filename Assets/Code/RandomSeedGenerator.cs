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

	public IObservable<int> Seeds { get { return seedsObservable; } }

	#endregion

	#region Private fields

	private Subject<int> seedsObservable = new Subject<int>();

	private string seedString = "";

	#endregion

	#region Unity methods

	void Start()
	{
		// Handle keystrokes
		Observable
			.EveryUpdate()
			.Where(_ => Input.anyKeyDown && Input.inputString != null && Input.inputString.Length > 0)
			.Select(_ => Input.inputString)
			.Subscribe(ProcessNewInput)
			.AddTo(this);

		// Fire out an initial seed, to start the pipeline
		seedsObservable.OnNext(SeedWhenEmpty);
	}

	#endregion

	#region Seed generation

	private void ProcessNewInput(string input)
	{
		if ((int)input[0] == 8)
		{
			if (seedString.Length > 0)
			{
				// Backspace, remove last character
				seedString = seedString.Substring(0, seedString.Length - 1);
			}
		}
		else
		{
			// Append new input
			seedString += input;
		}

		var seed = SeedWhenEmpty;
		if (seedString.Length > 0)
		{
			seed = seedString.AsSafeEnumerable().Aggregate(SeedWhenEmpty, (acc, c) => acc * (1 + (int)c));
		}

		seedsObservable.OnNext(seed);

		Debug.LogFormat("---[ {0} ]---", seedString.Length > 0 ? seedString : "EMPTY");
	}

	#endregion
}
