using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piston : MonoBehaviour {

    // Rigidbody
    private Rigidbody rgb;
    // Direction the ball is sent
    public Vector3 direction;
    public float power;

    // If the piston is activated
    public bool activated;
    // The name of the input used to activate the piston
    public string inputName;
    
    void Awake() {
        this.rgb = GetComponent<Rigidbody>();
        this.activated = false;
        if(this.direction == Vector3.zero) {
            this.direction = this.transform.right;
        }        
        this.direction *= power;
    }

    void Update() {
        if(Input.GetAxis(this.inputName) == 1) { // If the input is pressed
            if(!this.activated) {
                this.rgb.AddForce(-direction, ForceMode.Impulse);
                this.activated = true;
            }
        } else if(this.rgb.velocity.magnitude == 0) { // If the input is released
            this.activated = false;
        }
    }
}
