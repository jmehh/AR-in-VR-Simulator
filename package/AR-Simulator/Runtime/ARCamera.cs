using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ARCamera : MonoBehaviour
{
    public RenderTexture rt;
    public Camera eyeCam;

    private Camera arCam;

    private RenderTexture rt_tmp_1;
    private RenderTexture rt_tmp_2;
    private RenderTexture[] tmpRTs;

    public bool isLeft;

    bool screenshot = false;
    void Start()
    {
        arCam = GetComponent<Camera>();
    }

    void Update()
    {
        if (isLeft)
        {
            arCam.projectionMatrix = DisplayConfigurationManager.activeConfiguration.leftProjectionMatrix;
            arCam.worldToCameraMatrix = eyeCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);

        }
        else
        {
            arCam.projectionMatrix = DisplayConfigurationManager.activeConfiguration.rightProjectionMatrix;
            arCam.worldToCameraMatrix = eyeCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);

        }

        if (arCam.targetTexture == null)
        {
            RenderTexture srt = new RenderTexture(DisplayConfigurationManager.activeConfiguration.ARRenderResX, DisplayConfigurationManager.activeConfiguration.ARRenderResY, 0, RenderTextureFormat.Default);
            arCam.targetTexture = srt;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        if (rt == null || rt.width != source.width)
        { 
            rt = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.Default);
            rt_tmp_1 = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.Default);
            rt_tmp_2 = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.Default);
            tmpRTs = new RenderTexture[] { rt_tmp_1, rt_tmp_2 };
        }

        List<Material> materialsList;
        if (isLeft) {
            materialsList = DisplayConfigurationManager.activeConfiguration.leftARMaterials;
        } else
        {
            materialsList = DisplayConfigurationManager.activeConfiguration.rightARMaterials;
        }


        int blitCount = 0;
        for (int i = 0; i < materialsList.Count; i++)
        {
            Material mat = materialsList[i];
            if (blitCount == 0)
            {
                Graphics.Blit(source, tmpRTs[0], mat);
            } 
            else
            {
                Graphics.Blit(tmpRTs[(blitCount - 1) % 2], tmpRTs[blitCount % 2], mat);
            }
            blitCount++;
        }

        Graphics.Blit(tmpRTs[(blitCount - 1) % 2], rt);
    }

}
