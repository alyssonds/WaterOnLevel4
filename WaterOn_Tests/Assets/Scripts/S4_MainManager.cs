using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Linq;

public class S4_MainManager : MonoBehaviour {

	public enum WeatherStatus
	{
		Snowing,
		Raining,
		Nothing
	}

	// Change these variables
	[Range(0f,1f)]
	public float lake_water_level = 1f; // 0 -> 1
	public float waterLevel = 1f;


	// Lake
	protected float lake_generation_time = 3f;
	protected float lake_cloud_levitation_time = 20f;
	protected float lake_cloud_toMountains_time = 20f;
	protected float lake_min_Y = -15.1f;
	protected float lake_max_Y = -9.77f;
	// River
	protected float river_average_width = 0.6f;
	// WaterMill
	public List<GameObject> positionsDykesOnRivers = new List<GameObject> ();

	// Fixed Variables
		protected Texture2D originalMountainTex = null;
	protected Texture2D alteredMountainTex = null;
	protected GameObject river = null;
	protected GameObject mountain = null;
	protected GameObject mountainGO = null;
	protected GameObject lakeGO = null;
	protected GameObject environmentGO = null;
	protected GameObject villain = null;
	protected GameObject cloud = null;
	protected EllipsoidParticleEmitter snowEmitter = null;
	protected ParticleSystem rainParticleSystem = null;
	//protected List<GameObject> cloudsGO = new List<GameObject> ();
	protected Transform mountainPeak = null;
	protected Material mat_river = null;

	protected WeatherStatus _weatherStatus = WeatherStatus.Nothing;
	List<Vector3> lakeCloudsStartingPoints = new List<Vector3> ();

	public struct Edge
	{
		public Mesh mesh;
		public int v1;
		public int v2;
		public int triangleIndex;
		public Edge(Mesh m, int aV1, int aV2, int aIndex)
		{
			mesh = m;
			v1 = aV1;
			v2 = aV2;
			triangleIndex = aIndex;
		}
		public bool IsEqual(Edge _edge)
		{
			if ((_edge.v1 == v1 && _edge.v2 == v2) || (_edge.v1 == v2 && _edge.v2 == v1))
				return true;
			else
				return false;
		}

		public Vector3 GetMedianPoint()
		{
			Vector3 tmp = (mesh.vertices[v1] - mesh.vertices[v2]) * 0.5f; 
			return new Vector3(mesh.vertices[v2].x + tmp.x, mesh.vertices[v2].y + tmp.y, mesh.vertices[v2].z + tmp.z);
		}
	}

	void Start () 
	{
		// Setting Variables
		environmentGO = GameObject.Find ("S4_Environment");
		mountain = GameObject.Find ("01_rocky_mountain_north_america 01_MeshPart0");
		mountainGO = GameObject.Find ("mountain");
		villain = GameObject.Find ("S4_VillainFactory");
		river = new GameObject ("River2");
		river.AddComponent<S4_River> ();
		originalMountainTex =  mountain.GetComponent<Renderer>().GetComponent<MeshRenderer>().materials [1].mainTexture as Texture2D;
		alteredMountainTex = Instantiate (originalMountainTex);

		lakeGO = GameObject.Find ("WaterBasicDaytime").gameObject;
		snowEmitter = GameObject.Find ("Snow").GetComponent<EllipsoidParticleEmitter>();
		rainParticleSystem = GameObject.Find ("Rain").GetComponent<ParticleSystem>();
		/*foreach (GameObject go in FindObjectsOfType(typeof(GameObject))) {
			if (go.name.StartsWith ("Cloud"))
				cloudsGO.Add (go);
		}*/
		mountainPeak = GameObject.Find ("MountainPeak").transform;

		// Find Lake Vertices Average Area
		lakeCloudsStartingPoints = FindPointsInsideMesh (lakeGO);

		//create the rivers
		foreach (Transform child in GameObject.Find ("RiverPoints").transform)
		{
			river.GetComponent<S4_River> ().CreateRiver (child.gameObject); 
		}

		InitializeShootingPoints ();

		ChangeWeatherStatus (WeatherStatus.Nothing);
		StartCoroutine (PlayLakeSteam());
	}

	void OnQuit () {
		mountain.GetComponent<Renderer> ().GetComponent<MeshRenderer> ().materials [1].SetTexture ("_MainTex",originalMountainTex);
	}

	List<Vector3> FindPointsInsideMesh(GameObject go)
	{
		Mesh m = go.GetComponent<MeshFilter>().mesh;
		List<Edge> meshEdges = new List<Edge>();
		int[] meshTris = m.triangles;

		// Populate meshEdges
		for (int i = 0; i < meshTris.Length; i += 3)
		{
			int v1 = meshTris[i];
			int v2 = meshTris[i + 1];
			int v3 = meshTris[i + 2];
			meshEdges.Add(new Edge(m, v1, v2, i));
			meshEdges.Add(new Edge(m, v2, v3, i));
			meshEdges.Add(new Edge(m, v3, v1, i));
		}

		// Take just internal edges
		List<Edge> internalEdges = new List<Edge> ();
		for (int e = 0; e < meshEdges.Count; e++) {
			int idx = 0;
			for (int f = 0; f < meshEdges.Count; f++) {
				if (meshEdges [e].IsEqual (meshEdges [f]))
					idx++;
			}
			if (idx > 1)
				internalEdges.Add (meshEdges[e]);
		}

		List<Vector3> internalVertices = new List<Vector3> ();
		foreach(Edge e in internalEdges)
		{
			internalVertices.Add (e.GetMedianPoint());
		}

/*		foreach (Vector3 v in internalVertices) {
			GameObject tmp1 = GameObject.CreatePrimitive (PrimitiveType.Cube);
			tmp1.transform.position = go.transform.TransformPoint (v);
		}
*/
		List<Vector3> internalVerticesWorld = new List<Vector3> ();
		foreach(Vector3 v in internalVertices)
		{
			internalVerticesWorld.Add (go.transform.TransformPoint (v));
		}
		return internalVerticesWorld;
	}

