using UnityEngine;
using UnityEditor;
using libmapgen;

[System.Serializable()]
public class DummyPassEditor : DummyPass, IMapPassEditor {
    public void Draw(MapContext context) {
    }
}
