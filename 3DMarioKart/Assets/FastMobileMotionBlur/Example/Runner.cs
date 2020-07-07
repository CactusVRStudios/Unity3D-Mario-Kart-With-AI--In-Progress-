using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runner : MonoBehaviour {
	public float speed=0.01f;
	public float acceleration = 1f;

	void Update () {
		if (speed < 1f) {
			speed += acceleration*Time.deltaTime;
		}
		transform.position += Vector3.forward * speed;	
	}
}
