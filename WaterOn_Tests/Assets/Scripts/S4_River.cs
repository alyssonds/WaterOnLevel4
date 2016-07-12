using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class S4_River : MonoBehaviour {

	// River
	protected float river_average_width = 0.6f;

	protected Material mat_river = null;
	protected Material mat_river_encounter = null;
	public static List<GameObject> positions_dykes_on_rivers = new List<GameObject> ();
	protected List<Transform> river_points = new List<Transform>();


	// Use this for initialization
	void Awake () {
		mat_river = Resources.Load ("Materials/M_S4_River", typeof(Material)) as Material;
		mat_river_encounter = Resources.Load ("Materials/M_S4_River_Encounter", typeof(Material)) as Material;

		//create the rivers
		foreach (Transform child in GameObject.Find ("RiverPoints").transform)
		{
			CreateRiver (child.gameObject); 
		}

		//Initialize the possible shooting points in the river
		InitializeShootingPoints ();

	}

	//Initialize the possible shooting points in the river
	void InitializeShootingPoints(){

		Transform transform = GameObject.Find ("River2").transform;
		foreach (Transform child in transform)
		{
			foreach (Transform grandchild in child)
			{
				if (grandchild.gameObject.GetComponent<S4_RiverPiece>().IsShootingPoint()) {
					//create a new shooting point, false indicates it is free
					positions_dykes_on_rivers.Add (grandchild.gameObject);
				}
			}
		}
	}

	//Fill a branch. 
	public void FillBranch (Vector3 position) {
		bool branchStart = false;
		//goes through all the river pieces
		foreach (Transform riverParent in this.gameObject.transform) {
			foreach (Transform riverPiece in riverParent) {
				//if the branch is blocked, break
				if (riverPiece.GetComponent<S4_RiverPiece>().IsBlocked()){ 
					break;
				}
				//finds the start of the branch
				if (riverPiece.gameObject.GetComponent<S4_RiverPiece> ().startingPoint.position == position) {
					branchStart = true;
				}
				//fill branch
				if (branchStart)
					riverPiece.gameObject.GetComponent<S4_RiverPiece> ().SetFull ();
			}
			branchStart = false;
		}
	}

	//Dry a branch
	public void DryBranch (Vector3 position) {
		bool branchStart = false;
		//goes through all the river pieces
		foreach (Transform riverParent in this.gameObject.transform) {
			foreach (Transform riverPiece in riverParent) {
				//finds the start of the branch
				if (riverPiece.gameObject.GetComponent<S4_RiverPiece> ().startingPoint.position == position) {
					branchStart = true;
				}
				//dries or fill branch
				if (branchStart)
						riverPiece.gameObject.GetComponent<S4_RiverPiece> ().SetDry ();
			}
			branchStart = false;
		}
	}

	//return the quantity of dry branches
	public int BlockedBranches () {
		int blocked_branches = 0;
		foreach (Transform riverParent in this.gameObject.transform) {
			foreach (Transform riverPiece in riverParent) {
				//if the branch is blocked, break
				if (riverPiece.GetComponent<S4_RiverPiece> ().IsBlocked ()) { 
					blocked_branches++;
					break;
				} else
					continue;
			}
		}
		return blocked_branches;
	}

	protected void CreateRiver(GameObject riverParent) {
		//clear the control points, in case not the first call
		river_points.Clear ();

		GameObject riverBranch = new GameObject(riverParent.ToString());
		riverBranch.transform.SetParent (this.gameObject.transform);

		//initialize the control points
		foreach (Transform child in riverParent.transform)
		{
			river_points.Add (child);
		}
		//create the river pieces
		for (int i = 0; i < (river_points.Count - 1); i++) {
			//if it is the last piece
			if (i == river_points.Count - 2) {  
				GameObject watermill = null;
				Debug.Log (riverParent.ToString ());
				if(riverParent.ToString().Equals("RiverParent (UnityEngine.GameObject)"))
					watermill = GameObject.Find ("WaterMill1");
				else if (riverParent.ToString().Equals("RiverParent2 (UnityEngine.GameObject)"))
					watermill = GameObject.Find ("WaterMill2");
				else if (riverParent.ToString().Equals("RiverParent3 (UnityEngine.GameObject)"))
					watermill = GameObject.Find ("WaterMill3");
				Debug.Log (watermill.ToString());
				CreateRiverPiece (river_points [i], river_points [i + 1], riverBranch, true, watermill);
			}
			else
				CreateRiverPiece (river_points [i], river_points [i + 1], riverBranch);

		}
	}

	private void CreateRiverPiece (Transform point1, Transform point2, GameObject riverBranch, bool lastPiece = false, GameObject watermill = null) 
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
		if (point1.CompareTag ("ShootingTarget"))
			riverPiece.GetComponent<S4_RiverPiece> ().MakeTarget ();
		if (lastPiece) {
			riverPiece.GetComponent<S4_RiverPiece> ().MakeLastPiece ();
			riverPiece.GetComponent<S4_RiverPiece> ().waterMill = watermill;
		}

	}

	protected Vector3 GetRandomPositionOnRivers()
	{
		//it should be guaranteed that there are any free spaces before. If not it goes into an infinite loop!
		int index = Random.Range (0, positions_dykes_on_rivers.Count);
		//while is busy, look for a new one
		while(positions_dykes_on_rivers[index].GetComponent<S4_RiverPiece>().dyke)
			index = Random.Range (0, positions_dykes_on_rivers.Count);

		return positions_dykes_on_rivers[index].GetComponent<S4_RiverPiece>().startingPoint.position;
	}


}
