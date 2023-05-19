using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.WorldGenerator
{
    public class WorldGenerator : MonoBehaviour
    {
        private class Cell
        {
            private Transform _cellTransform;
            private bool _isFree = false;
            private Vector3Int _coord;

            public Cell(Transform cellTransform)
            {
                _cellTransform = cellTransform;
            }

            public Vector3Int GetTransPos()
            {
                return _coord;
            }

            public void SetTransPos(Vector3Int newPos)
            {
                _cellTransform.position = newPos;
                _coord = newPos;
            }

            public bool IsFree()
            {
                return _isFree;
            }

            public void SetFree(bool isFree)
            {
                _isFree = isFree;
            }
        }

        [SerializeField]
        private GameObject[] _floorObjects;
        [SerializeField]
        private Vector2Int _worldSize;
        [SerializeField]
        private float _cellSpace;
        [SerializeField]
        private Transform _playerTrans;

        private Cell[] _cells;
        private bool[,] _cellsPlaces;

        private Vector3 _oldPlayerPos;

        private void Start()
        {
            _cells = new Cell[_worldSize.x * _worldSize.y];
            _cellsPlaces = new bool[_worldSize.x, _worldSize.y];
            for (int i = 0; i < _cells.Length; i++)
            {
                var tempObj = Instantiate(_floorObjects[Random.Range(0, _floorObjects.Length)]);
                tempObj.transform.SetParent(transform);
                _cells[i] = new Cell(tempObj.transform);
            }

            SetFloor();
        }

        private void Update()
        {
            if (Vector3.SqrMagnitude(_playerTrans.position - _oldPlayerPos) > _cellSpace * _cellSpace)
            {
                SetFloor();
                _oldPlayerPos = _playerTrans.position;
            }
        }

        private void SetFloor()
        {
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i].SetFree(true);
            }

            for (int y = 0; y < _worldSize.y; y++)
            {
                for (int x = 0; x < _worldSize.x; x++)
                {
                    _cellsPlaces[x, y] = false;

                    var newPos = Vector3Int.zero;
                    newPos.x = Mathf.RoundToInt(x * _cellSpace + _playerTrans.position.x);
                    newPos.y = 0;
                    newPos.z = Mathf.RoundToInt(y * _cellSpace + _playerTrans.position.z);

                    for (int i = 0; i < _cells.Length; i++)
                    {
                        if (Vector3.SqrMagnitude(_cells[i].GetTransPos() - newPos) < _cellSpace * _cellSpace)
                        {
                            _cellsPlaces[x, y] = true;
                            _cells[i].SetFree(false);
                            break;
                        }
                    }
                }
            }

            for (int y = 0; y < _worldSize.y; y++)
            {
                for (int x = 0; x < _worldSize.x; x++)
                {
                    if (_cellsPlaces[x, y])
                        continue;

                    for (int i = 0; i < _cells.Length; i++)
                    {
                        if (!_cells[i].IsFree())
                            continue;

                        _cellsPlaces[x, y] = true;
                        _cells[i].SetFree(false);

                        var newPos = Vector3Int.zero;
                        newPos.x = Mathf.RoundToInt(x * _cellSpace);
                        newPos.y = 0;
                        newPos.z = Mathf.RoundToInt(y * _cellSpace);

                        _cells[i].SetTransPos(newPos);
                        break;
                    }
                }
            }


        }
    }
}