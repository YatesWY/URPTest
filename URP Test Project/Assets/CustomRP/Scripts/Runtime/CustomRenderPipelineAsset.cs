using UnityEngine;
using UnityEngine.Rendering;

namespace Yates.SPR
{
    [CreateAssetMenu(menuName = "Rendering/YatesRenderPipeline")]
    public class CustomRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField]
        private bool srpBatcher = true;

        public bool UseScriptableRenderPipelineBatching => srpBatcher;

        protected override RenderPipeline CreatePipeline()
        {
            return new CustomRenderPipeline(this);
        }
    }
}