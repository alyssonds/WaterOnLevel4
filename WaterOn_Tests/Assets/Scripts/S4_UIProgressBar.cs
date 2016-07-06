using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class S4_UIProgressBar : MonoBehaviour {

	public GameObject background;
	public GameObject filler;
	protected float offset = -8.0f;
	protected float step = 0.0f;

	// Use this for initialization
	void Start () {
		filler.transform.localPosition = new Vector3 (offset + (background.transform.localPosition.x + background.transform.GetComponent<RectTransform> ().rect.width)/2f,
			filler.transform.localPosition.y, filler.transform.localPosition.z);
		step = background.transform.GetComponent<RectTransform> ().rect.width/20f;
	}

	public void Increase() {
		filler.transform.localPosition = new Vector3 (filler.transform.localPosition.x + step, filler.transform.localPosition.y, filler.transform.localPosition.z);
	}

	public void Decrease() {
	//	Debug.Log ("DECREASE");
		filler.transform.localPosition = new Vector3 (filler.transform.localPosition.x - step, filler.transform.localPosition.y, filler.transform.localPosition.z);
	}
	// Update is called once per frame
	void Update () {
	
	}
}
