using UnityEngine;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour
{

    [SerializeField]
    HexGridChunk chunkPrefab;

    HexGridChunk[] chunks;

    [SerializeField]
    int cellCountX = 20, cellCountZ = 15;

    int chunkCountX, chunkCountZ;

    [SerializeField]
    HexCell cellPrefab;

    HexCell[] cells;

    [SerializeField]
    Texture2D noiseSource;

    [SerializeField]
    Color[] colors;

    HexCellPriorityQueue searchFrontier;

    int searchFrontierPhase;

    HexCell currentPathFrom, currentPathTo;
    bool currentPathExists;

    List<HexUnit> units = new List<HexUnit>();

    [SerializeField]
    HexUnit unitPrefab;

    public int CellCountX
    {
        get
        {
            return cellCountX;
        }
    }

    public int CellCountZ
    {
        get
        {
            return cellCountZ;
        }
    }

    public bool HasPath
    {
        get
        {
            return currentPathExists;
        }
    }

    public List<HexUnit> Units
    {
        get
        {
            return units;
        }
    }

    public int RedUnitsCount { get; set; }

    public int BlueUnitsCount { get; set; }

    void Awake()
    {
        HexMetrics.NoiseSource = noiseSource;
        HexUnit.UnitPrefab = unitPrefab;
        HexMetrics.Colors = colors;
        CreateMap(CellCountX, CellCountZ);
    }

    void OnEnable()
    {
        if (!HexMetrics.NoiseSource)
        {
            HexMetrics.NoiseSource = noiseSource;
            HexUnit.UnitPrefab = unitPrefab;
            HexMetrics.Colors = colors;
        }
    }

    // Метод для создания карты
    public bool CreateMap(int x, int z)
    {
        if (
            x <= 0 || x % HexMetrics.chunkSizeX != 0 ||
            z <= 0 || z % HexMetrics.chunkSizeZ != 0
        )
        {
            Debug.LogError("Unsupported map size.");
            return false;
        }
        ClearPath();
        ClearUnits();
        if (chunks != null)
        {
            for (int i = 0; i < chunks.Length; i++)
            {
                Destroy(chunks[i].gameObject);
            }
        }

        cellCountX = x;
        cellCountZ = z;
        chunkCountX = CellCountX / HexMetrics.chunkSizeX;
        chunkCountZ = CellCountZ / HexMetrics.chunkSizeZ;
        CreateChunks();
        CreateCells();
        return true;
    }

    // Метод для создания фрагментов
    void CreateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for (int z = 0, i = 0; z < chunkCountZ; z++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
            }
        }
    }

    // Метод для создания ячеек
    void CreateCells()
    {
        cells = new HexCell[CellCountZ * CellCountX];

        for (int z = 0, i = 0; z < CellCountZ; z++)
        {
            for (int x = 0; x < CellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    // Метод для создания ячейки

    void CreateCell(int x, int z, int i)
    {
        
        // Вычисляем позицию ячейки

        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        // Создаём ячейку

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.localPosition = position;
        cell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        // Соединение соседей

        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }
        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - CellCountX]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - CellCountX - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - CellCountX]);
                if (x < CellCountX - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - CellCountX + 1]);
                }
            }
        }

        cell.Elevation = 0; // Задаём высоту ячейки

        AddCellToChunk(x, z, cell);
    }

    // Метод для добавления ячейки во фрагмент
    void AddCellToChunk(int x, int z, HexCell cell)
    {
        int chunkX = x / HexMetrics.chunkSizeX;
        int chunkZ = z / HexMetrics.chunkSizeZ;
        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = x - chunkX * HexMetrics.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }

    // Метод для получения ячейки
    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * CellCountX + coordinates.Z / 2;
        return cells[index];
    }

    public HexCell GetCell(HexCoordinates coordinates)
    {
        int z = coordinates.Z;
        if (z < 0 || z >= CellCountZ)
        {
            return null;
        }
        int x = coordinates.X + z / 2;
        if (x < 0 || x >= CellCountX)
        {
            return null;
        }
        return cells[x + z * CellCountX];
    }

    public HexCell GetCell(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return GetCell(hit.point);
        }
        return null;
    }

    // Метод для нахождения расстояний до ячеек
    public void FindPath(HexCell fromCell, HexCell toCell, int speed)
    {
        ClearPath();
        currentPathFrom = fromCell;
        currentPathTo = toCell;
        currentPathExists = Search(fromCell, toCell, speed);
    }

    bool Search(HexCell fromCell, HexCell toCell, int speed)
    {
        searchFrontierPhase += 2;
        if (searchFrontier == null)
        {
            searchFrontier = new HexCellPriorityQueue();
        }
        else
        {
            searchFrontier.Clear();
        }

        fromCell.SearchPhase = searchFrontierPhase;
        fromCell.Distance = 0;
        searchFrontier.Enqueue(fromCell);
        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();
            current.SearchPhase += 1;

            if (current == toCell)
            {
                return true;
            }

            int currentTurn = (current.Distance - 1) / speed;

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase)
                {
                    continue;
                }
                if (neighbor.Unit)
                {
                    continue;
                }
                HexEdgeType edgeType = current.GetEdgeType(neighbor);
                if (edgeType == HexEdgeType.Cliff)
                {
                    continue;
                }
                int moveCost;
                moveCost = edgeType == HexEdgeType.Flat ? 1 : 5;
                int distance = current.Distance + moveCost;
                int turn = (distance - 1) / speed;
                if (turn > currentTurn)
                {
                    distance = turn * speed + moveCost;
                }
                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    neighbor.SearchPhase = searchFrontierPhase;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic =
                        neighbor.Coordinates.DistanceTo(toCell.Coordinates);
                    searchFrontier.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
                {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    searchFrontier.Change(neighbor, oldPriority);
                }
            }
        }
        return false;
    }

    // Метод для очистки пути
    public void ClearPath()
    {
        if (currentPathExists)
        {
            HexCell current = currentPathTo;
            while (current != currentPathFrom)
            {
                current = current.PathFrom;
            }
            currentPathExists = false;
        }
        currentPathFrom = currentPathTo = null;
    }

    // Метод для очистки карты от юнитов
    public void ClearUnits()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].Die();
        }
        units.Clear();
    }

    // Метод для добавления юнита к сетке
    public void AddUnit(HexUnit unit, HexCell location, float orientation, int health, int unitTeamIndex)
    {
        units.Add(unit);
        unit.transform.SetParent(transform, false);
        unit.Location = location;
        unit.Orientation = orientation;
        unit.Health = health;
        unit.TeamIndex = unitTeamIndex;
        if (unitTeamIndex == 0)
        {
            ++RedUnitsCount;
        }
        else if (unitTeamIndex == 1)
        {
            ++BlueUnitsCount;
        }
    }

    // Метод для удаления юнита из сетки
    public void RemoveUnit(HexUnit unit)
    {
        if (unit.TeamIndex == 0)
        {
            --RedUnitsCount;
        }
        else if (unit.TeamIndex == 1)
        {
            --BlueUnitsCount;
        }
        units.Remove(unit);
        unit.Die();
    }

    // Метод для получения пути
    public List<HexCell> GetPath()
    {
        if (!currentPathExists)
        {
            return null;
        }
        List<HexCell> path = ListPool<HexCell>.Get();
        for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom)
        {
            path.Add(c);
        }
        path.Add(currentPathFrom);
        path.Reverse();
        return path;
    }
}
