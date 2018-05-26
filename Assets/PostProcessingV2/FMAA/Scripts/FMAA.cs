using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FMAA : MonoBehaviour
{
    struct CameraTransforms
    {
        public Matrix4x4 projTransform;
        public Matrix4x4 viewProjTransform;
    };
    public Material fxaaMaterial;

    private Camera Mcamera;
    const int historyFramesCount = 2;
    //RenderTexture[] prevRT = new RenderTexture[2];
    //CameraTransforms[] cameraTransforms = new CameraTransforms[historyFramesCount];
    CommandBuffer buffer;
    static int temp1 = Shader.PropertyToID("_TempTex1");
    int width, height;
    void Awake()
    {
        Mcamera = Camera.main;
        Mcamera.depthTextureMode |= DepthTextureMode.Depth;
        buffer = new CommandBuffer();
        buffer.name = "FMAA";
        SetBuffer();
    }

    void Update()
    {
        //Resize Here!
        if (width != Mcamera.pixelWidth || height != Mcamera.pixelHeight)
        {
            SetBuffer();
        }
    }

    void SetBuffer(){
            buffer.Clear();
            width = Mcamera.pixelWidth;
            height = Mcamera.pixelHeight;
            buffer.GetTemporaryRT(temp1, Mcamera.pixelWidth, Mcamera.pixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.DefaultHDR);
            buffer.Blit(BuiltinRenderTextureType.CameraTarget, temp1, fxaaMaterial);
            buffer.Blit(temp1, BuiltinRenderTextureType.CameraTarget, fxaaMaterial);
    }
    void OnEnable()
    {
        Mcamera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, buffer);
    }

    void OnDisable()
    {
        Mcamera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, buffer);
    }
    void OnDestroy()
    {
        buffer.Dispose();
    }
}
