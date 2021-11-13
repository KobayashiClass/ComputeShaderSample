using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �R���s���[�g�V�F�[�_�[�����s����N���X
/// </summary>
public class ComputeRunner : MonoBehaviour
{
    //���s����R���s���[�g�V�F�[�_�[
    [SerializeField] ComputeShader Shader;
    //�������������ރ����_�[�e�N�X�`��
    public RenderTexture heightTex;
    //�e�N�X�`���T�C�Y
    [SerializeField] UInt2 texSize;

    //uint2 waveSource �p�o�b�t�@
    ComputeBuffer waveSource;
    //�������ݗp�e�N�X�`��
    RenderTexture result;

    //�uaddSource�v�̃J�[�l��ID
    int addSourceKernel;
    //�uaddSource�v�̃X���b�h��
    uint addSourceGroupSize;

    //�uculcWave�v�̃J�[�l��ID
    int culcWaveKernel;
    //�uculcWave�v�̃X���b�h��
    UInt2 culcWaveGroupSize;

    private void Start()
    {
        //�J�[�l��ID�ƃX���b�h���̎擾
        addSourceKernel = Shader.FindKernel("AddSource");
        Shader.GetKernelThreadGroupSizes(addSourceKernel, out uint x, out uint y, out uint z);
        addSourceGroupSize = x;

        culcWaveKernel = Shader.FindKernel("CulcWave");
        Shader.GetKernelThreadGroupSizes(culcWaveKernel, out x, out y, out z);
        culcWaveGroupSize = new UInt2 { x = x, y = y };

        //�e�N�X�`���̃Z�b�g�A�b�v
        heightTex.Release();
        heightTex.enableRandomWrite = true;

        heightTex.width = (int)texSize.x;
        heightTex.height = (int)texSize.y;

        result = new RenderTexture(heightTex);
        result.enableRandomWrite = true;

        //Shader�ɑ��镨�̓o�^
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
            //�o�b�t�@�ɔz���ݒ�
            var source = new[] { GetRandomPoint(), GetRandomPoint() };
            waveSource.SetData(source);

            //�uaddSource�v�����s���āA���ʂ��R�s�[���Ă���
            Shader.Dispatch(addSourceKernel, source.Length / (int)addSourceGroupSize, 1, 1);
            Graphics.CopyTexture(result, heightTex);
        }
    }

    private void FixedUpdate()
    {
        //�uculcWave�v�����s����
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