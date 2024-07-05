using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Model : MonoBehaviour
{

    [SerializeField]
    HexGrid grid;

    HexUnit selectedUnit;

    HexUnit selectedEnemy;

    HexCell enemyCell;

    bool isEnemyNeighbor;

    List<HexUnit> units;

    int redUnitsCount;

    int blueUnitsCount;

    private void Awake()
    {
        SetEditMode(true);
    }

    void Update()
    {
        units = grid.Units;
        units.Shuffle();
        redUnitsCount = grid.RedUnitsCount;
        blueUnitsCount = grid.BlueUnitsCount;
        if ((units.Count > 1) && (redUnitsCount != 0) && (blueUnitsCount != 0))
        {
            StopAllCoroutines();
            StartCoroutine(ModelCoroutine());
        }
    }

    IEnumerator ModelCoroutine()
    {
        for (int i = 0; i < units.Count; i++)
        {
            selectedUnit = units[i];
            if (selectedUnit && (selectedUnit.Opponent == null))
            {
                HexUnit targetEnemy = null;
                int minPathCount = int.MaxValue;
                List<HexCell> minPath = null;
                for (int j = 0; j < units.Count; j++)
                {
                    selectedEnemy = units[j];
                    if (selectedEnemy && (selectedUnit != selectedEnemy) && (selectedEnemy.Opponent == null) && (selectedUnit.TeamIndex != selectedEnemy.TeamIndex))
                    {
                        enemyCell = selectedEnemy.Location;
                        DoPathfinding();
                        if (isEnemyNeighbor)
                        {
                            targetEnemy = selectedEnemy;
                            selectedUnit.Opponent = targetEnemy;
                            targetEnemy.Opponent = selectedUnit;
                            break;
                        }
                        else if (grid.HasPath)
                        {
                            List<HexCell> path = grid.GetPath();
                            if (path.Count < minPathCount)
                            {
                                targetEnemy = selectedEnemy;
                                minPath = path;
                                minPathCount = path.Count;
                            }
                        }
                    }
                }
                if (targetEnemy && (minPath != null || isEnemyNeighbor))
                {
                    if (isEnemyNeighbor)
                    {
                        selectedUnit.AttackUnit(targetEnemy);
                        yield return null;
                    }
                    else
                    {
                        selectedUnit.Opponent = targetEnemy;
                        targetEnemy.Opponent = selectedUnit;
                        DoMove(minPath, targetEnemy);
                        yield return null;
                    }
                }
            }
        }
    }

    // Метод для включения/отключения режима редактирования
    public void SetEditMode(bool toggle)
    {
        enabled = !toggle;
        grid.ClearPath();
        if (toggle == true)
        {
            grid.ClearUnits();
        }
    }

    // Метод для нахождения пути
    void DoPathfinding()
    {
        grid.ClearPath();
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            HexCell neighbor = selectedUnit.Location.GetNeighbor(d);
            if ((enemyCell == neighbor) && (selectedUnit.Location.GetEdgeType(neighbor) != HexEdgeType.Cliff))
            {
                isEnemyNeighbor = true;
                return;
            }
        }
        isEnemyNeighbor = false;
        int neighborCount = 0;
        int neighborUnitsCount = 0;
        List<HexCell> freeNeighbors = new List<HexCell>();
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            HexCell neighbor = enemyCell.GetNeighbor(d);
            if (neighbor)
            {
                if (neighbor.Unit)
                {
                    ++neighborUnitsCount;
                }
                else if (enemyCell.GetEdgeType(neighbor) != HexEdgeType.Cliff)
                {
                    freeNeighbors.Add(neighbor);
                }
                ++neighborCount;
            }
        }
        if ((neighborUnitsCount != neighborCount) && (freeNeighbors.Count != 0))
        {
            HexCell toCell = freeNeighbors[Random.Range(0, (freeNeighbors.Count - 1))];
            grid.FindPath(selectedUnit.Location, toCell, 10);
        }
    }

    // Метод для движения
    void DoMove(List<HexCell> path, HexUnit enemy = null)
    {
        selectedUnit.Travel(path, enemy);
        grid.ClearPath();
    }
}
