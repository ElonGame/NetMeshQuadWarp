using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utilities.Debug;

namespace Utilities.Debug
{
    [ExecuteInEditMode]
    public class GLDebugRenderer : SingletonMonoBehaviour<GLDebugRenderer>
    {   
        [SerializeField]
        Shader _glDrawShader = null;

        Material _glDrawMaterial;

        int _propId_SrcBlend = -1;
        int _propId_DstBlend = -1;
        int _propId_Cull     = -1;
        int _propId_ZWrite   = -1;

        void Start()
        {
            _propId_SrcBlend = Shader.PropertyToID("_SrcBlend");
            _propId_DstBlend = Shader.PropertyToID("_DstBlend");
            _propId_Cull     = Shader.PropertyToID("_Cull");
            _propId_ZWrite   = Shader.PropertyToID("_ZWrite");
        }

        /// <summary>
        /// OnRenderObject
        /// </summary>
        void OnRenderObject()
        {
            SetGLMaterial();
        }

        /// <summary>
        /// GL描画のためのマテリアルを作成
        /// </summary>
        void CreateGLDrawMaterial()
        {
            if (_glDrawMaterial == null)
            {
                // Crate material
                _glDrawMaterial = new Material(_glDrawShader);
                _glDrawMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                _glDrawMaterial.SetInt(_propId_SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _glDrawMaterial.SetInt(_propId_DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                _glDrawMaterial.SetInt(_propId_Cull, (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                _glDrawMaterial.SetInt(_propId_ZWrite, 0);
            }
        }

        /// <summary>
        /// GL描画のためのマテリアルをセット
        /// </summary>
        public void SetGLMaterial()
        {
            UnityEngine.Assertions.Assert.IsTrue(_glDrawShader != null, "GL Draw shader is null.");
            // マテリアルを作成
            CreateGLDrawMaterial();
            // マテリアルをセット
            _glDrawMaterial.SetPass(0);
        }
    }
}