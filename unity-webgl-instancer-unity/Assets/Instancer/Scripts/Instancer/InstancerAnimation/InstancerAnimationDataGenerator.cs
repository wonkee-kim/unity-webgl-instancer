using UnityEngine;
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
#endif

public class InstancerAnimationDataGenerator : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] private Animator _animator;
    [SerializeField] private string _animationClipName;

#if UNITY_EDITOR
    [ContextMenu(nameof(GenerateAnimationData))]
    public void GenerateAnimationData()
    {
        _animator.enabled = false; // disable animator to sample animation

        // Get Animation Clip
        AnimationClip animationClip;
        if (String.IsNullOrEmpty(_animationClipName))
        {
            animationClip = _animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        }
        else
        {
            animationClip = Array.Find(_animator.GetCurrentAnimatorClipInfo(0), c => c.clip.name == _animationClipName).clip;
        }


        // Get Positions and Normals
        float animationLength = animationClip.length;
        int frameRate = Mathf.CeilToInt(animationClip.frameRate);
        int frameCount = Mathf.CeilToInt(animationLength * frameRate);

        Vector3[][] positions = new Vector3[frameCount][];
        Vector3[][] normals = new Vector3[frameCount][];

        Mesh targetMesh = new Mesh();

        for (int i = 0; i < frameCount; i++) // ignore the last frame
        {
            float time = i / (float)frameRate;
            animationClip.SampleAnimation(_animator.gameObject, time);
            _skinnedMeshRenderer.BakeMesh(targetMesh);

            positions[i] = targetMesh.vertices;
            normals[i] = targetMesh.normals;
        }


        // Convert to Texture
        int vertexCount = positions[0].Length;
        Color[] positionColors = new Color[vertexCount * frameCount];
        Color[] normalColors = new Color[vertexCount * frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            for (int j = 0; j < vertexCount; j++)
            {
                positionColors[i * vertexCount + j] = new Color(positions[i][j].x, positions[i][j].y, positions[i][j].z, 0f);
                normalColors[i * vertexCount + j] = new Color(normals[i][j].x, normals[i][j].y, normals[i][j].z, 0f);
            }
        }

        Texture2D positionTexture = new Texture2D(vertexCount, frameCount, TextureFormat.RGBAHalf, false);
        positionTexture.filterMode = FilterMode.Bilinear;
        positionTexture.wrapMode = animationClip.isLooping ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        Texture2D normalTexture = new Texture2D(vertexCount, frameCount, TextureFormat.RGBAHalf, false);
        normalTexture.filterMode = FilterMode.Bilinear;
        normalTexture.wrapMode = animationClip.isLooping ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        positionTexture.SetPixels(positionColors);
        normalTexture.SetPixels(normalColors);
        positionTexture.Apply();
        normalTexture.Apply();

        string path = GetFilePath() + animationClip.name + "/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        // Save texture asset
        AssetDatabase.CreateAsset(positionTexture, path + animationClip.name + "_pos.asset");
        AssetDatabase.CreateAsset(normalTexture, path + animationClip.name + "_norm.asset");

        // Save to ScriptableObject
        InstancerAnimationDataObject animationDataObject = ScriptableObject.CreateInstance<InstancerAnimationDataObject>();
        animationDataObject.animationName = animationClip.name;
        animationDataObject.positionTexture = positionTexture;
        animationDataObject.normalTexture = normalTexture;
        animationDataObject.vertexCount = vertexCount;
        animationDataObject.frameCount = frameCount;
        animationDataObject.frameRate = frameRate;
        animationDataObject.isLooping = animationClip.isLooping;

        AssetDatabase.CreateAsset(animationDataObject, path + animationClip.name + ".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private string GetFilePath()
    {
        string[] res = System.IO.Directory.GetFiles(Application.dataPath, nameof(InstancerAnimationDataObject) + ".cs", SearchOption.AllDirectories);
        if (res.Length == 0)
        {
            return "Assets/" + nameof(InstancerAnimationDataObject) + "/";
        }
        else
        {
            string path = res[0];
            path = path.Replace('\\', '/');
            path = path.Replace(Application.dataPath, "Assets");
            path = path.Substring(0, path.LastIndexOf('/') + 1);
            path += nameof(InstancerAnimationDataObject) + "/";
            return path;
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(InstancerAnimationDataGenerator))]
public class InstancerAnimationDataGeneratorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10f);
        var generator = target as InstancerAnimationDataGenerator;
        if (GUILayout.Button(nameof(generator.GenerateAnimationData)))
        {
            generator.GenerateAnimationData();
        }
    }
}
#endif