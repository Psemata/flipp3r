using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Concurrent;

public class ElectricArc : MonoBehaviour {

    // The gameobject for the electric arc
    public GameObject electricArc;

    // All the arcs between this ball and the others
    private ConcurrentDictionary<GameObject, GameObject> electricArcs = new ConcurrentDictionary<GameObject, GameObject>();

    private bool soundPlayed = false;
    
    void Update() {
        if(!this.electricArcs.IsEmpty) {
            foreach(KeyValuePair<GameObject, GameObject> electricArc in electricArcs) {                
                if(electricArc.Key != null && electricArc.Value != null) {
                    // This ball
                    electricArc.Value.transform.Find("Pos1").transform.position = this.transform.position;
                    // The other ball
                    electricArc.Value.transform.Find("Pos4").transform.position = electricArc.Key.transform.position;

                    // The other balls, the inbetween
                    Vector3 betweenBalls = (electricArc.Key.transform.position - this.transform.position).normalized;
                    electricArc.Value.transform.Find("Pos2").transform.position = this.transform.TransformPoint(betweenBalls * 1);
                    electricArc.Value.transform.Find("Pos3").transform.position = this.transform.TransformPoint(betweenBalls * 2);
                }
            }
        }        
    }

    void OnTriggerEnter(Collider collider) {
        if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ball"))) {
            if(collider.gameObject.transform == this.transform.parent) { // Protection
                return;
            }

            if(collider.gameObject != null) {
                electricArcs[collider.gameObject] = Instantiate(this.electricArc, this.transform.position, Quaternion.identity);
                electricArcs[collider.gameObject].transform.parent = this.transform;
                electricArcs[collider.gameObject].transform.Find("ElectricArcVFXGraph").gameObject.SetActive(true);

                if(!this.soundPlayed) {
                    // Audio
                    AudioManager.Instance.Play("electro-inter-bille");
                    collider.gameObject.GetComponent<Ball>().electricContactSoundPlayed = true;
                    this.soundPlayed = true;
                }
            }
            
        }    
    }

    void OnTriggerExit(Collider collider) {
        if(collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ball"))) {
            if(collider.gameObject.transform == this.transform.parent) { // Protection
                return;
            }
            
            DestroyElectricArc(collider.gameObject);
        }    
    }

    // Deactivate and destroy an electric arc
    void DestroyElectricArc(GameObject ball) {
        if(ball != null && electricArcs.ContainsKey(ball)) {
            GameObject obj = electricArc;
            electricArcs.Remove(ball, out obj);

            // Audio
            AudioManager.Instance.StopPlaying("electro-inter-bille");
            this.soundPlayed = false;

            Destroy(obj);
        }
    }

    // Kill all the electric arcs
    public void ElectricArcsDeath() {
        foreach(KeyValuePair<GameObject, GameObject> electricArc in electricArcs) {
            if(electricArc.Key.transform.Find("ElectricArcs").GetComponent<ElectricArc>().electricArcs.ContainsKey(this.transform.parent.gameObject)) {
                electricArc.Key.transform.Find("ElectricArcs").GetComponent<ElectricArc>().DestroyElectricArc(this.transform.parent.gameObject);
            }
        }
    }
}
