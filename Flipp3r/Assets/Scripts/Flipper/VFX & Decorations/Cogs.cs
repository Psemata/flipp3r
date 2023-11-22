using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cogs : MonoBehaviour {

    [SerializeField]
    private GameObject cog1;
    [SerializeField]
    private GameObject cog2;
    [SerializeField]
    private GameObject cog3;
    [SerializeField]
    private GameObject cog4;

    private bool cogsMoved = false;

    private float speed = 2f;

    void Update() {
        if(!cogsMoved) {
            StartCoroutine(CogsRotation());
        }
    }

    // Animation of the cogs
    IEnumerator CogsRotation() {
        cogsMoved = true;

        StartCoroutine(CogRotation(cog1));
        StartCoroutine(CogRotation(cog2));
        StartCoroutine(CogRotation(cog3));
        StartCoroutine(CogRotation(cog4));

        float waiting = Random.Range(5f, 15f);

        yield return new WaitForSeconds(waiting);

        StartCoroutine(CogRotationBack(cog1));
        StartCoroutine(CogRotationBack(cog2));
        StartCoroutine(CogRotationBack(cog3));
        StartCoroutine(CogRotationBack(cog4));

        yield return new WaitForSeconds(waiting);

        cogsMoved = false;
    }

    IEnumerator CogRotation(GameObject cog) {
        int index = 0;
        while(index < 30) {
            cog.transform.RotateAround(cog.transform.position, cog.transform.forward, this.speed);
            index++;
            yield return new WaitForSeconds(0.001f);
        }
    }

    IEnumerator CogRotationBack(GameObject cog) {
        int index = 0;
        while(index < 30) {
            cog.transform.RotateAround(cog.transform.position, cog.transform.forward, -this.speed);
            index++;
            yield return new WaitForSeconds(0.001f);
        }
    }
}
