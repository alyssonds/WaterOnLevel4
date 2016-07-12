using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class S4_WaterCycleManager : MonoBehaviour {

	private bool cycle_started = false;

	public float balanced_water_level = 0.7f;

	private float increase_mountain = 0.0f;
	private float decrease_mountain = 0.0f;
	private float increase_lake = 0.0f;
	private float decrease_lake = 0.0f;
	private float lake_water_level = 1.0f; // 0 -> 1
	private float mountain_water_level = 1.0f;

	//values to balance the water in the scenario
	private const float water_speed = 0.1f/3f;
	private float pressure_to_balance = 0.0f;

	// Lake
	protected GameObject lakeGO = null;
	protected float lake_generation_time = 3f;
	protected float lake_cloud_levitation_time = 20f;
	protected float lake_cloud_toMountains_time = 20f;
	protected float lake_min_Y = -15.1f;
	protected float lake_max_Y = -9.77f;
	List<Vector3> lakeCloudsStartingPoints = new List<Vector3> ();

	//Clouds
	protected List<GameObject> cloudsGO = new List<GameObject> ();
	protected bool cloudCoroutineStopped = false;  

	//Mountain
	protected GameObject mountain = null;
	protected Texture2D originalMountainTex = null;
	protected Texture2D alteredMountainTex = null;
	protected Transform mountainPeak = null;

	//Rivers
//	protected GameObject river = null;

	//Weather
	public enum WeatherStatus
	{
		Snowing,
		Raining,
		Nothing
	}
	protected WeatherStatus _weatherStatus = WeatherStatus.Nothing;
	protected EllipsoidParticleEmitter snowEmitter = null;
	protected ParticleSystem rainParticleSystem = null;

	void Awake() {
		// Setting Variables
		lake_water_level = balanced_water_level; // 0 -> 1
		mountain_water_level = balanced_water_level;
		lakeGO = GameObject.Find ("WaterBasicDaytime").gameObject;
		mountain = GameObject.Find ("01_rocky_mountain_north_america 01_MeshPart0");
		originalMountainTex =  mountain.GetComponent<Renderer>().GetComponent<MeshRenderer>().materials [1].mainTexture as Texture2D;
		alteredMountainTex = Instantiate (originalMountainTex);
		mountainPeak = GameObject.Find ("MountainPeak").transform;
		snowEmitter = GameObject.Find ("Snow").GetComponent<EllipsoidParticleEmitter>();
		rainParticleSystem = GameObject.Find ("Rain").GetComponent<ParticleSystem>();

		// Find Lake Vertices Average Area
		lakeCloudsStartingPoints = S4_Utils.FindPointsInsideMesh (lakeGO);

		ChangeWeatherStatus (WeatherStatus.Nothing);

		StartCoroutine (PlayLakeSteam());
	}

	void OnQuit () {
		mountain.GetComponent<Renderer> ().GetComponent<MeshRenderer> ().materials [1].SetTexture ("_MainTex",originalMountainTex);
	}


	void ChangeWeatherStatus(WeatherStatus status)
	{
		//Debug.Log ("ChangeWeatherStatus :: To " + status);
		switch (status) 
		{
		case WeatherStatus.Nothing:
			snowEmitter.emit = false;
			rainParticleSystem.Stop ();
			break;
		case WeatherStatus.Snowing:
			if (_weatherStatus == WeatherStatus.Raining) {
				rainParticleSystem.Stop ();
				snowEmitter.emit = true;
			} else if (_weatherStatus == WeatherStatus.Nothing)
				snowEmitter.emit = true;
			if (!cycle_started) {
				cycle_started = true;
				StartCoroutine (MoveWaterFromMountainToLake ());
			}
			break;
		case WeatherStatus.Raining:
			if (_weatherStatus == WeatherStatus.Snowing) 
			{
				snowEmitter.emit = false;
				rainParticleSystem.Play ();
			} 
			else if (_weatherStatus == WeatherStatus.Nothing)
				rainParticleSystem.Play ();
			break;
		}
		_weatherStatus = status;
	}

	IEnumerator PlayLakeSteam()
	{
		Vector3 rndPoint = lakeCloudsStartingPoints[Random.Range(0,lakeCloudsStartingPoints.Count)];
		GameObject rndCloud = Instantiate(Resources.Load("Prefab/Cloud", typeof(GameObject)) as GameObject);
		rndCloud.tag = "Cloud";
		rndCloud.AddComponent<S4_Cloud> ();
		if(cycle_started)
			lake_water_level = DecreaseWaterLevel (lake_water_level, water_speed*(1 + decrease_lake));
		cloudsGO.Add (rndCloud);
		foreach (Transform child in rndCloud.transform) {
			child.gameObject.SetActive (false);
		}
		rndCloud.transform.localScale = Vector3.one * 1.5f; 
		//rndCloud.transform.localScale = Vector3.Scale(Vector3.one,new Vector3(1.1f,10f,1.1f));
		rndCloud.transform.position = rndPoint;
		//rndCloud.transform.DOScale (Vector3.one * 5f, lake_cloud_levitation_time / 4f).SetEase(Ease.Linear);
		rndCloud.transform.DOScale (Vector3.Scale(Vector3.one,new Vector3(5f,8f,5f)), lake_cloud_levitation_time / 4f).SetEase(Ease.Linear);
		rndCloud.transform.DOMoveY (18f, lake_cloud_levitation_time).SetEase(Ease.InOutQuad).OnComplete(() => {StartCoroutine(MoveCloudsToMountain(rndCloud));});
		yield return new WaitForSeconds (lake_generation_time);
		if (lake_water_level > 0.1f) {
			cloudCoroutineStopped = false;
			StartCoroutine (PlayLakeSteam ());
		}
		else {
			cloudCoroutineStopped = true;
			StopCoroutine (PlayLakeSteam ());
			ChangeWeatherStatus (WeatherStatus.Nothing);
		}
	}

	IEnumerator MoveCloudsToMountain(GameObject cloud)
	{
		Vector3 toPosition = S4_Utils.GetPointRandomInCircle (mountainPeak.transform.position, 2.5f);
		cloud.transform.DOMove (toPosition, lake_cloud_toMountains_time).SetEase(Ease.InOutQuad);
		yield return new WaitForSeconds (lake_cloud_toMountains_time);
		//cloud.GetComponent<Renderer> ().material.DOFade (0f, 5f);
		cloudsGO.Remove(cloud);
		Destroy (cloud, 5f);
		//MOVE WATER TO THE MOUNTAIN
		if (cycle_started)
			mountain_water_level = IncreaseWaterLevel (mountain_water_level,water_speed*(1 + increase_mountain)); 
		if (cloudsGO.Count > 1)
			ChangeWeatherStatus (WeatherStatus.Snowing);
		else
			ChangeWeatherStatus (WeatherStatus.Nothing);
	}

	IEnumerator MoveWaterFromMountainToLake() {

		if (cycle_started) {
			//MOVE WATER FROM THE MOUNTAIN TO THE LAKE
			int blocked_branches = GameObject.Find ("River2").GetComponent<S4_River> ().BlockedBranches ();
			//mountain_water_level -= water_speed * (1 + pressure_to_balance) * ((3 - blocked_branches) / 3f);
			mountain_water_level = DecreaseWaterLevel (mountain_water_level, water_speed * (1 + pressure_to_balance + decrease_mountain) * ((3 - blocked_branches) / 3f));
			lake_water_level = IncreaseWaterLevel (lake_water_level, water_speed * (1 + pressure_to_balance + increase_lake) * ((3 - blocked_branches) / 3f));

		}
		yield return new WaitForSeconds (lake_generation_time);
		StartCoroutine (MoveWaterFromMountainToLake());
	}



	protected float DecreaseWaterLevel(float actual_water_level, float value_to_decrease) {
		actual_water_level -= value_to_decrease;
		if (actual_water_level >= 0)
			return actual_water_level;
		else
			return 0f;
	}

	protected float IncreaseWaterLevel(float actual_water_level, float value_to_increase) {
		actual_water_level += value_to_increase;
		if (actual_water_level <= 1)
			return actual_water_level;
		else
			return 1f;
	}

	public void UpdateGeneralPressure() {
		pressure_to_balance = Mathf.Abs(lake_water_level - mountain_water_level);
	}

	public void UpdateMountainPressure() {
		float pressure_to_balance_mountain = mountain_water_level - balanced_water_level;
		if (pressure_to_balance_mountain > 0) {
			decrease_mountain = pressure_to_balance_mountain;
			increase_mountain = 0.0f;
		} else if (pressure_to_balance_mountain < 0) {
			decrease_mountain = 0.0f;
			increase_mountain = Mathf.Abs(pressure_to_balance_mountain);
		} else {
			decrease_mountain = 0.0f;
			increase_mountain = 0.0f;
		}
	}

	public void UpdateLakePressure() {
		float pressure_to_balance_lake = lake_water_level - balanced_water_level;
		if (pressure_to_balance_lake > 0) {
			decrease_lake = pressure_to_balance_lake;
			increase_lake = 0.0f;
		} else if (pressure_to_balance_lake < 0) {
			decrease_lake = 0.0f;
			increase_lake = Mathf.Abs(pressure_to_balance_lake);
		} else {
			decrease_lake = 0.0f;
			increase_lake = 0.0f;
		}
	}

	public void UpdateLakeWaterLevel () {
		float lakePosY = lake_min_Y + (-lake_min_Y + lake_max_Y) * lake_water_level;
		lakeGO.transform.position = new Vector3(lakeGO.transform.position.x,lakePosY,lakeGO.transform.position.z);
		if (lake_water_level > 0.1 && cloudCoroutineStopped) {
			cloudCoroutineStopped = false;
			StartCoroutine (PlayLakeSteam ());
		}

	}

	public void UpdateMountainWaterLevel() {
		Color32[] cor = new Color32[alteredMountainTex.width*alteredMountainTex.height];
		for (int i = 0; i < cor.Length; i++) {
			if (i < ((-10)*mountain_water_level + 11)) {
				cor [i].r = 84;
				cor [i].g = 55;
				cor [i].b = 11;
			} else {
				cor [i].r = 255;
				cor [i].g = 255;
				cor [i].b = 255;
			}
		}
		alteredMountainTex.SetPixels32 (cor);
		alteredMountainTex.Apply ();
		mountain.GetComponent<Renderer> ().GetComponent<MeshRenderer> ().materials [1].SetTexture ("_MainTex",alteredMountainTex);
	}

	public float GetSustainabilityLevel() {
		//the bigger the pressure to balance, the smaller the sustainability level, and vice versa
		return 1 - pressure_to_balance;
	}

	public bool IsCycleStarted() {
		return cycle_started;
	}

}
