using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAnimation : MonoBehaviour
{
    Material objectMaterial;
    float fillrate = -0.01f;
    bool add = true;
    public float speed = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float delta = 0.02f;
        delta *= Time.deltaTime * speed;

        if(add){
            fillrate += delta;
        }else{
            fillrate -= delta;
        }

        if(fillrate >= 0.01f){
            add = false;
        }else if(fillrate <= -0.01f){
            add = true;
        }

        objectMaterial = GetComponent<Renderer>().material;
        objectMaterial.SetFloat("_FillRate",fillrate);
    }
}
