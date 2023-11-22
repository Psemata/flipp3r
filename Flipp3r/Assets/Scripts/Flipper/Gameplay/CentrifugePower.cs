using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentrifugePower : MonoBehaviour {
    
    // Animation duration
    [SerializeField]
    private float duration = 3f;

    // Parent
    public Centrifuge centrifuge;

    // Ball position
    public Transform loadPosition;

    [SerializeField]
    private float outSpeed = 10f;

    // VFX
    public ParticleSystem energyGain;

    void OnTriggerEnter(Collider collider) {
        if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ball"))) {
            if(this.centrifuge.isActive) {
                StartCoroutine(Centrifuge(collider.gameObject));
                this.GetComponent<Collider>().enabled = false;
            }
        }
    }

    IEnumerator Centrifuge(GameObject ball) {
        // Power activation
        this.energyGain.Play();

        // Audio
		AudioManager.Instance.Play("portal-loading");

        ball.GetComponent<Ball>().isInPortal = true;
        ball.transform.position = this.loadPosition.position;
        ball.GetComponent<Ball>().speed = Vector3.zero;
        
        yield return new WaitForSeconds(duration);

        this.energyGain.Stop();
        Vector3 newPosition = ball.transform.position;
        newPosition.y = 1f;
        ball.transform.position = newPosition;
        ball.GetComponent<Ball>().isInPortal = false;
        ball.GetComponent<Ball>().PlayElectricityTrail();
        ball.GetComponent<Ball>().speed = SetNewSpeed();

        this.GetComponent<Collider>().enabled = true;

        // Audio
		AudioManager.Instance.Play("portal-exit");

        // Cooldown
        StartCoroutine(this.centrifuge.Cooldown());
    }

    Vector3 SetNewSpeed() {
        float x = Random.Range(0f, outSpeed);
        float z = Random.Range(-outSpeed, outSpeed);
        return new Vector3(x, 0f, z);
    }
}
