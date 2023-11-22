using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Slingshot : MonoBehaviour
{
    public Light mylight;
    private HDAdditionalLightData lightData;
    public bool isLighting = false;

    [SerializeField]
    private ParticleSystem collision;

    // Start is called before the first frame update
    void Start()
    {
        lightData = mylight.GetComponent<HDAdditionalLightData>();
    }

    void OnTriggerEnter(Collider collider) {
        if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ball"))){
            this.collision.transform.position = collider.ClosestPointOnBounds(this.transform.position);
            Vector3 direction = collider.transform.position - this.collision.transform.position;
            this.collision.transform.LookAt(direction);
            this.collision.Play();

            // Audio
            AudioManager.Instance.Play("slingshot-hit");

            StartCoroutine(Flash());
        }
    }

    IEnumerator Flash(){
        if(this.lightData != null && !isLighting) {
            isLighting = true;
            while(this.lightData.intensity <= 15000){
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