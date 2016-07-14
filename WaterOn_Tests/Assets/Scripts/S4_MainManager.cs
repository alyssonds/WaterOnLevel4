using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class S4_MainManager : MonoBehaviour {

	public enum LevelStatus
	{
		Danger,
		Balancing,
		Sustainable
	}
		
	//value to control the UI 
	public static bool controling_sust = false; //this value is altered in S4_UIManager

	// Fixed Variables
	S4_UIManager ui_manager;
	S4_WaterCycleManager water_manager;
	S4_InteractionManager interaction_manager;

	protected GameObject river = null;
	protected GameObject villain = null;
	protected LevelStatus _levelStatus = LevelStatus.Sustainable;

	void Start () 
	{
		villain = GameObject.Find ("S4_VillainFactory");
		river = new GameObject ("River2");
		river.AddComponent<S4_River> ();
		ui_manager = GameObject.Find ("Canvas").GetComponent<S4_UIManager> ();
		water_manager = this.GetComponent<S4_WaterCycleManager> ();
		interaction_manager = this.GetComponent<S4_InteractionManager> ();
	}

	void Update()
	{
		interaction_manager.SimulateColdAndHotTokens ();

		if (Input.GetKeyDown (KeyCode.D))
			villain.GetComponent<S4_VillainFactory>().ShootDyke ();
		if (Input.GetKeyDown (KeyCode.F))
			villain.GetComponent<S4_VillainFactory>().ShootTurret ();
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
		if (sust_level <= 0.4 && _levelStatus != LevelStatus.Danger)
			ChangeLevelStatus (LevelStatus.Danger);
		else if(sust_level > 0.4 && _levelStatus != LevelStatus.Balancing)
			ChangeLevelStatus (LevelStatus.Balancing);
		else if(sust_level > 0.9 && _levelStatus != LevelStatus.Sustainable)
			ChangeLevelStatus (LevelStatus.Sustainable);
		//calculate the mountain and lake pressure to balance
		water_manager.UpdateMountainPressure ();
		water_manager.UpdateLakePressure ();

		//update weather
		water_manager.UpdateWeather();

	}

	//Controls the overall status of the level
	public void ChangeLevelStatus(LevelStatus status)
	{
		//Debug.Log ("ChangeLevelStatus :: To " + status);
		switch (status) 
		{
		case LevelStatus.Balancing:
			if (_levelStatus == LevelStatus.Danger)
				ui_manager.StopDangerGlow ();
			break;
		case LevelStatus.Danger:
			ui_manager.StartDangerGlow ();
			break;
		case LevelStatus.Sustainable:
			if (_levelStatus == LevelStatus.Danger)
				ui_manager.StopDangerGlow ();
			break;
		}
		_levelStatus = status;
	}
		
}
