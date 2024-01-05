using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VATGroupTester : MonoBehaviour
{
    // [SerializeField] private VertexAnimationRenderer[] _renderers;
    [SerializeField] private MeshRenderer[] _mrs;
    [SerializeField] private SkinnedMeshRenderer[] _smrs;
    [SerializeField] private Animator[] _anims;
    [SerializeField] private bool _useVertexAnimation = true;

    [ContextMenu(nameof(GetAllAnimationRenderer))]
    public void GetAllAnimationRenderer()
    {
        // _renderers = this.GetComponentsInChildren<VertexAnimationRenderer>();
        _mrs = this.GetComponentsInChildren<MeshRenderer>();
        _smrs = this.GetComponentsInChildren<SkinnedMeshRenderer>();
        _anims = this.GetComponentsInChildren<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            _useVertexAnimation = !_useVertexAnimation;
            foreach (var mr in _mrs)
            {
                mr.enabled = _useVertexAnimation;
            }
            foreach (var smr in _smrs)
            {
                smr.enabled = !_useVertexAnimation;
            }
            foreach (var anim in _anims)
            {
                anim.enabled = !_useVertexAnimation;
            }
        }
    }
}
