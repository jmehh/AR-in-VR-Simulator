using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

public class DisplayConfigurationManager : MonoBehaviour
{
    [Serializable]
    public enum ProjectionMatrixType {Manual, Generated}

    [Serializable]
    public struct ProjectionParams
    {
        [SerializeField] public float horizontalFOV;
        [SerializeField] public float verticalFOV;
        [SerializeField] public float IPD;
        [SerializeField] public float projectionPlaneDistance;
    }

    [Serializable]
    public struct DisplayConfiguration
    {
        [SerializeField] public ProjectionMatrixType projectionMatrixType;
        [SerializeField] public ProjectionParams projectionParams;
        [SerializeField] public Matrix4x4 leftProjectionMatrix;
        [SerializeField] public Matrix4x4 rightProjectionMatrix;
        [SerializeField] public List<Material> leftARMaterials;
        [SerializeField] public List<Material> rightARMaterials;
        [SerializeField] public List<Material> leftEyeMaterials;
        [SerializeField] public List<Material> rightEyeMaterials;
    }

    public static DisplayConfiguration activeConfiguration;
    public static int configurationsCount;

    [SerializeField] 
    public List<DisplayConfiguration> displayConfigurations;

    void Start()
    {
        configurationsCount = displayConfigurations.Count;
        for (int i = 0; i < configurationsCount; i++)
        {
            DisplayConfiguration cfg = displayConfigurations[i];
            if (cfg.projectionMatrixType == ProjectionMatrixType.Generated)
            {
                cfg.leftProjectionMatrix = GenerateProjectionMatrix(cfg.projectionParams.horizontalFOV, cfg.projectionParams.verticalFOV, cfg.projectionParams.IPD, cfg.projectionParams.projectionPlaneDistance, true);
                cfg.rightProjectionMatrix = GenerateProjectionMatrix(cfg.projectionParams.horizontalFOV, cfg.projectionParams.verticalFOV, cfg.projectionParams.IPD, cfg.projectionParams.projectionPlaneDistance, false);
            }
            displayConfigurations[i] = cfg;
        }

        activeConfiguration = displayConfigurations[0];
    }

    // see https://paulbourke.net/stereographics/stereorender/
    public static Matrix4x4 GenerateProjectionMatrix(float horizontalFOV, float verticalFOV, float IPD, float projectionPlane, bool isLeft)
    {
        float aspectRatio = horizontalFOV / verticalFOV;
        float ndfl = 0.3f / projectionPlane;
        float tanHalfFOV = math.tan(math.radians(0.5f * verticalFOV));

        float left = (-aspectRatio * 0.3f * tanHalfFOV) - 0.5f * IPD * ndfl;
        float right = (aspectRatio * 0.3f * tanHalfFOV) - 0.5f * IPD * ndfl;

        float top = 0.3f * tanHalfFOV;
        float bottom = -0.3f * tanHalfFOV;

        return float4x4.PerspectiveOffCenter(isLeft ? -right : left, isLeft ? -left : right, bottom, top, 0.3f, 1000f);
    }
}
