using UnityEngine;
using System.Collections;

public class S4_RiverPiece : MonoBehaviour {

	public Transform startingPoint { get; set;}
	//private Vector3 startingPoint = new Vector3(0,0,0);
	public bool busy = false;

	public bool isBusy() {
		return busy;
	}

	public void SetBusy () {
		busy = true;
	}
	
	public void SetFree () {
		busy = false;
	}
}
