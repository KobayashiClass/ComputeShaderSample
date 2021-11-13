using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コンピュートシェーダーを実行するクラス
/// </summary>
public class ComputeRunner : MonoBehaviour
{
    //実行するコンピュートシェーダー
    [SerializeField] ComputeShader Shader;
    //高さを書き込むレンダーテクスチャ
    public RenderTexture heightTex;
    //テクスチャサイズ
    [SerializeField] UInt2 texSize;

    //uint2 waveSource 用バッファ
    ComputeBuffer waveSource;
    //書き込み用テクスチャ
    RenderTexture result;

    //「addSource」のカーネルID
    int addSourceKernel;
    //「addSource」のスレッド数
    uint addSourceGroupSize;

    //「culcWave」のカーネルID
    int culcWaveKernel;
    //「culcWave」のスレッド数
    UInt2 culcWaveGroupSize;

    private void Start()
    {
        //カーネルIDとスレッド数の取得
        addSourceKernel = Shader.FindKernel("AddSource");
        Shader.GetKernelThreadGroupSizes(addSourceKernel, out uint x, out uint y, out uint z);
        addSourceGroupSize = x;

        culcWaveKernel = Shader.FindKernel("CulcWave");
        Shader.GetKernelThreadGroupSizes(culcWaveKernel, out x, out y, out z);
        culcWaveGroupSize = new UInt2 { x = x, y = y };

        //テクスチャのセットアップ
        heightTex.Release();
        heightTex.enableRandomWrite = true;

        heightTex.width = (int)texSize.x;
        heightTex.height = (int)texSize.y;

        result = new RenderTexture(heightTex);
        result.enableRandomWrite = true;

        //Shaderに送る物の登録
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
            //バッファに配列を設定
            var source = new[] { GetRandomPoint(), GetRandomPoint() };
            waveSource.SetData(source);

            //「addSource」を実行して、結果をコピーしておく
            Shader.Dispatch(addSourceKernel, source.Length / (int)addSourceGroupSize, 1, 1);
            Graphics.CopyTexture(result, heightTex);
        }
    }

    private void FixedUpdate()
    {
        //「culcWave」を実行する
        Shader.Dispatch(culcWaveKernel, texSize.X / culcWaveGroupSize.X, texSize.Y / culcWaveGroupSize.Y, 1);
        Graphics.CopyTexture(result, heightTex);
    }

    UInt2 GetRandomPoint()
    {
        return new UInt2 { x = (uint)Random.Range(0, texSize.x), y = (uint)Random.Range(0, texSize.y) };
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