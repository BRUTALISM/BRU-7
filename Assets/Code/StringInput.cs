using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class StringInput : MonoBehaviour
{
	#region Editor public fields
	#endregion

	#region Public properties

	public IObservable<string> InputStrings { get { return inputStringSubject; } }

	#endregion

	#region Private fields

	private Subject<string> inputStringSubject = new Subject<string>();

	private string currentInputString = "";

	#endregion

	#region Unity methods

	void Start()
	{
		// Handle keystrokes
		Observable
			.EveryUpdate()
			.Where(_ => Input.anyKeyDown && Input.inputString != null && Input.inputString.Length > 0)
			.Select(_ => Input.inputString[0])
			.Subscribe(ProcessNewInput)
			.AddTo(this);
	}

	#endregion

	#region Input processing

	public void Append(char input)
	{
		ProcessNewInput(input);
	}

	private void ProcessNewInput(char input)
	{
		if ((int)input == 8)
		{
			if (currentInputString.Length > 0)
			{
				// Backspace, remove last character
				currentInputString = currentInputString.Substring(0, currentInputString.Length - 1);
			}
		}
		else if (!char.IsLetterOrDigit(input) && !char.IsWhiteSpace(input))
		{
			return;
		}
		else
		{
			// Append new input
			currentInputString += input;
		}

		inputStringSubject.OnNext(currentInputString);
	}

	#endregion
}
