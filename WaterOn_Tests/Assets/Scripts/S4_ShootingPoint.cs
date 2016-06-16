using UnityEngine;
using System.Collections;

public class S4_ShootingPoint {

	public Transform transform { get; set;}
	//private bool busy;
	public GameObject dyke = null;

	public S4_ShootingPoint(Transform input, bool busy) {
		transform = input;
	//	this.busy = busy;
	}
		

	/*public bool IsBusy(){
		return busy;
	}

	public void SetFree() {
		busy = false;
	}

	public void SetBusy() {
		busy = true;
	}*/
}
