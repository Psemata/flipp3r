using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Centrifuge : MonoBehaviour {

    // If the element is active or not
    [SerializeField]
    private float cooldown = 10f;
    public bool isActive = true;
    

    // Rotation speed
    private float speed;
    public float startRotationSpeed = 0.5f;
    public float activeRotationSpeed = 1f;


    // Springs speeds
    private float springsSpeed;
    public float startSpringRotationSpeed = 4f;
    public float activeSpringRotationSpeed = 20f;

    // The 4 springs on the portal
    public GameObject[] springsX = new GameObject[2];
    public GameObject[] springsY = new GameObject[2];

    private Material[] springMats = new Material[4];
    private Color baseIntensity;

    // VFX
    public GameObject[] lightnings = new GameObject[4];

    void Awake() {
        this.springMats[0] = springsX[0].GetComponent<Renderer>().material;
        this.springMats[1] = springsX[1].GetComponent<Renderer>().material;
        this.springMats[2] = springsY[0].GetComponent<Renderer>().material;
        this.springMats[3] = springsY[1].GetComponent<Renderer>().material;

        this.baseIntensity = springMats[0].GetColor("_EmissiveColor");
    }

    void Start() {
        Activate();
    }

    // Rotate the springs
    void Update() {
        this.transform.Rotate(new Vector3(0f, this.speed, 0f));
        for(int i = 0; i < this.springsX.Length ; i++) {
            this.springsX[i].transform.Rotate(new Vector3(this.springsSpeed, 0f, 0f));
        }
        for(int i = 0; i < this.springsY.Length ; i++) {
            this.springsY[i].transform.Rotate(new Vector3(0f, this.springsSpeed, 0f));
        }
    }

    // Activate the portal
    void Activate() {
        this.speed = activeRotationSpeed;
        this.springsSpeed = activeSpringRotationSpeed;
        for(int i = 0; i < this.lightnings.Length; i++) {
            this.lightnings[i].SetActive(true);
        }
        for(int i = 0; i < this.springMats.Length ; i++) {
            this.springMats[i].SetColor("_EmissiveColor", this.baseIntensity);
        }
        this.isActive = true;
    }

    // Desactivate the portal
    void Desactivate() {
        this.speed = startRotationSpeed;
        this.springsSpeed = startSpringRotationSpeed;
        for(int i = 0; i < this.lightnings.Length; i++) {
            this.lightnings[i].SetActive(false);
        }
        for(int i = 0; i < this.springMats.Length ; i++) {
            this.springMats[i].SetColor("_EmissiveColor", this.baseIntensity / 10);
        }
        this.isActive = false;
    }

    // Cooldown management
	public IEnumerator Cooldown() {		
        Desactivate();

		yield return new WaitForSeconds(this.cooldown);

		Activate();
	}
}
