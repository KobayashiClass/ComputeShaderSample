using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeRunner : MonoBehaviour
{
    public ComputeShader Shader;
    public RenderTexture heightTex;
    public RenderTexture result;
    ComputeBuffer waveSource;
    int addSourceKernel;
    uint addSourceGroupSize;
    int culcWaveKernel;
    UInt2 culcWaveGroupSize;
    [SerializeField] UInt2 texSize;

    private void Start()
    {
        addSourceKernel = Shader.FindKernel("AddSource");
        Shader.GetKernelThreadGroupSizes(addSourceKernel, out uint x, out uint y, out uint z);
        addSourceGroupSize = x;

        culcWaveKernel = Shader.FindKernel("CulcWave");
        Shader.GetKernelThreadGroupSizes(culcWaveKernel, out x, out y, out z);
        culcWaveGroupSize = new UInt2 { x = x, y = y };

        heightTex.Release();
        heightTex.enableRandomWrite = true;

        result.Release();
        result.enableRandomWrite = true;

        result.width = heightTex.width = (int)texSize.x;
        result.height = heightTex.height = (int)texSize.y;

        waveSource = new ComputeBuffer(2, UInt2.Size);
        Shader.SetBuffer(addSourceKernel, "waveSource", waveSource);
        Shader.SetTexture(addSourceKernel, "heightTex", heightTex);
        Shader.SetTexture(addSourceKernel, "result", result);
        Shader.SetTexture(culcWaveKernel, "heightTex", heightTex);
        Shader.SetTexture(culcWaveKernel, "result", result);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var source = new[] { GetRandomPoint(), GetRandomPoint() };
            waveSource.SetData(source);

            Shader.Dispatch(addSourceKernel, source.Length / (int)addSourceGroupSize, 1, 1);
            Graphics.CopyTexture(result, heightTex);
        }
    }

    UInt2 GetRandomPoint()
    {
        return new UInt2 { x = (uint)Random.Range(0, texSize.x), y = (uint)Random.Range(0, texSize.y) };
    }

    private void FixedUpdate()
    {
        Shader.Dispatch(culcWaveKernel, texSize.X / culcWaveGroupSize.X, texSize.Y / culcWaveGroupSize.Y, 1);
        Graphics.CopyTexture(result, heightTex);
    }

    private void OnDestroy()
    {
        heightTex.Release();
        waveSource.Release();
        heightTex = null;
    }
}

[System.Serializable]
struct UInt2
{
    public uint x;
    public uint y;

    public int X => (int)x;
    public int Y => (int)y;

    public static int Size => sizeof(uint) * 2;
}