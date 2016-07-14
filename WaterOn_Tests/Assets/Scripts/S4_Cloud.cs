using UnityEngine;
using System.Collections;
using DG.Tweening;

public class S4_Cloud : MonoBehaviour {

	S4_WaterCycleManager water_manager;
	public float cloud_time_to_destroy = 4f; 
	private float time_to_wait = 4f;

	void Awake ()
	{
		water_manager = GameObject.Find ("S4_MainManager").GetComponent<S4_WaterCycleManager> ();
	}

	void OnTriggerEnter (Collider col) {
		if (col.CompareTag ("Umbrella")) {
			this.transform.DOPause ();
		} else if (col.CompareTag ("MountainPeak")) {
			//cloud.GetComponent<Renderer> ().material.DOFade (0f, 5f); - COLOCAR EM CLOUD
			S4_WaterCycleManager.clouds.Add(this.gameObject);
			//StartCoroutine (Wait(time_to_wait));


		}
	}

	IEnumerator Wait(float time_to_wait) {
		yield return new WaitForSeconds (time_to_wait);
	}

	public void Freeze () {
		this.transform.DOPause();
	}

	public void Defreeze () {
		this.transform.DOPlay();
	}
}
