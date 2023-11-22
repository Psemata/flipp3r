using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour {
    void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ball"))) {
            collider.gameObject.GetComponent<Ball>().speed = Vector3.zero;
            collider.gameObject.GetComponent<Collider>().enabled = false;
            collider.transform.Find("ElectricArcs").GetComponent<Collider>().enabled = false;
            collider.transform.Find("ElectricArcs").GetComponent<ElectricArc>().ElectricArcsDeath();
            
            // Audio
            if(this.name == "DeathBorder") {
                GameManager.Instance.DeathBall(collider.gameObject, true);
                AudioManager.Instance.Play("glitch");
            } else {
                GameManager.Instance.DeathBall(collider.gameObject, false);
                AudioManager.Instance.Play("death-bille");
            }
            if (collider.gameObject.GetComponent<Ball>().flameTrailSoundPlayed)
            {
                AudioManager.Instance.StopPlaying("fire-bille");
                collider.gameObject.GetComponent<Ball>().flameTrailSoundPlayed = false;
            }
            if (collider.gameObject.GetComponent<Ball>().electricTrailSoundPlayed)
            {
                AudioManager.Instance.StopPlaying("electro-bille");
                collider.gameObject.GetComponent<Ball>().electricTrailSoundPlayed = false;
            }
            if (collider.gameObject.GetComponent<Ball>().electricContactSoundPlayed)
            {
                AudioManager.Instance.StopPlaying("electro-inter-bille");
                collider.gameObject.GetComponent<Ball>().electricContactSoundPlayed = false;
            }
            
            // GameObject destruction - Ball death
            Destroy(collider.gameObject, 0.7f);
        }
    }
}
