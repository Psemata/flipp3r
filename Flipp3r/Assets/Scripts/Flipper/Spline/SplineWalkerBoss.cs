using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineWalkerBoss : MonoBehaviour {
	// Spline used to move the gameobject
    public BezierSpline spline;

	// Movement values
    public float duration;
	private float progress;
	
	// Which direction is the gameobject taking in the spline
    public bool goingForward = true;
	
	// The ball which is using
    private GameObject go;

	public ParticleSystem smoke;
	public Light bossLight;
	
    void Update() {
		// Move the gameobject
		if(this.go != null) {
			if (this.goingForward) {
				this.progress += Time.deltaTime / this.duration;
				if (this.progress >= 1f) {
					progress -= 1f;
				}
			} else {
				this.progress -= Time.deltaTime / this.duration;
				if (this.progress <= 0f) {
					progress += 1f;
				}
			}
			this.go.transform.position = spline.GetPoint(progress);
		}
    }

	public void SetSmoke() {
		this.smoke.Play();
		
		// Audio
		AudioManager.Instance.Play("smoke");

		this.duration = 3f;
		this.goingForward = true;
		this.go = this.smoke.gameObject;
	}

	public void SetLight() {
		this.bossLight.gameObject.SetActive(true);
		this.duration = 10f;
		this.goingForward = false;
		this.go = this.bossLight.gameObject;
	}

	public void StopSplineLight() {
		this.bossLight.gameObject.SetActive(false);

		this.go = null;
	}

	public void StopSplineSmoke() {
		this.smoke.Stop();

		// Audio
		AudioManager.Instance.StopPlaying("smoke");

		this.go = null;
	}
}