	public void ChangeWeatherStatus(WeatherStatus status)
	{
		//Debug.Log ("ChangeSnowStatus :: To " + status);
		switch (status) 
		{
		case WeatherStatus.Nothing:
			snowEmitter.emit = false;
			rainParticleSystem.Stop ();
			break;
		case WeatherStatus.Snowing:
			if (_weatherStatus == WeatherStatus.Raining) 
			{
				rainParticleSystem.Stop ();
				snowEmitter.emit = true;
			} 
			else if (_weatherStatus == WeatherStatus.Nothing)
				snowEmitter.emit = true;
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
		//GameObject rndCloud = Instantiate(cloudsGO[Random.Range(0,cloudsGO.Count)]);

		GameObject rndCloud = Instantiate(Resources.Load("Prefab/Cloud", typeof(GameObject)) as GameObject);
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
		StartCoroutine(PlayLakeSteam ());
	}
	IEnumerator MoveCloudsToMountain(GameObject cloud)
	{
		Vector3 toPosition = GetPointRandomInCircle (mountainPeak.transform.position, 5f);
		cloud.transform.DOMove (toPosition, lake_cloud_toMountains_time).SetEase(Ease.InOutQuad);
		yield return new WaitForSeconds (lake_cloud_toMountains_time);
		//cloud.GetComponent<Renderer> ().material.DOFade (0f, 5f);
		Destroy (cloud, 5f);
		ChangeWeatherStatus (WeatherStatus.Snowing);
	}

	Vector3 GetPointRandomInCircle(Vector3 pos, float radius)
	{
		int angle = Random.Range(0,360);
		float newX = pos.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
		float newY = pos.y + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
		return new Vector3(newX, newY, pos.z);
	}

	void InitializeShootingPoints(){
	/*	Transform transform = GameObject.Find ("RiverPoints").transform;
		foreach (Transform child in transform)
		{

			foreach (Transform grandchild in child)
			{
				if (grandchild.gameObject.CompareTag ("ShootingTarget")) {
					//create a new shooting point, false indicates it is free
					positionsDykesOnRivers.Add (new S4_ShootingPoint (grandchild, false));
				}
			}
		}*/
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
				//TESTAR SE EH UM DYKE PRIMEIRO
				if (!(hit.collider.gameObject.GetComponent<S4_Dyke> ().iceCube))
					hit.collider.gameObject.GetComponent<S4_Dyke> ().CreateCube ();
				else if (hit.collider.gameObject.GetComponent<S4_Dyke> ().iceCube.transform.localScale.y < 95.0f)
					hit.collider.gameObject.GetComponent<S4_Dyke> ().Freeze ();
			}
		}

		if (Input.GetMouseButtonDown (1)) {
			ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit)) {
				//TESTAR SE EH UM DYKE PRIMEIRO
				if (hit.collider.gameObject.GetComponent<S4_Dyke> ().iceCube.transform.localScale.y >= 95.0f) {
					Vector3 riverPosition = Vector3.zero;
					for (int i = 0; i < positionsDykesOnRivers.Count; i++) {
						riverPosition = positionsDykesOnRivers[i].GetComponent<S4_RiverPiece>().GetRiverPositionOfDyke (hit.collider.gameObject);
						if (riverPosition != Vector3.zero) {
							positionsDykesOnRivers[i].GetComponent<S4_RiverPiece>().SetFree ();
							river.GetComponent<S4_River> ().FillBranch (riverPosition);
							GameObject.Destroy (hit.collider.gameObject);
							break;
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
		if (Input.GetKeyDown (KeyCode.F)) {
			//mountainGO.GetComponent<S4_Mountain> ().waterLevel -= 0.1f;
			waterLevel -= 0.1f;
		}
		if (Input.GetKeyDown (KeyCode.G)) {
			//mountainGO.GetComponent<S4_Mountain> ().waterLevel -= 0.1f;
			waterLevel += 0.1f;
		}
		if(Input.GetKeyDown (KeyCode.R))
			ChangeWeatherStatus (WeatherStatus.Raining);
		if(Input.GetKeyDown(KeyCode.S))
			ChangeWeatherStatus (WeatherStatus.Snowing);
		if(Input.GetKeyDown(KeyCode.N))
			ChangeWeatherStatus (WeatherStatus.Nothing);
		if(Input.GetKeyDown(KeyCode.Space))
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

		// Lake Level
		float lakePosY = lake_min_Y + (-lake_min_Y + lake_max_Y) * lake_water_level;
		lakeGO.transform.position = new Vector3(lakeGO.transform.position.x,lakePosY,lakeGO.transform.position.z);
	
		//mountain level
		Color32[] cor = new Color32[alteredMountainTex.width*alteredMountainTex.height];
		for (int i = 0; i < cor.Length; i++) {
			if (i < ((-10)*waterLevel + 11)) {
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
}
