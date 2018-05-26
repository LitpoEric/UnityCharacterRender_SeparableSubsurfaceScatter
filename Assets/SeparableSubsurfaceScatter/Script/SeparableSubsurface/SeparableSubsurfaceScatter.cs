using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SeparableSubsurfaceScatter : MonoBehaviour {
	///////SSS Property
	[Range(0,6)]
	public float SubsurfaceScaler = 0.25f;
    public Color SubsurfaceColor;
    public Color SubsurfaceFalloff;
	private Camera RenderCamera;
	private CommandBuffer SubsurfaceBuffer;
	private Material SubsurfaceEffects = null;
	private List<Vector4> KernelArray = new List<Vector4>();
	static int SceneColorID = Shader.PropertyToID("_SceneColor");
	static int Kernel = Shader.PropertyToID("_Kernel");
    static int SSSScaler = Shader.PropertyToID("_SSSScale");

	void OnEnable() {
        ///Initialization SubsurfaceMaskBufferProperty
        RenderCamera = GetComponent<Camera>();
        SubsurfaceBuffer = new CommandBuffer();
		SubsurfaceEffects = new Material(Shader.Find("Hidden/SeparableSubsurfaceScatter"));
        SubsurfaceBuffer.name = "Separable Subsurface Scatter";
        RenderCamera.clearStencilAfterLightingPass = true;  //Clear Deferred Stencil
		RenderCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, SubsurfaceBuffer);
	}

    void OnDisable() {
        ClearSubsurfaceBuffer();
    }

	void OnPreRender() {
		UpdateSubsurface();
	}

	void UpdateSubsurface() {
		///SSS Color
		Vector3 SSSC = Vector3.Normalize(new Vector3 (SubsurfaceColor.r, SubsurfaceColor.g, SubsurfaceColor.b));
		Vector3 SSSFC = Vector3.Normalize(new Vector3 (SubsurfaceFalloff.r, SubsurfaceFalloff.g, SubsurfaceFalloff.b));
		SeparableSSS.CalculateKernel(KernelArray, 25, SSSC, SSSFC);
		SubsurfaceEffects.SetVectorArray (Kernel, KernelArray);
		SubsurfaceEffects.SetFloat (SSSScaler, SubsurfaceScaler);
		///SSS Buffer
		SubsurfaceBuffer.Clear();
		SubsurfaceBuffer.GetTemporaryRT (SceneColorID, RenderCamera.pixelWidth, RenderCamera.pixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.DefaultHDR);

        SubsurfaceBuffer.BlitStencil(BuiltinRenderTextureType.CameraTarget, SceneColorID, BuiltinRenderTextureType.CameraTarget, SubsurfaceEffects, 0);
        SubsurfaceBuffer.BlitSRT(SceneColorID, BuiltinRenderTextureType.CameraTarget, SubsurfaceEffects, 1);
    }

	void ClearSubsurfaceBuffer() {
		SubsurfaceBuffer.ReleaseTemporaryRT(SceneColorID);
		RenderCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, SubsurfaceBuffer);
		SubsurfaceBuffer.Release();
		SubsurfaceBuffer.Dispose();
	}

	void SetFPSFrame(bool UseHighFPS, int TargetFPS) {
		if(UseHighFPS == true){
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = TargetFPS;
		}
		else{
			QualitySettings.vSyncCount = 1;
			Application.targetFrameRate = 60;
		}
	}
}
