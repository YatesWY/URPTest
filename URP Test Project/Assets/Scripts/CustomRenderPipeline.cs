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

    public partial class CameraRenderPipeline
    {
        private const string _BUFFER_NAME = "(Yates Camera Render)";
        private static ShaderTagId _unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        private ScriptableRenderContext _context;
        private Camera _camera;
        private CommandBuffer _buffer = new CommandBuffer() { name = _BUFFER_NAME};
        private CullingResults _cullingResults;

        public void Render(ScriptableRenderContext context, Camera camera)
        {
            this._context = context;
            this._camera = camera;

            this.PrepareBuffer();
            this.PrepareForSceneWindow();
            if (!this.Cull())
                return;

            this.SetUp();
            this.DrawVisibleGeometry();
            this.DrawUnSupportedShaders();
            this.DrawGizmos();
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
            // ��һ����������unity_MatrixVP
            this._context.SetupCameraProperties(this._camera);
            this._buffer.ClearRenderTarget(true, true, Color.clear);

            // ���￪ʼprofiler����
            this._buffer.BeginSample(SampleName);
            this.ExecuteCommandBuffer();
        }

        /// <summary>
        /// ���Ȼ��Ʋ�͸�����壬Ȼ����պУ����͸�����壨͸�����岻��д����Ȼ���������Ϊ��Ҫ͸����������������壩
        /// </summary>
        private void DrawVisibleGeometry()
        {
            // ���ݴ��������������������ǻ����������ǻ���͸��
            var sortingSettings = new SortingSettings(_camera);
            // _unlitShaderTagId ����ָ����Ӱpass
            var drawingSettings = new DrawingSettings(_unlitShaderTagId, sortingSettings);
            // ָ��������Ⱦ��renderQueue
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            this._context.DrawRenderers(this._cullingResults, ref drawingSettings, ref filteringSettings);

            this._context.DrawSkybox(this._camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            this._context.DrawRenderers(this._cullingResults, ref drawingSettings, ref filteringSettings);
        }

        /// <summary>
        /// ����Editorģʽ�±�ǲ�֧�ֵ�shader
        /// </summary>
        partial void DrawUnSupportedShaders();

        /// <summary>
        /// ����Editorģʽ�»���Gizmos
        /// </summary>
        partial void DrawGizmos();

        partial void PrepareBuffer();

        /// <summary>
        /// ����Editorģʽ����Scene������ʾһЩ����
        /// </summary>
        partial void PrepareForSceneWindow();

        /// <summary>
        /// buffer��ִ�к��������һ�������ʹ�õ�
        /// </summary>
        private void ExecuteCommandBuffer()
        {
            _context.ExecuteCommandBuffer(_buffer);
            _buffer.Clear();
        }

        private void Submit()
        {
            this._buffer.EndSample(SampleName);
            this.ExecuteCommandBuffer();
            this._context.Submit();
        }
    }
}