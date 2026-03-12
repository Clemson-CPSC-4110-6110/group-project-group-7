using UnityEngine;

public class WindowCleaner : MonoBehaviour
{
    [Header("Cleaning Settings")]
    public int textureSize = 512; 
    public int brushRadius = 60;  
    public float paintRate = 0.05f;

    [Header("Squeegee Blade Settings")]
    public float bladeWidth = 80f;  
    public float bladeHeight = 15f; 
    
    [Header("Calibration")]
    [Tooltip("Use small numbers like -0.05 to nudge the squeegee line down")]
    public float verticalOffset = 0f;

    private Texture2D maskTexture;
    private Renderer rend;
    private Collider windowCollider;
    private float nextPaintTime = 0f;
    private Vector2 lastUV = -Vector2.one; 

    void Start()
    {
        maskTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false, true);
        Color[] pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.black; 
        maskTexture.SetPixels(pixels);
        maskTexture.Apply();

        rend = GetComponent<Renderer>();
        if (rend != null) rend.material.SetTexture("_MaskTex", maskTexture); 
        windowCollider = GetComponent<Collider>(); 
    }

    void OnTriggerStay(Collider other)
    {
        // --- NEW BUBBLE LOGIC: Turn on particles if they exist ---
        ParticleSystem toolFX = other.GetComponentInChildren<ParticleSystem>();
        if (toolFX != null && !toolFX.isEmitting)
        {
            toolFX.Play();
        }

        if (Time.time < nextPaintTime) return;

        CleaningTool tool = other.GetComponent<CleaningTool>();
        if (tool == null) return; 

        Vector3 hitPoint = windowCollider.ClosestPoint(other.transform.position);
        Vector3 localPoint = transform.InverseTransformPoint(hitPoint);
        Vector2 uv = new Vector2(localPoint.x + 0.5f, localPoint.y + 0.5f + verticalOffset);

        if (uv.x >= 0f && uv.x <= 1f && uv.y >= 0f && uv.y <= 1f)
        {
            PaintMask(uv, tool.typeOfTool, other.transform.rotation);
            nextPaintTime = Time.time + paintRate; 
        }
    }

    void OnTriggerExit(Collider other)
    {
        // --- NEW BUBBLE LOGIC: Turn off particles when we leave the window ---
        ParticleSystem toolFX = other.GetComponentInChildren<ParticleSystem>();
        if (toolFX != null)
        {
            toolFX.Stop();
        }

        if (other.GetComponent<CleaningTool>() != null)
        {
            lastUV = -Vector2.one;
        }
    }

    void PaintMask(Vector2 uv, ToolType toolType, Quaternion toolRotation)
    {
        bool textureChanged = false;

        if (toolType == ToolType.Sponge)
        {
            int cX = (int)(uv.x * textureSize);
            int cY = (int)(uv.y * textureSize);
            if (PaintCircle(cX, cY)) textureChanged = true;
        }
        else if (toolType == ToolType.Squeegee)
        {
            if (lastUV != -Vector2.one)
            {
                float distance = Vector2.Distance(lastUV, uv) * textureSize;
                int steps = Mathf.CeilToInt(distance / (bladeHeight / 2f)); 

                for (int i = 0; i <= steps; i++)
                {
                    float t = (float)i / steps;
                    Vector2 lerpedUV = Vector2.Lerp(lastUV, uv, t);
                    if (PaintBlade(lerpedUV, toolRotation)) textureChanged = true;
                }
            }
            else
            {
                if (PaintBlade(uv, toolRotation)) textureChanged = true;
            }
            lastUV = uv;
        }

        if (textureChanged) maskTexture.Apply();
    }

    bool PaintCircle(int centerX, int centerY)
    {
        bool changed = false;
        int radiusSqr = brushRadius * brushRadius;
        for (int x = centerX - brushRadius; x <= centerX + brushRadius; x++)
        {
            for (int y = centerY - brushRadius; y <= centerY + brushRadius; y++)
            {
                if (x >= 0 && x < textureSize && y >= 0 && y < textureSize)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    if ((dx * dx + dy * dy) <= radiusSqr)
                    {
                        Color currentColor = maskTexture.GetPixel(x, y);
                        if (currentColor.r < 0.2f) 
                        {
                            maskTexture.SetPixel(x, y, new Color(0.5f, 0.5f, 0.5f, 1f));
                            changed = true;
                        }
                    }
                }
            }
        }
        return changed;
    }

    bool PaintBlade(Vector2 uv, Quaternion toolRotation)
    {
        int cX = (int)(uv.x * textureSize);
        int cY = (int)(uv.y * textureSize);
        bool changed = false;
        
        Vector3 bladeDir = toolRotation * Vector3.right; 
        float halfWidth = bladeWidth / 2f;
        int searchRange = (int)bladeWidth;

        for (int x = cX - searchRange; x <= cX + searchRange; x++)
        {
            for (int y = cY - searchRange; y <= cY + searchRange; y++)
            {
                if (x >= 0 && x < textureSize && y >= 0 && y < textureSize)
                {
                    Vector2 dirToPixel = new Vector2(x - cX, y - cY);
                    float projection = Vector2.Dot(dirToPixel, new Vector2(bladeDir.x, bladeDir.y));
                    float distFromLine = Vector2.Distance(dirToPixel, new Vector2(bladeDir.x, bladeDir.y) * projection);

                    if (Mathf.Abs(projection) < halfWidth && distFromLine < (bladeHeight / 2f))
                    {
                        Color currentColor = maskTexture.GetPixel(x, y);
                        if (currentColor.r > 0.3f && currentColor.r < 0.7f)
                        {
                            maskTexture.SetPixel(x, y, Color.white);
                            changed = true;
                        }
                    }
                }
            }
        }
        return changed;
    }
   
    public float GetCleanPercentage()
    {
        if (maskTexture == null) return 0f;

        Color[] pixels = maskTexture.GetPixels();
        int cleanPixelCount = 0;

        // We check the Red channel. 
        // Dirt = 0.0, Soap = 0.5, Clean = 1.0
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].r > 0.8f) 
            {
                cleanPixelCount++;
            }
        }

        return (float)cleanPixelCount / pixels.Length;
    }
}