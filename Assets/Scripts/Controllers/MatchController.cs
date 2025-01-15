using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class MatchController : Singleton<MatchController>
{
    [SerializeField] private GameGrid _gameGrid;
    
    private bool[,] _visitedCells;
    private int _minimumNumberOfSameColors = 2;
    
    /*Inject]
    void Construct(GameGrid gameGrid)
    {
        _gameGrid = gameGrid;
    }*/

    void Start()
    {
        _visitedCells = new bool[_gameGrid.colums, _gameGrid.rows];
    }

    public List<Cell> FindMatches(Cell cell, MatchType matchType)
    {
        var matchedItems = new List<Cell>();
        ResetVisitedCells();
        FindMatchesRecursively(cell, matchType, matchedItems);
    
        return matchedItems;
    }
    
    private void ResetVisitedCells()
    {
        for (int x = 0; x < _visitedCells.GetLength(0); x++)
        {
            for (int y = 0; y < _visitedCells.GetLength(1); y++)
            {
                _visitedCells[x, y] = false;
            }
        }
    }
    private void FindMatchesRecursively(Cell cell, MatchType matchType, List<Cell> matchedItems) //Flood Fill Algorithm that uses DFS.
    {
        if (cell == null) return; //don't search if it is null
        
        var x = cell.x;
        var y = cell.y;

        if (_visitedCells[x, y]) return;

        if (cell.item != null && cell.item.GetMatchType() == matchType && cell.item.GetMatchType() != MatchType.None)
        {
            _visitedCells[x, y] = true;
            matchedItems.Add(cell);

            if (!cell.item.canClickable) return;

            var neighbours = cell.neighbours;
            if (neighbours.Count == 0) return;

            for (int i = 0; i < neighbours.Count; i++)
            {
                FindMatchesRecursively(neighbours[i], matchType, matchedItems);
            }
        }
    }

    public int CountMatchedNormalItems(List<Cell> cells)
    {
        int _count = 0;
        foreach(Cell cell in cells)
        {
            if(cell.item.canClickable)
                _count++;
        }

        return _count;
    }

    public void ExplodeMatchingCells(Cell cell)
    {
        var previousCells = new List<Cell>();

        var validItems = FindMatches(cell, cell.item.GetMatchType());
        var validNormalItems = CountMatchedNormalItems(validItems);

        if (validNormalItems < _minimumNumberOfSameColors) return;

        for (int i = 0; i < validItems.Count; i++)
        {
            var explodedCell = validItems[i];
            
            ExplodeValidCellsInNeighbours(explodedCell, previousCells);
            
            var item = explodedCell.item;
            item.Execute();
        }
        
       // _ = MovesManager.Instance.DecreaseMovesAsync();
    }

    private void ExplodeValidCellsInNeighbours(Cell cell, List<Cell> previousCells)
    {
        var explodedCellsInNeighbours = cell.neighbours;

        for (int j = 0; j < explodedCellsInNeighbours.Count; j++)
        {
            var neighbourCell = explodedCellsInNeighbours[j];
            var neighbourCellItem = neighbourCell.item;

            if (neighbourCellItem != null && !previousCells.Contains(neighbourCell))
            {
                previousCells.Add(neighbourCell);

                if (neighbourCellItem.canExplode)
                {
                    neighbourCellItem.Execute();
                }
            }
        }
    }
}
