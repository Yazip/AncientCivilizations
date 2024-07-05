using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{

    [SerializeField]
    HexGrid hexGrid;

    int activeElevation;

    bool applyElevation = true;

    int brushSize;

    int activeTerrainTypeIndex;

    int activeUnitTeamIndex;

    HexCell previousCell;

    [SerializeField]
    Material terrainMaterial;

    void Awake()
    {
        terrainMaterial.DisableKeyword("GRID_ON");
        SetEditMode(true);
    }

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButton(0))
            {
                HandleInput();
                return;
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    DestroyUnit();
                }
                else
                {
                    CreateUnit();
                }
                return;
            }
        }
        previousCell = null;
    }

    // Метод для пуска на сцену луча из позиции курсора мыши
    void HandleInput()
    {
        HexCell currentCell = GetCellUnderCursor();
        if (currentCell)
        {
            EditCells(currentCell);
            previousCell = currentCell;
        }
        else
        {
            previousCell = null;
        }
    }

    // Метод для изменения ячеек
    void EditCells(HexCell center)
    {
        int centerX = center.Coordinates.X;
        int centerZ = center.Coordinates.Z;

        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    // Метод для изменения ячейки
    void EditCell(HexCell cell)
    {
        if (cell)
        {
            if (activeTerrainTypeIndex >= 0)
            {
                cell.TerrainTypeIndex = activeTerrainTypeIndex;
            }
            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
        }
    }

    // Метод для выбора активного типа рельефа
    public void SetTerrainTypeIndex(int index)
    {
        activeTerrainTypeIndex = index;
    }

    // Метод для выбора активной высоты
    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    // Метод для выбора возможности задания высоты
    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    // Метод для выбора размера кисти
    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    // Метод для включения/выключения сетки
    public void ShowGrid(bool visible)
    {
        if (visible)
        {
            terrainMaterial.EnableKeyword("GRID_ON");
        }
        else
        {
            terrainMaterial.DisableKeyword("GRID_ON");
        }
    }

    // Метод для выбора активной фракции юнита
    public void SetUnitTeamIndex(int index)
    {
        activeUnitTeamIndex = index;
    }

    // Метод для включения/выключения режима редактирования
    public void SetEditMode(bool toggle)
    {
        enabled = toggle;
    }

    // Метод для получения ячейки под курсором при нажатии
    HexCell GetCellUnderCursor()
    {
        return hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
    }

    // Метод для создания юнита
    void CreateUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && !cell.Unit)
        {
            hexGrid.AddUnit(Instantiate(HexUnit.UnitPrefab), cell, HexMapCamera.RotationAngle, 5, activeUnitTeamIndex);
        }
    }

    // Метод для уничтожения юнита
    void DestroyUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && cell.Unit)
        {
            hexGrid.RemoveUnit(cell.Unit);
        }
    }
}
