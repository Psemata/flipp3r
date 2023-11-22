using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calibration : MonoBehaviour
{
    public static Calibration Instance;
    // All values of the projection matrix
    public float m00;
    public float m01;
    public float m02;
    public float m03;
    public float m10;
    public float m11;
    public float m12;
    public float m13;
    public float m20;
    public float m21;
    public float m22;
    public float m23;
    public float m30;
    public float m31;
    public float m32;
    public float m33;
    public Camera cam;
    Matrix4x4 originalProjection;
    private bool changed = false;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        originalProjection = cam.projectionMatrix;
        m00 = cam.projectionMatrix.m00;
        m01 = cam.projectionMatrix.m01;
        m02 = cam.projectionMatrix.m02;
        m03 = cam.projectionMatrix.m03;
        m10 = cam.projectionMatrix.m10;
        m11 = cam.projectionMatrix.m11;
        m12 = cam.projectionMatrix.m12;
        m13 = cam.projectionMatrix.m13;
        m20 = cam.projectionMatrix.m20;
        m21 = cam.projectionMatrix.m21;
        m22 = cam.projectionMatrix.m22;
        m23 = cam.projectionMatrix.m23;
        m30 = cam.projectionMatrix.m30;
        m31 = cam.projectionMatrix.m31;
        m32 = cam.projectionMatrix.m32;
        m33 = cam.projectionMatrix.m33;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Shortcuts to either reset the projection matrix or change the values of specific parts of the matrix (left, right, up or down offset)
        if(Input.GetKey("left ctrl")) {
            if(Input.GetKeyDown("r"))
            {
                cam.ResetProjectionMatrix();
                m00 = cam.projectionMatrix.m00;
                m01 = cam.projectionMatrix.m01;
                m02 = cam.projectionMatrix.m02;
                m03 = cam.projectionMatrix.m03;
                m10 = cam.projectionMatrix.m10;
                m11 = cam.projectionMatrix.m11;
                m12 = cam.projectionMatrix.m12;
                m13 = cam.projectionMatrix.m13;
                m20 = cam.projectionMatrix.m20;
                m21 = cam.projectionMatrix.m21;
                m22 = cam.projectionMatrix.m22;
                m23 = cam.projectionMatrix.m23;
                m30 = cam.projectionMatrix.m30;
                m31 = cam.projectionMatrix.m31;
                m32 = cam.projectionMatrix.m32;
                m33 = cam.projectionMatrix.m33;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (!changed)
                {
                    m01 -= 0.0001f;
                }
                else
                {
                    m03 -= 0.001f;
                }
            }else if(Input.GetKeyDown(KeyCode.RightArrow)) {
                if (!changed)
                {
                    m01 += 0.0001f;
                }
                else
                {
                    m03 += 0.001f;
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (!changed)
                {
                    m31 += 0.0001f;
                }
                else
                {
                    m13 += 0.001f;
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (!changed)
                {
                    m31 -= 0.0001f;
                }
                else
                {
                    m13 -= 0.001f;
                }
            }
            else if (Input.GetKeyDown("e"))
            {
                changed = !changed;
            }
        }
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = m00;
        m[0, 1] = m01;
        m[0, 2] = m02;
        m[0, 3] = m03;
        m[1, 0] = m10;
        m[1, 1] = m11;
        m[1, 2] = m12;
        m[1, 3] = m13;
        m[2, 0] = m20;
        m[2, 1] = m21;
        m[2, 2] = m22;
        m[2, 3] = m23;
        m[3, 0] = m30;
        m[3, 1] = m31;
        m[3, 2] = m32;
        m[3, 3] = m33;
        cam.projectionMatrix = m;
    }
}
