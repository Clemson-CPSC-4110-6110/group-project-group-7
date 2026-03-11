using UnityEngine;

public class WindowCleaner : MonoBehaviour
{
    public int textureSize = 512; 
    public int brushRadius = 30;  

    private Texture2D maskTexture;
    private Renderer rend;
    private MeshCollider meshCollider;

    void Start()
    {
        maskTexture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.black; 
        
        maskTexture.SetPixels(pixels);
        maskTexture.Apply();

        rend = GetComponent<Renderer>();
        rend.material.SetTexture("_MaskTex", maskTexture); 
        meshCollider = GetComponent<MeshCollider>();
    }

    void OnTriggerStay(Collider other)
    {
        // 1. Is it a tool?
        CleaningTool tool = other.GetComponent<CleaningTool>();
        if (tool == null) return; 

        // IF YOU SEE THIS IN CONSOLE, THE COLLISION WORKS!
        Debug.Log("COLLISION DETECTED WITH: " + tool.typeOfTool);

        bool isSponge = (tool.typeOfTool == ToolType.Sponge);
        bool isSqueegee = (tool.typeOfTool == ToolType.Squeegee);

        // 2. We shoot a ray from the tool, pointing in the direction the tool is facing
        Ray ray = new Ray(other.transform.position, -transform.forward);        
        // 3. Does the ray actually hit the window?
        if (meshCollider.Raycast(ray, out RaycastHit hit, 2.0f))
        {
            // IF YOU SEE THIS, THE RAYCAST WORKS AND IT SHOULD BE PAINTING!
            Debug.Log("RAYCAST HIT THE WINDOW AT UV: " + hit.textureCoord);
            PaintMask(hit.textureCoord, isSponge, isSqueegee);
        }
        else
        {
            // IF YOU SEE THIS, THE TOOL IS TOUCHING, BUT THE RAYCAST MISSED
            Debug.LogWarning("Raycast missed! The tool's 'Forward' direction might be pointing away from the window.");
        }
    }

    void PaintMask(Vector2 uv, bool isSponge, bool isSqueegee)
    {
        int centerX = (int)(uv.x * textureSize);
        int centerY = (int)(uv.y * textureSize);
        bool textureChanged = false;

        for (int x = centerX - brushRadius; x <= centerX + brushRadius; x++)
        {
            for (int y = centerY - brushRadius; y <= centerY + brushRadius; y++)
            {
                if (x >= 0 && x < textureSize && y >= 0 && y < textureSize)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    if (distance <= brushRadius)
                    {
                        Color currentColor = maskTexture.GetPixel(x, y);

                        if (isSponge && currentColor.r < 0.1f) 
                        {
                            maskTexture.SetPixel(x, y, new Color(0.5f, 0.5f, 0.5f));
                            textureChanged = true;
                        }
                        else if (isSqueegee && currentColor.r > 0.4f && currentColor.r < 0.6f) 
                        {
                            maskTexture.SetPixel(x, y, Color.white);
                            textureChanged = true;
                        }
                    }
                }
            }
        }

        if (textureChanged) maskTexture.Apply(); 
    }
}