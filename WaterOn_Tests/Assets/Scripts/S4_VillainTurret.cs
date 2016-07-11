using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class S4_VillainTurret : MonoBehaviour {

	//variables for the idle movement
	public float movementAmplitude = 0.25f;
	public float idleSpeed = 1.0F;
	private Vector3 initialPos;
	private Vector3 startMarker;
	private Vector3 endMarker;
	private float startTime;
	private float journeyLength;
	private bool goingUp = true;
	private float fracJourney;

	//variables for the sight detection
	public float fieldOfViewAngle = 110f;           // Number of degrees, centred on forward, for the enemy see.
	public float movingSpeed = 2.0f;				// Speed with the turret moves while aiming 
	private bool cloudInSight;                      // Whether or not the player is currently sighted.
	private SphereCollider col;                     // Reference to the sphere collider trigger component.
	private GameObject lockedCloud;					// Cloud in lock to be shot


	//variables for shooting
	public AudioClip shotClip;                          // An audio clip to play when a shot happens.
	public AudioClip chargeClip;                        // An audio clip to play when the turret charges.
	public float flashIntensity = 3f;                   // The intensity of the light when the shot happens.
	public float timeToFade = 2f;                       // How fast the light will fade after the shot.
	public float timeToCharge = 1f; 					// Time the turret takes to charge before shooting.
	private LineRenderer laserShotLine;                 // Reference to the laser shot line renderer.
	private Light laserShotLight;                       // Reference to the laser shot light.
	private Transform player;                           // Reference to the player's transform.
	private bool shooting;                              // A bool to say whether or not the enemy is currently shooting.
	private List<GameObject> frozenClouds = new List<GameObject>();

	public enum TurretStatus
	{
		Idle,
		Searching,
		Charging,
		Fire
	}
	protected TurretStatus _turretStatus = TurretStatus.Idle;

	void Start () {
		startTime = Time.time;
		initialPos = this.transform.position;
		startMarker = new Vector3 (initialPos.x, initialPos.y - movementAmplitude, initialPos.z);
		endMarker = new Vector3 (initialPos.x, initialPos.y + movementAmplitude, initialPos.z);
		journeyLength = Vector3.Distance(startMarker, endMarker);
		col = this.GetComponent<SphereCollider> ();	
		laserShotLine = this.GetComponentInChildren<LineRenderer> ();
		laserShotLight = this.GetComponentInChildren<Light> ();

		cloudInSight = false;

		laserShotLine.enabled = false;
		laserShotLight.intensity = 0f;
	}
		
	void OnTriggerEnter	 (Collider other)
	{
		// If a cloud has entered the trigger sphere, and is not already frozen, and there was no other in sight
		if(other.CompareTag("Cloud") && !CloudIsFrozen(other.gameObject) && !cloudInSight)
		{
			// By default the player is not in sight.
			//playerInSight = false;

			// Create a vector from the enemy to the player and store the angle between it and forward.
			Vector3 direction = other.transform.position - this.transform.position;
			float angle = Vector3.Angle(direction, transform.forward);

			// If the angle between forward and where the player is, is less than half the angle of view...
			if(angle < fieldOfViewAngle * 0.5f)
			{
				RaycastHit hit;

				// ... and if a raycast towards the player hits something...
				if(Physics.Raycast(transform.position, direction.normalized, out hit, col.radius))
				{
					// ... and if the raycast hits the player...
					if(hit.collider.gameObject.CompareTag("Cloud"))
					{
						
						// ... the player is in sight.
						cloudInSight = true;
						Debug.Log ("SHOOT");
						StartCoroutine(Shoot (other.gameObject));
						//FADE

						//lockedCloud = hit.collider.gameObject;
					}
				}
			}
		}
	}

	IEnumerator Shoot (GameObject target)
	{
		Quaternion initialRotation = this.transform.rotation;
		Vector3 initialPosition = this.transform.position;

		//Aim
		Quaternion neededRotation = Quaternion.LookRotation(target.transform.position - this.transform.position);
		float acc = 0;
		while(acc < 0.99f) {
			this.transform.rotation = Quaternion.Slerp(this.transform.rotation, neededRotation, Time.deltaTime * movingSpeed);
			yield return null;
			acc += Time.deltaTime;
		}
		cloudInSight = true;
		lockedCloud = target;

		//Charge
		float chargeSpeed = (1f - laserShotLight.intensity) / timeToCharge;
		float elapse_time = 0f;
		Debug.Log ("Charging");
		AudioSource.PlayClipAtPoint(chargeClip, laserShotLight.transform.position);
		while (elapse_time < timeToCharge)
		{
			laserShotLight.intensity = Mathf.Lerp(laserShotLight.intensity, 1f, chargeSpeed * Time.deltaTime);

			elapse_time += Time.deltaTime;

			yield return null;
		}

		Debug.Log ("Shooting");

		//Shoot
		// Set the initial position of the line renderer to the position of the muzzle.
		laserShotLine.SetPosition(0, laserShotLine.transform.position);
		// Set the end position of the player's centre of mass.
		laserShotLine.SetPosition(1, target.transform.position);
		// Turn on the line renderer.
		laserShotLine.enabled = true;
		// Make the light flash.
		laserShotLight.intensity = flashIntensity;
		// Play the gun shot clip at the position of the muzzle flare.
		AudioSource.PlayClipAtPoint(shotClip, laserShotLight.transform.position);



		//freeze the cloud
		frozenClouds.Add(target);
		target.gameObject.GetComponent<S4_Cloud> ().Freeze ();

		yield return new WaitForSeconds(0.1f);

		Debug.Log ("Decharging");
		//After shooting
		laserShotLine.enabled = false;

		float fadeSpeed = (laserShotLight.intensity) / timeToFade;

		elapse_time = 0f;
		while (elapse_time < timeToFade)
		{
			laserShotLight.intensity = Mathf.Lerp(laserShotLight.intensity, 0f, fadeSpeed * Time.deltaTime);

			elapse_time += Time.deltaTime;

			yield return null;
		}

		acc = 0f;
	//	neededRotation = Quaternion.LookRotation(initialPosition - this.transform.position);
		while(acc < 1f) {
			this.transform.rotation = Quaternion.Slerp(this.transform.rotation, initialRotation, Time.deltaTime * movingSpeed);
			yield return null;
			acc += Time.deltaTime;
		}

		cloudInSight = false;

	}


	/*void OnTriggerExit (Collider other)
	{
		// If the player leaves the trigger zone...
		if(other.CompareTag("Cloud"))
			// ... the player is not in sight.
			cloudInSight = false;
	}*/

	public bool CloudIsFrozen (GameObject cloud) {
		bool cloudIsFrozen = false;
		foreach (GameObject frozen in frozenClouds) {
			if (cloud.GetInstanceID () == frozen.GetInstanceID ())
				cloudIsFrozen = true;
		}
		return cloudIsFrozen;
	}

	private void IdleMovement() {
		//Create idle movement
		float distCovered = (Time.time - startTime) * idleSpeed;;
		fracJourney = distCovered / journeyLength;
		transform.position = Vector3.Lerp(startMarker, endMarker, fracJourney);

		if (goingUp && fracJourney >= 0.99) {	
			goingUp = false;
			Vector3 temp = startMarker;
			startMarker = endMarker;
			endMarker = temp;
			startTime = Time.time;
			fracJourney = 0.0f;
		} else if (!goingUp && fracJourney >= 0.99) {
			goingUp = true;
			Vector3 temp = startMarker;
			startMarker = endMarker;
			endMarker = temp;
			startTime = Time.time;
			fracJourney = 0.0f;
		}
	}


	void Update() {
		
		if(!cloudInSight)
			IdleMovement ();
		//else
		//	this.transform.LookAt (lockedCloud.transform);

	}

	void ChangeStatus (TurretStatus status){
		switch (status) {
		case TurretStatus.Idle:
			//if(_turretStatus == TurretStatus.Fire)
			break;
		case TurretStatus.Searching:
			break;
		case TurretStatus.Charging:
			break;
		case TurretStatus.Fire:
			break;
		}
	}

}
