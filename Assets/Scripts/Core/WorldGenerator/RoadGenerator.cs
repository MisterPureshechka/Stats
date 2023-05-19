using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject _segmentPref;
    [SerializeField]
    private float _segmentLenght;
    [SerializeField]
    private int _segmentCount;

    private Transform _playerTrans;
    private Transform[] _segmentTrans;
    
    private void Start ()
    {
        _playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        _segmentTrans = new Transform[_segmentCount];

        for (int i = 0; i < _segmentCount; i++)
        {
            var segmentTemp = Instantiate(_segmentPref);
            segmentTemp.SetActive(true);
            segmentTemp.transform.SetParent(transform);
            segmentTemp.transform.rotation = Quaternion.Euler(Vector3.up * (Random.Range(-10, 10) > 0 ? 0 : 180));
            segmentTemp.transform.position = Vector3.forward * i * _segmentLenght;
            _segmentTrans[i] = segmentTemp.transform;
        }
    }

    private void Update()
    {
        for (int i = 0; i < _segmentCount; i++)
        {
            var deltaZ = _playerTrans.position.z - _segmentTrans[i].position.z;
            if (Mathf.Abs(deltaZ) <= (_segmentCount - Mathf.RoundToInt(_segmentCount / 3f)) * _segmentLenght)
                continue;

            _segmentTrans[i].gameObject.SetActive(false);
            _segmentTrans[i].position += Vector3.forward * _segmentCount * _segmentLenght * (deltaZ > 0 ? 1f : -1f);
            _segmentTrans[i].rotation = Quaternion.Euler(Vector3.up * (Random.Range(-10, 10) > 0 ? 0 : 180));
            _segmentTrans[i].gameObject.SetActive(true);
        }
    }
}
