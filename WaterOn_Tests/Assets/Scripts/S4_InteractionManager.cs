using UnityEngine;
using System.Collections;
using DG.Tweening;

public class S4_InteractionManager : MonoBehaviour {

	public void SimulateColdAndHotTokens() {
		Ray ray;
		RaycastHit hit;
		// DEBUG ONLY!
		if (Input.GetMouseButton (0)) {
			ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit)) {
				if (hit.collider.gameObject.CompareTag ("Dyke")) {
					if (!(hit.collider.gameObject.GetComponent<S4_Dyke> ().iceCube))
						hit.collider.gameObject.GetComponent<S4_Dyke> ().CreateCube ();
					else if (hit.collider.gameObject.GetComponent<S4_Dyke> ().iceCube.transform.localScale.y < 95.0f)
						hit.collider.gameObject.GetComponent<S4_Dyke> ().Freeze ();
				} 
				else if (hit.collider.gameObject.CompareTag ("Cloud")) {
					hit.collider.transform.DOPause ();
				}
			}
		}

		if (Input.GetMouseButtonDown (1)) {
			ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit)) {
				if(hit.collider.gameObject.CompareTag("Dyke")) {
					if (hit.collider.gameObject.GetComponent<S4_Dyke> ().iceCube.transform.localScale.y >= 95.0f) {
						Vector3 riverPosition = Vector3.zero;
						for (int i = 0; i < S4_River.positionsDykesOnRivers.Count; i++) {
							riverPosition = S4_River.positionsDykesOnRivers [i].GetComponent<S4_RiverPiece> ().GetRiverPositionOfDyke (hit.collider.gameObject);
							if (riverPosition != Vector3.zero) {
								S4_River.positionsDykesOnRivers [i].GetComponent<S4_RiverPiece> ().SetFree ();
								GameObject.Find("River2").GetComponent<S4_River> ().FillBranch (riverPosition);
								GameObject.Destroy (hit.collider.gameObject);
								break;
							}
						}
					}

				}

			}
		}

	}
}
