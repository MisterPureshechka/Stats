using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.CellularAutomata
{
    public class CellularAutomataGenerator : MonoBehaviour
    {
        [SerializeField]
        private GameObject _floorPref;
        [SerializeField]
        private GameObject _wallPref;

        [SerializeField]
        private int _width = 10;
        [SerializeField]
        private int _height = 10;
        [SerializeField]
        private float _chanceToStartAlive = 0.45f;
        [SerializeField]
        private int _deathLimit = 3;
        [SerializeField]
        private int birthLimit = 3;
        [SerializeField]
        private int _steps = 5;

        [ContextMenu ("Start")]
        private void Start()
        {
            var cellmap = new bool[_width, _height];
            cellmap = InitialiseMap(cellmap, _chanceToStartAlive);
            for (int i = 0; i < _steps; i++)
            {
                cellmap = DoSimulationStep(cellmap, _deathLimit, birthLimit);
            }

            var childs = new Transform[transform.childCount];
            for (int i = 0; i < childs.Length; i++)
            {
                childs[i] = transform.GetChild(i);
            }

            for (int i = 0; i < childs.Length; i++)
            {
                Destroy(childs[i].gameObject);
            }

            var width = cellmap.GetLength(0);
            var height = cellmap.GetLength(1);

            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    if (!cellmap[x, y])
                    {
                        var floorGO = Instantiate(_floorPref, transform);
                        floorGO.transform.localPosition = new Vector3(x * 5f, 0, y * 5f);
                    }
                }
            }

            for (int x = 0; x < width-1; x++)
            {
                for (int y = 0; y < height-1; y++)
                {
                    if (cellmap[x,y] != cellmap[x+1, y])
                    {
                        var wallGO = Instantiate(_wallPref, transform);
                        wallGO.transform.localPosition = new Vector3(((x + x + 1) * 0.5f) * 5f, 0, y * 5f);

                        if (cellmap[x, y])
                            wallGO.transform.localEulerAngles = Vector3.up * 90f;
                        else
                            wallGO.transform.localEulerAngles = Vector3.up * -90f;
                    }
                    
                    if (cellmap[x, y] != cellmap[x, y + 1])
                    {
                        var wallGO = Instantiate(_wallPref, transform);
                        wallGO.transform.localPosition = new Vector3(x * 5f, 0, ((y + y + 1) * 0.5f) * 5f);

                        if (cellmap[x, y])
                            wallGO.transform.localEulerAngles = Vector3.up * 0f;
                        else
                            wallGO.transform.localEulerAngles = Vector3.up * 180;
                    }
                    
                }
            }
        }

        private bool[,] InitialiseMap(bool[,] map, float chanceToStartAlive)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = (Random.Range(0f, 1f) < chanceToStartAlive);
                }
            }
            return map;
        }

        private int CountAliveNeighbours(bool[,] map, int x, int y)
        {
            var count = 0;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    var neighbour_x = x + i;
                    var neighbour_y = y + j;

                    if (i == 0 && j == 0)
                        continue;

                    var isNeighbour = neighbour_x < 0 ||
                        neighbour_y < 0 ||
                        neighbour_x >= map.GetLength(0) ||
                        neighbour_y >= map.GetLength(1) ||
                        map[neighbour_x, neighbour_y];

                    if (isNeighbour)
                        count++;
                }
            }
            return count;
        }

        private bool[,] DoSimulationStep(bool[,] oldMap, int deathLimit, int birthLimit)
        {
            var width = oldMap.GetLength(0);
            var height = oldMap.GetLength(1);

            var newMap = new bool[width, height];

            for (int x = 0; x < oldMap.GetLength(0); x++)
            {
                for (int y = 0; y < oldMap.GetLength(1); y++)
                {
                    if (x == 0 || y == 0 || x == oldMap.GetLength(0) - 1 || y == oldMap.GetLength(1) - 1)
                    {
                        newMap[x, y] = true;
                        continue;
                    }

                    var nbs = CountAliveNeighbours(oldMap, x, y);

                    newMap[x, y] = oldMap[x, y] ? nbs >= deathLimit : nbs > birthLimit;
                }
            }
            return newMap;
        }
    }
}