using UnityEngine;
using libmapgen;

public interface IMapHandler {
    // TODO: reexport MapArea so people don't need to use 'using libmapgen'
    void OnMapFinish(MapArea map);
    void OnMapDelete(MapArea map);
}
