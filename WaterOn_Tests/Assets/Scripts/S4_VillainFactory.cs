using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class S4_VillainFactory : MonoBehaviour {


	public Transform starting_bullet_position = null;
	protected float firing_angle = 60.0f;
	protected float gravity = 50f;
	private float maximum_height = 7.8f; 
	protected float dish_antenna_turning_speed = 20f;
	protected float catapult_opening_time = 0.6f;

	//needed for old shooting system
	protected Transform dyke_destination = null;
	protected float bullet_path_time = 2f;

	//fixed variables
	protected GameObject dish_antenna = null;
	protected GameObject catapult_opening_left = null;
	protected GameObject catapult_opening_right = null;
	protected bool catapult_is_open = false;

	void Start () 
	{
		dish_antenna = transform.Find ("DishAntenna").gameObject;
		catapult_opening_left = transform.Find ("openingLeft").gameObject;
		catapult_opening_right = transform.Find ("openingRight").gameObject;
	}

	void Update () 
	{
		dish_antenna.transform.Rotate (Vector3.up * Time.deltaTime * dish_antenna_turning_speed, Space.World);

	}

	public void ShootDyke() {
		int i = 0;
		bool freeSpace = false;
		//check if there are any free spaces
		while (i < S4_River.positions_dykes_on_rivers.Count) {
			if (!(S4_River.positions_dykes_on_rivers [i].GetComponent<S4_RiverPiece>().dyke)) {
				freeSpace = true;
				break;
			}
			i++;
		}
		if (freeSpace) {
			GameObject bullet = Instantiate(Resources.Load("Prefab/S4_ClosedDyke", typeof(GameObject)) as GameObject);
			bullet.tag = "Dyke";
			bullet.AddComponent<S4_Dyke> ();
			//it should be guaranteed that there are any free spaces before. If not it goes into an infinite loop!
			int index = Random.Range (0, S4_River.positions_dykes_on_rivers.Count);
			//while is busy, look for a new one
			while(S4_River.positions_dykes_on_rivers[index].GetComponent<S4_RiverPiece>().dyke)
				index = Random.Range (0, S4_River.positions_dykes_on_rivers.Count);

			S4_River.positions_dykes_on_rivers [index].GetComponent<S4_RiverPiece>().dyke = bullet;
			StartShootingCoroutine(bullet, S4_River.positions_dykes_on_rivers[index].GetComponent<S4_RiverPiece>().startingPoint.position);
		} else {
			Debug.Log ("Theres no free space anymore");
		}
	}

	protected void StartShootingCoroutine(GameObject bullet, Vector3 position)
	{
		//StartCoroutine (ShootingDykes());
		StartCoroutine(SimulateProjectile(bullet, position));
	}

	IEnumerator SimulateProjectile(GameObject bullet, Vector3 targetPosition)
	{
		catapult_is_open = true;
		// opening doors
		float catLeftYtmp = catapult_opening_left.transform.localScale.y;
		catapult_opening_left.transform.DOScaleY(0f, catapult_opening_time);
		float catRightYtmp = catapult_opening_right.transform.localScale.y;
		catapult_opening_right.transform.DOScaleY (0f, catapult_opening_time);
		yield return new WaitForSeconds (catapult_opening_time);

		//GameObject bullet = Instantiate(Resources.Load("Prefab/S4_ClosedDyke", typeof(GameObject)) as GameObject);
		//bullet.AddComponent<S4_Dyke> ();
		bullet.transform.position = starting_bullet_position.position;

		// Calculate distance to target
		float target_Distance = Vector3.Distance(bullet.transform.position, targetPosition);

		// Calculate the velocity needed to throw the object to the target at specified angle.
		float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firing_angle * Mathf.Deg2Rad) / gravity);

		// Extract the X  Y component of the velocity
		float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firing_angle * Mathf.Deg2Rad);
		float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firing_angle * Mathf.Deg2Rad);

		// Calculate flight time.
		bullet_path_time = target_Distance / Vx;

		// Rotate projectile to face the target.
		bullet.transform.rotation = Quaternion.LookRotation(targetPosition - bullet.transform.position);

		float elapse_time = 0;

		while (elapse_time < bullet_path_time)
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
		catapult_opening_left.transform.DOScaleY(catLeftYtmp, catapult_opening_time);
		catapult_opening_right.transform.DOScaleY (catRightYtmp, catapult_opening_time);
		yield return new WaitForSeconds (catapult_opening_time);
		catapult_is_open = false;
	}

	IEnumerator ShootingDykes()
	{
		catapult_is_open = true;
		// opening doors
		float catLeftYtmp = catapult_opening_left.transform.localScale.y;
		catapult_opening_left.transform.DOScaleY(0f, catapult_opening_time);
		float catRightYtmp = catapult_opening_right.transform.localScale.y;
		catapult_opening_right.transform.DOScaleY (0f, catapult_opening_time);
		yield return new WaitForSeconds (catapult_opening_time);

		// Shooting
		GameObject bullet = Instantiate(Resources.Load("Prefab/S4_ClosedDyke", typeof(GameObject)) as GameObject);

		bullet.transform.position = starting_bullet_position.position;
		// Setting Path Points
		Vector3 highestPoint = new Vector3((bullet.transform.position.x + dyke_destination.transform.position.x)/2f, maximum_height,(bullet.transform.position.z + dyke_destination.transform.position.z)/2f);
		Vector3 goingUp = new Vector3 ((bullet.transform.position.x + highestPoint.x)/2f, 0.75f*maximum_height,(bullet.transform.position.z + highestPoint.z)/2f );
		Vector3 goingDown = new Vector3 ((dyke_destination.transform.position.x + highestPoint.x)/2f, 0.75f*maximum_height,(dyke_destination.transform.position.z + highestPoint.z)/2f );

		Vector3[] waypoints = new [] {bullet.transform.position, highestPoint,dyke_destination.transform.position};
		Vector3[] waypointsUp = new Vector3[] {bullet.transform.position, goingUp ,highestPoint};
		Vector3[] waypointsDown = new Vector3[] {highestPoint, goingDown ,dyke_destination.transform.position};

		Tween movement = bullet.transform.DOPath (waypointsUp, bullet_path_time, PathType.CatmullRom, PathMode.Full3D, 10).SetEase(Ease.OutCubic);
	//	yield return movement.WaitForCompletion ();
		//movement.wai
		yield return new WaitForSeconds(bullet_path_time-0.2f);
		//	Debug.Log ("Terminou o tween");
		bullet.transform.DOPath (waypointsDown, bullet_path_time, PathType.CatmullRom, PathMode.Full3D, 10).SetEase(Ease.InCubic);

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
		catapult_opening_left.transform.DOScaleY(catLeftYtmp, catapult_opening_time);
		catapult_opening_right.transform.DOScaleY (catRightYtmp, catapult_opening_time);
		yield return new WaitForSeconds (catapult_opening_time);
		catapult_is_open = false;
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
