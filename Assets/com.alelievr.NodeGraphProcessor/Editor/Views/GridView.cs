using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    public class GridView : VisualElement
    {
        private static readonly int ID_RT_TRANSFORM = Shader.PropertyToID("_RtTransform");
        private static readonly int ID_SCALE = Shader.PropertyToID("_Scale");

        private Material material;
        private RenderTexture renderTexture;
        private VisualElement transformSource;

        public GridView()
        {

        }

        public GridView(VisualElement transformSource)
        {
            this.transformSource = transformSource;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            style.position = Position.Absolute;
            style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
            style.width = new Length(Screen.currentResolution.width, LengthUnit.Pixel);
            style.height = new Length(Screen.currentResolution.height, LengthUnit.Pixel);
        }

        private void OnAttachToPanel(AttachToPanelEvent e)
        {
            renderTexture = RenderTexture.GetTemporary(
                Screen.currentResolution.width,
                Screen.currentResolution.height,
                0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default);
            renderTexture.Create();
            material = new Material(Shader.Find("GraphView/Grid"));
            style.backgroundImage = Background.FromRenderTexture(renderTexture);

            Redraw();

            EditorApplication.update -= Redraw;
            EditorApplication.update += Redraw;
        }

        private void Redraw()
        {
            if (transformSource == null || renderTexture == null || material == null)
                return;

            material.SetVector(ID_RT_TRANSFORM, new Vector4(
                Screen.currentResolution.width, Screen.currentResolution.height,
                transformSource.resolvedStyle.translate.x, transformSource.resolvedStyle.translate.y
            ));
            material.SetFloat(ID_SCALE, transformSource.resolvedStyle.scale.value.x);
            Graphics.SetRenderTarget(renderTexture);
            Graphics.Blit(null, material);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent e)
        {
            if (renderTexture)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }
            
            if (material)
            {
                Object.DestroyImmediate(material);
            }

            style.backgroundImage = null;
            EditorApplication.update -= Redraw;
        }
    }
}
