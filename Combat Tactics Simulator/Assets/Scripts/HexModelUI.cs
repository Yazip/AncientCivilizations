using UnityEngine;
using UnityEngine.EventSystems;

public class HexModelUI : MonoBehaviour
{
    
    public HexGrid grid;

    HexCell currentCell;

    HexUnit selectedUnit;

    HexCell enemyCell;

    bool isEnemyCell;

    bool isEnemyNeighbor;

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                DoSelection();
            }
            else if (selectedUnit)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    if (isEnemyCell && enemyCell.Unit)
                    {
                        if (isEnemyNeighbor)
                        {
                            selectedUnit.AttackUnit(enemyCell.Unit);
                        }
                        else
                        {
                            DoMove(true);
                        }
                    }
                    else
                    {
                        DoMove();
                    }
                }
                else
                {
                    DoPathfinding();
                }
            }
        }
    }

    bool UpdateCurrentCell()
    {
        HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
        if (cell != currentCell)
        {
            currentCell = cell;
            return true;
        }
        return false;
    }

    // Метод для включения/отключения режима редактирования
    public void SetEditMode(bool toggle)
    {
        enabled = !toggle;
        grid.ShowUI(!toggle);
        grid.ClearPath();
    }

    // Метод для выбора юнита
    void DoSelection()
    {
        grid.ClearPath();
        UpdateCurrentCell();
        if (currentCell)
        {
            selectedUnit = currentCell.Unit;
        }
    }

    // Метод для нахождения пути
    void DoPathfinding()
    {
        if (UpdateCurrentCell())
        {
            if (currentCell)
            {
                if (selectedUnit.IsValidDestination(currentCell))
                {
                    isEnemyCell = false;
                    grid.FindPath(selectedUnit.Location, currentCell, 10);
                }
                else
                {
                    isEnemyCell = true;
                    enemyCell = currentCell;
                    grid.ClearPath();
                    for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                    {
                        if (currentCell == selectedUnit.Location.GetNeighbor(d))
                        {
                            isEnemyNeighbor = true;
                            return;
                        }
                    }
                    isEnemyNeighbor = false;
                    grid.FindPath(selectedUnit.Location, currentCell.GetNeighbor((HexDirection)(Random.Range(0, 5))), 10);
                }
            }
            else
            {
                grid.ClearPath();
            }
        }
    }

    // Метод для движения
    void DoMove(bool attack = false)
    {
        if (grid.HasPath)
        {
            if (attack)
            {
                selectedUnit.Travel(grid.GetPath(), enemyCell.Unit);
                isEnemyNeighbor = true;
            }
            else
            {
                selectedUnit.Travel(grid.GetPath());
            }
            grid.ClearPath();
        }
    }
}
