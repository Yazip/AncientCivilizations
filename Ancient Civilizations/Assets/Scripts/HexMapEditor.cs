using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{

    public HexGrid hexGrid;

    int activeElevation;

    bool applyElevation = true;

    int brushSize;

    int activeTerrainTypeIndex;

    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell, searchFromCell, searchToCell;

    public Material terrainMaterial;

    bool editMode;

    public HexUnit unitPrefab;

    enum OptionalToggle
    {
        Ignore, Yes, No
    }

    void Awake()
    {
        terrainMaterial.DisableKeyword("GRID_ON");
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
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }
            if (editMode)
            {
                EditCells(currentCell);
            }
            else if (Input.GetKey(KeyCode.LeftShift) && searchToCell != currentCell)
            {
                if (searchFromCell != currentCell)
                {
                    if (searchFromCell)
                    {
                        searchFromCell.DisableHighlight();
                    }
                    searchFromCell = currentCell;
                    searchFromCell.EnableHighlight(Color.blue);
                    if (searchToCell)
                    {
                        hexGrid.FindPath(searchFromCell, searchToCell, 10);
                    }
                }
            }
            else if (searchFromCell && searchFromCell != currentCell)
            {
                if (searchToCell != currentCell)
                {
                    searchToCell = currentCell;
                    hexGrid.FindPath(searchFromCell, searchToCell, 10);
                }
            }
            previousCell = currentCell;
            isDrag = true;
        }
        else
        {
            previousCell = null;
        }
    }

    // Метод для изменения ячеек
    void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

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

    // Метод для проверки перетаскивания
    void ValidateDrag(HexCell currentCell)
    {
        for (
            dragDirection = HexDirection.NE;
            dragDirection <= HexDirection.NW;
            dragDirection++
        )
        {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
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

    // Метод для включения/выключения режима редактирования
    public void SetEditMode(bool toggle)
    {
        editMode = toggle;
        hexGrid.ShowUI(!toggle);
    }

    // Метод для получения ячейки под курсором при нажатии
    HexCell GetCellUnderCursor()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            return hexGrid.GetCell(hit.point);
        }
        return null;
    }

    // Метод для создания юнита
    void CreateUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && !cell.Unit)
        {
            HexUnit unit = Instantiate(unitPrefab);
            unit.transform.SetParent(hexGrid.transform, false);
            unit.Location = cell;
            unit.Orientation = Random.Range(0f, 360f);
        }
    }

    // Метод для уничтожения юнита
    void DestroyUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && cell.Unit)
        {
            cell.Unit.Die();
        }
    }
}
