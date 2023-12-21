using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compositor : MonoBehaviour
{
    public ARCamera arCam;

    private Camera eyeCamera;
    private Material blendMat;
    public Camera.StereoscopicEye renderEye;

    void Start()
    {
        blendMat = new Material(Shader.Find("ARSimulator/LightBlend"));
        eyeCamera = GetComponent<Camera>();
    }

    void Update()
    {
        
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        
        blendMat.SetMatrix("_InvViewMatrix", eyeCamera.GetStereoViewMatrix(renderEye).inverse);
        blendMat.SetMatrix("_InvProjMatrix", eyeCamera.GetStereoProjectionMatrix(renderEye).inverse);

        blendMat.SetMatrix("_ARViewMatrix", eyeCamera.GetStereoViewMatrix(renderEye));

        Matrix4x4 arProjMatrix = renderEye == Camera.StereoscopicEye.Left ? DisplayConfigurationManager.activeConfiguration.leftProjectionMatrix : DisplayConfigurationManager.activeConfiguration.rightProjectionMatrix;
        blendMat.SetMatrix("_ARProjMatrix", arProjMatrix);

        blendMat.SetTexture("_ARTexture", arCam.rt);

        Vector4 p = (eyeCamera.GetStereoProjectionMatrix(renderEye) * new Vector4(0, 0, -1.3f, 1.0f));
        float z = p.z / p.w;
        blendMat.SetFloat("_nonLinearZ", z);

        Graphics.Blit(source, destination, blendMat);
    }
}
