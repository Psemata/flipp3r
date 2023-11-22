using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall: MonoBehaviour {

    [SerializeField]
    private bool direction; // true : x axis, false : y axis

    // Animations
    private bool inAnimation;
    public float animationTime = 0.6f; // Same as the ball

    // Shake parameters
    [SerializeField]
    private float shakeAmount = 1f;
    private Vector3 originalPos;

    // VFX
    [SerializeField]
    private ParticleSystem collision;
    
    void Start() {
        // Animation state
        this.inAnimation = false;
    }

    void OnTriggerEnter(Collider collider) {
        // When a ball collided with the bumper
        if(!inAnimation && collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ball"))) {
            // Shake animation
            StartCoroutine(Shake());
            // VFX
            this.collision.Play();
            // Audio
		    AudioManager.Instance.Play("wall-hit");
        }
    }

    // Animations
    void Update() {
        if (this.inAnimation) {
            ShakeAnimation();
        }
    }

    // Shaking animation of the bumper
    void ShakeAnimation() {
        Vector3 newPos = this.originalPos + Random.insideUnitSphere * (Time.deltaTime * this.shakeAmount);
        if(direction) {
            newPos.y = transform.position.y;
            newPos.z = transform.position.z;
        } else {
            newPos.y = transform.position.y;
            newPos.x = transform.position.x;
        }
        
        transform.position = newPos;
    }

    // Coroutine used to set the shake animation
    IEnumerator Shake() {
        this.originalPos = transform.position;
        this.inAnimation = true;
        
        yield return new WaitForSeconds(this.animationTime);

        this.inAnimation = false;
        transform.position = this.originalPos;
    }

}
