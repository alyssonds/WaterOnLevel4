using UnityEngine;
using System.Collections;
using DG.Tweening;

public class S4_InteractionManager : MonoBehaviour {

	public float cold_token_freezing_speed = 1;
	public float hot_token_heating_speed = 1;

	public void SimulateColdAndHotTokens() {
		Ray ray;
		RaycastHit hit;

		//when the left mouse hits a dyke, behaves like the cold token. When it hits a turret, behaves like hot token
		if (Input.GetMouseButton (0)) {
			ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit)) {
				if (hit.collider.CompareTag ("Dyke")) {
					if (!(hit.collider.GetComponent<S4_Dyke> ().ice_cube))
						hit.collider.GetComponent<S4_Dyke> ().CreateCube ();
					else if (hit.collider.GetComponent<S4_Dyke> ().ice_cube.transform.localScale.y < 95.0f)
						hit.collider.GetComponent<S4_Dyke> ().Freeze (cold_token_freezing_speed);
				} 
				else if (hit.collider.CompareTag ("Turret")) {
					hit.collider.GetComponent<S4_VillainTurret> ().Heat (hot_token_heating_speed);
				}
			}
		}

		if (Input.GetMouseButtonDown (1)) {
			ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit)) {
				if(hit.collider.gameObject.CompareTag("Dyke")) {
					if (hit.collider.gameObject.GetComponent<S4_Dyke> ().ice_cube.transform.localScale.y >= 95.0f) {
						Vector3 riverPosition = Vector3.zero;
						for (int i = 0; i < S4_VillainFactory.positions_dykes_on_rivers.Count; i++) {
							riverPosition = S4_VillainFactory.positions_dykes_on_rivers [i].GetComponent<S4_RiverPiece> ().GetRiverPositionOfDyke (hit.collider.gameObject);
							if (riverPosition != Vector3.zero) {
								S4_VillainFactory.positions_dykes_on_rivers [i].GetComponent<S4_RiverPiece> ().SetFree ();
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
