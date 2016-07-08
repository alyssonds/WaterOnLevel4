using UnityEngine;
using System.Collections;

public class S4_VillainTurret : MonoBehaviour {

	//variables for the idle movement
	public float movementAmplitude = 0.25f;
	public float speed = 1.0F;
	private Vector3 initialPos;
	private Vector3 startMarker;
	private Vector3 endMarker;
	private float startTime;
	private float journeyLength;
	private bool goingUp = true;
	private float fracJourney;

	//variables for the sight detection
	public float fieldOfViewAngle = 110f;           // Number of degrees, centred on forward, for the enemy see.
	private bool cloudInSight;                      // Whether or not the player is currently sighted.
	private SphereCollider col;                     // Reference to the sphere collider trigger component.
	private GameObject lockedCloud;

	//variables for shooting
	public AudioClip shotClip;                          // An audio clip to play when a shot happens.
	public float flashIntensity = 3f;                   // The intensity of the light when the shot happens.
	public float fadeSpeed = 10f;                       // How fast the light will fade after the shot.
	private LineRenderer laserShotLine;                 // Reference to the laser shot line renderer.
	private Light laserShotLight;                       // Reference to the laser shot light.
	//private SphereCollider col;                         // Reference to the sphere collider.
	private Transform player;                           // Reference to the player's transform.
	private bool shooting;                              // A bool to say whether or not the enemy is currently shooting.

	void Start () {
		startTime = Time.time;
		initialPos = this.transform.position;
		startMarker = new Vector3 (initialPos.x, initialPos.y - movementAmplitude, initialPos.z);
		endMarker = new Vector3 (initialPos.x, initialPos.y + movementAmplitude, initialPos.z);
		journeyLength = Vector3.Distance(startMarker, endMarker);
		col = this.GetComponent<SphereCollider> ();	
		cloudInSight = false;
	}
		
	void OnTriggerStay (Collider other)
	{
		// If the player has entered the trigger sphere...
		if(other.CompareTag("Cloud") && !cloudInSight)
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
						this.transform.LookAt (hit.collider.transform);
						lockedCloud = hit.collider.gameObject;
					}
				}
			}
		}
	}


	void OnTriggerExit (Collider other)
	{
		// If the player leaves the trigger zone...
		if(other.CompareTag("Cloud"))
			// ... the player is not in sight.
			cloudInSight = false;
	}

	private void IdleMovement() {
		//Create idle movement
		float distCovered = (Time.time - startTime) * speed;
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

	void Shot ()
	{
		// Set the initial position of the line renderer to the position of the muzzle.
		laserShotLine.SetPosition(0, laserShotLine.transform.position);

		// Set the end position of the player's centre of mass.
		laserShotLine.SetPosition(1, player.position + Vector3.up * 1.5f);

		// Turn on the line renderer.
		laserShotLine.enabled = true;

		// Make the light flash.
		laserShotLight.intensity = flashIntensity;

		// Play the gun shot clip at the position of the muzzle flare.
		//	AudioSource.PlayClipAtPoint(shotClip, laserShotLight.transform.position);
	}

	void Update() {
		
		if(!cloudInSight)
			IdleMovement ();
		else
			this.transform.LookAt (lockedCloud.transform);

	}

/*		void Awake ()
		{
			// Setting up the references.
			anim = GetComponent<Animator>();
			laserShotLine = GetComponentInChildren<LineRenderer>();
			laserShotLight = laserShotLine.gameObject.light;
			col = GetComponent<SphereCollider>();
			player = GameObject.FindGameObjectWithTag(Tags.player).transform;
			playerHealth = player.gameObject.GetComponent<PlayerHealth>();
			hash = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<HashIDs>();

			// The line renderer and light are off to start.
			laserShotLine.enabled = false;
			laserShotLight.intensity = 0f;

			// The scaledDamage is the difference between the maximum and the minimum damage.
			scaledDamage = maximumDamage - minimumDamage;
		}


		void Update ()
		{
			// Cache the current value of the shot curve.
			float shot = anim.GetFloat(hash.shotFloat);

			// If the shot curve is peaking and the enemy is not currently shooting...
			if(shot > 0.5f && !shooting)
				// ... shoot
				Shoot();

			// If the shot curve is no longer peaking...
			if(shot < 0.5f)
			{
				// ... the enemy is no longer shooting and disable the line renderer.
				shooting = false;
				laserShotLine.enabled = false;
			}

			// Fade the light out.
			laserShotLight.intensity = Mathf.Lerp(laserShotLight.intensity, 0f, fadeSpeed * Time.deltaTime);
		}*/

}
