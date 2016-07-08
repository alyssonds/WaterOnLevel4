using UnityEngine;
using System.Collections;

public class S4_Utils {

	public static bool Approximately(float a, float b, float threshold)
	{
		return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
	}
}
