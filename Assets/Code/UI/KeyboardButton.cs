using UnityEngine;
using System.Collections.Generic;

public class KeyboardButton : MonoBehaviour
{
	#region Editor public fields
	#endregion

	#region Public properties

	public VirtualKeyboard Keyboard { get; set; }
	public char Key { get; set; }

	#endregion

	#region Private fields
	#endregion

	#region Unity methods

	public void KeyPressed()
	{
		Keyboard.KeyPress(Key);
	}

	#endregion
}
