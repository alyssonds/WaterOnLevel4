using UnityEngine;
using System.Collections;

public class S4_WaterMill : MonoBehaviour {

	public GameObject wheelGO = null;
	public bool stop = false;
	public float watermill_rotation_speed = 20f;
	public float watermill_acceleration = 0.2f;

	void Start () {

	}
	

	void Update () {
		if (stop && watermill_rotation_speed > 0.0f)
			watermill_rotation_speed -= watermill_acceleration;
		if(!stop && watermill_rotation_speed < 20f)
			watermill_rotation_speed += watermill_acceleration;
		
		wheelGO.transform.Rotate (-Vector3.forward * Time.deltaTime * watermill_rotation_speed);
	}
}
