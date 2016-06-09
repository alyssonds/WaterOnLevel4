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
	public bool testegit = true;
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
	public static float watermill_rotation_speed = 20f;

	// Fixed Variables

	protected Texture2D originalMountainTex = null;
	protected Texture2D alteredMountainTex = null;
	protected GameObject mountain = null;
	protected GameObject mountainGO = null;
	protected GameObject lakeGO = null;
	protected GameObject environmentGO = null;
	protected EllipsoidParticleEmitter snowEmitter = null;
	protected ParticleSystem rainParticleSystem = null;
	protected List<GameObject> cloudsGO = new List<GameObject> ();
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
		originalMountainTex =  mountain.GetComponent<Renderer>().GetComponent<MeshRenderer>().materials [1].mainTexture as Texture2D;
		alteredMountainTex = Instantiate (originalMountainTex);

		lakeGO = GameObject.Find ("WaterBasicDaytime").gameObject;
		snowEmitter = GameObject.Find ("Snow").GetComponent<EllipsoidParticleEmitter>();
		rainParticleSystem = GameObject.Find ("Rain").GetComponent<ParticleSystem>();
		foreach (GameObject go in FindObjectsOfType(typeof(GameObject))) {
			if (go.name.StartsWith ("Cloud"))
				cloudsGO.Add (go);
		}
		mountainPeak = GameObject.Find ("MountainPeak").transform;

		// Find Lake Vertices Average Area
		lakeCloudsStartingPoints = FindPointsInsideMesh (lakeGO);

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
		GameObject rndCloud = Instantiate(cloudsGO[Random.Range(0,cloudsGO.Count)]);
		rndCloud.transform.localScale = Vector3.one * 5f;
		rndCloud.transform.position = rndPoint;
		rndCloud.transform.DOScale (Vector3.one * 50f, lake_cloud_levitation_time / 4f).SetEase(Ease.Linear);
		rndCloud.transform.DOMoveY (18f, lake_cloud_levitation_time).SetEase(Ease.InOutQuad).OnComplete(() => {StartCoroutine(MoveCloudsToMountain(rndCloud));});
		yield return new WaitForSeconds (lake_generation_time);
		StartCoroutine(PlayLakeSteam ());
	}
	IEnumerator MoveCloudsToMountain(GameObject cloud)
	{
		Vector3 toPosition = GetPointRandomInCircle (mountainPeak.transform.position, 5f);
		cloud.transform.DOMove (toPosition, lake_cloud_toMountains_time).SetEase(Ease.InOutQuad);
		yield return new WaitForSeconds (lake_cloud_toMountains_time);
		cloud.GetComponent<Renderer> ().material.DOFade (0f, 5f);
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

	void Update()
	{
		// DEBUG ONLY!
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
			//	if (i < ((-30)*mountainGO.GetComponent<S4_Mountain> ().waterLevel + 31)) {
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



	List<GameObject> riverPoints = new List<GameObject>();
	GameObject riverParent = null;
	GameObject riverGO = null;

	public GameObject AddingPoint () {
		GameObject riverPoint = new GameObject ("RiverPoint");
		riverPoints.Add (riverPoint);
		if (riverParent == null) 
			riverParent = new GameObject ("RiverParent");
		riverPoint.transform.SetParent (riverParent.transform);
		return riverPoint;
	}
	public void DeleteLastPoint () {
		if(riverPoints.Count > 0)
			riverPoints.RemoveAt (riverPoints.Count - 1);
	}
	public void CreatingRiver () 
	{
		if (riverPoints.Count < 2) {
			Debug.LogError ("ERROR :: CreatingRiver :: You need at least 2 points to build a river!");
			return;
		}

		Debug.Log ("Creating the River! [" + riverPoints.Count + "]");
		Mesh riverMesh = new Mesh ();
		Vector3[] verts = new Vector3[riverPoints.Count * 2];
		Vector2[] uvs = new Vector2[riverPoints.Count * 2];
		int[] tris = new int[(riverPoints.Count - 1) * 6];
		//int[] tris = new int[] { 0, 1, 2, 2, 1, 3 };

		// Vertex Layout
		// 0 ------ 1
		// |\       |
		// |   \    |
		// |      \ |
		// 2 ------ 3

	// Making the first quad
		verts [0] = riverPoints [0].transform.position + (Vector3.left * (river_average_width / 2f));
		verts [1] = riverPoints [0].transform.position - (Vector3.left * (river_average_width / 2f));
		verts [2] = riverPoints [1].transform.position + (Vector3.left * (river_average_width / 2f));
		verts [3] = riverPoints [1].transform.position - (Vector3.left * (river_average_width / 2f));

/*		uvs [0] = new Vector2 (0f, 1f);
		uvs [1] = new Vector2 (1f, 1f);
		uvs [2] = new Vector2 (0f, 0f);
		uvs [3] = new Vector2 (1f, 0f);
*/
	// Making all the other pieces
		for (int r = 1; r < riverPoints.Count - 1; r++)  // starting from 1 to avoid the first quad
		{
			float dimDelta = 0f; // (river_average_width / 2f) / (riverPoints.Count / 1f);
			float dimX = (river_average_width / 2f) + r * dimDelta;
			verts [r * 2 + 2] = riverPoints [r + 1].transform.position + (Vector3.left * dimX);
			verts [r * 2 + 3] = riverPoints [r + 1].transform.position - (Vector3.left * dimX);
		}

		// Triangles order!
/*		0, 1, 2, 2, 1, 3
		2, 3, 4, 4, 3, 5
		4, 5, 6, 6, 5, 7
		6, 7, 8, 8, 7, 9
*/
		int t = 0;
		for (int rr = 0; rr < riverPoints.Count - 1; rr++) 
		{
			// first triangle
			tris [t] = rr * 2;
			t += 1;
			tris [t] = rr * 2 + 1;
			t += 1;
			tris [t] = rr * 2 + 2;
			t += 1;
			// second triangle
			tris [t] = rr * 2 + 2;
			t += 1;
			tris [t] = rr * 2 + 1;
			t += 1;
			tris [t] = rr * 2 + 3;
			t += 1;
		}

/*		string tmpTris = "";
		foreach(int ti in tris)
			tmpTris += ti + ", ";
		Debug.Log (tmpTris);
*/
		// UVs order
		// 0.1 ------ 1.1
		//  |          |
		//  |          |
		//  |          |
		// 0.0 ------ 1.0

		// Calculate the whole river Length
		float totalRiverLength = 0f;
		for (int l = 0; l < riverPoints.Count-1; l++) 
			totalRiverLength += Vector3.Distance (riverPoints [l].transform.position, riverPoints [l + 1].transform.position);
		Debug.Log ("River Total Length :: " + totalRiverLength);

		/*
		// First UV
		uvs [0] = new Vector2 (0f, 1f);
		uvs [1] = new Vector2 (1f, 1f);
		// Lastest UV
		uvs [uvs.Length - 2] = new Vector2 (0f, 0f);
		uvs [uvs.Length - 1] = new Vector2 (1f, 0f);
		// Mids UV
		//float uvsYstep = 1f / (float)(riverPoints.Count - 1);
		float uvsYstep = 0f;
		if (uvs.Length > 4) 
		{
			for (int u = uvs.Length - 3; u > 1; u-=2) 
			{
				int riverPts = u / 2;
				float tmpDist = Vector3.Distance (riverPoints [riverPts].transform.position, riverPoints [riverPts + 1].transform.position);
				uvsYstep += tmpDist / totalRiverLength;
				uvs [u] = new Vector2 (1f, uvsYstep * Mathf.Abs(u / 2f - riverPoints.Count));
				uvs [u-1] = new Vector2 (0f, uvsYstep * Mathf.Abs(u / 2f - riverPoints.Count));
			}
		}*/
		Debug.Log ("UVs length: " + uvs.Length);
		// First UV
		uvs [0] = new Vector2 (0f, 0f);
		uvs [1] = new Vector2 (0f, 1f);
		// Lastest UV
		uvs [uvs.Length - 2] = new Vector2 (1f, 0f);
		uvs [uvs.Length - 1] = new Vector2 (1f, 1f);
		// Mids UV
		//float uvsYstep = 1f / (float)(riverPoints.Count - 1);
		float uvsYstep = 0f;
		if (uvs.Length > 4) 
		{
			for (int u = uvs.Length - 3; u > 1; u-=2) 
			{
				int riverPts = u / 2;
				float tmpDist = Vector3.Distance (riverPoints [riverPts].transform.position, riverPoints [riverPts + 1].transform.position);
				uvsYstep += tmpDist / totalRiverLength;
				uvs [u] = new Vector2 (uvsYstep * Mathf.Abs(u / 2f - riverPoints.Count), 1f);
				uvs [u-1] = new Vector2 (uvsYstep * Mathf.Abs(u / 2f - riverPoints.Count), 0f);
			}
		}
			
		riverMesh.vertices = verts;
		riverMesh.triangles = tris;
		riverMesh.uv = uvs;
		riverMesh.uv2 = uvs;

		riverMesh.RecalculateNormals ();
		riverGO = new GameObject ("River");
		riverGO.AddComponent<MeshFilter> ().mesh = riverMesh;
		Renderer rend = riverGO.AddComponent<MeshRenderer> ();
		mat_river = Resources.Load ("Materials/M_S4_River", typeof(Material)) as Material;
		rend.material = mat_river;
	}
	public void ResetAll()
	{
		foreach (GameObject g in riverPoints) 
		{
			DestroyImmediate (g);
		}
		riverPoints.Clear ();
		DestroyImmediate (riverParent);
		riverParent = null;
		if (riverGO != null)
			DestroyImmediate (riverGO);
		riverGO = null;
	}
	void OnDrawGizmos()
	{
		foreach(GameObject g in riverPoints)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere (g.transform.position, 0.3f);
		}

		for (int i = 1; i < riverPoints.Count; i++) 
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine (riverPoints[i].transform.position, riverPoints[i-1].transform.position);
		}
	}
}
