using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour {
    // The duration the shell is lit
    public float duration = 5f;
    
    // The material's new color
    private Color originalColor;
    [SerializeField]
    private Color newColor;
    
    // The Shell's material
    private Material mat;

    void Awake() {
        // Copying the shell material
        this.mat = gameObject.GetComponent<Renderer>().material;

        // Getting its color
        this.originalColor = this.mat.GetColor("_EmissiveColor");
    }

    void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ball"))) {
           // Change the color
           StartCoroutine(EmitColor());
        }
    }

    IEnumerator EmitColor() {
        // Change the material's color
        this.mat.SetColor("_EmissiveColor", newColor * 10);

        // Audio
        AudioManager.Instance.ShellSound();

        yield return new WaitForSeconds(duration);

        // Change the material's color to its original
        this.mat.SetColor("_EmissiveColor", originalColor);
    }
}
