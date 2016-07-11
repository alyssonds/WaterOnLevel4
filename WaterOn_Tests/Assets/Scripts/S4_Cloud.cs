using UnityEngine;
using System.Collections;
using DG.Tweening;

public class S4_Cloud : MonoBehaviour {

	void OnTriggerEnter (Collider col) {
		if(col.CompareTag("Umbrella")) {
			this.transform.DOPause ();
		}
	}

	public void Freeze () {
		this.transform.DOPause();
	}

	public void Defreeze () {
		this.transform.DOPlay();
	}
}
