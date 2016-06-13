using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class S4_River : MonoBehaviour {

	// River
	protected float river_average_width = 0.6f;

	protected Material mat_river = null;

	List<Transform> riverPoints = new List<Transform>();
	GameObject riverParent = null;
	GameObject riverGO = null;
	//GameObject 

	// Use this for initialization
	void Start () {
		riverGO = new GameObject ("River2");
		riverParent = GameObject.Find ("RiverParent");
		foreach (Transform child in riverParent.transform)
		{
			riverPoints.Add (child);
		}
		mat_river = Resources.Load ("Materials/M_S4_River", typeof(Material)) as Material;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void CreateRiver() {
		
		for (int i = 0; i < (riverPoints.Count - 1); i++) 
			CreateRiverPiece (riverPoints [i], riverPoints [i + 1]);
	}

	private void CreateRiverPiece (Transform point1, Transform point2) 
	{
		Mesh riverMesh = new Mesh ();
		Vector3[] verts = new Vector3[4];
		Vector2[] uvs = new Vector2[4];
		//int[] tris = new int[6];
		int[] tris = new int[] { 0, 1, 2, 2, 1, 3 };

		// Vertex Layout
		// 0 ------ 1
		// |\       |
		// |   \    |
		// |      \ |
		// 2 ------ 3

		// Making the first quad
		verts [0] = point1.position + (Vector3.left * (river_average_width / 2f));
		verts [1] = point1.position - (Vector3.left * (river_average_width / 2f));
		verts [2] = point2.position + (Vector3.left * (river_average_width / 2f));
		verts [3] = point2.position - (Vector3.left * (river_average_width / 2f));

		uvs [0] = new Vector2 (0f, 1f);
		uvs [1] = new Vector2 (1f, 1f);
		uvs [2] = new Vector2 (0f, 0f);
		uvs [3] = new Vector2 (1f, 0f);

		// UVs order
		// 0.1 ------ 1.1
		//  |          |
		//  |          |
		//  |          |
		// 0.0 ------ 1.0

		// First UV
		uvs [0] = new Vector2 (0f, 0f);
		uvs [1] = new Vector2 (0f, 1f);
		// Lastest UV
		uvs [uvs.Length - 2] = new Vector2 (1f, 0f);
		uvs [uvs.Length - 1] = new Vector2 (1f, 1f);
		// Mids UV
		//float uvsYstep = 1f / (float)(riverPoints.Count - 1);
/*		float uvsYstep = 0f;
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
*/
		riverMesh.vertices = verts;
		riverMesh.triangles = tris;
		riverMesh.uv = uvs;
		riverMesh.uv2 = uvs;

		riverMesh.RecalculateNormals ();
		GameObject riverPiece = new GameObject("RiverPiece");
		riverPiece.transform.SetParent (riverGO.transform);
		riverPiece.AddComponent<MeshFilter> ().mesh = riverMesh;
		Renderer rend = riverPiece.AddComponent<MeshRenderer> ();
		rend.material = mat_river;
		riverPiece.AddComponent <ScrollingUVs_Layers>();
	}

}
