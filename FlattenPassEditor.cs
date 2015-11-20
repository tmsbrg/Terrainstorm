using UnityEngine;
using UnityEditor;
using libmapgen;

[System.Serializable()]
public class FlattenPassEditor : FlattenPass, IMapPassEditor {
    public void Draw(MapContext context) {
        h = EditorGUILayout.Slider(h, context.min_h, context.max_h);
        greater_than = EditorGUILayout.Toggle("Greater than", greater_than);
    }
}
