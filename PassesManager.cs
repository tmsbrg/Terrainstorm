using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using libmapgen;

[InitializeOnLoad]
public static class PassesManager {
    public delegate IMapPassEditor PassGenerator();

    static List<PassGenerator> passes = new List<PassGenerator>();
    static List<string> passNameList = new List<string>();
    static string[] passNameArray; // used for Unity dropdown menu

    static PassesManager () {
        ClearPasses();
        AddDefaultPasses();
        GeneratePassNameArray();
    }

    public static string GetPassName(int i) {
        if (i < 0 || i >= passes.Count) {
            Debug.LogWarning("PassesManager: Tried to get pass name from out of bounds value: "+i);
            return "Unknown pass";
        }
        return passNameList[i];
    }

    public static int GetPassCount() {
        return passes.Count;
    }

    public static void ClearPasses() {
        passes.Clear();
        passNameList.Clear();
        passNameArray = null;
    }

    public static void GeneratePassNameArray() {
        passNameArray = new string[passes.Count + 1];
        passNameArray[0] = "<Add new pass>";
        for (int i = 0; i < passes.Count; i++) {
            passNameArray[i+1] = passNameList[i];
        }
    }


    public static void AddDefaultPasses() {
        AddPass(delegate { return new DummyPassEditor(); }, "Dummy");
    }

    public static void AddPass(PassGenerator pass, string name) {
        passes.Add(pass);
        passNameList.Add(name);
        passNameArray = null; // reset so you can't accidentally use it when it's out of date
    }

    public static IMapPassEditor CreatePass(int i) {
        return passes[i]();
    }

    public static int? SelectPass() {
        if (passNameArray != null) {
            int i = EditorGUILayout.Popup(0, passNameArray);
            if (i == 0) {
                return null;
            } else {
                return i - 1;
            }
        } else {
            EditorGUILayout.LabelField("Name array out of date. Call GeneratePassNameArray() to regenerate");
            return null;
        }
    }
}
