using UnityEngine;

public class Demo : MonoBehaviour
{
    public ComputeShader computeShader;
    public Material material;

    void Start()
    {
        RenderTexture mRenderTexture = new RenderTexture(256, 256, 16);
        mRenderTexture.enableRandomWrite = true;
        mRenderTexture.Create();

        material.mainTexture = mRenderTexture;
        int kernelIndex = computeShader.FindKernel("CSMain");
        computeShader.SetTexture(kernelIndex, "Result", mRenderTexture);

        computeShader.Dispatch(kernelIndex, mRenderTexture.width / 8, mRenderTexture.height / 8, 1);
    }
}
