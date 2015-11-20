using UnityEngine;
using System.Collections;

public class MapContext {
    public int width;
    public int height;
    public float min_h;
    public float max_h;

    public MapContext(int width=10, int height=10, float min_h=1.0f, float max_h=1.0f) {
        this.width = width;
        this.height = height;
        this.min_h = min_h;
        this.max_h = max_h;
    }
}
