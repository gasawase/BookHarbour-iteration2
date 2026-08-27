using System.Linq;
using UnityEngine;

public class DebuggingScript : MonoBehaviour
{
    [SerializeField] public Material debugMaterial;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetMeshColorsBasedOnVertices();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetMeshColorsBasedOnVertices()
    {
        Mesh spawnedMesh = null;
        GameObject presentGO = this.gameObject;
        int level = 0;

        do
        {
            if (presentGO.TryGetComponent<MeshFilter>(out MeshFilter component))
            {
                spawnedMesh = component.mesh;
            }
            else if (presentGO.transform.childCount > 0)
            {
                presentGO = presentGO.transform.GetChild(0).gameObject;
                level++;
            }
            else
            {
                Debug.LogWarning($"No MeshFilter found after descending {level} level(s).");
                return;
            }
        } while (spawnedMesh == null);

        if (presentGO.TryGetComponent<MeshRenderer>(out MeshRenderer presentGOMeshRenderer))
        {
            if (presentGOMeshRenderer.material != debugMaterial)
            {
                presentGOMeshRenderer.material = debugMaterial;
            }
        }
        Vector3[] verts = spawnedMesh.vertices;
        int[] triangles = spawnedMesh.triangles;
        Color[] coloringVerts = new Color[verts.Length];
        string joinedVertices = string.Join(",", verts);
        string joinedTriangles = string.Join(",", triangles);
        Debug.Log($"Vertices: {joinedVertices} | Triangles: {joinedTriangles}");
        //Debug.Log($"Vertex count: {verts.Count()} | Triangles count: {triangles.Count()}");
        string joinedUVs = string.Join(",", spawnedMesh.uv);
        Debug.Log($"UVs: {joinedUVs}");

        // THIS PART ONLY WORKS IF YOU'RE DEALING WITH A CUBE BTW

        int[][] faceIndexGroups = new int[][] // we're just labeling this here for our ease so we can understand how things are grouped better
        {
                new int[] { 0,1,2,3}, // +Z
                new int[] { 4,5,8,9}, // +Y
                new int[] { 6,7,10,11}, // -Z
                new int[] { 12,13,14,15}, // -Y
                new int[] { 16,17,18,19}, // -X
                new int[] { 20,21,22,23} // +X
        };

        Color[] faceColors = new Color[] // the collective colors each vertex connected to a given face should be
        {
                Color.red,
                Color.green,
                Color.blue,
                Color.yellow,
                Color.magenta,
                Color.gray
        };

        for (int i = 0; i < faceIndexGroups.Length; i++)
        {
            foreach (int integer in faceIndexGroups[i])
            {
                coloringVerts[integer] = faceColors[i];
            }
        }
        spawnedMesh.colors = coloringVerts;
    }
}
