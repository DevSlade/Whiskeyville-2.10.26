// ============================================================================
// GRIDMANAGER.CS
// ============================================================================
// PURPOSE:      Creates and manages the tile grid using PREFABS with seeded RNG
// VERSION:      v5 — Removed tree offset (fixed via sprite pivot instead)
// UPDATED:      February 19, 2026
// ============================================================================

using UnityEngine;

public class GridManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static GridManager Instance { get; private set; }

    // ========================================================================
    // 📐 INSPECTOR - GRID SETTINGS
    // ========================================================================

    [Header("Grid Dimensions")]
    [SerializeField] private int _gridWidth = 20;
    [SerializeField] private int _gridHeight = 15;
    [SerializeField] private float _tileSize = 1f;

    // ========================================================================
    // 🌿 INSPECTOR - TILE PREFABS
    // ========================================================================

    [Header("Tile Prefabs")]
    [SerializeField] private GameObject _grassTilePrefab;
    [SerializeField] private GameObject _dirtTilePrefab;

    // ========================================================================
    // 🌲 INSPECTOR - OBSTACLE PREFABS
    // ========================================================================

    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject _treePrefab;
    [SerializeField] private GameObject _rockPrefab;

    // ========================================================================
    // 🎲 INSPECTOR - SPAWN RATES
    // ========================================================================

    [Header("Spawn Rates")]
    [Range(0f, 0.5f)]
    [SerializeField] private float _dirtChance = 0.12f;

    [Range(0f, 0.3f)]
    [SerializeField] private float _treeChance = 0.08f;

    [Range(0f, 0.2f)]
    [SerializeField] private float _rockChance = 0.05f;

    // ========================================================================
    // 🔒 PRIVATE DATA
    // ========================================================================

    private TileBehavior[,] _tiles;
    private Transform _tileContainer;
    private Transform _obstacleContainer;
    private int _currentSeed = -1;
    private bool _isGenerated = false;

    // ========================================================================
    // 📊 PUBLIC CONSTANTS
    // ========================================================================

    public const string SORT_LAYER_OBJECTS = "Buildings";
    public const int SORT_MULTIPLIER = 10;

    // ========================================================================
    // 📊 PUBLIC PROPERTIES
    // ========================================================================

    public int GridWidth => _gridWidth;
    public int GridHeight => _gridHeight;
    public float TileSize => _tileSize;
    public int CurrentSeed => _currentSeed;
    public bool IsGenerated => _isGenerated;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[GridManager] 🌐 Singleton initialized.");
        }
        else
        {
            Debug.LogWarning("[GridManager] ⚠️ Duplicate destroyed.");
            Destroy(gameObject);
            return;
        }

        _tiles = new TileBehavior[_gridWidth, _gridHeight];
    }

    private void Start()
    {
        if (!_isGenerated)
        {
            GenerateGrid(-1);
        }
    }

    // ========================================================================
    // 🏗️ GRID GENERATION
    // ========================================================================

    public void GenerateGrid(int seed)
    {
        if (_isGenerated)
        {
            Debug.Log("[GridManager] ⚠️ Grid already generated. Call ClearGrid first.");
            return;
        }

        if (!ValidatePrefabs())
        {
            Debug.LogError("[GridManager] ❌ Missing prefabs! Cannot generate grid.");
            return;
        }

        _tileContainer = new GameObject("Tiles").transform;
        _tileContainer.SetParent(transform);

        _obstacleContainer = new GameObject("Obstacles").transform;
        _obstacleContainer.SetParent(transform);

        if (seed < 0)
        {
            _currentSeed = System.Environment.TickCount;
        }
        else
        {
            _currentSeed = seed;
        }

        Random.InitState(_currentSeed);
        Debug.Log($"[GridManager] 🎲 Using seed: {_currentSeed}");

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                TileBehavior tile = CreateTile(x, y);
                _tiles[x, y] = tile;
            }
        }

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                TileBehavior tile = _tiles[x, y];

                if (tile.TerrainType == TerrainType.Grass && !tile.IsOccupied)
                {
                    TrySpawnObstacle(x, y, tile);
                }
            }
        }

        _isGenerated = true;
        Debug.Log($"[GridManager] ✅ Grid complete: {_gridWidth}x{_gridHeight} = {_gridWidth * _gridHeight} tiles (Seed: {_currentSeed})");
    }

    public void ClearGrid()
    {
        if (_tileContainer != null)
        {
            Destroy(_tileContainer.gameObject);
            _tileContainer = null;
        }

        if (_obstacleContainer != null)
        {
            Destroy(_obstacleContainer.gameObject);
            _obstacleContainer = null;
        }

        _tiles = new TileBehavior[_gridWidth, _gridHeight];
        _isGenerated = false;
        _currentSeed = -1;

        Debug.Log("[GridManager] 🗑️ Grid cleared.");
    }

    public void RegenerateWithSeed(int seed)
    {
        ClearGrid();
        GenerateGrid(seed);
    }

    // ========================================================================
    // ✅ VALIDATION
    // ========================================================================

    private bool ValidatePrefabs()
    {
        if (_grassTilePrefab == null)
        {
            Debug.LogError("[GridManager] ❌ Grass tile prefab not assigned!");
            return false;
        }
        return true;
    }

    // ========================================================================
    // 🏗️ TILE CREATION
    // ========================================================================

    private TileBehavior CreateTile(int x, int y)
    {
        bool isDirt = Random.value < _dirtChance && _dirtTilePrefab != null;
        GameObject prefab = isDirt ? _dirtTilePrefab : _grassTilePrefab;
        TerrainType terrain = isDirt ? TerrainType.Dirt : TerrainType.Grass;

        Vector3 worldPos = GridToWorldPosition(x, y);

        GameObject tileGO = Instantiate(prefab, worldPos, Quaternion.identity, _tileContainer);
        tileGO.name = $"Tile_{x}_{y}";

        TileBehavior tileBehavior = tileGO.GetComponent<TileBehavior>();
        if (tileBehavior == null)
        {
            tileBehavior = tileGO.AddComponent<TileBehavior>();
        }

        tileBehavior.Initialize(x, y, terrain);

        BoxCollider2D col = tileGO.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = tileGO.AddComponent<BoxCollider2D>();
            col.size = new Vector2(_tileSize, _tileSize);
        }

        tileGO.tag = "Tile";

        SpriteRenderer sr = tileGO.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Ground";
            sr.sortingOrder = 0;

            if (isDirt)
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }
        }

        if (terrain == TerrainType.Dirt)
        {
            tileBehavior.SetOccupied(true);
        }

        return tileBehavior;
    }

    private void TrySpawnObstacle(int x, int y, TileBehavior tile)
    {
        if (tile.IsOccupied)
        {
            return;
        }

        Vector3 worldPos = GridToWorldPosition(x, y);

        if (Random.value < _treeChance && _treePrefab != null)
        {
            SpawnObstacle(_treePrefab, worldPos, y);
            tile.SetOccupied(true);
            return;
        }

        if (Random.value < _rockChance && _rockPrefab != null)
        {
            SpawnObstacle(_rockPrefab, worldPos, y);
            tile.SetOccupied(true);
        }
    }

    private void SpawnObstacle(GameObject prefab, Vector3 position, int y)
    {
        GameObject obstacleGO = Instantiate(prefab, position, Quaternion.identity, _obstacleContainer);

        int baseSortOrder = (_gridHeight - y) * SORT_MULTIPLIER;

        SpriteRenderer[] allRenderers = obstacleGO.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < allRenderers.Length; i++)
        {
            allRenderers[i].sortingLayerName = SORT_LAYER_OBJECTS;
            allRenderers[i].sortingOrder = baseSortOrder + allRenderers[i].sortingOrder;
        }
    }

    // ========================================================================
    // 🔧 PUBLIC UTILITY
    // ========================================================================

    public static int GetSortOrder(int gridY, int gridHeight)
    {
        return (gridHeight - gridY) * SORT_MULTIPLIER;
    }

    public TileBehavior GetTileAt(int x, int y)
    {
        if (x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight)
            return null;
        return _tiles[x, y];
    }

    public Vector3 GridToWorldPosition(int x, int y)
    {
        return new Vector3(x * _tileSize, y * _tileSize, 0f);
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / _tileSize);
        int y = Mathf.FloorToInt(worldPos.y / _tileSize);
        return new Vector2Int(x, y);
    }
}