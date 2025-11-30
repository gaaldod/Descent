using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreepyDoorTextureGenerator
{
    private const int TextureWidth = 1024;
    private const int TextureHeight = 2048;
    private const string OutputDirectory = "Assets/Textures/CreepyDoor";
    private const string OutputFileName = "CreepyDoor.png";

    [MenuItem("Tools/Generate Creepy Door Texture")]
    public static void Generate()
    {
        Directory.CreateDirectory(OutputDirectory);
        string outputPath = Path.Combine(OutputDirectory, OutputFileName);

        var texture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false)
        {
            name = "CreepyDoor"
        };

        PaintWoodBase(texture);
        AddBloodStreaks(texture);
        AddEdgeBurn(texture);
        texture.Apply();

        File.WriteAllBytes(outputPath, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(outputPath);

        if (AssetImporter.GetAtPath(outputPath) is TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.sRGBTexture = true;
            importer.SaveAndReimport();
        }

        Debug.Log($"Creepy door texture written to {outputPath}");
    }

    private static void PaintWoodBase(Texture2D texture)
    {
        for (int y = 0; y < TextureHeight; y++)
        {
            float v = (float)y / TextureHeight;
            for (int x = 0; x < TextureWidth; x++)
            {
                float u = (float)x / TextureWidth;

                float grain = Mathf.PerlinNoise(u * 6f, v * 55f);
                float knots = Mathf.PerlinNoise(u * 1.8f + 3f, v * 4f + 1f);

                float baseShade = Mathf.Lerp(0.07f, 0.18f, grain);
                float woodTone = baseShade + knots * 0.12f;

                Color color = new Color(
                    woodTone + 0.02f,
                    woodTone * 0.55f,
                    woodTone * 0.35f,
                    1f);

                float panel = Mathf.Abs(Mathf.Repeat(u * 4.5f, 1f) - 0.5f) * 2f;
                if (panel > 0.92f)
                {
                    color *= 0.45f;
                }

                texture.SetPixel(x, y, color);
            }
        }
    }

    private static void AddBloodStreaks(Texture2D texture)
    {
        var random = new System.Random(1337);

        for (int i = 0; i < 42; i++)
        {
            int startX = random.Next(0, TextureWidth);
            int length = random.Next(TextureHeight / 4, TextureHeight / 2);
            int thickness = random.Next(2, 6);

            Color smearColor = new Color(
                0.3f + (float)random.NextDouble() * 0.25f,
                0.01f,
                0.01f);

            for (int j = 0; j < length; j++)
            {
                int y = Mathf.Clamp((int)(TextureHeight * 0.15f) + j, 0, TextureHeight - 1);
                float wobble = Mathf.Sin((i * 7 + j) * 0.035f) * 5f;
                int x = Mathf.Clamp(startX + Mathf.RoundToInt(wobble), 0, TextureWidth - 1);

                for (int t = -thickness; t <= thickness; t++)
                {
                    int px = Mathf.Clamp(x + t, 0, TextureWidth - 1);
                    float falloff = 1f - Mathf.Abs(t) / (thickness + 0.001f);

                    Color baseColor = texture.GetPixel(px, y);
                    Color target = Color.Lerp(baseColor, smearColor, 0.85f);
                    texture.SetPixel(px, y, Color.Lerp(baseColor, target, falloff));
                }
            }
        }
    }

    private static void AddEdgeBurn(Texture2D texture)
    {
        for (int y = 0; y < TextureHeight; y++)
        {
            float v = Mathf.Abs((float)y / TextureHeight - 0.5f) * 2f;
            for (int x = 0; x < TextureWidth; x++)
            {
                float u = Mathf.Abs((float)x / TextureWidth - 0.5f) * 2f;
                float falloff = Mathf.Clamp01(1f - (u * 0.6f + v * 0.4f));
                Color baseColor = texture.GetPixel(x, y);
                texture.SetPixel(x, y, Color.Lerp(baseColor * 0.7f, baseColor, falloff));
            }
        }
    }
}

