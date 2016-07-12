using UnityEngine;
using System.Collections;

public class S4_Dyke : MonoBehaviour {

	public GameObject ice_cube = null;
	public float ice_cube_size = 95.0f;
	//public float freezing_speed = 5.0f;
	public float defreezing_speed = 2.5f;

	// Use this for initialization
	void Awake () {
		this.gameObject.AddComponent<BoxCollider> ();
	}
	
	public void CreateCube() {
		ice_cube = Instantiate (Resources.Load ("Prefab/S4_IceBox", typeof(GameObject)) as GameObject);
		ice_cube.transform.position = this.transform.position + new Vector3 (0, 0, -0.95f);
		ice_cube.transform.localScale = new Vector3 (ice_cube_size, 0, ice_cube_size);
	}

	public void Freeze(float freezing_power) {
		if(ice_cube.transform.localScale.y < ice_cube_size)
			ice_cube.transform.localScale += new Vector3 (0,freezing_power*ice_cube_size*Time.deltaTime,0);
	}

	public void Defreeze() {
		if(ice_cube.transform.localScale.y > 0.0f)
			ice_cube.transform.localScale -= new Vector3 (0,defreezing_speed,0);
	}

	void OnDestroy() {
		GameObject.Destroy (ice_cube);
	}

}
