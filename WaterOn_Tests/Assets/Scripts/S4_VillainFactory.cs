using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class S4_VillainFactory : MonoBehaviour {

	//protected Transform[] positionsDykesOnRivers = null;
	protected List<GameObject> positionsDykesOnRivers = new List<GameObject> ();
	public Transform startingBulletPosition = null;
	public Transform dykeDestination = null;

	protected float firingAngle = 60.0f;
	protected float gravity = 50f;

	private float maximumHeight = 7.8f; 

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
	}

	void Update () 
	{
		dishAntenna.transform.Rotate (Vector3.up * Time.deltaTime * dishAntennaTurningSpeed, Space.World);

	}

	public void ShotDykes(GameObject bullet, Vector3 position)
	{
		//StartCoroutine (ShootingDykes());
		StartCoroutine(SimulateProjectile(bullet, position));
	}

	IEnumerator SimulateProjectile(GameObject bullet, Vector3 targetPosition)
	{
		catapultIsOpen = true;
		// opening doors
		float catLeftYtmp = catapultOpeningLeft.transform.localScale.y;
		catapultOpeningLeft.transform.DOScaleY(0f, catapultOpeningTime);
		float catRightYtmp = catapultOpeningRight.transform.localScale.y;
		catapultOpeningRight.transform.DOScaleY (0f, catapultOpeningTime);
		yield return new WaitForSeconds (catapultOpeningTime);

		//GameObject bullet = Instantiate(Resources.Load("Prefab/S4_ClosedDyke", typeof(GameObject)) as GameObject);
		//bullet.AddComponent<S4_Dyke> ();
		bullet.transform.position = startingBulletPosition.position;

		// Calculate distance to target
		float target_Distance = Vector3.Distance(bullet.transform.position, targetPosition);

		// Calculate the velocity needed to throw the object to the target at specified angle.
		float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);

		// Extract the X  Y componenent of the velocity
		float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
		float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);

		// Calculate flight time.
		bulletPathTime = target_Distance / Vx;

		// Rotate projectile to face the target.
		bullet.transform.rotation = Quaternion.LookRotation(targetPosition - bullet.transform.position);

		float elapse_time = 0;

		while (elapse_time < bulletPathTime)
		{
			bullet.transform.Translate(0, (Vy - (gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);

			elapse_time += Time.deltaTime;

			yield return null;
		}

		//DIZER QUE TA BUSY
		/*foreach (Transform riverParent in GameObject.Find("River2").transform) {
			foreach (Transform riverPiece in riverParent) {
				if (riverPiece.gameObject.GetComponent<S4_RiverPiece> ().startingPoint.position == targetPosition)
					riverPiece.gameObject.GetComponent<S4_RiverPiece> ().SetDry ();
			}
		}*/
		GameObject.Find ("River2").GetComponent<S4_River> ().DryBranch (targetPosition);

		//closing doors
		catapultOpeningLeft.transform.DOScaleY(catLeftYtmp, catapultOpeningTime);
		catapultOpeningRight.transform.DOScaleY (catRightYtmp, catapultOpeningTime);
		yield return new WaitForSeconds (catapultOpeningTime);
		catapultIsOpen = false;
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
		Vector3 highestPoint = new Vector3((bullet.transform.position.x + dykeDestination.transform.position.x)/2f, maximumHeight,(bullet.transform.position.z + dykeDestination.transform.position.z)/2f);
		Vector3 goingUp = new Vector3 ((bullet.transform.position.x + highestPoint.x)/2f, 0.75f*maximumHeight,(bullet.transform.position.z + highestPoint.z)/2f );
		Vector3 goingDown = new Vector3 ((dykeDestination.transform.position.x + highestPoint.x)/2f, 0.75f*maximumHeight,(dykeDestination.transform.position.z + highestPoint.z)/2f );

		Vector3[] waypoints = new [] {bullet.transform.position, highestPoint,dykeDestination.transform.position};
		Vector3[] waypointsUp = new Vector3[] {bullet.transform.position, goingUp ,highestPoint};
		Vector3[] waypointsDown = new Vector3[] {highestPoint, goingDown ,dykeDestination.transform.position};

		Tween movement = bullet.transform.DOPath (waypointsUp, bulletPathTime, PathType.CatmullRom, PathMode.Full3D, 10).SetEase(Ease.OutCubic);
	//	yield return movement.WaitForCompletion ();
		//movement.wai
		yield return new WaitForSeconds(bulletPathTime-0.2f);
		//	Debug.Log ("Terminou o tween");
		bullet.transform.DOPath (waypointsDown, bulletPathTime, PathType.CatmullRom, PathMode.Full3D, 10).SetEase(Ease.InCubic);

	//	Vector3[] waypoints = new [] {bullet.transform.position, new Vector3(11.5f,7.8f,-11.5f),GetRandomPositionOnRivers()};
	//	Vector3[] waypointsUp = new [] {bullet.transform.position, new Vector3(19.9f, 4.3f,-16.8f) ,new Vector3(11.5f,7.8f,-11.5f)};
	//	Vector3[] waypointsDown = new [] {new Vector3(11.5f,7.8f,-11.5f), new Vector3(4.83f, 3.9f,-13.82f) ,new Vector3(0f,-3.55f,-16.55f)};
		//Vector3[] waypointsDown = new [] {bullet.transform.position, new Vector3(0f,-3.55f,-16.55f)};


		/*yield return movement.WaitForPosition (bulletPathTime/2f);
		movement.Pause ();
		yield return movement.WaitForElapsedLoops (1);
		movement.Play ();*/
		//movement.wa
		//movement.SetEase (Ease.InCubic);
		//Vector3[] waypointsUp = new [] {bullet.transform.position, new Vector3(11.5f,7.8f,-11.5f)};
	//	Vector3[] waypointsDown = new [] {new Vector3(11.5f,7.8f,-11.5f), GetRandomPositionOnRivers()};
		//Tween up = bullet.transform.DOPath (waypointsUp, bulletPathTime/2, PathType.CatmullRom, PathMode.Full3D, 10).SetEase(Ease.OutCubic);
		//yield return up.WaitForCompletion (); 
		//yield return new WaitForSeconds (bulletPathTime/4);
		//Vector3[] waypointsDown = new [] {bullet.transform.position, GetRandomPositionOnRivers()};
		//bullet.transform.DOPath (waypointsDown, bulletPathTime/2, PathType.CatmullRom, PathMode.Full3D, 10).SetEase(Ease.InCubic);
	
		//closing doors
		catapultOpeningLeft.transform.DOScaleY(catLeftYtmp, catapultOpeningTime);
		catapultOpeningRight.transform.DOScaleY (catRightYtmp, catapultOpeningTime);
		yield return new WaitForSeconds (catapultOpeningTime);
		catapultIsOpen = false;
	}

/*	protected Vector3 GetRandomPositionOnRivers()
	{
		//it should be guaranteed that there are any free spaces before. If not it goes into an infinite loop!
		int index = Random.Range (0, positionsDykesOnRivers.Count);
		while(positionsDykesOnRivers[index].IsBusy())
			index = Random.Range (0, positionsDykesOnRivers.Count);
		positionsDykesOnRivers[index].SetBusy ();
		return positionsDykesOnRivers[index].transform.position;
	}*/
}
