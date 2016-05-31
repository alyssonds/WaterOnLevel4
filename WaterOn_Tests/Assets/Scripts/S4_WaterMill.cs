using UnityEngine;
using System.Collections;

public class S4_WaterMill : MonoBehaviour {

	public GameObject wheelGO = null;

	void Start () {
	
	}
	

	void Update () {
		wheelGO.transform.Rotate (-Vector3.forward * Time.deltaTime * S4_MainManager.watermill_rotation_speed);
	}
}
