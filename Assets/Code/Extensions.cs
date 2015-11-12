using UnityEngine;
using System.Collections.Generic;

public static class Extensions
{
	public static void Times(this int self, System.Action<int> loopBody)
	{
		if (loopBody != null) for (int i = 0; i < self; i++) loopBody(i);
	}
}
