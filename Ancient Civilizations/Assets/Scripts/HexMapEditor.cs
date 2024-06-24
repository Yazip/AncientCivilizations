using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{

    public Color[] colors;

    public HexGrid hexGrid;

    private Color activeColor;

    int activeElevation;

    void Awake()
    {
        SelectColor(0);
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
    }

    // Метод для пуска на сцену луча из позиции курсора мыши
    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            EditCell(hexGrid.GetCell(hit.point));
        }
    }

    // Метод для изменения ячейки
    void EditCell(HexCell cell)
    {
        cell.Color = activeColor;
        cell.Elevation = activeElevation;
        //hexGrid.Refresh();
    }

    // Метод для выбора активного цвета
    public void SelectColor(int index)
    {
        activeColor = colors[index];
    }

    // Метод для выбора активной высоты
    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }
}
