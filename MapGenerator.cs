using UnityEngine;
using libmapgen;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class MapGenerator : MonoBehaviour {

    public GameObject mapHandler = null;
    public bool createMesh = true;

    MapArea mapArea = null;
    int width = 12;
    int height = 3;

    Ruleset ruleset;

	// Use this for initialization
	void Start () {

        mapArea = GenerateMap();

        if (mapHandler != null) {
            IMapHandler handler = mapHandler.GetComponent<IMapHandler>();

            if (handler != null) {
                handler.OnMapFinish(mapArea);
            } else {
                Debug.LogWarning("map handler lacks IMapHandler script");
            }
        }

        if (createMesh) {
            CreateMesh();
        }
	}

    MapArea GenerateMap() {
        // TODO
	    Debug.Log("Build map!");
        return new MapArea(width, height);
    }

    void CreateMesh() {

        if (width == 0 || height == 0) {
            return; // nothing to generate!
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
        for (var y=0; y<height; y++) {
            for (var x=0; x<width; x++) {
                vertices[y*width + x] = new Vector3((float)x + 0.5f, 0.0f, (float)y + 0.5f);
            }
        }

        int cvertex = height * width; // corner vertices start at this index

        // add corner vertices. These have a grid of (width+1, height+1)
        for (var y=0; y < height + 1; y++) {
            for (var x=0; x < width + 1; x++) {
                vertices[cvertex + y*(width+1) + x] = new Vector3((float)x, 0.0f, (float)y);
            }
        }

        Vector2[] uv = new Vector2[vertices_num];

        // don't care about uv right now
        for (var i=0; i<vertices_num; i++) {
            uv[i] = new Vector2(0.0f, 0.0f);
        }

        int[] triangles = new int[triangles_num];

        // adding triangles
        for (var y=0; y<height; y++) {
            for (var x=0; x<width; x++) {
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
        GetComponent<MeshFilter>().mesh = mesh;
    }

	// Update is called once per frame
	void Update () {
	
	}
}
