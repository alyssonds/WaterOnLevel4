using UnityEngine;
using System.Collections;

[System.Serializable]
public class S4_ShootingPoint {

	public Transform transform { get; set;}
	//private bool busy;
	public GameObject dyke = null;

	public S4_ShootingPoint(Transform input, bool busy) {
		transform = input;
	//	this.busy = busy;
	}
		
	public Vector3 GetRiverPositionOfDyke(GameObject dyke) {
		if (this.dyke && dyke.GetInstanceID() == this.dyke.GetInstanceID()) {
			return this.transform.position;

		}else
			return Vector3.zero;
	}

	public S4_ShootingPoint GetShootingPointOfDyke(GameObject dyke) {
		if (this.dyke && dyke.GetInstanceID() == this.dyke.GetInstanceID()) {
			return this;

		}else
			return null;
	}

	public void SetFree() {
		this.dyke = null;
	}
	/*public bool IsBusy(){
		return busy;
	}



	public void SetBusy() {
		busy = true;
	}*/
}
