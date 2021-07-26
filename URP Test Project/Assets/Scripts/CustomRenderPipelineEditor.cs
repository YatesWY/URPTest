using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Yates.SPR
{
    public partial class CameraRenderPipeline
    {
#if UNITY_EDITOR
        private static ShaderTagId[] _legacyShaderTagIds =
        {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM"),
        };

        private static Material _errorMaterial;

        partial void DrawUnSupportedShaders()
        {
            if (_errorMaterial == null)
            {
                _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
            }

            var drawingSettings = new DrawingSettings(_legacyShaderTagIds[0], new SortingSettings(this._camera))
            {
                overrideMaterial = _errorMaterial
            };

            for (int i = 1; i < _legacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, _legacyShaderTagIds[i]);
            }

            var filteringSettings = FilteringSettings.defaultValue;
            this._context.DrawRenderers(this._cullingResults, ref drawingSettings, ref filteringSettings);
        }
#endif
    }
}

