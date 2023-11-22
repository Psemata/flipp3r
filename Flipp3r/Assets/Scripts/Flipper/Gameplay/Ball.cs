using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public enum SpeedFactors
{
    Border = 60,
    Bumper = 60,
    Flipper = 60,
    Slingshot = 60,
    Piston = 60,
    PistonBorder = 60,
    SlideGate = 60 
}

public class Ball : MonoBehaviour
{
    // Radius of the ball
    [SerializeField]
    private float radius = 1.25f;
    private float threshold = 0f;

    // The ball's rigidbody
    private Rigidbody rgb;
    private Material mat;

    // Spawn and death
    private bool isIntegrating = false;
    private bool isDesintegrating = false;
    public float desintegrationSpeed = 0.01f;
    private float desintegrationRate = 1f;

    // GameObject used to create the integration effect
    [SerializeField]
    private float integrationRate = 0.05f;
    public GameObject integrationRing1;
    public GameObject integrationRing2;

    private bool firstTime = false;
    public int originZone;

    // Speed and movement of the ball
    [SerializeField]
    private float speedFactor = 1f;
    public Vector3 speed;

    // "Gravity" values
    private int gravityZone;
    public float gravityPower = 0.1f;

    // Collisions and rebounds
    public LayerMask layerMask;
    // Collision map
    // The bool value is for a lock (when the ball has detected a corner, then the value must remain the same till the collision is done)
    private Dictionary<string, (Vector3, bool)> collisionsNormals = new Dictionary<string, (Vector3, bool)>();

    // Portal management
    public bool isInPortal = false;

    // Animations of the ball
    [SerializeField]
    private float shakeAmount = 1f;
    [SerializeField]
    private float shakeTime = 1.5f;
    private Vector3 originalPos;
    private bool ballAnimation;

    // Trails
    // Flames
    [SerializeField]
    private int maxScoreToUnlock = 1500;
    public int accumulatedScore = 0;
    public ParticleSystem flameTrail;
    public bool inFlames = false;

    // Electricity
    public GameObject electricityTrail;
    public VisualEffect electricityTrail_VFX;
    static readonly ExposedProperty ballDirection = "BallDirection";
    public VisualEffect electricitySource_VFX;
    public bool inElectricity = false;

    // Flippers management
    private Flipper flipper;
    public Transform oldParent;
    private bool collidingFlipper;

    [SerializeField]
    private float glidingSpeedFactor = 1f;

    private bool volleyCatched;
    [SerializeField]
    private float caughtFactor = 2f;
    [SerializeField]
    private float aimedFactor = 1.5f;

    // Slingshot management
    private bool collidingSlingshot;

    // Piston management
    private Piston piston;

    // Slide management
    public bool inSlide;

    // Visual Effect    
    // Particle System
    public ParticleSystem sparkles;

    // VFX with gameObject
    private bool magnetismAnimation = false;
    public GameObject magnetismEffect;
    [SerializeField]
    private float magnetismSpeed = 0.05f;

    // Spline electricity
    public VisualEffect electricity;

    // Score text
    public GameObject floatingPoints;

    // SFX management
    private bool soundPlayed = false;
    public bool flameTrailSoundPlayed = false;
    public bool electricTrailSoundPlayed = false;
    public bool electricContactSoundPlayed = false;

    void Awake() {
        // Get the rigidbody of the ball
        this.rgb = GetComponent<Rigidbody>();

        // Get the mat / shader
        this.mat = gameObject.GetComponent<Renderer>().material;

        // The spawn value
        this.isIntegrating = false;
        this.isDesintegrating = false;
        this.desintegrationRate = 1f;

        // Set the parent of the ball to its zone
        this.oldParent = null;

        // Initial speed
        this.speed = Vector3.zero;

        // The threshold used to detect corner
        this.threshold = Mathf.Sqrt(this.radius * this.radius * 2);

        // The flipper management values
        this.flipper = null;
        this.piston = null;
        this.collidingFlipper = false;
        this.volleyCatched = false;

        // The slingshot management value
        this.collidingSlingshot = false;

        // The slide management value
        this.inSlide = false;
    }

    void FixedUpdate() {
        // If the ball isn't in an animation
        if (!this.ballAnimation) {
            // "Gravity" management
            if(!this.collidingFlipper && !this.inSlide && !this.isIntegrating && !this.isInPortal) {
                if(this.gravityZone == 1) {
                    this.speed -= new Vector3(0, 0, this.gravityPower);
                } else if(this.gravityZone == 2) {
                    this.speed -= new Vector3(this.gravityPower, 0, 0);
                } else if(this.gravityZone == 3) {
                    this.speed -= new Vector3(-this.gravityPower, 0, 0);
                }
            }            
            this.rgb.MovePosition(transform.position + this.speed * Time.fixedDeltaTime);
            GetCollisionNormal();
        }
    }

