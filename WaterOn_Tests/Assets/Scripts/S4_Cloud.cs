using UnityEngine;
using System.Collections;
using DG.Tweening;

public class S4_Cloud : MonoBehaviour {

	void OnTriggerEnter (Collider col) {
		this.transform.DOPause ();
	}
}
