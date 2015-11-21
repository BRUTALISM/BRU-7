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
		Nasum.Seed = SeedWhenEmpty;
		seedsSubject.OnNext(SeedWhenEmpty);
	}

	#endregion

	#region Seed generation

	private void ProcessNewInput(string input)
	{
		var inputChar = input[0];
		if ((int)inputChar == 8)
		{
			if (seedString.Length > 0)
			{
				// Backspace, remove last character
				seedString = seedString.Substring(0, seedString.Length - 1);
			}
		}
		else if (!char.IsLetterOrDigit(inputChar) && !char.IsWhiteSpace(inputChar))
		{
			return;
		}
		else
		{
			// Append new input
			seedString += inputChar;
		}

		var seed = SeedWhenEmpty;
		if (seedString.Length > 0)
		{
			seed = seedString.AsSafeEnumerable().Aggregate(SeedWhenEmpty, (acc, c) => acc * (1 + (int)c));
		}

		// Set the global seed
		Nasum.Seed = seed;

		Debug.LogFormat("[ {0} ] => [ {1} ]", seedString.Length > 0 ? seedString : "EMPTY", seed);

		seedsSubject.OnNext(seed);
	}

	#endregion
}
