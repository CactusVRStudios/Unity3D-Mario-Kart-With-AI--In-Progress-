using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastMotionBlur : MonoBehaviour
{
    [Range(0,1)]
    public float Distance;
	[Range(1,5)]
	public int FastFilter=4;
    Camera cam;
    public Material motionBlurMaterial;
	public Texture2D mask;
	public static readonly int currentPrevString = Shader.PropertyToID("_CurrentToPreviousViewProjectionMatrix");
	public static readonly int distanceString = Shader.PropertyToID("_Distance");
	public static readonly int blurTexString = Shader.PropertyToID("_BlurTex");

	private Matrix4x4 previousViewProjection;
	private Matrix4x4 viewProj;
	private Matrix4x4 currentToPreviousViewProjectionMatrix;
    void Start()
    {
        cam = GetComponent<Camera>();
		Shader.SetGlobalTexture("_MaskTex", mask == null ? Texture2D.whiteTexture : mask);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        viewProj = cam.projectionMatrix * cam.worldToCameraMatrix;
		currentToPreviousViewProjectionMatrix = previousViewProjection * viewProj.inverse;
		motionBlurMaterial.SetMatrix(currentPrevString, currentToPreviousViewProjectionMatrix);
		motionBlurMaterial.SetFloat(distanceString, 1-Distance);
        previousViewProjection = viewProj;
		RenderTexture rt = RenderTexture.GetTemporary (Screen.width / FastFilter,Screen.height / FastFilter, 0, src.format);
        Graphics.Blit(src, rt, motionBlurMaterial, 0);
		motionBlurMaterial.SetTexture (blurTexString, rt);
		RenderTexture.ReleaseTemporary (rt);
		Graphics.Blit (src, dest, motionBlurMaterial, 1);
    }
}
