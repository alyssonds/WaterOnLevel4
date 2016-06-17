using UnityEngine;
using System.Collections;

public class S4_Dyke : MonoBehaviour {

	public GameObject iceCube = null;
	public float freezingSpeed = 5.0f;
	public float defreezingSpeed = 2.5f;

	// Use this for initialization
	void Awake () {
		this.gameObject.AddComponent<BoxCollider> ();
	}
	
	public void CreateCube() {
		iceCube = Instantiate (Resources.Load ("Prefab/S4_IceBox", typeof(GameObject)) as GameObject);
		iceCube.transform.position = this.transform.position + new Vector3 (0, 0, -0.95f);
		iceCube.transform.localScale = new Vector3 (95, 0, 95);
	}

	public void Freeze() {
		if(iceCube.transform.localScale.y < 95.0f)
			iceCube.transform.localScale += new Vector3 (0,freezingSpeed,0);
	}

	public void Defreeze() {
		if(iceCube.transform.localScale.y > 0.0f)
			iceCube.transform.localScale -= new Vector3 (0,defreezingSpeed,0);
	}

	void OnDestroy() {
		GameObject.Destroy (iceCube);
		//this.gameObject
	}
	/*void OnMouseDown () {
		if (!iceCube) { 
			iceCube = Instantiate (Resources.Load ("Prefab/S4_IceBox", typeof(GameObject)) as GameObject);
			iceCube.transform.position = this.transform.position + new Vector3 (0, 0, -0.95f);
			iceCube.transform.localScale = new Vector3 (95, 0, 95);
		}
		else if(iceCube.transform.localScale.y >= 95.0f) {
			Debug.Log ("Destroy Cube");
		}
	}

	void OnMouseDrag () {
		if(iceCube.transform.localScale.y < 95.0f)
			iceCube.transform.localScale += new Vector3 (0,freezingSpeed,0);
	}*/

}
