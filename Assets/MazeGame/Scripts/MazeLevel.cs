using UnityEngine;

/// <summary>
/// The level layout, drawn as ASCII art. Edit the rows below and rebuild the
/// scene from the menu: Tools > Maze Game > Build Level Scene.
/// '#' is a wall, '.' is floor, 'P' is the player start, 'G' is the goal.
/// </summary>
public static class MazeLevel
{
    public const char Wall = '#';
    public const char Floor = '.';
    public const char PlayerStart = 'P';
    public const char Goal = 'G';

    public static readonly string[] Layout =
    {
        "#####################",
        "#P..#...#...#.....#.#",
        "###.#.#.#.#.###.#.#.#",
        "#.#.#.....#.#...#...#",
        "#.#.#.###.#.#.#####.#",
        "#.#.....#...#.....#.#",
        "#.#.###...#######.#.#",
        "#.#.....#.#.....#.#.#",
        "#.#######.#.###.#.#.#",
        "#.......#.#.#.....#.#",
        "#.#####.#.#.#######.#",
        "#.#...#...#...#.....#",
        "#.###.#######.#.#####",
        "#.............#....G#",
        "#####################",
    };

    public static int Rows => Layout.Length;
    public static int Columns => Layout[0].Length;

    /// <summary>Grid position of the first cell holding <paramref name="marker"/>, or (-1, -1).</summary>
    public static Vector2Int Find(char marker)
    {
        for (int y = 0; y < Layout.Length; y++)
        {
            int x = Layout[y].IndexOf(marker);
            if (x >= 0) return new Vector2Int(x, y);
        }
        return new Vector2Int(-1, -1);
    }
}
