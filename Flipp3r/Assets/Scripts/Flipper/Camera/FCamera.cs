using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FCamera : MonoBehaviour {

    // Activate the wanted screens
    void Start() {
        Display.displays[0].Activate(3840, 2160, 60);
        if(Display.displays.Length > 1 && !Display.displays[1].active) {
            Display.displays[1].Activate(1920, 1200, 60);
        }
    }
}