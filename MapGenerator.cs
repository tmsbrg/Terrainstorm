using UnityEngine;
using libmapgen;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class MapGenerator : MonoBehaviour {

    public GameObject mapHandler = null;
    public bool createMesh = true;

    public bool generateMapInRuntime = true;

    [UnityEngine.SerializeField] // need this to make mapArea not reset when starting the scene
    MapArea mapArea;
    int width = 8;
    int height = 12;

    Ruleset ruleset;

	void Start () {
        if (generateMapInRuntime == true) {
            RegenerateMap();
        }
    }

    public void RegenerateMap() {
        mapArea = GenerateMap(new RandomHeightmap(width, height, 0.0f, 2.0f));

        if (mapHandler != null) {
            IMapHandler handler = mapHandler.GetComponent<IMapHandler>();

            if (handler != null) {
                handler.OnMapFinish(mapArea);
            } else {
                Debug.LogWarning("map handler lacks IMapHandler script");
            }
        }

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

    MapArea GenerateMap(IInitialMapGenerator generator) {
        return generator.generate();
    }

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

                // triangle 1 TODO: Fix broken tile_i placement. WTF is going on!?
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
}
