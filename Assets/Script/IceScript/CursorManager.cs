using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D normalCursor;
    public Texture2D clickCursor;
    public Vector2 hotspot = Vector2.zero;

    private Texture2D resizedCursor;
    private Texture2D resizedClickCursor;

    void Start()
    {
        resizedCursor = ResizeTexture(normalCursor, 32, 32); 
        resizedClickCursor = ResizeTexture(clickCursor, 32, 32);
        SetNormalCursor();
    }

    void Update()
    {
        // Check if either mouse button is pressed down
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            SetClickCursor();
        }
        // Check if both mouse buttons are released
        if ((Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1)) || 
            (Input.GetMouseButtonUp(1) && !Input.GetMouseButton(0)))
        {
            SetNormalCursor();
        }
    }

    public void SetNormalCursor()
    {
        Cursor.SetCursor(resizedCursor, hotspot, CursorMode.Auto);
    }

    public void SetClickCursor()
    {
        Cursor.SetCursor(resizedClickCursor, hotspot, CursorMode.Auto);
    }

    private Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(source, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D result = new Texture2D(width, height, source.format, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }
}