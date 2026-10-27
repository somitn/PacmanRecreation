using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }

    private int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };

    [SerializeField] private Vector2 topLeftWorld = new Vector2(-6, 4);

    public const int Rows = 29;
    public const int Cols = 28;

    private int quadrantRows;
    private int quadrantCols;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        quadrantRows = levelMap.GetLength(0);
        quadrantCols = levelMap.GetLength(1);
    }

    public int GetTile(int row, int col)
    {
        if (row < 0 || row >= Rows) return -1;
        if (col < 0 || col >= Cols) return -1;

        int quadCol = col;
        if (col >= quadrantCols)
            quadCol = (Cols - 1) - col;

        int quadRow = row;
        if (row >= quadrantRows)
            quadRow = (Rows - 1) - row;

        if (quadRow < 0 || quadRow >= quadrantRows) return -1;
        if (quadCol < 0 || quadCol >= quadrantCols) return -1;

        return levelMap[quadRow, quadCol];
    }

    public bool IsWalkable(int row, int col)
    {
        if (row < 0 || row >= Rows) return false;
        if (col < 0 || col >= Cols) return false;

        int tile = GetTile(row, col);
        return tile == 0 || tile == 5 || tile == 6;
    }

    public bool IsTunnel(int row, int col)
    {
        if (row != 14 && row != 15) return false;
        if (col < 0 || col >= Cols) return true;
        return false;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int col = Mathf.RoundToInt(worldPos.x - topLeftWorld.x);
        int row = Mathf.RoundToInt(topLeftWorld.y - worldPos.y);
        return new Vector2Int(col, row);
    }

    public Vector3 GridToWorld(int row, int col)
    {
        float x = topLeftWorld.x + col;
        float y = topLeftWorld.y - row;
        return new Vector3(x, y, 0f);
    }
}