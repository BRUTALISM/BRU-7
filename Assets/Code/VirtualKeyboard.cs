using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class VirtualKeyboard : MonoBehaviour
{
	#region Editor public fields

	public GameObject SingleKeyPrefab;
	public GameObject SingleSpacerPrefab;
	public List<HorizontalLayoutGroup> Rows;

	#endregion

	#region Public properties
	#endregion

	#region Private fields

	private List<string> rowCharacters = new List<string>()
	{
		"qwertyuiop",
		"_asdfghjkl_",
		"__zxcvbnm_⌫",
		" "
	};

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

				if (letter != '_')
				{
					var displayString = letter.ToString();
					if (letter == ' ') displayString = "space";

					letterObject.GetComponentInChildren<Text>().text = displayString;
				}
			}
		}
	}

	#endregion
}
