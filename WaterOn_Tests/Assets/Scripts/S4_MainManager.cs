using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Linq;

public class S4_MainManager : MonoBehaviour {

	public enum LevelStatus
	{
		Danger,
		Balancing,
		Sustainable
	}
		
	//values to control the UI 
	public static bool controling_sust = false; //this value is altered in S4_UIManager
	S4_UIManager ui_manager;
	S4_WaterCycleManager water_manager;

	// WaterMill
	public List<GameObject> positionsDykesOnRivers = new List<GameObject> ();

	// Fixed Variables
	protected GameObject river = null;
	protected GameObject environmentGO = null;
	protected GameObject villain = null;

	protected Material mat_river = null;
	protected LevelStatus _levelStatus = LevelStatus.Sustainable;

	void Start () 
	{
		environmentGO = GameObject.Find ("S4_Environment");
		villain = GameObject.Find ("S4_VillainFactory");
		river = new GameObject ("River2");
		river.AddComponent<S4_River> ();
		ui_manager = GameObject.Find ("Canvas").GetComponent<S4_UIManager> ();
		water_manager = this.GetComponent<S4_WaterCycleManager> ();

		//create the rivers
		foreach (Transform child in GameObject.Find ("RiverPoints").transform)
		{
			river.GetComponent<S4_River> ().CreateRiver (child.gameObject); 
		}

		InitializeShootingPoints ();

	}



	public void ChangeLevelStatus(LevelStatus status)
	{
		//Debug.Log ("ChangeLevelStatus :: To " + status);
		switch (status) 
		{
		case LevelStatus.Balancing:
			if (_levelStatus == LevelStatus.Danger) {
				ui_manager.StopDangerGlow ();
			}
			break;
		case LevelStatus.Danger:
			ui_manager.StartDangerGlow ();
			break;
		case LevelStatus.Sustainable:
			break;
		}
		_levelStatus = status;
	}

	void InitializeShootingPoints(){

		Transform transform = GameObject.Find ("River2").transform;
		foreach (Transform child in transform)
		{
			foreach (Transform grandchild in child)
			{
				if (grandchild.gameObject.GetComponent<S4_RiverPiece>().IsShootingPoint()) {
					//create a new shooting point, false indicates it is free
					positionsDykesOnRivers.Add (grandchild.gameObject);
				}
			}
		}
	}

	protected Vector3 GetRandomPositionOnRivers()
	{
		//it should be guaranteed that there are any free spaces before. If not it goes into an infinite loop!
		int index = Random.Range (0, positionsDykesOnRivers.Count);
		//while is busy, look for a new one
		while(positionsDykesOnRivers[index].GetComponent<S4_RiverPiece>().dyke)
			index = Random.Range (0, positionsDykesOnRivers.Count);
		
		return positionsDykesOnRivers[index].GetComponent<S4_RiverPiece>().startingPoint.position;
	}


	void Update()
	{
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
						for (int i = 0; i < positionsDykesOnRivers.Count; i++) {
							riverPosition = positionsDykesOnRivers [i].GetComponent<S4_RiverPiece> ().GetRiverPositionOfDyke (hit.collider.gameObject);
							if (riverPosition != Vector3.zero) {
								positionsDykesOnRivers [i].GetComponent<S4_RiverPiece> ().SetFree ();
								river.GetComponent<S4_River> ().FillBranch (riverPosition);
								GameObject.Destroy (hit.collider.gameObject);
								break;
							}
						}
					}

				}

			}
		}

		if (Input.GetKeyDown (KeyCode.D)) {
			int i = 0;
			bool freeSpace = false;
			//check if there are any free spaces
			while (i < positionsDykesOnRivers.Count) {
				if (!(positionsDykesOnRivers [i].GetComponent<S4_RiverPiece>().dyke)) {
					freeSpace = true;
					break;
				}
				i++;
			}
			if (freeSpace) {
				GameObject bullet = Instantiate(Resources.Load("Prefab/S4_ClosedDyke", typeof(GameObject)) as GameObject);
				bullet.tag = "Dyke";
				bullet.AddComponent<S4_Dyke> ();
				//it should be guaranteed that there are any free spaces before. If not it goes into an infinite loop!
				int index = Random.Range (0, positionsDykesOnRivers.Count);
				//while is busy, look for a new one
				while(positionsDykesOnRivers[index].GetComponent<S4_RiverPiece>().dyke)
					index = Random.Range (0, positionsDykesOnRivers.Count);

				positionsDykesOnRivers [index].GetComponent<S4_RiverPiece>().dyke = bullet;
				villain.GetComponent<S4_VillainFactory>().ShotDykes(bullet, positionsDykesOnRivers[index].GetComponent<S4_RiverPiece>().startingPoint.position);
			} else {
				Debug.Log ("Theres no free space anymore");
			}
		}
		if (Input.GetKeyDown (KeyCode.C))
			ChangeLevelStatus(LevelStatus.Danger);
		if (Input.GetKeyDown (KeyCode.V))
			ChangeLevelStatus(LevelStatus.Balancing);
		if(Input.GetKeyDown(KeyCode.Space))
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		if (Input.GetKeyDown (KeyCode.Escape))
			Application.Quit ();
		
		// Lake Level
		water_manager.UpdateLakeWaterLevel();
		//mountain level
		water_manager.UpdateMountainWaterLevel();
		//calculate the general pressure to balance
		water_manager.UpdateGeneralPressure ();
		//calculate the sustainability level
		float sust_level = water_manager.GetSustainabilityLevel();
		
		//move the progressbar
		if (water_manager.IsCycleStarted() & !controling_sust)
			ui_manager.StartControlLevelStatus ();
		//danger level stat
		if (sust_level < 0.4 && _levelStatus != LevelStatus.Danger)
			ChangeLevelStatus (LevelStatus.Danger);
		else if(sust_level > 0.4 && _levelStatus != LevelStatus.Balancing)
			ChangeLevelStatus (LevelStatus.Balancing);

		//calculate the mountain and lake pressure to balance
		water_manager.UpdateMountainPressure ();
		water_manager.UpdateLakePressure ();

	}
		
}
