using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft.Examples
{
	/// <summary>
	/// Rotates an object by a random value around its y axis each frame, and by a fraction of 90 degrees each frame
	/// </summary>
	public class Rotate : UnityEngine.MonoBehaviour
	{
		Vector3 euler;
		// Use this for initialization
		void Start()
		{
			euler = new Vector3(0f, (Random.value < 0.5f ? -1.0f : 1.0f) * 90f * Random.Range(0.6f, 1.4f), 90f);
		}

		// Update is called once per frame
		void Update()
		{
			transform.Rotate(euler * Time.deltaTime);
		}
	}
}
