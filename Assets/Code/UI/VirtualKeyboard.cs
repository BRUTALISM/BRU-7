using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UniRx;

public class VirtualKeyboard : MonoBehaviour
{
	#region Editor public fields

	public GameObject SingleKeyPrefab;
	public GameObject SingleSpacerPrefab;
	public List<HorizontalLayoutGroup> Rows;

	#endregion

	#region Public properties

	public IObservable<char> KeyPresses { get { return keyPressesSubject; } }

	public static readonly char BackspaceCharacter = '⌫';

	#endregion

	#region Private fields

	private List<string> rowCharacters = new List<string>()
	{
		"qwertyuiop",
		"_asdfghjkl_",
		"__zxcvbnm_" + BackspaceCharacter,
		" "
	};

	private Subject<char> keyPressesSubject = new Subject<char>();

	#endregion

	#region Unity methods

	void Start()
	{
		for (int i = 0; i < Rows.Count && i < rowCharacters.Count; i++)
		{
			var currentRow = Rows[i];
			foreach (var letter in rowCharacters[i])
			{
				// Instantiate a prefab and add it to the row
				var letterObject = Instantiate(SingleKeyPrefab);
				letterObject.transform.SetParent(currentRow.transform, false);

				var keyboardButton = letterObject.GetComponent<KeyboardButton>();
				keyboardButton.Keyboard = this;

				if (letter == '_')
				{
					letterObject.GetComponent<Button>().interactable = false;
				}
				else
				{
					var displayString = letter.ToString();
					if (letter == ' ') displayString = "space";

					letterObject.GetComponentInChildren<Text>().text = displayString;

					keyboardButton.Key = letter;
				}
			}
		}
	}

	#endregion

	#region Key handling

	public void KeyPress(char key)
	{
		keyPressesSubject.OnNext(key);
	}

	#endregion
}
