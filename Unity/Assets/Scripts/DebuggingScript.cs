using System.Linq;
using UnityEngine;

public class DebuggingScript : MonoBehaviour
{
    [SerializeField] public Material debugMaterial;
    [Header("Debug/Tuning")]
    [SerializeField] public int debugPageCount = 200;
    [SerializeField] public GameObject objectWithMeshHolder;
    [SerializeField] public MeshRenderer meshRenderer;
    [SerializeField] public Mesh mesh;
    public const float pageCountScalar = 0.0005f;
    public Vector3 originalLocalScale;
    private bool originalScaleCached = false;
    private const float minSpineSize = 0.4525f;
    private const float maxSpineSize = 1.968f;
    private bool hasCachedOriginalSize = false;
    private float originalSizeX;
    public const float sizeScalar = 0.0035f;
    public const float sizeExponent = 0.56f;
    public const int maxPageCountForSizing = 2000; // safety cap for bad metadata

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

    private void OnValidate()
    {
        if (objectWithMeshHolder == null) return;
        if (meshRenderer == null) meshRenderer = objectWithMeshHolder.GetComponent<MeshRenderer>();
        if (mesh == null) mesh = objectWithMeshHolder.GetComponent<MeshFilter>()?.sharedMesh;
        if (mesh == null) return;
        if (!originalScaleCached)
        {
            originalLocalScale = objectWithMeshHolder.transform.localScale;
            originalScaleCached = true;
        }
        SetBookSize(debugPageCount);
    }

    public void SetBookSize(int pageCount)
    {
        if (!hasCachedOriginalSize)
        {
            Vector3 currentScale = objectWithMeshHolder.transform.localScale;
            float currentSizeX = meshRenderer.bounds.size.x;

            if (currentSizeX <= 0f || float.IsNaN(currentScale.x))
            {
                Debug.LogWarning("Refusing to cache invalid original size — reset the transform first.");
                return;
            }

            originalLocalScale = currentScale;
            originalSizeX = currentSizeX;
            hasCachedOriginalSize = true;
        }

        int clampedPageCount = Mathf.Min(pageCount, maxPageCountForSizing);
        float newSizeX = sizeScalar * Mathf.Pow(clampedPageCount, sizeExponent);
        float scaleRatio = newSizeX / originalSizeX;

        Vector3 scaleFactor = new Vector3(
            originalLocalScale.x * scaleRatio,
            originalLocalScale.y,
            originalLocalScale.z
        );
        objectWithMeshHolder.transform.localScale = scaleFactor;
        Debug.Log($"pageCount: {pageCount} newSizeX: {newSizeX} scaleRatio: {scaleRatio}");
    }
}
