using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitCellManager : MonoBehaviour
{
    public bool showGridLines = false;
    public int debugCellCount = 30;
    public float debugCellWidth = 5f;


    private static int cellCount;
    public static float cellWidth;
    private static Cell[,] Cells;
    public static Cell GetCell (Vector3 pos)
    {
        int x = Mathf.CeilToInt((pos.x/cellWidth + cellCount/2f));
        int y = Mathf.CeilToInt((pos.z/cellWidth + cellCount/2f));

        return Cells[x, y];
    }

    public static Cell GetCell (int x, int y)
    {
        if (x >= Cells.GetLength(0))
            return null;

        if (x < 0 || y < 0)
            return null;

        if (y >= Cells.GetLength(1))
            return null;

        return Cells[x, y];
    }

    private void Awake()
    {
        cellCount = debugCellCount;
        cellWidth = debugCellWidth;

        SpawnCells();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        cellCount = debugCellCount;
        cellWidth = debugCellWidth;

        //foreach (Cell cell in Cells)
        //{
        //    if (cell.unitsInCell.Count > 0)
        //        Debug.Log(cell.pos + " holds " + cell.unitsInCell.Count + " units");
        //}
    }

    private void SpawnCells ()
    {
        Cells = new Cell[debugCellCount, debugCellCount];
        for (int x = 0; x < Cells.GetLength(0); x++)
        {
            for (int y = 0; y < Cells.GetLength(1); y++)
            {
                Cells[x, y] = new Cell(new Vector2Int (x, y), new List<Identifier>(0));
            }
        }
    }

    /// <summary>
    /// returns all cells within square area from bottomLeftIndex to topRightIndex
    /// </summary>
    public static Cell[] GetCells(Vector2Int bottomLeftIndex, Vector2Int topRightIndex)
    {
        int size = (topRightIndex.x - bottomLeftIndex.x) + 1; // it's always a square, so you don't care about the y

        Cell[] cells = new Cell[size*size + 4];

        Vector2Int startCoord = new Vector2Int((topRightIndex.x + bottomLeftIndex.x) / 2, (topRightIndex.y + bottomLeftIndex.y) / 2);

        // checks orders the cells into the array starting from the middle and spiraling outward
        // I like this better because then I can stop when I find an enemy and don't have to loop through more cells than I have to
        int i = 0;
        for (int k = 0; k < cells.Length; k++)
        {
            for (int n = 0; n <= k; n++)
            {
                int x = startCoord.x - k + n;
                int y = startCoord.y - n;
                cells[i] = GetCell(x, y);
                i++;
                if (i >= cells.Length)
                    return cells;
            }
            for (int n = 1; n <= k; n++)
            {
                int x = startCoord.x + n;
                int y = startCoord.y - k + n;
                cells[i] = GetCell(x, y);
                i++;
                if (i >= cells.Length)
                    return cells;
            }
            for (int n = 1; n <= k; n++)
            {
                int x = startCoord.x + k - n;
                int y = startCoord.y + n;
                cells[i] = GetCell(x, y);
                i++;
                if (i >= cells.Length)
                    return cells;
            }
            for (int n = 1; n <= k - 1; n++)
            {
                int x = startCoord.x - n;
                int y = startCoord.y + k - n;
                cells[i] = GetCell(x, y);
                i++;
                if (i >= cells.Length)
                    return cells;
            }

            if (i >= cells.Length)
                break;
        }


        //for (int x = 0; x < size; x++)
        //{
        //    for (int y = 0; y < size; y++)
        //    {
        //        cells[x*size + y] = GetCell(bottomLeftIndex.x + x, bottomLeftIndex.y + y);
        //    }
        //}

        return cells;
    }

    public static void UpdateActiveCell(Identifier identifier, Vector3 pos, ref Cell lastCell)
    {
        Cell activeCell = UnitCellManager.GetCell(pos);
        if (lastCell == null || lastCell != activeCell)
        {
            if (lastCell != null)
                lastCell.unitsInCell.Remove(identifier);

            activeCell.unitsInCell.Add(identifier);
            lastCell = activeCell;
        }
    }

    private void OnDrawGizmosSelected()
    {
        //Vector3 pos = transform.position / debugCellWidth;
        //Gizmos.DrawCube(new Vector3(Mathf.CeilToInt(pos.x) - debugCellWidth/4f, 1f, Mathf.CeilToInt(pos.z) - debugCellWidth/4f) * debugCellWidth, Vector3.one * debugCellWidth);

        if (!showGridLines)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(debugCellCount * debugCellWidth, 1f, 0.5f));
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.5f, 1f, debugCellCount * debugCellWidth));

        Gizmos.color = Color.blue;
        for (int x = 0; x < debugCellCount; x++)
        {
            for (int y = 0; y < debugCellCount; y++)
            {
                //Vector3 pos = new Vector3 ()
                Gizmos.DrawWireCube(new Vector3(x * debugCellWidth - debugCellCount/2f* debugCellWidth + debugCellWidth / 2f, 0f, y * debugCellWidth - debugCellCount / 2f* debugCellWidth + debugCellWidth / 2f), Vector3.one * debugCellWidth);
                //Gizmos.DrawWireCube(new Vector3(x * debugCellWidth - debugCellCount/2f*debugCellWidth + debugCellWidth/2f, 0, y * debugCellWidth - debugCellCount / 2f*debugCellWidth + debugCellWidth / 2f), new Vector3(debugCellWidth, debugCellWidth, debugCellWidth));
            }
        }
    }
}


public class Cell
{
    public Vector2Int pos;
    public List<Identifier> unitsInCell;

    public Cell (Vector2Int pos, List<Identifier> unitsInCell)
    {
        this.pos = pos;
        this.unitsInCell = unitsInCell;
    }
}