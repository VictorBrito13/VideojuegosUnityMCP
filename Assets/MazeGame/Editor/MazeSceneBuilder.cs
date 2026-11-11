using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds the whole playable scene from <see cref="MazeLevel.Layout"/>:
/// camera, merged wall rectangles, the player circle and the goal.
/// Re-run it after editing the ASCII layout.
/// </summary>
public static class MazeSceneBuilder
{
    const string ScenePath = "Assets/Scenes/MazeLevel.unity";
    const string SpriteFolder = "Assets/MazeGame/Sprites";
    const string CirclePath = SpriteFolder + "/circle.png";
    const string SquarePath = SpriteFolder + "/square.png";

    const float CellSize = 1f;
    const float PlayerDiameter = 0.7f;
    const float GoalSize = 0.8f;

    static readonly Color BackgroundColor = new Color(0.08f, 0.09f, 0.12f);
    static readonly Color WallColor = new Color(0.33f, 0.40f, 0.56f);
    static readonly Color PlayerColor = new Color(1f, 0.78f, 0.24f);
    static readonly Color GoalColor = new Color(0.33f, 0.82f, 0.46f);

    [MenuItem("Tools/Maze Game/Build Level Scene")]
    public static void BuildScene()
    {
        Scene current = EditorSceneManager.GetActiveScene();
        if (current.isDirty)
        {
            if (string.IsNullOrEmpty(current.path))
            {
                Debug.LogError("The open scene has unsaved changes and no file on disk. Save it first, then rebuild.");
                return;
            }

            EditorSceneManager.SaveScene(current);
            Debug.Log($"Saved unsaved changes in {current.path} before building the maze scene.");
        }

        Sprite circle = EnsureSprite(CirclePath, 64, true);
        Sprite square = EnsureSprite(SquarePath, 32, false);
        if (circle == null || square == null)
        {
            Debug.LogError("Could not create the circle/square sprites. Aborting.");
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateWalls(square);
        CreatePlayer(circle);
        CreateGoal(square);
        new GameObject("GameManager").AddComponent<MazeGameManager>();

        EditorSceneManager.SaveScene(scene, ScenePath);
        RegisterInBuildSettings();

        Debug.Log($"Maze scene built: {MazeLevel.Columns}x{MazeLevel.Rows} grid saved to {ScenePath}");
    }

    static void CreateCamera()
    {
        var go = new GameObject("Main Camera") { tag = "MainCamera" };
        Camera camera = go.AddComponent<Camera>();
        camera.orthographic = true;
        // Fit the whole maze height on screen, with a little margin.
        camera.orthographicSize = MazeLevel.Rows * 0.5f * CellSize + 0.75f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = BackgroundColor;
        camera.farClipPlane = 100f;
        go.AddComponent<MazeCameraFit>();

        Vector3 center = CellToWorld((MazeLevel.Columns - 1) * 0.5f, (MazeLevel.Rows - 1) * 0.5f);
        go.transform.position = new Vector3(center.x, center.y, -10f);
    }

    static void CreateWalls(Sprite square)
    {
        Transform parent = new GameObject("Walls").transform;

        for (int y = 0; y < MazeLevel.Rows; y++)
        {
            string row = MazeLevel.Layout[y];
            int x = 0;
            while (x < row.Length)
            {
                if (row[x] != MazeLevel.Wall)
                {
                    x++;
                    continue;
                }

                // Merge each horizontal run of walls into one rectangle.
                int start = x;
                while (x < row.Length && row[x] == MazeLevel.Wall) x++;
                CreateWall(parent, square, start, y, x - start);
            }
        }
    }

    static void CreateWall(Transform parent, Sprite square, int startX, int y, int length)
    {
        var go = new GameObject($"Wall_{startX}_{y}");
        go.transform.SetParent(parent, false);
        go.transform.position = CellToWorld(startX + (length - 1) * 0.5f, y);
        go.transform.localScale = new Vector3(length * CellSize, CellSize, 1f);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = square;
        renderer.color = WallColor;

        go.AddComponent<BoxCollider2D>().size = Vector2.one;
    }

    static void CreatePlayer(Sprite circle)
    {
        Vector2Int cell = MazeLevel.Find(MazeLevel.PlayerStart);
        var go = new GameObject("Player");
        go.transform.position = CellToWorld(cell.x, cell.y);
        go.transform.localScale = Vector3.one * PlayerDiameter;

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = circle;
        renderer.color = PlayerColor;
        renderer.sortingOrder = 10;

        Rigidbody2D body = go.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        go.AddComponent<CircleCollider2D>().radius = 0.5f;
        go.AddComponent<PlayerController>();
    }

    static void CreateGoal(Sprite square)
    {
        Vector2Int cell = MazeLevel.Find(MazeLevel.Goal);
        var go = new GameObject("Goal");
        go.transform.position = CellToWorld(cell.x, cell.y);
        go.transform.localScale = Vector3.one * GoalSize;

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = square;
        renderer.color = GoalColor;
        renderer.sortingOrder = 5;

        BoxCollider2D trigger = go.AddComponent<BoxCollider2D>();
        trigger.size = Vector2.one;
        trigger.isTrigger = true;

        go.AddComponent<GoalZone>();
    }

    /// <summary>Grid cells run left-to-right and top-to-bottom, so Y is flipped.</summary>
    static Vector3 CellToWorld(float x, float y)
    {
        return new Vector3(x * CellSize, -y * CellSize, 0f);
    }

    static Sprite EnsureSprite(string path, int size, bool circle)
    {
        if (!AssetDatabase.IsValidFolder(SpriteFolder))
            AssetDatabase.CreateFolder("Assets/MazeGame", "Sprites");

        if (!File.Exists(path))
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float radius = size * 0.5f;
            var center = new Vector2(radius, radius);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    byte alpha = 255;
                    if (circle)
                    {
                        float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                        // One pixel of feathering so the circle edge is not jagged.
                        alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((radius - distance) * 255f), 0, 255);
                    }

                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }

        if (AssetImporter.GetAtPath(path) is TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            // One sprite unit per grid cell.
            importer.spritePixelsPerUnit = size;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void RegisterInBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(entry => entry.path == ScenePath)) return;

        // First in the list so the maze is what a build starts with.
        scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
