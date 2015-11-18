using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using libmapgen;

[InitializeOnLoad]
public static class PassesManager {
    public delegate IMapPassEditor PassGenerator();

    static List<PassGenerator> passes = new List<PassGenerator>();
    static string[] passNames = new string[32]; // has to be an array for Unity

    static PassesManager () {
        ClearPasses();
        AddDefaultPasses();
    }

    public static string GetPassName(int i) {
        if (i < 0 || i >= passes.Count) {
            Debug.LogWarning("PassesManager: Tried to get pass name from out of bounds value: "+i);
            return "Unknown pass";
        }
        return passNames[i];
    }

    public static int GetPassCount() {
        return passes.Count;
    }

    public static void ClearPasses() {
        passes.Clear();
    }

    public static void AddDefaultPasses() {
        AddPass(delegate { return new DummyPassEditor(); }, "Dummy");
    }

    public static void AddPass(PassGenerator pass, string name) {
        passes.Add(pass);
        if (passes.Count > passNames.Length) {
            Array.Resize(ref passNames, passNames.Length * 2);
        }
        passNames[passes.Count - 1] = name;
    }

    public static IMapPassEditor CreatePass(int i) {
        return passes[i]();
    }
}
