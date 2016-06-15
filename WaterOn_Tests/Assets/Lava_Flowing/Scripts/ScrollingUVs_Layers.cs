using UnityEngine;
using System.Collections;

public class ScrollingUVs_Layers : MonoBehaviour 
{
	//public int materialIndex = 0;
	public Vector2 uvAnimationRate = new Vector2( 0.25f, 0.0f );
	public string textureName = "_MainTex";
	
	Vector2 uvOffset = Vector2.zero;
	
	void LateUpdate() 
	{
		//changed here, previously was as commented
		//uvOffset += ( uvAnimationRate * Time.deltaTime );
		uvOffset -= ( uvAnimationRate * Time.deltaTime );
		if( GetComponent<Renderer>().enabled )
		{
			GetComponent<Renderer>().sharedMaterial.SetTextureOffset( textureName, uvOffset );
			//GetComponent<Renderer> ().sharedMaterial.SetColor (1,new Color(0,0,0,1));
		}
	}
}