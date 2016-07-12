using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class S4_Utils {

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


	public static List<Vector3> FindPointsInsideMesh(GameObject go)
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

	public static Vector3 GetPointRandomInCircle(Vector3 pos, float radius)
	{
		int angle = Random.Range(0,360);
		float newX = pos.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
		float newY = pos.y + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
		return new Vector3(newX, newY, pos.z);
	}

	public static bool Approximately(float a, float b, float threshold)
	{
		return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
	}
}
