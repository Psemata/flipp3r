using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

using UnityEngine.Audio;

public class Bumper : MonoBehaviour {

    // Animations
    private bool inAnimation;
    public float animationTime = 0.6f; // Same as the ball

    // Tilt parameters
    private Quaternion originalOrientation;
    public float orientationFactor = 2.5f; // The smaller the bigger the orientation
    
    // Shake parameters
    [SerializeField]
    private float shakeAmount = 1f;
    private Vector3 originalPos;

    // Gameplay
    public bool isWeakness = false;
    public int baseScoreValue = 100;
    public int score = 100;

    // Light
    public Light mylight;
    public Light lightChange;
    private HDAdditionalLightData lightData;
    private HDAdditionalLightData lightDataFlash;
    public bool isLighting = false;

    // VFX
    public ParticleSystem collision;
    
    public Material neon;
    private Material baseMat;

    public GameObject target;

    void Start() {
        // Animation state
        this.inAnimation = false;
        // Start orientaton
        this.originalOrientation = this.transform.rotation;
        // If the bumper is considered a weakness
        lightData = lightChange.GetComponent<HDAdditionalLightData>();
        lightDataFlash = mylight.GetComponent<HDAdditionalLightData>();
        baseMat = gameObject.GetComponent<MeshRenderer>().material;
    }

    // Set this bumper as a weakness
    public void SetWeakness(bool weakness) {
        this.isWeakness = weakness;
        if(weakness == true){
            GameObject target_object = Instantiate(target, transform.position, Quaternion.identity);
            lightData.intensity = 25000;
            gameObject.GetComponent<MeshRenderer>().material = neon;
        }else{
            lightData.intensity = 0;
            gameObject.GetComponent<MeshRenderer>().material = baseMat;
        }
    }

    void OnTriggerEnter(Collider collider) {
        // When a ball collided with the bumper
        if(!inAnimation && collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ball"))) {
            if(collider.GetComponent<Ball>().inFlames) {
                this.score = this.baseScoreValue * 2;

                // Audio
                AudioManager.Instance.Play("bumper-hit-fire");
            } else if(collider.GetComponent<Ball>().inElectricity) {
                this.score = this.baseScoreValue * 4;

                // Audio
                AudioManager.Instance.Play("bumper-hit-electro");
            } else {
                this.score = baseScoreValue;
                // Audio
                AudioManager.Instance.Play("bumper-hit");
            }

            // Collision VFX 
            this.collision.Play();

            // Vector calculated for the tilt animation
            Vector3 upwardPoint = this.transform.position;
            upwardPoint.y += orientationFactor;            
            Vector3 shiftedOrientationVector = upwardPoint - collider.ClosestPointOnBounds(this.transform.position);

            // Tilt animation
            StartCoroutine(Tilt(shiftedOrientationVector, collider.gameObject.tag));
            // Shake animation
            StartCoroutine(Shake());
            // Light activation
            StartCoroutine(Flash());
        }
    }

    // Animations
    void Update() {
        if (this.inAnimation) {
            ShakeAnimation();
        }
    }

    // Shaking animation of the bumper
    void ShakeAnimation() {
        Vector3 newPos = this.originalPos + Random.insideUnitSphere * (Time.deltaTime * this.shakeAmount);
        newPos.y = transform.position.y;
        newPos.z = transform.position.z;

        transform.position = newPos;
    }

    // Coroutine used to set the tilt animation
    IEnumerator Tilt(Vector3 inversedCollisionNormal, string ballTag) {
        this.transform.rotation = Quaternion.LookRotation(inversedCollisionNormal);
        this.inAnimation = true;
        
        yield return new WaitForSeconds(this.animationTime);

        this.transform.rotation = this.originalOrientation;
        this.inAnimation = false;
        if(isWeakness && GameManager.Instance.State == GameState.Game){
            GameManager.Instance.DisableAllWeakness();
            GameManager.Instance.AttackBoss();
        }
        // Score
        if(GameManager.Instance.State == GameState.Game){
            GameManager.Instance.AddScore(ballTag, score);
        }
    }

    // Coroutine used to set the shake animation
    IEnumerator Shake() {
        this.originalPos = transform.position;
        this.inAnimation = true;
        
        yield return new WaitForSeconds(this.animationTime);

        this.inAnimation = false;
        transform.position = this.originalPos;
    }

    // Coroutine used to light activation animation
    IEnumerator Flash(){
        if(this.lightDataFlash != null && !isLighting) {
            isLighting = true;
            while(this.lightDataFlash.intensity <= 10000){
                this.lightDataFlash.intensity += 400;
                yield return null;
            }

            while(this.lightDataFlash.intensity > 0){
                this.lightDataFlash.intensity -= 400;
                yield return null;
            }

            isLighting = false;
        }
    }
}
