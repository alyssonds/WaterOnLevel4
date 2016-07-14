using UnityEngine;
using System.Collections;

public class S4_VillainUmbrella : MonoBehaviour {

	Transform pivot;

	// Use this for initialization
	void Start () {
		pivot = GameObject.Find ("umbrella_springlock").transform;
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.RotateAround (pivot.position,Vector3.up,20*Time.deltaTime);
	}
}
