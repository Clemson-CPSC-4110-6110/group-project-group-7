using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HoverHighlight : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Material[] originalMaterials;

    public Material[] highlightMaterials;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // this stores original materials
        originalMaterials = meshRenderer.sharedMaterials;

        // i added this make sure object starts with original materials
        meshRenderer.materials = originalMaterials;
    }

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (highlightMaterials.Length == meshRenderer.materials.Length)
        {
            meshRenderer.materials = highlightMaterials;
        }
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        meshRenderer.materials = originalMaterials;
    }
}