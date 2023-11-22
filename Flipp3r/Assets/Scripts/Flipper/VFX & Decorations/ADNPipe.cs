using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ADNPipe : MonoBehaviour {
    
    public float speed = 0.5f;
    public float duration = 15f;

    private bool animationRotation = true;

    // Animate the ADN pipes
    void Update() {
        if(this.animationRotation) {
            this.transform.Rotate(new Vector3(0f, 0f, this.speed));
            StartCoroutine(AnimationWait());
        }
    }

    IEnumerator AnimationWait() {
        yield return new WaitForSeconds(duration);
        
        this.animationRotation = false;

        yield return new WaitForSeconds(duration);

        this.animationRotation = true;
    }
}