    // Get the new speed of the ball
    float SpeedFactor(string colliderTag) {
        float newSpeed = 0f;
        switch (colliderTag) {
            case "Border":
                newSpeed = ((float)SpeedFactors.Border);
                break;
            case "Bumper":
                newSpeed = ((float)SpeedFactors.Bumper);
                break;
            case "Flipper":
                newSpeed = ((float)SpeedFactors.Flipper);
                break;
            case "Slingshot":
                newSpeed = ((float)SpeedFactors.Slingshot);
                break;
            case "SlideGate":
                newSpeed = ((float)SpeedFactors.SlideGate);
                break;
            case "Piston" :
                newSpeed = ((float)SpeedFactors.Piston);
                break;
            case "PistonBorder" :
                newSpeed = ((float)SpeedFactors.PistonBorder);
                break;
            default:
                break;
        }
        return newSpeed * this.speedFactor;
    }

    // Normalize the vector and change its y value to 0
    Vector3 NormalizedVector(Vector3 vector, Vector3 speed) {
        Vector3 modified = vector;
        modified = modified.normalized;
        modified.y = 0;
        if (Vector3.Dot(speed, modified) > 0) {
            modified = -modified;
        }
        return modified;
    }

    // Get the normal of the collision of the ball with an element
    void GetCollisionNormal() {
        // Rebound detection
        // Mask to prevent the ball to check the zone layer => 6
        // Sends spheres to get the collision
        if (Physics.SphereCast(this.transform.position, this.radius, this.speed, out RaycastHit hit_first, 5f, layerMask, QueryTriggerInteraction.Collide)) {
            if(!this.collisionsNormals.ContainsKey(hit_first.collider.name) || this.collisionsNormals.ContainsKey(hit_first.collider.name) && !this.collisionsNormals[hit_first.collider.name].Item2) {
                this.collisionsNormals[hit_first.collider.name] = (NormalizedVector(hit_first.normal, this.speed), false);
            }

            hit_first.collider.enabled = false;

            if (Physics.SphereCast(this.transform.position, this.radius, this.speed, out RaycastHit hit_second, 5f, layerMask, QueryTriggerInteraction.Collide)) {
                // If the two collisions point are close enough - pythagore with the radius
                if (Vector3.Distance(hit_first.point, hit_second.point) <= threshold && !this.collisionsNormals[hit_first.collider.name].Item2) {
                    Vector3 result = this.collisionsNormals[hit_first.collider.name].Item1 + NormalizedVector(hit_second.normal, this.speed);

                    this.collisionsNormals[hit_first.collider.name] = (NormalizedVector(result, this.speed), true);
                    this.collisionsNormals[hit_second.collider.name] = (NormalizedVector(result, this.speed), true);
                }
            }

            hit_first.collider.enabled = true;
        }
    }

    // Calculate the collision between the two balls - the speed is conserved
    void GetCollisionBall(Ball ball) {
        Vector3 A = this.transform.position;
        Vector3 B = ball.transform.position;

        Vector3 normal = Vector3.Normalize(A - B);

        Vector3 newSpeed = Vector3.Reflect(this.speed, normal);
        newSpeed.y = 0;

        this.speed = newSpeed;
    }

    // Reset all the lock on the normals
    void ResetLock() {
        List<string> keys = new List<string>(this.collisionsNormals.Keys);
        foreach(string key in keys) {
            this.collisionsNormals[key] = (this.collisionsNormals[key].Item1, false);
        }
    }

    // Get the vector which is the next direction after a collision between the ball and an element
    Vector3 CollisionVector(Vector3 collisionNormal, float speedMagnitude) {
        if (Vector3.Dot(this.speed.normalized, collisionNormal) > 0) {
            this.speed = -this.speed;
        }

        Vector3 newDirection = Vector3.Reflect(this.speed.normalized, collisionNormal);

        float randX = this.speed.x >= 0 ? Random.Range(0f, 1f) : Random.Range(-1f, 0f);
        float randZ = this.speed.z >= 0 ? Random.Range(0f, 1f) : Random.Range(-1f, 0f);

        // Vector used to keep the ball out of a cycle
        Vector3 deltaVector = new Vector3(randX, 0f, randZ);
        return (newDirection * speedMagnitude) + deltaVector;
    }

