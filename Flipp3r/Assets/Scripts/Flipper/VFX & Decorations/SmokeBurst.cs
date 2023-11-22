using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeBurst : MonoBehaviour {

    // Places where the smoke will burst
    [SerializeField]
    private List<Transform> smokeSpot = new List<Transform>();

    // The smoke burst
    [SerializeField]
    private ParticleSystem smokeBurst;

    // The interval between bursts
    [SerializeField]
    private float intervalMin = 50f;
    [SerializeField]
    private float intervalMax = 200f;
    private bool hasBurst = false;

    void Update() {
        if(!hasBurst) {
            StartCoroutine(Smoke());
        }
    }

    IEnumerator Smoke() {
        this.hasBurst = true;
        float interval = Random.Range(this.intervalMin, this.intervalMax);

        yield return new WaitForSeconds(interval);

        int index = Random.Range(0, this.smokeSpot.Count);

        smokeBurst.transform.position = smokeSpot[index].position;
        smokeBurst.transform.rotation = smokeSpot[index].rotation;

        this.smokeBurst.Play();

        // Audio
        AudioManager.Instance.Play("vapor");

        this.hasBurst = false;
    }
}
