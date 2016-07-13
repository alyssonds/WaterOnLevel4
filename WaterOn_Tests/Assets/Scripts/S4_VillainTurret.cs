using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class S4_VillainTurret : MonoBehaviour {

	//variables for the idle movement
	public float idle_movement_amplitude = 0.25f;
	public float idle_movement_speed = 1.0F;
	private Vector3 initial_pos;
	private Vector3 start_marker;
	private Vector3 end_marker;
	private float start_time;
	private float journey_length;
	private bool going_up = true;
	private float frac_journey;

	//variables for the sight detection
	public float field_of_view_angle = 110f;           // Number of degrees, centred on forward, for the enemy see.
	public float moving_speed = 4.0f;				// Speed with the turret moves while aiming 
	private bool cloud_in_sight;                      // Whether or not the player is currently sighted.
	private SphereCollider col;                     // Reference to the sphere collider trigger component.


	//variables for shooting
	public AudioClip shoot_audioclip;                          // An audio clip to play when a shot happens.
	public AudioClip charge_audioclip;                        // An audio clip to play when the turret charges.
	public float shoot_flash_intensity = 4f;                   // The intensity of the light when the shot happens.
	public float time_to_fade_after_shoot = 2f;                       // How fast the light will fade after the shot.
	public float time_to_charge_shoot = 0.7f; 					// Time the turret takes to charge before shooting.
	private LineRenderer laser_shot_line;                 // Reference to the laser shot line renderer.
	private Light laser_shot_light;                       // Reference to the laser shot light.
	private Transform player;                           // Reference to the player's transform.
	private bool shooting;                              // A bool to say whether or not the enemy is currently shooting.
	private List<GameObject> frozen_clouds = new List<GameObject>();

	//variable for altering color when heated
	[Range(0f,1f)]
	private float interpolate = 0.0f;

	public enum TurretStatus
	{
		Idle,
		Searching,
		Charging,
		Fire
	}
	protected TurretStatus _turretStatus = TurretStatus.Idle;

	void Start () {
		start_time = Time.time;
		initial_pos = this.transform.position;
		start_marker = new Vector3 (initial_pos.x, initial_pos.y - idle_movement_amplitude, initial_pos.z);
		end_marker = new Vector3 (initial_pos.x, initial_pos.y + idle_movement_amplitude, initial_pos.z);
		journey_length = Vector3.Distance(start_marker, end_marker);
		col = this.GetComponent<SphereCollider> ();	
		laser_shot_line = this.GetComponentInChildren<LineRenderer> ();
		laser_shot_light = this.GetComponentInChildren<Light> ();
		shoot_audioclip = Instantiate(Resources.Load("Sounds/fx-shot", typeof(AudioClip)) as AudioClip);
		charge_audioclip = Instantiate(Resources.Load("Sounds/fx-charge", typeof(AudioClip)) as AudioClip);

		cloud_in_sight = false;

		laser_shot_line.enabled = false;
		laser_shot_light.intensity = 0f;
	}
		
	void OnTriggerEnter	 (Collider other)
	{
		// If a cloud has entered the trigger sphere, and is not already frozen, and there was no other in sight
		if(other.CompareTag("Cloud") && !CloudIsFrozen(other.gameObject) && !cloud_in_sight)
		{
			// By default the player is not in sight.
			//playerInSight = false;

			// Create a vector from the enemy to the player and store the angle between it and forward.
			Vector3 direction = other.transform.position - this.transform.position;
			float angle = Vector3.Angle(direction, transform.forward);

			// If the angle between forward and where the player is, is less than half the angle of view...
			if(angle < field_of_view_angle * 0.5f)
			{
				RaycastHit hit;

				// ... and if a raycast towards the player hits something...
				if(Physics.Raycast(transform.position, direction.normalized, out hit, col.radius))
				{
					// ... and if the raycast hits the player...
					if(hit.collider.gameObject.CompareTag("Cloud"))
					{
						
						// ... the player is in sight.
						cloud_in_sight = true;
						StartCoroutine(Shoot (other.gameObject));
					}
				}
			}
		}
	}

	protected void Aim(GameObject target) {
		Quaternion neededRotation = Quaternion.LookRotation(target.transform.position - this.transform.position);
		float acc = 0;
		while(acc < 0.99f) {
			this.transform.rotation = Quaternion.Slerp(this.transform.rotation, neededRotation, Time.deltaTime * moving_speed);
			acc += Time.deltaTime;
		}
	}

	IEnumerator KeepInSight (GameObject target) {
		Quaternion neededRotation = Quaternion.LookRotation(target.transform.position - this.transform.position);
		this.transform.rotation = Quaternion.Slerp(this.transform.rotation, neededRotation, Time.deltaTime * moving_speed);
		yield return null;
		StartCoroutine ("KeepInSight", target);
	}

	IEnumerator Shoot (GameObject target)
	{
		Quaternion initialRotation = this.transform.rotation;
		Vector3 initialPosition = this.transform.position;

		//Aim
		Quaternion neededRotation = Quaternion.LookRotation(target.transform.position - this.transform.position);
		float acc = 0;
		while(acc < 0.99f) {
			this.transform.rotation = Quaternion.Slerp(this.transform.rotation, neededRotation, Time.deltaTime * moving_speed);
			yield return null;
			acc += Time.deltaTime;
		}
		//Aim (target);
		cloud_in_sight = true;
		StartCoroutine ("KeepInSight", target);

		//Charge
		float chargeSpeed = (1f - laser_shot_light.intensity) / time_to_charge_shoot;
		float elapse_time = 0f;
		AudioSource.PlayClipAtPoint(charge_audioclip, laser_shot_light.transform.position);
		while (elapse_time < time_to_charge_shoot)
		{
			laser_shot_light.intensity = Mathf.Lerp(laser_shot_light.intensity, 1f, chargeSpeed * Time.deltaTime);

			elapse_time += Time.deltaTime;

			yield return null;
		}
			
		//Shoot
		// Set the initial position of the line renderer to the position of the muzzle.
		laser_shot_line.SetPosition(0, laser_shot_line.transform.position);
		// Set the end position of the player's centre of mass.
		laser_shot_line.SetPosition(1, target.transform.position);
		// Turn on the line renderer.
		laser_shot_line.enabled = true;
		// Make the light flash.
		laser_shot_light.intensity = shoot_flash_intensity;
		// Play the gun shot clip at the position of the muzzle flare.
		AudioSource.PlayClipAtPoint(shoot_audioclip, laser_shot_light.transform.position);
		// Stop aiming
		StopCoroutine("KeepInSight");

		//freeze the cloud
		frozen_clouds.Add(target);
		target.gameObject.GetComponent<S4_Cloud> ().Freeze ();

		yield return new WaitForSeconds(0.1f);

		//After shooting
		laser_shot_line.enabled = false;

		//Decharging the turret
		float fadeSpeed = (laser_shot_light.intensity) / time_to_fade_after_shoot;
		elapse_time = 0f;
		while (elapse_time < time_to_fade_after_shoot)
		{
			laser_shot_light.intensity = Mathf.Lerp(laser_shot_light.intensity, 0f, fadeSpeed * Time.deltaTime);

			elapse_time += Time.deltaTime;

			yield return null;
		}

		//Moving back to original position 
		acc = 0f;
		while(acc < 1f) {
			this.transform.rotation = Quaternion.Slerp(this.transform.rotation, initialRotation, Time.deltaTime * moving_speed);
			yield return null;
			acc += Time.deltaTime;
		}

		cloud_in_sight = false;

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
		foreach (GameObject frozen in frozen_clouds) {
			if (cloud.GetInstanceID () == frozen.GetInstanceID ())
				cloudIsFrozen = true;
		}
		return cloudIsFrozen;
	}

	private void IdleMovement() {
		//Create idle movement
		float distCovered = (Time.time - start_time) * idle_movement_speed;;
		frac_journey = distCovered / journey_length;
		transform.position = Vector3.Lerp(start_marker, end_marker, frac_journey);

		if (going_up && frac_journey >= 0.99) {	
			going_up = false;
			Vector3 temp = start_marker;
			start_marker = end_marker;
			end_marker = temp;
			start_time = Time.time;
			frac_journey = 0.0f;
		} else if (!going_up && frac_journey >= 0.99) {
			going_up = true;
			Vector3 temp = start_marker;
			start_marker = end_marker;
			end_marker = temp;
			start_time = Time.time;
			frac_journey = 0.0f;
		}
	}


	void Update() {
		
		if(!cloud_in_sight)
			IdleMovement ();
		//else
		//	this.transform.LookAt (lockedCloud.transform);
		Component[] rendererArray = this.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer renderer in rendererArray) {
			renderer.material.SetColor("_Color",Color.Lerp(Color.white, Color.red, interpolate));
		}
	}

	public void Heat(float heating_power) {
		if (interpolate < 1.0f)
			interpolate += heating_power * Time.deltaTime;
		else {
			DestroyTurret ();
		}
	}

	private void DestroyTurret () {
		ParticleSystem explosion = this.transform.GetChild (2).GetComponent<ParticleSystem> ();
		explosion.Play ();
		//Make the object invisible
		MeshRenderer[] renderers = this.gameObject.GetComponentsInChildren<MeshRenderer> ();
		foreach (MeshRenderer renderer in renderers) {
			renderer.enabled = false;
		}
		//Stop the engines
		this.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
		this.transform.GetChild(1).GetComponent<ParticleSystem>().Stop();

		Destroy (this.gameObject, explosion.duration);

	}

	void ChangeStatus (TurretStatus status){
		switch (status) {
		case TurretStatus.Idle:
			//if(_turretStatus == TurretStatus.Fire)
			break;
		case TurretStatus.Searching:
			break;
		case TurretStatus.Charging:
			//StartCoroutine(KeepInSight)
			break;
		case TurretStatus.Fire:
			break;
		}
	}

}
