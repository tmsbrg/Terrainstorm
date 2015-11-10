using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        MapGenerator generator = (MapGenerator) target;

        generator.InitRuleset();

        if (GUILayout.Button("Reset ruleset")) {
            generator.ResetRuleset();
        }
        if (GUILayout.Button("Generate statically")) {
            generator.RegenerateMap();
        }
        if (GUILayout.Button("Remove static map")) {
            Mesh mesh = generator.DeleteMap();
            if (mesh != null) {
                DestroyImmediate(mesh);
            }
        }
    }

}
