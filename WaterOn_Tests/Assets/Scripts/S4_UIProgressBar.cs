using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class S4_UIProgressBar : MonoBehaviour {

	public GameObject background;
	public GameObject filler;

	[Range(0.0f,1.0f)]
	protected float interpolationValue = 1f;
	protected Color maximumColor;//, minimumColor;
	public Color minimumColor;
	// Use this for initialization
	void Start () {
		maximumColor = background.GetComponent<Image> ().color;
	//	minimumColor = new Color (84,55,11);
	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log (interpolationValue);
		//background.GetComponent<Image> ().color = Color.LerpUnclamped (minimumColor, maximumColor, interpolationValue);

		if (Input.GetKeyDown (KeyCode.X)) {
			filler.transform.localPosition = new Vector3 (filler.transform.localPosition.x - 10f, filler.transform.localPosition.y, filler.transform.localPosition.z);
		//	interpolationValue -= (1f / 18f); 
		}
		if (Input.GetKeyDown (KeyCode.C)) {
			filler.transform.localPosition = new Vector3 (filler.transform.localPosition.x + 10f, filler.transform.localPosition.y, filler.transform.localPosition.z);
		//	interpolationValue +=( 1f / 18f); 
		}
	}
}
