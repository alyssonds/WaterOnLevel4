using UnityEngine;
using System.Collections;

public class S4_UIManager : MonoBehaviour {

	public float time_to_check_sust = 3f;
	private float background_interpolate = 0.0f;
	private bool growing = true;
	protected GameObject backgroundUI = null;
	protected GameObject progress = null;
	S4_WaterCycleManager water_manager;

	void Awake() {
		backgroundUI = GameObject.Find ("Background");
		progress = GameObject.Find ("ProgressBarUI");
		water_manager = GameObject.Find("S4_MainManager").GetComponent<S4_WaterCycleManager> ();
	}

	IEnumerator DangerGlow() {
		if (background_interpolate <= 0.0f)
			growing = true;
		else if (background_interpolate >= 0.8f)
			growing = false;

		if (growing)
			background_interpolate += 0.01f;
		else
			background_interpolate -= 0.01f;
		backgroundUI.GetComponent<CanvasRenderer> ().SetColor(Color.Lerp (Color.white, Color.red, background_interpolate));

		yield return null;
		StartCoroutine ("DangerGlow");
	}

	IEnumerator StopGlow() {
		StopCoroutine ("DangerGlow");

		if (background_interpolate < 0.0f)
			background_interpolate += 0.01f;
		if (background_interpolate > 0.0f)
			background_interpolate -= 0.01f;

		//Test to see if it is zero
		if (background_interpolate >= -0.1f && background_interpolate <= 0.1f)
			background_interpolate = 0.0f;

		backgroundUI.GetComponent<CanvasRenderer> ().SetColor(Color.Lerp (Color.white, Color.red, background_interpolate));
		yield return null;
		if (Mathf.Approximately (background_interpolate, 0.0f)) {
			growing = true;
			StopCoroutine ("StopGlow");
		}
		else {
			StartCoroutine ("StopGlow");
		}
	}

	IEnumerator ControlLevelStatus() {
		float sust_level = water_manager.GetSustainabilityLevel();
		progress.GetComponent<S4_UIProgressBar> ().SetNormalizedPosition (sust_level);
		yield return new WaitForSeconds (time_to_check_sust);
		StartCoroutine ("ControlLevelStatus");
	}

	public void StartControlLevelStatus() {
		S4_MainManager.controling_sust = true;
		StartCoroutine ("ControlLevelStatus");
	}

	public void StartDangerGlow() {
		StartCoroutine ("DangerGlow");
	}

	public void StopDangerGlow() {
		StopCoroutine ("DangerGlow");
		StartCoroutine ("StopGlow");
	}


}
