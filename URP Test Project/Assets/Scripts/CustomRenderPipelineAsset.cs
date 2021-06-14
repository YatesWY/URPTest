using UnityEngine;
using UnityEngine.Rendering;

namespace Yates.SPR
{
    [CreateAssetMenu(menuName = "Rendering/YatesRenderPipeline")]
    public class CustomRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new CustomRenderPipeline();
        }
    }
}