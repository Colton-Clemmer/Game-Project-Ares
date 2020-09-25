using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landmark_Controller : MonoBehaviour
{
    [SerializeField] private List<GameObject> LandmarkPrefabs;
    private List<GameObject> GeneratedLandmarks = new List<GameObject>();

    [SerializeField] private int _landmarkSeparationDistance_sett;
    [SerializeField] private int _numGeneratedLandmarks_sett;

    // Start is called before the first frame update
    void Start()
    {
        for (var i = 0; i < _numGeneratedLandmarks_sett; i++)
        {
            _generateLandmark();
        }
    }

    private void _generateLandmark()
    {
        var landmarkFab = LandmarkPrefabs[(int) (UnityEngine.Random.value * LandmarkPrefabs.Count)];
        var landmark = Instantiate(landmarkFab);
        GeneratedLandmarks.Add(landmark);
        var pos = transform.position;
        pos.y += _landmarkSeparationDistance_sett * GeneratedLandmarks.Count;
        pos.x += 20;
        pos.z = 0;
        landmark.transform.position = pos;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
