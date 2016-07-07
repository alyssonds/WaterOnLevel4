using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class S4_UIProgressBar : MonoBehaviour {

	public GameObject background;
	public GameObject filler;
	protected float offset = -8.0f;
	protected float step = 0.01f;
	protected float max_pos = 0.0f;
	protected float min_pos = 0.0f;

	// Use this for initialization
	void Start () {
		max_pos = offset + (background.transform.localPosition.x + background.transform.GetComponent<RectTransform> ().rect.width) / 2f;
		min_pos = offset + (background.transform.localPosition.x - background.transform.GetComponent<RectTransform> ().rect.width) / 2f;
		filler.transform.localPosition = new Vector3 (max_pos,filler.transform.localPosition.y, filler.transform.localPosition.z);
		step = background.transform.GetComponent<RectTransform> ().rect.width/20f;
	}

	public void Increase() {
		filler.transform.localPosition = new Vector3 (filler.transform.localPosition.x + step, filler.transform.localPosition.y, filler.transform.localPosition.z);
	}

	public void Decrease() {
	//	Debug.Log ("DECREASE");
		filler.transform.localPosition = new Vector3 (filler.transform.localPosition.x - step, filler.transform.localPosition.y, filler.transform.localPosition.z);
	}
		

	//Set the position acording to a paramenter between 0 and 1
	public void SetNormalizedPosition(float positionValue) {
		float normalized_pos = positionValue * (max_pos - min_pos) + min_pos;
		/*while (!FastApproximately (normalized_pos, filler.transform.localPosition.x,0.1f)) {
			if (filler.transform.localPosition.x < normalized_pos)
				Increase ();
			else
				Decrease ();
		}*/
		filler.transform.localPosition = new Vector3 (normalized_pos, filler.transform.localPosition.y, filler.transform.localPosition.z);
	}

	public static bool FastApproximately(float a, float b, float threshold)
	{
		return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
	}
		
	// Update is called once per frame
	void Update () {
	
	}
}