    // Collision management
    void OnTriggerEnter(Collider collider) {        
        if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("Zones"))) { // Zones
            if (collider.gameObject.name == "Zone1") {
                this.gravityZone = 1;
            } else if (collider.gameObject.name == "Zone2") {
                this.gravityZone = 2;
            } else if (collider.gameObject.name == "Zone3") {
                this.gravityZone = 3;
            }

            if(!firstTime && this.originZone != this.gravityZone) {
                this.firstTime = true;
                StartCoroutine(GameManager.Instance.SpawnBall(this.tag));
            }
        } else if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("Elements"))) { // Other elements
            // Get the collider's tag
            string colliderTag = collider.gameObject.tag;

            if(colliderTag == "Flipper") { // If the ball collides with a flipper
                this.flipper = collider.transform.parent.GetComponent<Flipper>();
                this.collidingFlipper = true;
                
                if(this.flipper.isMovingUp && !this.flipper.isUp) { // If the flipper is up and used as a cushion
                    this.volleyCatched = true;
                    CaughtRebound(collider.name);
                }
            } else if(colliderTag == "Slingshot") { // If the ball collides with a slingshot
                this.collidingSlingshot = true;
                this.speed = CollisionVector(this.collisionsNormals[collider.name].Item1, SpeedFactor(colliderTag));
            } else if(colliderTag == "Bumper") { // If the ball collides with a bumper
                // Instantiate score game object and changing orientation depending on the zone
                GameObject points = Instantiate(floatingPoints, transform.position, Quaternion.identity);
                if(gravityZone == 1){
                    points.transform.localPosition += new Vector3(0, 10f, 10f);
                }else if(gravityZone == 2){
                    points.transform.Rotate(0.0f, 90f, 0.0f, Space.Self);
                    points.transform.localPosition += new Vector3(10f, 10f, 0);
                }else if(gravityZone == 3){
                    points.transform.Rotate(0.0f, -90f, 0.0f, Space.Self);
                    points.transform.localPosition += new Vector3(-10f, 10f, 0);
                }
                points.transform.GetChild(0).GetComponent<TextMeshPro>().text = "100";
                if (!this.ballAnimation) {
                    StartCoroutine(Shake(collider.name));                    
                }
            } else if(colliderTag == "Piston") { // If the ball collides with a piston
                this.piston = collider.transform.parent.GetComponent<Piston>();
                if(this.piston.activated) {
                    this.transform.SetParent(this.piston.transform);
                    this.speed = this.transform.TransformDirection(this.piston.direction);
                    
                    // Audio
                    if(this.inFlames) {
                        AudioManager.Instance.Play("bumper-hit");
                    } else if(this.inElectricity) {
                        AudioManager.Instance.Play("bumper-hit-fire");
                    } else {
                        AudioManager.Instance.Play("bumper-hit-electro");
                    }
                } else {
                    this.speed = CollisionVector(this.collisionsNormals[collider.name].Item1, SpeedFactor(colliderTag));
                }
            } else if(colliderTag == "Slide") { // If the ball enters or exits a slide
                Vector3 position = this.transform.position;
                position.y = 1f;
                this.transform.position = position;
            } else {
                this.speed = CollisionVector(this.collisionsNormals[collider.name].Item1, SpeedFactor(colliderTag));

                // Audio
                AudioManager.Instance.Play("collision-base");
            }

            // Reset
            ResetLock();

            // Sparkles animation
            StartCoroutine(Sparkles(collider.name));
        } else if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ball"))) { // Ball
            GetCollisionBall(collider.gameObject.GetComponent<Ball>());
            
            // Audio
            if(accumulatedScore < 200) {
                AudioManager.Instance.Play("collision-base");
            } else if(accumulatedScore < 500) {
                AudioManager.Instance.Play("collision-high");
            } else if(accumulatedScore < 1000) {
                AudioManager.Instance.Play("collision-super");
            } else if(accumulatedScore < 1500) {
                AudioManager.Instance.Play("collision-ultra");
            }
        } else if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("VFX"))) {
            // Protection used since the electricArc children has a kinematic rigidbody and used for portal
            return;
        }
    }

    void OnTriggerStay(Collider collider) {
        if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("Elements"))) { // Elements
            string colliderTag = collider.gameObject.tag;
            // If the ball stay collided with a flipper
            if (colliderTag == "Flipper") {
                this.flipper = collider.transform.parent.GetComponent<Flipper>();
                this.transform.SetParent(this.flipper.transform);
                Vector3 flipperOrientation = this.flipper.orientation.normalized * glidingSpeedFactor;

                // Check if direction is negative or positive
                if(!this.volleyCatched) {
                    if(collider.transform.root.name == "Zone1") {
                        if (Vector3.Dot(transform.forward, flipperOrientation) > 0) {
                            this.speed = -flipperOrientation;
                        } else {
                            this.speed = flipperOrientation;
                        }
                    } else if(collider.transform.root.name == "Zone2") {
                        if (Vector3.Dot(transform.right, flipperOrientation) > 0) {
                            this.speed = -flipperOrientation;
                        } else {
                            this.speed = flipperOrientation;
                        }
                    } else if(collider.transform.root.name == "Zone3") {
                        if (Vector3.Dot(-transform.right, flipperOrientation) > 0) {
                            this.speed = -flipperOrientation;
                        } else {
                            this.speed = flipperOrientation;
                        }
                    }
                }

                // If the flipper is up, and the ball is touching the flipper and the slingshot, then it stops moving
                if (this.collidingFlipper && this.collidingSlingshot && this.flipper.isUp) {
                    this.speed = Vector3.zero;
                    if(!this.soundPlayed) {
                        // Audio
                        AudioManager.Instance.Play("flipper-to-sling");
                        this.soundPlayed = true;
                    }
                } else {
                    this.soundPlayed = false;
                }

                // If the flipper is going up : 
                if((this.flipper.isUp && !this.flipper.isCushion || this.flipper.isReleased && !this.flipper.isCushion && this.flipper.wasMovingUp) && !this.volleyCatched) {
                    AimedRebound();
                }
            }
        }
    }

    void OnTriggerExit(Collider collider) {
        if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("Elements"))) {
            if (collider.gameObject.tag == "Flipper") { // If the ball exits the flipper
                LeavingFlipper();
            } else if (collider.gameObject.tag == "Slingshot") { // If the ball exits the slingshot
                this.collidingSlingshot = false;
            } else if(collider.gameObject.tag == "Bumper") {
                this.accumulatedScore += 10;
                if(this.accumulatedScore == this.maxScoreToUnlock) {
                    this.inFlames = true;
                    this.flameTrail.Play();
                    this.speedFactor = 1.5f;

                    if(!this.flameTrailSoundPlayed) {
                        // Audio
                        AudioManager.Instance.Play("fire-bille");
                        this.flameTrailSoundPlayed = true;
                    }                    
                }
            } else if(collider.gameObject.tag == "Piston") { // If the ball exits the piston
                this.transform.SetParent(this.oldParent);
                this.piston = null;
            }
        }
    }

    void LeavingFlipper() {
        this.transform.SetParent(this.oldParent);
        this.flipper = null;
        this.collidingFlipper = false;
        this.volleyCatched = false;
        this.soundPlayed = false;
    }

    // Make the ball rebound on the flipper
    // Caught in the movement
    void CaughtRebound(string name) {
        float speedFactor = SpeedFactor("Flipper");
        float distanceToHinge = this.flipper.DistanceToHingeJoint(this.transform.position);

        if(this.flipper.transform.parent.name.Contains("Slingshot")) {
            distanceToHinge *= 100f;
        }

        this.flipper.PlayCollisionVFX(); // Play the VFX

        // Audio
        AudioManager.Instance.Play("flipper-smash");
        
        this.speed = CollisionVector(this.collisionsNormals[name].Item1, speedFactor * distanceToHinge * caughtFactor);
        LeavingFlipper();
    }
    // Aimed
    void AimedRebound() {
        // Vector normal to the flipper
        Vector3 flipperOrientation = this.flipper.orientation.normalized;
        if(this.flipper.name.EndsWith("R")) {
            flipperOrientation = -flipperOrientation;
        }
        Vector3 dCollision = Vector3.Cross(flipperOrientation, Vector3.up).normalized;
        Vector3 hingeToBallVector = this.flipper.HingeJointToBallVector(this.transform.position).normalized;

        float speedFactor = SpeedFactor("Flipper");
        float distanceToHinge = this.flipper.DistanceToHingeJoint(this.transform.position);

        if(this.flipper.transform.parent.name.Contains("Slingshot")) {
            distanceToHinge *= 100f;
        }

        this.flipper.PlayCollisionVFX(); // Play the VFX

        if(!this.soundPlayed) {
            // Audio
            AudioManager.Instance.Play("flipper-launch");
            this.soundPlayed = true;
        }
        
        this.speed = (0.6f * dCollision + 0.3f * hingeToBallVector) * (speedFactor * distanceToHinge * aimedFactor);
        LeavingFlipper();
    }

    // Animations
    void Update() {
        // Shake animation
        if (this.ballAnimation) {
            BallShake();
        }

        // Integration and desintegration
        if(this.isIntegrating && this.desintegrationRate >= 0) {
            this.desintegrationRate -= desintegrationSpeed;
            this.integrationRing1.SetActive(true);
            this.integrationRing2.SetActive(true);
            this.integrationRing1.transform.localScale = new Vector3(this.integrationRing1.transform.localScale.x - this.integrationRate, this.integrationRing1.transform.localScale.y - this.integrationRate, this.integrationRing1.transform.localScale.z - this.integrationRate);
            this.integrationRing2.transform.localScale = new Vector3(this.integrationRing2.transform.localScale.x - this.integrationRate, this.integrationRing2.transform.localScale.y - this.integrationRate, this.integrationRing2.transform.localScale.z - this.integrationRate);
            if(this.desintegrationRate < 0f) {
                this.isIntegrating = false;
                this.integrationRing1.SetActive(false);
                this.integrationRing2.SetActive(false);
            }
        } else if(this.isDesintegrating && this.desintegrationRate <= 1) {
            this.desintegrationRate += desintegrationSpeed;
            if(this.desintegrationRate > 1f) {
                this.isDesintegrating = false;
            }
        }

        // Change value of the desintegration rate
        if(this.isIntegrating || this.isDesintegrating) {
            this.mat.SetFloat("_Disintegration_Rate", desintegrationRate);
        }

        // Magnetism
        if(this.magnetismAnimation) {
            this.magnetismEffect.transform.localScale = new Vector3(this.magnetismEffect.transform.localScale.x - this.magnetismSpeed, this.magnetismEffect.transform.localScale.y - this.magnetismSpeed, this.magnetismEffect.transform.localScale.z - this.magnetismSpeed);
            if(this.magnetismEffect.transform.localScale.x < 0.05f) {
                this.magnetismAnimation = false;
                this.magnetismEffect.SetActive(false);
                this.magnetismEffect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
        }
        
        // Electricity trail
        if(this.inElectricity) {
            Vector3 direction = this.speed.normalized;
            this.electricityTrail_VFX.SetVector3(ballDirection, direction / 2);
            this.speedFactor = 1.3f;
        }
    }

    // Shaking animation of the ball - on the bumpers
    void BallShake() {
        Vector3 newPos = this.originalPos + Random.insideUnitSphere * (Time.deltaTime * this.shakeAmount);
        newPos.y = transform.position.y;
        newPos.z = transform.position.z;

        transform.position = newPos;
    }

    // Coroutine used to set the shake animation
    IEnumerator Shake(string name) {
        this.originalPos = transform.position;
        if (!this.ballAnimation) {
            this.ballAnimation = true;
        }
        
        yield return new WaitForSeconds(this.shakeTime);

        this.speed = CollisionVector(this.collisionsNormals[name].Item1, SpeedFactor("Bumper"));

        this.ballAnimation = false;
        transform.position = this.originalPos;
    }

    // Sparkles particles system
    IEnumerator Sparkles(string name) {
        this.sparkles.transform.rotation = Quaternion.LookRotation(this.collisionsNormals[name].Item1, Vector3.up);

        this.sparkles.Play();

        yield return new WaitForSeconds(0.2f);
    }

    // Electricity Trail
    public void PlayElectricityTrail() {
        this.inElectricity = true;
        this.electricityTrail.SetActive(true);

        if(!this.electricTrailSoundPlayed) {
            // Audio
            AudioManager.Instance.Play("electro-bille");
            this.electricTrailSoundPlayed = true;
        }
        
    }

    // Magnetism
    public void Magnetism() {
        this.magnetismAnimation = true;
        this.magnetismEffect.SetActive(true);
    }

    // Spline electricity
    public void PlayElectricity() {
        this.electricity.enabled = true;
        this.electricity.Play();
    }

    public void StopElectricity() {
        this.electricity.Stop();
        this.electricity.enabled = false;
    }

    // Desintegration and integration
    public void Desintegration() {
        this.isDesintegrating = true;
    }

    public void Integration() {
        this.isIntegrating = true;
        // Audio
        AudioManager.Instance.Play("spawn");
    }
}
