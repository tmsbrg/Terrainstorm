using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using libmapgen;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class MapGenerator : MonoBehaviour, ISerializationCallbackReceiver {

    public GameObject mapHandler = null;
    public bool createMesh = true;

    public bool generateMapInRuntime = true;

    MapContext context = new MapContext();

    BinaryFormatter serializer = new BinaryFormatter(); // our own serializer for the ruleset!

    // ruleset values - serialized
    [HideInInspector]
    [UnityEngine.SerializeField]
    string generator_serialized;

    [HideInInspector]
    [UnityEngine.SerializeField]
    List<string> passes_serialized;

    [HideInInspector]
    [UnityEngine.SerializeField] // need this to make mapArea not reset when starting the scene
    MapArea mapArea;

    Ruleset ruleset;

    [HideInInspector]
    [UnityEngine.SerializeField]
    List<string> pass_names;

    [HideInInspector]
    [UnityEngine.SerializeField]
    int width = 8;

    [HideInInspector]
    [UnityEngine.SerializeField]
    int height = 12;

    [HideInInspector]
    [UnityEngine.SerializeField]
    float min_h = 0.0f;

    [HideInInspector]
    [UnityEngine.SerializeField]
    float max_h = 2.0f;

	void Start () {
        InitRuleset();
        if (generateMapInRuntime == true) {
            RegenerateMap();
        }
    }

    IInitialMapGenerator DefaultGenerator() {
        return new RandomHeightmap(width, height, min_h, max_h);
    }

    // make sure the ruleset is never null
    public void InitRuleset() {
        if (ruleset != null) {
            return;
        }

        ResetRuleset();
    }

    public void ResetRuleset() {
        ruleset = new Ruleset(DefaultGenerator());
        pass_names = new List<string>();
    }

    public void SetGenerator(IInitialMapGenerator g) {
        ruleset.generator = g;
    }

    public int GetPassCount() {
        return ruleset.passes.Count;
    }

    public string GetPassName(int i) {
        return pass_names[i];
    }

    public IMapPassEditor GetPass(int i) {
        return (IMapPassEditor)ruleset.passes[i];
    }

    public void AddPass(IMapPassEditor p, string name) {
        ruleset.passes.Add(p);
        pass_names.Add(name);
    }

    public void RemovePass(int i) {
        ruleset.passes.RemoveAt(i);
        pass_names.RemoveAt(i);
    }

    public void MovePass(int from, int to) {
        IMapPassEditor pass = (IMapPassEditor)ruleset.passes[from];
        string name = pass_names[from];

        ruleset.passes.RemoveAt(from);
        pass_names.RemoveAt(from);

        ruleset.passes.Insert(to, pass);
        pass_names.Insert(to, name);
    }

    public MapContext GetMapContext() {
        context.width = width;
        context.height = height;
        context.min_h = min_h;
        context.max_h = max_h;
        return context;
    }

    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }

    public void SetSize(int x, int y) {
        if (x == width && y == height) {
            return;
        }

        width = x;
        height = y;

        ruleset.generator.width = x;
        ruleset.generator.height = y;
    }

    public void RegenerateMap() {
        mapArea = ruleset.generate();

        if (mapHandler != null) {
            IMapHandler handler = mapHandler.GetComponent<IMapHandler>();

            if (handler != null) {
                handler.OnMapFinish(mapArea);
            } else {
                Debug.LogWarning("map handler lacks IMapHandler script");
            }
        }

        // TODO: remove new mesh when creating new map without createMesh
        if (createMesh) {
            GetComponent<MeshFilter>().mesh = CreateMesh(width, height);
        }
	}

    public Mesh DeleteMap() {
        if (mapHandler != null) {
            IMapHandler handler = mapHandler.GetComponent<IMapHandler>();

            if (handler != null) {
                handler.OnMapDelete(mapArea);
            } else {
                Debug.LogWarning("map handler lacks IMapHandler script");
            }
        }

        mapArea = null;
        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh mesh = filter.sharedMesh;
        filter.mesh = null;
        return mesh;
    }

    // TODO: factor creating a mesh into its own class
    Mesh CreateMesh(int width, int height) {

        if (width == 0 || height == 0) {
            return new Mesh(); // nothing to generate!
        }

        // we're making a vertex grid like this
        //
        //  (map size 2x1)
        //
        //  vertices:
        //
        //  2-------3-------4
        //  |       |       |
        //  |   0   |   1   |
        //  |       |       |
        //  5-------6-------7
        //
        //  here, cvertex = 2
        //
        //  triangle0 = 0, 3, 2
        //  triangle1 = 0, 6, 3
        //  triangle2 = 0, 5, 6
        //  triangle3 = 0, 5, 2
        //  etc.
        //

        int vertices_num = width * height + (width+1) * (height+1);

        // for every tile: 4 triangles, each has 3 points
        int triangles_num = 3 * 4 * width * height;


        Vector3[] vertices = new Vector3[vertices_num];

        // add central vertices
        for (int y=0; y<height; y++) {
            for (int x=0; x<width; x++) {
                vertices[y*width + x] = new Vector3((float)x + 0.5f, mapArea.getHeightAt(x, y), (float)y + 0.5f);
            }
        }

        int cvertex = height * width; // corner vertices start at this index

        // add corner vertices. These have a grid of (width+1, height+1)
        for (int y=0; y < height + 1; y++) {
            for (int x=0; x < width + 1; x++) {
                float z = CalculateCornerHeight(x, y, vertices, width, height);
                vertices[cvertex + y*(width+1) + x] = new Vector3((float)x, z, (float)y);
            }
        }

        Vector2[] uv = new Vector2[vertices_num];

        // don't care about uv right now
        for (int i=0; i<vertices_num; i++) {
            uv[i] = new Vector2(0.0f, 0.0f);
        }

        int[] triangles = new int[triangles_num];

        // adding triangles
        for (int y=0; y<height; y++) {
            for (int x=0; x<width; x++) {
                int tile_i = y*width + x; // index of the tile we're at
                int tri_i = tile_i * 4 * 3; // index in the triangle array
                int upperleft_vertex = cvertex + x + y*(width+1);
                int upperright_vertex = upperleft_vertex + 1;
                int lowerleft_vertex = upperleft_vertex + width+1;
                int lowerright_vertex = upperleft_vertex + 1 + width+1;

                // triangle 1
                triangles[tri_i] = tile_i; // central vertex
                triangles[tri_i+1] = upperright_vertex;
                triangles[tri_i+2] = upperleft_vertex;

                // triangle 2
                tri_i += 3;
                triangles[tri_i] = tile_i;
                // + width means one row below, aka the lower vertexes of the tile
                triangles[tri_i+1] = lowerright_vertex;
                triangles[tri_i+2] = upperright_vertex;

                // triangle 3
                tri_i += 3;
                triangles[tri_i] = tile_i;
                triangles[tri_i+1] = lowerleft_vertex;
                triangles[tri_i+2] = lowerright_vertex;

                // triangle 4
                tri_i += 3;
                triangles[tri_i] = tile_i;
                triangles[tri_i+1] = upperleft_vertex;
                triangles[tri_i+2] = lowerleft_vertex;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        return mesh;
    }

    float CalculateCornerHeight (int x, int y, Vector3[] vertices, int width, int height) {
        float?[] tiles = new float?[] {null, null, null, null};

        // get values of tiles around corner
        if (x - 1 >= 0 && y - 1 >= 0) {
            tiles[0] = vertices[x-1 + (y-1)*width].y;
        }
        if (x < width && y - 1 >= 0) {
            tiles[1] = vertices[x + (y-1)*width].y;
        }
        if (x - 1 >= 0 && y < height) {
            tiles[2] = vertices[x-1 + y*width].y;
        }
        if (x < width && y < height) {
            tiles[3] = vertices[x + y*width].y;
        }

        // get average value
        float sum = 0;
        float div = 0;
        foreach (var v in tiles) {
            if (v.HasValue) {
                sum += v.Value;
                div += 1;
            }
        }

        // if outside of grid, default to 0.0f
        if (div != 0) {
            return sum / div;
        } else {
            return 0.0f;
        }
    }

	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyUp(KeyCode.Space)) {
            RegenerateMap();
        }
	}

    //TODO: reuse MemoryStream? Don't need to convert bytes to base64string?

    public void OnBeforeSerialize() {
        using (var stream = new MemoryStream()) {
            serializer.Serialize(stream, ruleset.generator);
            stream.Flush();
            generator_serialized = Convert.ToBase64String(stream.ToArray());
        }

        passes_serialized.Clear();
        foreach (var pass in ruleset.passes) {
            using (var stream = new MemoryStream()) {
                serializer.Serialize(stream, pass);
                stream.Flush();
                passes_serialized.Add(Convert.ToBase64String(stream.ToArray()));
            }
        }
    }

    public void OnAfterDeserialize() {
        ruleset = new Ruleset();
        byte[] bytes = Convert.FromBase64String(generator_serialized);
        using (var stream = new MemoryStream(bytes)) {
            ruleset.generator = (IInitialMapGenerator)serializer.Deserialize(stream);
        }
        ruleset.passes.Capacity = passes_serialized.Count;
        foreach(var pass_serialized in passes_serialized) {
            bytes = Convert.FromBase64String(pass_serialized);
            using (var stream = new MemoryStream(bytes)) {
                ruleset.passes.Add((IMapPassEditor)serializer.Deserialize(stream));
            }
        }
    }
}
