using UnityEngine;

public class InstancerAnimationDataObject : ScriptableObject
{
    [Header("Animation Data")]
    public string animationName;

    public Texture2D positionTexture;
    public Texture2D normalTexture;
    // public Texture2D tangentTexture; // Use this when use normal map

    public int vertexCount; // texture width
    public int frameCount; // texture height
    public int frameRate = 30;
    public float frameTime => 1f / frameRate;
    public float animationLength => frameCount * frameTime;
    public float animationLengthInv => frameRate / frameCount;
    public float texelSize => 1f / vertexCount;
    public bool isLooping = true;

    [Header("Shader Property Names")]
    public string positionTexturePropertyName = "_AnimTexPos";
    public string normalTexturePropertyName = "_AnimTexNorm";
    // public string tangentTexturePropertyName = "_AnimTexTang";
    public string texelSizePropertyName = "_TexelSize";
    public string animationLengthInvPropertyName = "_AnimLengthInv";
}
