using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class SafeTime : MonoBehaviour
{
	#region Editor public fields
	#endregion

	#region Public properties

	public float Time { get { return time; } }

	#endregion

	#region Private fields

	private static SafeTime instance;
	public static SafeTime Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<SafeTime>();
			}

			return instance;
		}
	}

	private float time;

	#endregion

	#region Unity methods

	void Start()
	{
		Observable.EveryUpdate().Subscribe(_ => time = UnityEngine.Time.time).AddTo(this);
	}

	#endregion
}
