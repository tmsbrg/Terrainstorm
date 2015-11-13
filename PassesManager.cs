using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using libmapgen;

[InitializeOnLoad]
public static class PassesManager {
    public static List<System.Type> knownMapPasses = new List<System.Type>(); // shouldn't be publicly editable
    // public static string[] passNames;

    static PassesManager () {
        ClearKnownPasses();
        AddDefaultPasses();
    }

    public static void ClearKnownPasses() {
        knownMapPasses.Clear();
    }

    public static void AddDefaultPasses() {
        knownMapPasses.AddRange(libmapgen.DefaultPasses.get());
    }

    public static void AddPass(System.Type pass) {
        // check if type implements interface
        if (pass.GetInterfaces().Any( x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IMapPass))) {
            knownMapPasses.Add(pass);
        } else {
            // probably should throw an exception?
            Debug.LogWarning("PassesManager: Tried to add a pass that does not implement IMapPass");
        }
    }

}
