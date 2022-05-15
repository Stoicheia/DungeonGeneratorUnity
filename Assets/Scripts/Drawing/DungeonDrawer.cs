using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class DungeonDrawer : MonoBehaviour
{
    public TileGrid Info;

    /*private TileInfo[,] _squares = new TileInfo[GRID_SIZE, GRID_SIZE];
    private Dictionary<CoordinatePair, DoorInfo> _doors;
    private List<Room> _rooms = new List<Room>(GRID_SIZE);*/

    [SerializeField] private DungeonGenerator _generator;

    [SerializeField] private RectTransform _tilesContainer;
    [SerializeField] private RectTransform _doorsContainer;
    
    [SerializeField] private Image _tilePrefab;
    [SerializeField] private Image _verticalDoorPrefab;
    [SerializeField] private Image _horizontalDoorPrefab;

    private Dictionary<Vector2Int, RectTransform> _tiles = new Dictionary<Vector2Int, RectTransform>();

    private Dictionary<(Vector2Int, Vector2Int), RectTransform> _doors =
        new Dictionary<(Vector2Int, Vector2Int), RectTransform>();

    private Vector2 _tileSize;
    private Vector2 _doorSizeVertical;
    private Vector2 _doorSizeHorizontal;
    public void Generate()
    {
        Info = _generator.Generate();
    }
    public void Draw(TileGrid g)
    {
        Clear();
        InitialiseGrid();

        var tileInfo = g.Tiles;
        var doorInfo = g.Doors;
        var roomInfo = g.Rooms;

        for (int i = 0; i < tileInfo.GetLength(0); i++)
        {
            for (int j = 0; j < tileInfo.GetLength(1); j++)
            {
                TileInfo tile = tileInfo[i, j];
                if(tile.Active)
                    _tiles[new Vector2Int(i, j)].gameObject.SetActive(true);
            }
        }

        foreach (var door in doorInfo)
        {
            _doors[door.Key.Params].gameObject.SetActive(true);
        }
        
        
    }

    public void InitialiseGrid()
    {
        Clear();

        var rect = _tilePrefab.rectTransform.rect;
        _tileSize = new Vector2(rect.width, rect.height);
        
        var rect2 = _verticalDoorPrefab.rectTransform.rect;
        _doorSizeVertical = new Vector2(rect.width, rect.height);
        _doorSizeVertical = new Vector2(rect.height, rect.width);
        
        _tiles = new Dictionary<Vector2Int, RectTransform>();
        _doors = new Dictionary<(Vector2Int, Vector2Int), RectTransform>();
        for (int i = 0; i < TileGrid.GRID_SIZE; i++)
        {
            for (int j = 0; j < TileGrid.GRID_SIZE; j++)
            {
                RectTransform tile = Instantiate(_tilePrefab, new Vector3(_tileSize.x * i, _tileSize.y * j, 0)
                    , Quaternion.identity).rectTransform;
                tile.gameObject.SetActive(false);
                tile.SetParent(_tilesContainer);
                _tiles.Add(new Vector2Int(i, j), tile);
                
                /*if(j + 1 < GRID_SIZE) _doors[new CoordinatePair((i, j),(i, j + 1))] = DoorInfo.Empty();
                if(i + 1 < GRID_SIZE) _doors[new CoordinatePair((i, j),(i + 1, j))] = DoorInfo.Empty();*/

                if (i + 1 < TileGrid.GRID_SIZE)
                {
                    RectTransform verticalDoor = Instantiate(_verticalDoorPrefab,
                        new Vector3(_tileSize.x * i + _tileSize.x/2, _tileSize.y * j, 0), Quaternion.identity).rectTransform;
                    verticalDoor.SetParent(_doorsContainer);
                    verticalDoor.gameObject.SetActive(false);
                    _doors.Add((new Vector2Int(i, j), new Vector2Int(i + 1, j)), verticalDoor);
                    _doors.Add((new Vector2Int(i + 1, j), new Vector2Int(i, j)), verticalDoor);
                }
                
                if (j + 1 < TileGrid.GRID_SIZE)
                {
                    RectTransform horizontalDoor = Instantiate(_horizontalDoorPrefab,
                        new Vector3(_tileSize.x * i, _tileSize.y * j + _tileSize.y/2, 0), Quaternion.Euler(0,0,90)).rectTransform;
                    horizontalDoor.SetParent(_doorsContainer);
                    horizontalDoor.gameObject.SetActive(false);
                    _doors.Add((new Vector2Int(i, j), new Vector2Int(i, j + 1)), horizontalDoor);
                    _doors.Add((new Vector2Int(i, j + 1), new Vector2Int(i, j)), horizontalDoor);
                }
            }
        }
        
        for (int i = 0; i < TileGrid.GRID_SIZE; i++)
        {
            RectTransform horizontalDoor = Instantiate(_horizontalDoorPrefab,
                new Vector3(_tileSize.x * i, -_tileSize.y/2, 0), Quaternion.Euler(0,0,90)).rectTransform;
            horizontalDoor.SetParent(_doorsContainer);
            _doors.Add((new Vector2Int(i, 0), new Vector2Int(i, -1)), horizontalDoor);
            _doors.Add((new Vector2Int(i, -1), new Vector2Int(i, 0)), horizontalDoor);
            
            RectTransform horizontalDoor2 = Instantiate(_horizontalDoorPrefab,
                new Vector3(_tileSize.x * i, _tileSize.y * TileGrid.GRID_SIZE-_tileSize.y/2, 0), Quaternion.Euler(0,0,90)).rectTransform;
            horizontalDoor2.SetParent(_doorsContainer);
            _doors.Add((new Vector2Int(i, TileGrid.GRID_SIZE - 1), new Vector2Int(i, TileGrid.GRID_SIZE)), horizontalDoor2);
            _doors.Add((new Vector2Int(i, TileGrid.GRID_SIZE), new Vector2Int(i, TileGrid.GRID_SIZE - 1)), horizontalDoor2);
        }
        
        for (int i = 0; i < TileGrid.GRID_SIZE; i++)
        {
            RectTransform verticalDoor = Instantiate(_verticalDoorPrefab,
                new Vector3(-_tileSize.x/2, _tileSize.y * i, 0), Quaternion.identity).rectTransform;
            verticalDoor.SetParent(_doorsContainer);
            _doors.Add((new Vector2Int(0, i), new Vector2Int(-1, i)), verticalDoor);
            _doors.Add((new Vector2Int(-1, i), new Vector2Int(0, i)), verticalDoor);
            
            RectTransform verticalDoor2 = Instantiate(_verticalDoorPrefab,
                new Vector3(_tileSize.x * TileGrid.GRID_SIZE-_tileSize.x/2, _tileSize.y * i, 0), Quaternion.identity).rectTransform;
            verticalDoor2.SetParent(_doorsContainer);
            _doors.Add((new Vector2Int(TileGrid.GRID_SIZE - 1, i), new Vector2Int(TileGrid.GRID_SIZE, i)), verticalDoor2);
            _doors.Add((new Vector2Int(TileGrid.GRID_SIZE, i), new Vector2Int(TileGrid.GRID_SIZE - 1, i)), verticalDoor2);
        }
    }

    public void Clear()
    {
        var tilesList = _tilesContainer.Cast<RectTransform>().ToList();
        foreach (RectTransform t in tilesList)
        {
            DestroyImmediate(t.gameObject);
        }
        var doorsList = _doorsContainer.Cast<RectTransform>().ToList();
        foreach (RectTransform t in doorsList)
        {
            DestroyImmediate(t.gameObject);
        }

        _tiles.Clear();
        _doors.Clear();
    }

    private void DisableAll()
    {
        foreach (var t in _tiles)
        {
            t.Value.gameObject.SetActive(false);
        }
        
        foreach (var t in _doors)
        {
            t.Value.gameObject.SetActive(false);
        }
    }
}
