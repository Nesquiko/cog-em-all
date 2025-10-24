using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(RawImage))]
public class OperationPreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private string levelFileName = "testing-level.json";

    [Header("Texture Settings")]
    [SerializeField] private int textureSize = 300;
    [SerializeField] private Color backgroundColor = new(0, 0, 0, 0);
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private int lineThickness = 2;
    [SerializeField] private float padding = 0f;

    private RawImage rawImage;
    private Texture2D texture;
    private SplineContainer tempSplineContainer;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        LoadSplineFromJson();
        GenerateTexture();
    }

    private void LoadSplineFromJson()
    {
        string fullPath = Path.Combine(Application.dataPath, "Levels", levelFileName);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"Cannot find level file: {fullPath}");
            return;
        }

        string json = File.ReadAllText(fullPath);
        SerializableLevel loaded = SerializableLevel.FromJson(json);
        if (loaded == null || loaded.pathSplines == null)
        {
            Debug.LogError("Failed to parse level JSON or no pathSplines found");
        }

        tempSplineContainer = gameObject.AddComponent<SplineContainer>();
        foreach (var s in loaded.pathSplines)
            tempSplineContainer.AddSpline(s);
    }

    public void GenerateTexture()
    {
        texture = new(textureSize, textureSize, TextureFormat.RGBA32, false); ;
        ClearTexture();

        List<Vector2> points = SampleSplinePoints2D(tempSplineContainer, 200, padding);
        DrawPolyline(points);

        texture.Apply();
        rawImage.texture = texture;
    }

    private void ClearTexture()
    {
        Color32[] pixels = new Color32[textureSize *  textureSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = backgroundColor;
        texture.SetPixels32(pixels);
    }

    private List<Vector2> SampleSplinePoints2D(SplineContainer container, int samples, float pad)
    {
        Vector2 min = new(float.MaxValue, float.MaxValue);
        Vector2 max = new(float.MinValue, float.MinValue);

        foreach (var spline in container.Splines)
        {
            for (int i = 0; i <= samples; i++)
            {
                Vector3 p = spline.EvaluatePosition(i / (float)samples);

                Vector3 rotated = new(p.x, p.z, -p.y);
                Vector2 projected = new(rotated.x, rotated.y);

                min = Vector2.Min(min, projected);
                max = Vector2.Max(max, projected);
            }
        }

        Vector2 range = max - min;
        min -= range * pad;
        max += range * pad;

        List<Vector2> pts = new();
        foreach (var spline in container.Splines)
        {
            for (int i = 0; i <= samples; i++)
            {
                Vector3 p = spline.EvaluatePosition(i / (float)samples);

                // same projection logic
                Vector3 rotated = new Vector3(p.x, p.z, -p.y);
                Vector2 p2 = new(rotated.x, rotated.y);

                float nx = Mathf.InverseLerp(min.x, max.x, p2.x);
                float ny = 1f - Mathf.InverseLerp(min.y, max.y, p2.y);
                pts.Add(new Vector2(nx, ny));
            }
        }

        return pts;
    }

    private void DrawPolyline(List<Vector2> pts)
    {
        for (int i = 1; i < pts.Count; i++) DrawLine(pts[i - 1], pts[i]);
    }

    private void DrawLine(Vector2 a, Vector2 b)
    {
        int x0 = Mathf.RoundToInt(a.x * (textureSize - 1));
        int y0 = Mathf.RoundToInt(a.y * (textureSize - 1));
        int x1 = Mathf.RoundToInt(b.x * (textureSize - 1));
        int y1 = Mathf.RoundToInt(b.y * (textureSize - 1));

        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawThickPixel(x0, y0);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    private void DrawThickPixel(int x, int y)
    {
        for (int dx = -lineThickness; dx <= lineThickness; dx++)
            for (int dy = -lineThickness; dy <= lineThickness; dy++)
            {
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && nx < textureSize && ny >= 0 && ny < textureSize)
                    texture.SetPixel(nx, ny, lineColor);
            }
    }
}
