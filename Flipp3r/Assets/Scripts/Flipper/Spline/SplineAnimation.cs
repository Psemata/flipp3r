using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class SplineAnimation : MonoBehaviour {
    // Spring
    [SerializeField]
    private GameObject spring;

    // Rotation speed
    private float speed;
    [SerializeField]
    private float springSpeedInactive = 10f;
    [SerializeField]
    private float springSpeedActive = 20f;

    // Spline gate
    public GameObject gate;
    private Material bulbMat;
    private Color baseIntensityBulb;

    // Mat
    private Material springMat;
    private Color baseIntensitySpring;

    // Lights
    public Light springLight;

    // VFX
    public GameObject electricArc;

    void Update() {
        this.spring.transform.Rotate(new Vector3(0f, 0f, this.speed));
    }

    public void GetMatSpline() {
        this.springMat = spring.GetComponent<Renderer>().material;
        this.baseIntensitySpring = this.springMat.GetColor("_EmissiveColor");

        this.bulbMat = this.gate.GetComponent<Renderer>().materials[2];
        this.baseIntensityBulb = this.bulbMat.GetColor("_EmissiveColor");
    }

    // Activate the animations of the splines' entries
    public void Activate() {
        this.speed = springSpeedActive;
        this.electricArc.SetActive(true);

        this.springMat.SetColor("_EmissiveColor", this.baseIntensitySpring);
        this.bulbMat.SetColor("_EmissiveColor", this.baseIntensityBulb);

        this.springLight.enabled = true;
    }

    // Desactivate the animations of the splines' entries
    public void Desactivate() {
        this.speed = springSpeedInactive;
        this.electricArc.SetActive(false);

        this.springMat.SetColor("_EmissiveColor", this.baseIntensitySpring / 10);
        this.bulbMat.SetColor("_EmissiveColor", this.baseIntensityBulb / 10);
        
        this.springLight.enabled = false;
    }
}
