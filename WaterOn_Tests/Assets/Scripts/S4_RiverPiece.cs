using UnityEngine;
using System.Collections;

public class S4_RiverPiece : MonoBehaviour {

	public float dryingSpeed = 0.01f;
	public GameObject waterMill = null;
	public Transform startingPoint { get; set;}
	public bool dry = false;
	//The blocking dyke
	public GameObject dyke = null;
	//If it is a shooting point
	protected bool shootingPoint = false;
	//If it is the last piece of the river
	protected bool lastPiece = false;


	public Vector3 GetRiverPositionOfDyke(GameObject dyke) {
		if (this.dyke && dyke.GetInstanceID() == this.dyke.GetInstanceID()) {
			return this.startingPoint.position;

		}else
			return Vector3.zero;
	}

	public bool IsDry() {
		return dry;
	}

	public void SetDry () {
		if (waterMill)
			waterMill.GetComponent<S4_WaterMill> ().stop = true;
		dry = true;
	}
	
	public void SetFull () {
		if (waterMill)
			waterMill.GetComponent<S4_WaterMill> ().stop = false;
		dry = false;
	}

	public bool IsShootingPoint(){
		return shootingPoint;
	}

	public void MakeTarget(){
		shootingPoint = true;
	}

	public bool IsBlocked() {
		if (dyke)
			return true;
		else {
			return false;
		}
	}

	public void SetFree() {
		this.dyke = null;
	}

	public void MakeLastPiece(){
		this.lastPiece = true;
	}

	void Update() {
		if (dry && this.gameObject.GetComponent<MeshRenderer> ().material.GetFloat ("_Magnitude") < 1.0f)
			this.gameObject.GetComponent<MeshRenderer> ().material.SetFloat ("_Magnitude", this.gameObject.GetComponent<MeshRenderer> ().material.GetFloat ("_Magnitude") + dryingSpeed);
		if (!dry && this.gameObject.GetComponent<MeshRenderer> ().material.GetFloat ("_Magnitude") > 0.0f)
			this.gameObject.GetComponent<MeshRenderer> ().material.SetFloat ("_Magnitude", this.gameObject.GetComponent<MeshRenderer> ().material.GetFloat ("_Magnitude") - dryingSpeed);	
	}
}
