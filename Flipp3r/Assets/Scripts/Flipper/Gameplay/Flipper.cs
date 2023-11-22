using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Flipper : MonoBehaviour
{
    // Position of the flipper
    // At rest
    public float restPosition = 0f;
    // When actionned
    public float pressedPosition = 45f;
    // The power of the flip
    public float hitStrength = 100000f;
    // Damper of the spring
    public float flipperDamper = 150f;

    // Hinge of the flipper
    private HingeJoint hinge;

    // Inputs names for the flipper
    public string inputName;
    public string inputName2;

    // Movement and states of the flipper
    public bool isReleased = false;
    public bool isMovingUp = false;
    public bool wasMovingUp = false;
    public bool isCushion = false;
    public bool isUp = false;

    // Orientation
    public GameObject pointA;
    public GameObject pointB;
    public Vector3 orientation;

    // VFX
    public ParticleSystem collision;
    private bool vfxPlayed;

    // Sound
    private bool soundPlayed = false;

    // Lights
    public Light mylight;
    private HDAdditionalLightData lightData;
    public bool isLighting = false;

    void Awake() {
        this.hinge = GetComponent<HingeJoint>();        
    }

    void Start() {
        this.hinge.useSpring = true;
        this.vfxPlayed = false;
        if(this.mylight != null) {
            lightData = mylight.GetComponent<HDAdditionalLightData>();
        }
    }

    // Make the flipper move
    void Update() {
        JointSpring spring = new JointSpring();

        spring.spring = hitStrength;
        spring.damper = flipperDamper;

        // If the button is pressed, the flipper is actionned
        if(this.inputName2 != "") {
            if (Input.GetAxis(this.inputName) == 1 && Input.GetAxis(this.inputName2) == 1) {
                spring.targetPosition = pressedPosition;
                this.isMovingUp = true;

                // Audio
                if(!soundPlayed) {
                    AudioManager.Instance.Play("flipper-up");
                    this.soundPlayed = true;
                }
            } else {
                if(this.isMovingUp && !this.isUp) {
                    this.wasMovingUp = true;                    
                }

                spring.targetPosition = restPosition;
                this.isReleased = true;
                this.isMovingUp = false;
                
                if(this.soundPlayed && this.isReleased) {
                    AudioManager.Instance.Play("flipper-down");
                }

                this.soundPlayed = false;
            }
        } else {
            if (Input.GetAxis(this.inputName) == 1) {
                spring.targetPosition = pressedPosition;
                this.isMovingUp = true;

                // Audio
                if(!soundPlayed) {
                    AudioManager.Instance.Play("flipper-up");
                    this.soundPlayed = true;
                }
            } else {
                if(this.isMovingUp && !this.isUp) {
                    this.wasMovingUp = true;                    
                }
        
                spring.targetPosition = restPosition;
                this.isReleased = true;
                this.isMovingUp = false;
                
                // Audio
                if(this.soundPlayed && this.isReleased) {
                    AudioManager.Instance.Play("flipper-down");
                }

                this.soundPlayed = false;
            }
        }

        // Change the orientation vector of the flipper
        this.orientation = this.pointB.transform.position - this.pointA.transform.position;

        // If the flipper is at max angle
        if (Mathf.Round(this.hinge.angle) == this.pressedPosition) {
            this.isUp = true;
        } else {
            this.isUp = false;
            this.isCushion = false;
        }

        // If the flipper is at min angle
        if(Mathf.Round(this.hinge.angle) == this.restPosition) {
            this.isReleased = false;
            this.wasMovingUp = false;
            this.vfxPlayed = false;
        }

        hinge.spring = spring;
        hinge.useLimits = true;
    }

    void OnTriggerEnter(Collider collider) {
        if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ball"))){ // If the flipper collides with a ball
            if(this.isUp) {
                this.isCushion = true;

                // Audio
                AudioManager.Instance.Play("flipper-cushion");
            }
            
            StartCoroutine(Flash());
        }
    }

    // Get the vector formed by the ball and the hinge joint
    public Vector3 HingeJointToBallVector(Vector3 ballPosition) {
        Vector3 v = (this.transform.InverseTransformPoint(ballPosition) - this.hinge.anchor).normalized;
        v.y = 0;
        return this.transform.TransformDirection(v);
    }

    // Get the magnitude of the vector formed by the ball and the hinge joint
    public float DistanceToHingeJoint(Vector3 ballPosition) {
        float flipperLength = this.orientation.magnitude;
        float dTHJ = Mathf.Abs((this.transform.InverseTransformPoint(ballPosition) - this.hinge.anchor).magnitude);
        return dTHJ / flipperLength;
    }

    // VFX
    public void PlayCollisionVFX() {
        if(!vfxPlayed) {
            this.collision.Play();
            this.vfxPlayed = true;
        }
    }

    // Coroutine used to light activation animation
    IEnumerator Flash(){
        if(this.lightData != null && !isLighting) {
            isLighting = true;
            while(this.lightData.intensity <= 10000){
                this.lightData.intensity += 1000;
                yield return null;
            }

            while(this.lightData.intensity > 0){
                this.lightData.intensity -= 1000;
                yield return null;
            }

            isLighting = false;
        }
    }
}
