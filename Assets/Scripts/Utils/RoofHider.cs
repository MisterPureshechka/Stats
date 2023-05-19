using UnityEngine;
using UnityEngine.Rendering;

namespace Utils
{
    public class RoofHider : MonoBehaviour
    {
        private Renderer[] _renderers;
        private Renderer[] Renderers => _renderers ??= GetComponentsInChildren<Renderer>();

        public void SetRoofVisible(bool state = true)
        {
            foreach (var renderer in Renderers)
                renderer.shadowCastingMode = state
                    ? ShadowCastingMode.ShadowsOnly
                    : ShadowCastingMode.On;
        }
    }
}