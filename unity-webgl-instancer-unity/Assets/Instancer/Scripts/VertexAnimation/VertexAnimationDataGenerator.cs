using UnityEngine;
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
#endif

public class VertexAnimationDataGenerator : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] private Animator _animator;
    [SerializeField] private string[] _animationClipNames; // maximum 4

#if UNITY_EDITOR
    [ContextMenu(nameof(GenerateAnimationData))]
    public void GenerateAnimationData()
    {
        string path = GetFilePath() + _skinnedMeshRenderer.sharedMesh.name + "/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        int vertexCount = _skinnedMeshRenderer.sharedMesh.vertexCount;

        VertexAnimationDataObject.AnimationClipData[] animationClipDatas = new VertexAnimationDataObject.AnimationClipData[_animationClipNames.Length];
        for (int clipIndex = 0; clipIndex < _animationClipNames.Length; clipIndex++)
        {
            // Get Animation Clip
            string animationClipName = _animationClipNames[clipIndex];
            AnimationClip animationClip;
            if (String.IsNullOrEmpty(animationClipName))
            {
                animationClip = _animator.runtimeAnimatorController.animationClips[0];
            }
            else
            {
                animationClip = Array.Find(_animator.runtimeAnimatorController.animationClips, c => c.name == animationClipName);
            }

            // Get Positions and Normals
            float animationLength = animationClip.length;
            int frameRate = Mathf.CeilToInt(animationClip.frameRate);
            int frameCount = Mathf.CeilToInt(animationLength * frameRate);
            bool isLooping = animationClip.isLooping;

            Vector3[][] positions = new Vector3[frameCount][];
            Vector3[][] normals = new Vector3[frameCount][];

            Mesh targetMesh = new Mesh();

            _animator.enabled = false; // disable animator to sample animation
            for (int i = 0; i < frameCount; i++) // ignore the last frame
            {
                float time = i / (float)frameRate;
                animationClip.SampleAnimation(_animator.gameObject, time);
                _skinnedMeshRenderer.BakeMesh(targetMesh);

                positions[i] = targetMesh.vertices;
                normals[i] = targetMesh.normals;
            }
            _animator.enabled = true; // turn it back on

            // Convert to Texture
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
            positionTexture.wrapMode = isLooping ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            Texture2D normalTexture = new Texture2D(vertexCount, frameCount, TextureFormat.RGBAHalf, false);
            normalTexture.filterMode = FilterMode.Bilinear;
            normalTexture.wrapMode = isLooping ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            positionTexture.SetPixels(positionColors);
            normalTexture.SetPixels(normalColors);
            positionTexture.Apply();
            normalTexture.Apply();

            // Save texture asset
            AssetDatabase.CreateAsset(positionTexture, path + $"clip_{clipIndex}_{animationClip.name}_pos.asset");
            AssetDatabase.CreateAsset(normalTexture, path + $"clip_{clipIndex}_{animationClip.name}_norm.asset");

            animationClipDatas[clipIndex] = new VertexAnimationDataObject.AnimationClipData()
            {
                clipName = animationClip.name,
                positionTexture = positionTexture,
                normalTexture = normalTexture,
                frameCount = frameCount,
                frameRate = frameRate,
                isLooping = isLooping ? 1f : 0f,
            };

            // Destroy temporary mesh
            DestroyImmediate(targetMesh);
        }

        // Save to ScriptableObject
        VertexAnimationDataObject animationDataObject = ScriptableObject.CreateInstance<VertexAnimationDataObject>();
        animationDataObject.meshName = _skinnedMeshRenderer.sharedMesh.name;
        animationDataObject.mesh = _skinnedMeshRenderer.sharedMesh;
        animationDataObject.vertexCount = vertexCount;
        animationDataObject.animationClipDatas = animationClipDatas;

        AssetDatabase.CreateAsset(animationDataObject, path + $"{nameof(VertexAnimationDataObject)}_{animationDataObject.meshName}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private string GetFilePath()
    {
        string[] res = System.IO.Directory.GetFiles(Application.dataPath, nameof(VertexAnimationDataObject) + ".cs", SearchOption.AllDirectories);
        if (res.Length == 0)
        {
            return "Assets/" + nameof(VertexAnimationDataObject) + "/";
        }
        else
        {
            string path = res[0];
            path = path.Replace('\\', '/');
            path = path.Replace(Application.dataPath, "Assets");
            path = path.Substring(0, path.LastIndexOf('/') + 1);
            path += nameof(VertexAnimationDataObject) + "/";
            return path;
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(VertexAnimationDataGenerator))]
public class InstancerAnimationDataGeneratorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10f);
        var generator = target as VertexAnimationDataGenerator;
        if (GUILayout.Button(nameof(generator.GenerateAnimationData)))
        {
            generator.GenerateAnimationData();
        }
    }
}
#endif