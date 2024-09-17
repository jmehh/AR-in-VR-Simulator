using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Compositor : MonoBehaviour
{
    public ARCamera arCam;

    private Camera eyeCamera;
    private Material blendMat;
    public Camera.StereoscopicEye renderEye;

    public RenderTexture rtfinal;

    void Start()
    {
        blendMat = new Material(Shader.Find("ARSimulator/LightBlend"));
        eyeCamera = GetComponent<Camera>();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        if (rtfinal == null)
        {
            rtfinal = new RenderTexture(source.width, source.height,0);
        }


        Matrix4x4 eyeProj = eyeCamera.GetStereoProjectionMatrix(renderEye);

        // if a VR SDK is not setting the projection matrix, use standard unity one
        if (eyeProj ==  Matrix4x4.identity)
        {
            eyeProj = eyeCamera.projectionMatrix;
        }

        Matrix4x4 eyeView = eyeCamera.GetStereoViewMatrix(renderEye);

        blendMat.SetMatrix("_InvViewMatrix", eyeView.inverse);
        blendMat.SetMatrix("_InvProjMatrix", eyeProj.inverse);

        blendMat.SetMatrix("_ARViewMatrix", eyeView);

        Matrix4x4 arProjMatrix = renderEye == Camera.StereoscopicEye.Left ? DisplayConfigurationManager.activeConfiguration.leftProjectionMatrix : DisplayConfigurationManager.activeConfiguration.rightProjectionMatrix;
        blendMat.SetMatrix("_ARProjMatrix", arProjMatrix);

        blendMat.SetTexture("_ARTexture", arCam.rt);



        Vector4 p = (eyeProj * new Vector4(0, 0, -1.3f, 1.0f));
        float z = p.z / p.w;
        blendMat.SetFloat("_nonLinearZ", z);

        blendMat.SetFloat("_visorRelLuminance", DisplayConfigurationManager.activeConfiguration.visorRelLuminance);
        blendMat.SetFloat("_displayRelLuminance", DisplayConfigurationManager.activeConfiguration.displayRelLuminance);
        blendMat.SetFloat("_worldRelLuminance", DisplayConfigurationManager.activeConfiguration.worldRelLuminance);

        Graphics.Blit(source, destination, blendMat);

        Graphics.Blit(source, rtfinal, blendMat);

    }

    public static void DumpRenderTexture(RenderTexture rt, string pngOutPath)
    {
        var oldRT = RenderTexture.active;

        RenderTexture mrt = new RenderTexture(rt.width, rt.height, rt.depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

        Graphics.Blit(rt, mrt);

        var tex = new Texture2D(mrt.width, mrt.height, TextureFormat.ARGB32, false);
        RenderTexture.active = mrt;
        tex.ReadPixels(new Rect(0, 0, mrt.width, mrt.height), 0, 0);
        tex.Apply();

        File.WriteAllBytes(pngOutPath, tex.EncodeToJPG());
        RenderTexture.active = oldRT;
    }

}
