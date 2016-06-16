using UnityEngine;
using System.Collections;

public class S4_RiverPiece : MonoBehaviour {

	public float dryingSpeed = 0.01f;
	public Transform startingPoint { get; set;}
	public bool dry = false;

	public bool isDry() {
		return dry;
	}

	public void SetDry () {
		dry = true;
	}
	
	public void SetFull () {
		dry = false;
	}

	void Update() {
		if (dry && this.gameObject.GetComponent<MeshRenderer> ().material.GetFloat ("_Magnitude") < 1.0f)
			this.gameObject.GetComponent<MeshRenderer> ().material.SetFloat ("_Magnitude", this.gameObject.GetComponent<MeshRenderer> ().material.GetFloat ("_Magnitude") + dryingSpeed);
		if (!dry && this.gameObject.GetComponent<MeshRenderer> ().material.GetFloat ("_Magnitude") > 0.0f)
			this.gameObject.GetComponent<MeshRenderer> ().material.SetFloat ("_Magnitude", this.gameObject.GetComponent<MeshRenderer> ().material.GetFloat ("_Magnitude") - dryingSpeed);
	}
}
