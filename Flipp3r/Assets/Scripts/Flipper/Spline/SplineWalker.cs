using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineWalker : MonoBehaviour {
	// Spline used to move the ball
    public BezierSpline spline;
	private Transform entryPosition;

	// Movement values
    public float duration;
	private float progress;

	// Spline management
	public bool isOccupied;
	private bool isDone;

	[SerializeField]
	private float cooldown = 10f;
	public bool isActive = true;
	public Collider entryPortal;
	public Collider exitPortal;
	
	// Which direction is the ball taking in the spline
    private bool goingForward = true;

	// Center of the spline (used to differentiate top and bottom)
	private Vector3 center;

	// Out speed
	public Vector3 topSpeed;
	public Vector3 bottomSpeed;
	
	// The ball which is using
    private GameObject ball;

	// VFX
	public SplineAnimation splineAnimationEntry;
	public SplineAnimation splineAnimationExit;

	// Audio management
	private bool soundPlayed = false;

	void Awake() {
		// Spline management values
		this.isOccupied = false;
		this.isDone = false;

		// Center of the gameobject
		this.center = this.transform.position;
		
		// VFX
		// Get the materials of the portals
		this.splineAnimationEntry.GetMatSpline();
		this.splineAnimationExit.GetMatSpline();
		// Activate them
		this.splineAnimationEntry.Activate();
		this.splineAnimationExit.Activate();
	}
	
    void FixedUpdate() {
		// Move the ball
		if(this.ball != null && !this.isDone) {
			if (this.goingForward) {
				this.progress += Time.fixedDeltaTime / this.duration;
				if (this.progress >= 1f) {
					this.isDone = true;
				}
			} else {
				this.progress -= Time.fixedDeltaTime / this.duration;
				if (this.progress <= 0f) {
					this.isDone = true;
				}
			}
			this.ball.transform.position = spline.GetPoint(progress);

			// Audio - In the spline
			if(!this.soundPlayed) {
				AudioManager.Instance.Play("ramp");
				this.soundPlayed = true;
			}
		}
    }

	// When the ball enter the spline
	void OnTriggerEnter(Collider collider) {
		if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ball"))) {
			if(this.isActive) {
				if(!this.isOccupied) {
					if(this.ball == null) {
						Vector3 bPosition = collider.transform.position;
						if(bPosition.z > this.center.z) {
							this.goingForward = false;
							this.progress = 1f;
							this.entryPosition = this.transform.Find("Rampe-Gate-Exit").Find("LoadPosition").transform;
						} else {
							this.goingForward = true;
							this.progress = 0f;
							this.entryPosition = this.transform.Find("Rampe-Gate-Enter").Find("LoadPosition").transform;
						}

						// Start the different VFXs
						StartCoroutine(Magnetism(collider.gameObject));
					}
				} else {
					if(this.ball == collider.gameObject) {
						this.ball.GetComponent<Ball>().StopElectricity();
						this.ball.GetComponent<Ball>().speed = SetOutSpeed();
						this.ball.GetComponent<Ball>().inSlide = false;
						this.ball = null;
												
						// Audio - Exit the spline
						AudioManager.Instance.Play("ramp-exit");
						AudioManager.Instance.shellSoundPlayed = false;
						this.soundPlayed = false;

						// Start cooldown
						StartCoroutine(Cooldown());
					}
				}
			}			
		}
	}

	// Return the ball's speed at the exit of the spline, depending of the direction taken
	Vector3 SetOutSpeed() {
		return goingForward ? topSpeed : bottomSpeed;
	}

	// Cooldown management
	IEnumerator Cooldown() {
		this.isOccupied = false;
		this.isDone = false;

		this.isActive = false;
		this.entryPortal.enabled = true;
		this.exitPortal.enabled = true;

		this.splineAnimationEntry.Desactivate();
		this.splineAnimationExit.Desactivate();

		yield return new WaitForSeconds(this.cooldown);

		this.entryPortal.enabled = false;
		this.exitPortal.enabled = false;
		this.isActive = true;

		this.splineAnimationEntry.Activate();
		this.splineAnimationExit.Activate();
	}

	// Animations
	IEnumerator Magnetism(GameObject ball) {
		ball.transform.position = this.entryPosition.position;
		this.isOccupied = true;
		ball.GetComponent<Ball>().inSlide = true;
		ball.GetComponent<Ball>().speed = Vector3.zero;
		ball.GetComponent<Ball>().Magnetism();

		// Audio - Enter the ramp
		AudioManager.Instance.Play("ramp-enter");

		yield return new WaitForSeconds(0.5f);

		this.ball = ball;
		this.ball.GetComponent<Ball>().PlayElectricity();
	}
}
