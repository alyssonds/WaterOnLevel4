using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class S4_River : MonoBehaviour {

	// River
	protected float river_average_width = 0.6f;

	protected Material mat_river = null;
	protected Material mat_river_encounter = null;

	protected List<Transform> riverPoints = new List<Transform>();


	// Use this for initialization
	void Awake () {
		mat_river = Resources.Load ("Materials/M_S4_River", typeof(Material)) as Material;
		mat_river_encounter = Resources.Load ("Materials/M_S4_River_Encounter", typeof(Material)) as Material;
	}
	
	// Update is called once per frame
	/*void Update () {
		bool busyBranch = false;
		foreach (Transform riverParent in this.gameObject.transform) {
			foreach (Transform riverPiece in riverParent) {
				if (busyBranch == true && riverPiece.gameObject.GetComponent<MeshRenderer> ().material.GetFloat ("_Magnitude") < 1.0f)
					riverPiece.gameObject.GetComponent<MeshRenderer> ().material.SetFloat ("_Magnitude", riverPiece.gameObject.GetComponent<MeshRenderer> ().material.GetFloat ("_Magnitude") + 0.01f);
				else if (riverPiece.gameObject.GetComponent<S4_RiverPiece> ().isDry () && riverPiece.gameObject.GetComponent<MeshRenderer> ().material.GetFloat ("_Magnitude") < 1.0f) {
					riverPiece.gameObject.GetComponent<MeshRenderer> ().material.SetFloat ("_Magnitude", riverPiece.gameObject.GetComponent<MeshRenderer> ().material.GetFloat ("_Magnitude") + 0.01f);
					busyBranch = true;
				}
			}
			busyBranch = false;
		}
	}*/

	//Dry or fill branch. 
	public void AlterBranch (Vector3 position, bool drying) {
		bool branchStart = false;
		//goes through all the river pieces
		foreach (Transform riverParent in this.gameObject.transform) {
			foreach (Transform riverPiece in riverParent) {
				//finds the start of the branch
				if (riverPiece.gameObject.GetComponent<S4_RiverPiece> ().startingPoint.position == position) {
					branchStart = true;
				}
				//dries or fill branch
				if (branchStart) {
					if (drying)
						riverPiece.gameObject.GetComponent<S4_RiverPiece> ().SetDry ();
					else
						riverPiece.gameObject.GetComponent<S4_RiverPiece> ().SetFull ();
				}
			}
			branchStart = false;
		}
	}

	public void CreateRiver(GameObject riverParent) {
		//clear the control points, in case not the first call
		riverPoints.Clear ();

		GameObject riverBranch = new GameObject(riverParent.ToString());
		riverBranch.transform.SetParent (this.gameObject.transform);

		//initialize the control points
		foreach (Transform child in riverParent.transform)
		{
			riverPoints.Add (child);
		}
		//create the river pieces
		for (int i = 0; i < (riverPoints.Count - 1); i++) {
			CreateRiverPiece (riverPoints [i], riverPoints [i + 1], riverBranch);

		}
	}

	private void CreateRiverPiece (Transform point1, Transform point2, GameObject riverBranch) 
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
		riverPiece.transform.SetParent (riverBranch.transform);
		riverPiece.AddComponent<MeshFilter> ().mesh = riverMesh;
		Renderer rend = riverPiece.AddComponent<MeshRenderer> ();
		if(point1.gameObject.CompareTag("DivisionTile"))
			rend.material = mat_river_encounter;
		else
			rend.material = mat_river;
		riverPiece.AddComponent <ScrollingUVs_Layers>().textureName = "_LavaTex";
		//riverPiece.GetComponent <ScrollingUVs_Layers>().textureName = "_LavaTex";
		riverPiece.AddComponent <S4_RiverPiece>().startingPoint = point1;
	}

}
