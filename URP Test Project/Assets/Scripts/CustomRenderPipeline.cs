using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Yates.SPR
{
    public class CustomRenderPipeline : RenderPipeline
    {
        private CameraRenderPipeline _cameraRender;
        public CustomRenderPipeline()
        {
            _cameraRender = new CameraRenderPipeline();
        }
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var camera in cameras)
            {
                _cameraRender.Render(context, camera);
            }
        }
    }

    public class CameraRenderPipeline
    {
        private const string _BUFFER_NAME = "Yates Camera Render";
        private static ShaderTagId _unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        private ScriptableRenderContext _context;
        private Camera _camera;
        private CommandBuffer _buffer = new CommandBuffer() { name = _BUFFER_NAME};
        private CullingResults _cullingResults;
        public void Render(ScriptableRenderContext context, Camera camera)
        {
            this._context = context;
            this._camera = camera;

            if (!this.Cull())
                return;

            this.SetUp();
            this.DrawVisibleGeometry();
            this.Submit();
        }

        private bool Cull()
        {
            if (_camera.TryGetCullingParameters(out ScriptableCullingParameters param))
            {
                _cullingResults = _context.Cull(ref param);
                return true;
            }

            return false;
        }

        private void SetUp()
        {
            // 这一步中设置了unity_MatrixVP
            this._context.SetupCameraProperties(this._camera);
            this._buffer.ClearRenderTarget(true, true, Color.clear);

            // 上面两步是上一次渲染的末尾操作，下面两行是下一次渲染的起始操作s
            this._buffer.BeginSample(_BUFFER_NAME);
            this.ExecuteCommandBuffer();
        }

        /// <summary>
        /// 首先绘制不透明物体，然后天空盒，最后透明物体（透明物体不会写入深度缓冲区，因为需要透过它看到后面的物体）
        /// </summary>
        private void DrawVisibleGeometry()
        {
            // 根据传入的相机参数决定排序是基于正交还是基于透视
            var sortingSettings = new SortingSettings(_camera);
            // _unlitShaderTagId 用于指定阴影pass
            var drawingSettings = new DrawingSettings(_unlitShaderTagId, sortingSettings);
            // 指定允许渲染的renderQueue
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            this._context.DrawRenderers(this._cullingResults, ref drawingSettings, ref filteringSettings);

            this._context.DrawSkybox(this._camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            this._context.DrawRenderers(this._cullingResults, ref drawingSettings, ref filteringSettings);
        }

        /// <summary>
        /// buffer的执行和清除操作一般是配对使用的
        /// </summary>
        private void ExecuteCommandBuffer()
        {
            _context.ExecuteCommandBuffer(_buffer);
            _buffer.Clear();
        }

        private void Submit()
        {
            this._buffer.EndSample(_BUFFER_NAME);
            this.ExecuteCommandBuffer();
            this._context.Submit();
        }
    }
}