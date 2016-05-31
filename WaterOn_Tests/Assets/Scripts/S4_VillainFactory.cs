using UnityEngine;
using System.Collections;
using DG.Tweening;

public class S4_VillainFactory : MonoBehaviour {

	public Transform[] positionsDykesOnRivers = null;
	public Transform startingBulletPosition = null;

	protected float dishAntennaTurningSpeed = 20f;
	protected float catapultOpeningTime = 0.6f;
	protected float bulletPathTime = 2f;

	protected GameObject dishAntenna = null;
	protected GameObject catapultOpeningLeft = null;
	protected GameObject catapultOpeningRight = null;
	protected bool catapultIsOpen = false;

	void Start () 
	{
		dishAntenna = transform.Find ("DishAntenna").gameObject;
		catapultOpeningLeft = transform.Find ("openingLeft").gameObject;
		catapultOpeningRight = transform.Find ("openingRight").gameObject;

		ShotDykes ();
	}

	void Update () 
	{
		dishAntenna.transform.Rotate (Vector3.up * Time.deltaTime * dishAntennaTurningSpeed, Space.World);
	}

	public void ShotDykes()
	{
		StartCoroutine (ShootingDykes());
	}

	IEnumerator ShootingDykes()
	{
		catapultIsOpen = true;
		// opening doors
		float catLeftYtmp = catapultOpeningLeft.transform.localScale.y;
		catapultOpeningLeft.transform.DOScaleY(0f, catapultOpeningTime);
		float catRightYtmp = catapultOpeningRight.transform.localScale.y;
		catapultOpeningRight.transform.DOScaleY (0f, catapultOpeningTime);
		yield return new WaitForSeconds (catapultOpeningTime);

		// Shooting
		GameObject bullet = Instantiate(Resources.Load("Prefab/S4_ClosedDyke", typeof(GameObject)) as GameObject);
		bullet.transform.position = startingBulletPosition.position;
		// Setting Path Points
		Vector3[] waypoints = new [] {bullet.transform.position, new Vector3(11.5f,7.8f,-11.5f), GetRandomPositionOnRivers()};
		bullet.transform.DOPath (waypoints, bulletPathTime, PathType.CatmullRom, PathMode.Full3D, 10);

		//closing doors
		catapultOpeningLeft.transform.DOScaleY(catLeftYtmp, catapultOpeningTime);
		catapultOpeningRight.transform.DOScaleY (catRightYtmp, catapultOpeningTime);
		yield return new WaitForSeconds (catapultOpeningTime);
		catapultIsOpen = false;
	}

	protected Vector3 GetRandomPositionOnRivers()
	{
		return positionsDykesOnRivers[Random.Range(0,positionsDykesOnRivers.Length)].position;
	}
}
