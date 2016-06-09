using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class S4_River : MonoBehaviour {

	// River
	protected float river_average_width = 0.6f;

	protected Material mat_river = null;

	List<GameObject> riverPoints = new List<GameObject>();
	GameObject riverParent = null;
	GameObject riverGO = null;
	//GameObject 

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void CreateRiver () 
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

}
