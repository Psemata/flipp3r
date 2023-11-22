using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
using UnityEngine.Rendering.HighDefinition;

public class BallBoss : MonoBehaviour
{
    // Radius of the ball
    [SerializeField]
    private float radius = 2f;
    private float threshold = 0f;

    // The ball's rigidbody
    private Rigidbody rgb;
    private Material mat;

    public float duration = 15f;


    // Speed and movement of the ball
    [SerializeField]
    private float speedFactor = 5f;
    public Vector3 speed;

    // Collisions and rebounds
    public LayerMask layerMask;
    // Collision map
    // The bool value is for a lock (when the ball has detected a corner, then the value must remain the same till the collision is done)
    private Dictionary<string, (Vector3, bool)> collisionsNormals = new Dictionary<string, (Vector3, bool)>();
    [SerializeField]
    public Transform borders;

    // Visual Effect    
    // Particle System
    public ParticleSystem sparkles;
    public ParticleSystem explosion;

    // Light
    public Light myLight;
    private HDAdditionalLightData lightData;
    private bool lightStop = false;

    void Awake() {
        // Get the rigidbody of the ball
        this.rgb = GetComponent<Rigidbody>();

        // Get the mat / shader
        this.mat = gameObject.GetComponent<Renderer>().material;

        // The threshold used to detect corner
        this.threshold = Mathf.Sqrt(this.radius * this.radius * 2);

        // Light
        this.lightData = this.myLight.GetComponent<HDAdditionalLightData>();
    }

    public void StartBossSequence() {
        StartCoroutine(BossBall());
    }

    void FixedUpdate() {
        this.rgb.MovePosition(transform.position + this.speed * Time.fixedDeltaTime);
        GetCollisionNormal();
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

        float randX = this.speed.x >= 0 ? Random.Range(0f, 2f) : Random.Range(-2f, 0f);
        float randZ = this.speed.z >= 0 ? Random.Range(0f, 2f) : Random.Range(-2f, 0f);

        // Vector used to keep the ball out of a cycle
        Vector3 deltaVector = new Vector3(randX, 0f, randZ);
        return ((newDirection * speedMagnitude) + deltaVector) * speedFactor;
    }

    // Collision management
    void OnTriggerEnter(Collider collider) {        
        if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("Elements"))) { // Other elements
            // Get the collider's tag
            string colliderTag = collider.gameObject.tag;

            if(collider.gameObject.transform.parent == this.borders.transform) {
                this.speed = CollisionVector(this.collisionsNormals[collider.name].Item1, 70);

                // Reset
                ResetLock();

                // Sparkles animation
                StartCoroutine(Sparkles(collider.name));
            }
        } else if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ball"))) { // Ball
            GetCollisionBall(collider.gameObject.GetComponent<Ball>());

            // Audio
            AudioManager.Instance.Play("collision-base");
        }
    }

    // Coroutine
    IEnumerator BossBall() {
        // Initial speed
        this.speed = new Vector3(0f, 0f, -40f) * speedFactor;
        // Audio
        AudioManager.Instance.Play("boss-ball");

        StartCoroutine(Flash());
        StartCoroutine(Death());

        yield return new WaitForSeconds(5f);

        StartCoroutine(Explosions());        
    }

    IEnumerator Death() {
        yield return new WaitForSeconds(duration);

        // Audio
        AudioManager.Instance.StopPlaying("boss-ball");

        Destroy(gameObject);
    }

    // VFX
    // Sparkles particles system
    IEnumerator Sparkles(string name) {
        this.sparkles.transform.rotation = Quaternion.LookRotation(this.collisionsNormals[name].Item1, Vector3.up);

        this.sparkles.Play();

        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator Explosions() {
        yield return new WaitForSeconds(1f);

        AudioManager.Instance.StopPlaying("boss-ball");

        for(int i = 0; i < 50; i++) {
            if(i == 20)
            {
                StartCoroutine(GameManager.Instance.FadeWhiteCoroutine());
            }

            StartCoroutine(Explosion());

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator Explosion() {
        float x = Random.Range(-40f, 40f);
        float y = 1.5f;
        float z = Random.Range(-106.8f, 106.8f);
        
        Vector3 newExplosionPos = new Vector3(x, y, z);

        ParticleSystem explosionClone = Instantiate(this.explosion, newExplosionPos, Quaternion.identity);

        explosionClone.Play();
        AudioManager.Instance.Play("explosion");

        yield return null;
    }

    // Lights
    // Coroutine used to light activation animation
    IEnumerator Flash(){
        while(true) {
            if(this.lightStop) {
                break;
            }

            this.lightData.intensity = 0f;

            yield return new WaitForSeconds(0.05f);

            this.lightData.intensity = 500000f;

            yield return new WaitForSeconds(0.05f);
        }
    }
}
