using UnityEngine;
using System.Collections;

[System.Serializable]
public class S4_ShootingPoint {

	//transform of the shooting point
	public Transform transform { get; set;}
	//element that is on the shooting point
	public GameObject element = null;

	public S4_ShootingPoint(Transform input) {
		transform = input;
	}

	public void SetFree() {
		this.element = null;
	}

	public bool IsBusy(){
		if (element)
			return true;
		else
			return false;
	}

	public void SetBusy(GameObject _element) {
		this.element = _element;
	}

	//OLD
	/*public Vector3 GetRiverPositionOfDyke(GameObject dyke) {
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
*/
}
