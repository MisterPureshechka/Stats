using UnityEngine;

namespace Core.CellularAutomata
{
    public class FloorManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] _floorPrefs;
        [SerializeField]
        private GameObject[] _furniturePrefs;
        [SerializeField]
        private float _furnChance = 0.2f;

        private void Start()
        {
            var randomNum = Random.Range(0, _floorPrefs.Length);
            _floorPrefs[randomNum].SetActive(true);

            if (Random.Range(0f, 1f) > _furnChance)
                return;

            randomNum = Random.Range(0, _floorPrefs.Length);
            _furniturePrefs[randomNum].SetActive(true);
        }
    }
}

