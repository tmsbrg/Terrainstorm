using UnityEngine;
using libmapgen;

public class MapGenerator : MonoBehaviour {

    public GameObject mapHandler = null;
    MapArea mapArea = null;

    Ruleset ruleset;

	// Use this for initialization
	void Start () {

        mapArea = GenerateMap();

        if (mapHandler != null) {
            IMapHandler handler = mapHandler.GetComponent<IMapHandler>();

            if (handler != null) {
                handler.OnMapFinish(mapArea);
            } else {
                Debug.LogWarning("map handler lacks IMapHandler");
            }
        }
	}

    MapArea GenerateMap() {
        // TODO
	    Debug.Log("Build map!");
        return new MapArea(10, 10);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
