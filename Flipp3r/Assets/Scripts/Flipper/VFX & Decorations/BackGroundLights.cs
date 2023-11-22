using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundLights : MonoBehaviour
{
    // All gameobject to change light in order
    public GameObject[] redLightOrder;
    private GameObject[][] redLightOrderArray;
    private int redIndex = 0;
    public GameObject[] greenLightOrder;
    private int greenIndex = 0;
    public GameObject[] greenArrowLightOrder;
    private int greenArrowIndex = 0;
    public GameObject[] yellowLightOrder;
    private int previousYellowIndex1;
    private int previousYellowIndex2;
    // Materials of the different zones
    public Material redMat;
    public Material greenMat;
    public Material yellowMat;
    public Material baseMat;

    private bool redFill = true;
    // Start is called before the first frame update
    void Start()
    {
        //Change the first two index for each color
        redLightOrderArray = new GameObject[15][];
        CreateRedLightOrderArray();

        ChangeMat(redLightOrderArray[redIndex], redMat);
        redIndex++;
        InvokeRepeating("ChangeRedMat", 0f, .05f);

        greenLightOrder[greenIndex].GetComponent<MeshRenderer>().material = greenMat;
        greenLightOrder[greenIndex+1].GetComponent<MeshRenderer>().material = greenMat;
        greenLightOrder[(greenLightOrder.Length/2)].GetComponent<MeshRenderer>().material = greenMat;
        greenLightOrder[(greenLightOrder.Length/2)+1].GetComponent<MeshRenderer>().material = greenMat;
        greenIndex++;
        InvokeRepeating("ChangeGreenMat", 0f, 0.1f);

        greenArrowLightOrder[0].GetComponent<MeshRenderer>().material = greenMat;
        greenArrowLightOrder[1].GetComponent<MeshRenderer>().material = greenMat;
        greenArrowLightOrder[2].GetComponent<MeshRenderer>().material = greenMat;
        greenArrowLightOrder[3].GetComponent<MeshRenderer>().material = greenMat;
        greenArrowIndex += 4;
        InvokeRepeating("ChangeGreenArrowMat", 0f, 0.1f);

        previousYellowIndex1 = Random.Range(0,yellowLightOrder.Length-1);
        previousYellowIndex2 = Random.Range(0,yellowLightOrder.Length-1);
        yellowLightOrder[previousYellowIndex1].GetComponent<MeshRenderer>().material = yellowMat;
        yellowLightOrder[previousYellowIndex2].GetComponent<MeshRenderer>().material = yellowMat;
        InvokeRepeating("ChangeYellowMat", 0f, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void CreateRedLightOrderArray(){
        redLightOrderArray[0] = new GameObject[] {redLightOrder[0]};

        redLightOrderArray[1] = new GameObject[] {redLightOrder[1]};

        redLightOrderArray[2] = new GameObject[] {redLightOrder[2],redLightOrder[3],redLightOrder[4]};

        redLightOrderArray[3] = new GameObject[] {redLightOrder[5],redLightOrder[6]};

        redLightOrderArray[4] = new GameObject[] {redLightOrder[7],redLightOrder[8],redLightOrder[9]};
        
        redLightOrderArray[5] = new GameObject[] {redLightOrder[10]};
        
        redLightOrderArray[6] = new GameObject[] {redLightOrder[11]};
        
        redLightOrderArray[7] = new GameObject[] {redLightOrder[12]};

        redLightOrderArray[8] = new GameObject[] {redLightOrder[13]};

        redLightOrderArray[9] = new GameObject[] {redLightOrder[14],redLightOrder[15],redLightOrder[16]};
        
        redLightOrderArray[10] = new GameObject[] {redLightOrder[17]};

        redLightOrderArray[11] = new GameObject[] {redLightOrder[18]};
        
        redLightOrderArray[12] = new GameObject[] {redLightOrder[19],redLightOrder[20]};
        
        redLightOrderArray[13] = new GameObject[] {redLightOrder[21],redLightOrder[22]};
        
        redLightOrderArray[14] = new GameObject[] {redLightOrder[23]};
    }
    // Function to change red materials in specific order
    void ChangeRedMat(){
        if(redFill){
            ChangeMat(redLightOrderArray[redIndex], redMat);
        }else{
            ChangeMat(redLightOrderArray[redIndex], baseMat);
        }
        if(redIndex + 1 < redLightOrderArray.Length){
            redIndex++;
        }else{
            redIndex = 0;
            redFill = !redFill;
        }
    }

    void ChangeMat(GameObject[] ob, Material a){
        foreach(GameObject o in ob){
            o.GetComponent<MeshRenderer>().material = a;
        }
    }
    // Function to change green materials in specific order
    void ChangeGreenMat(){
        if(greenIndex > 0){
            greenLightOrder[greenIndex-1].GetComponent<MeshRenderer>().material = baseMat;
        }else{
            greenLightOrder[greenLightOrder.Length-1].GetComponent<MeshRenderer>().material = baseMat;
        }
        greenLightOrder[((greenLightOrder.Length/2)+greenIndex-1)%greenLightOrder.Length].GetComponent<MeshRenderer>().material = baseMat;

        if(greenIndex < greenLightOrder.Length-1){
            greenLightOrder[greenIndex+1].GetComponent<MeshRenderer>().material = greenMat;
        }else{
            greenLightOrder[0].GetComponent<MeshRenderer>().material = greenMat;
        }
        greenLightOrder[((greenLightOrder.Length/2)+greenIndex+1)%greenLightOrder.Length].GetComponent<MeshRenderer>().material = greenMat;

        if(greenIndex + 1 < greenLightOrder.Length){
            greenIndex++;
        }else{
            greenIndex = 0;
        }
    }
    // Function to change green materials in specific order
    void ChangeGreenArrowMat(){
        if(greenArrowIndex >= 4){
            greenArrowLightOrder[greenArrowIndex-3].GetComponent<MeshRenderer>().material = baseMat;
            greenArrowLightOrder[greenArrowIndex-4].GetComponent<MeshRenderer>().material = baseMat;
        }

        if(greenArrowIndex+1 <= greenArrowLightOrder.Length-1){
            greenArrowLightOrder[greenArrowIndex].GetComponent<MeshRenderer>().material = greenMat;
            greenArrowLightOrder[greenArrowIndex+1].GetComponent<MeshRenderer>().material = greenMat;
        }

        greenArrowIndex += 2;
        if(greenArrowIndex > 12){
            greenArrowIndex = 0;
        }
    }
    // Function to change yellow materials in specific order
    void ChangeYellowMat(){
        yellowLightOrder[previousYellowIndex1].GetComponent<MeshRenderer>().material = baseMat;
        yellowLightOrder[previousYellowIndex2].GetComponent<MeshRenderer>().material = baseMat;
        previousYellowIndex1 = Random.Range(0,yellowLightOrder.Length-1);
        previousYellowIndex2 = Random.Range(0,yellowLightOrder.Length-1);
        yellowLightOrder[previousYellowIndex1].GetComponent<MeshRenderer>().material = yellowMat;
        yellowLightOrder[previousYellowIndex2].GetComponent<MeshRenderer>().material = yellowMat;
    }
}
