using Assets.Scripts.Models;
using Assets.Scripts.Services;
using SQLite;
using SQLitePCL;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public struct UVRect
{
    public float uMin, uMax, vMin, vMax;
    public UVRect(float umin, float umax, float vmin, float vmax)
    {
        uMin = umin;
        uMax = umax;
        vMin = vmin;
        vMax = vmax;
    }
}

struct FullBookWrapRegion
{
    public UVRect backRegion, spineRegion, coverRegion;
    public FullBookWrapRegion(UVRect backRegion, UVRect spineRegion, UVRect coverRegion)
    {
        this.backRegion = backRegion;
        this.spineRegion = spineRegion;
        this.coverRegion = coverRegion;
    }
}
public class BookObject3D : MonoBehaviour
{
    public const float pageCountScalar = 0.0005f;
    public const float minSpineValue = 0.05f;
    public const float maxSpineValue = 0.30f;
    public const int sampleStep = 4;
    public const int bucketSize = 10;
    private const float minSpineSize = 0.4525f;
    private const float maxSpineSize = 1.968f;
    private bool hasCachedOriginalSize = false;
    private float originalSizeX;
    public const float sizeScalar = 0.0035f;
    public const float sizeExponent = 0.56f;
    public const int maxPageCountForSizing = 2000; // safety cap for bad metadata
    [SerializeField] public GameObject objectWithMeshHolder;
    [Header("Image Quality")]
    [SerializeField] public int texelsPerUnit = 512;
    internal MeshRenderer meshRenderer;
    internal MeshFilter meshFilter;
    internal string bookUID;
    internal float backWidthFraction;
    internal float spineWidthFraction;
    internal float frontWidthFraction;
    internal Mesh mesh;

    internal EpubMetadataModel bookInfo;
    internal int pageCount;
    internal int[][] faceIndexGroups;
    internal Vector3 originalLocalScale;
    internal Vector3 originalSize;

    public void Initialize(string bookUID)
    {

        this.bookUID = bookUID;
        this.bookInfo = PersistanceManager.Instance.sqliteService.SelectLiteBookInfo(this.bookUID);
        this.pageCount = GetPageCount(bookInfo);
        meshRenderer = objectWithMeshHolder.GetComponent<MeshRenderer>();
        meshFilter = objectWithMeshHolder.gameObject.GetComponent<MeshFilter>();
        mesh = objectWithMeshHolder.GetComponent<MeshFilter>().mesh;
        originalSize = mesh.bounds.size;
        originalLocalScale = objectWithMeshHolder.transform.localScale;

        mesh.subMeshCount = 2; // so we know that we have 2 separate sets of triangles: 1 for the pages, 1 for the book wrap

        // setup the triangles to be to the right material
        int[][] faceTriangleGroups = new int[][] // the specific triangles for each group (repeats because it's the path between the vertices)
        {
            new int[] { 0, 2, 3, 0, 3, 1 }, // +Z
            new int[] { 8, 4, 5, 8, 5, 9 }, // +Y
            new int[] { 10, 6, 7, 10, 7, 11 }, // -Z
            new int[] { 12, 13, 14, 12, 14, 15 }, // -Y
            new int[] { 16, 17, 18, 16, 18, 19 }, // -X
            new int[] { 20, 21, 22, 20, 22, 23 } // +X
        };

        faceIndexGroups = new int[][] // the specific verticies for each face
{
                new int[] { 0,1,2,3}, // +Z 0
                new int[] { 4,5,8,9}, // +Y 1
                new int[] { 6,7,10,11}, // -Z 2 
                new int[] { 12,13,14,15}, // -Y 3
                new int[] { 16,17,18,19}, // -X 4 
                new int[] { 20,21,22,23} // +X 5
};

        int[] wrapGroupTriangles = faceTriangleGroups[5].Concat(faceTriangleGroups[2]).Concat(faceTriangleGroups[4]).ToArray();
        int[] pageEdgesGroupTriangles = faceTriangleGroups[0].Concat(faceTriangleGroups[1]).Concat(faceTriangleGroups[3]).ToArray();

        mesh.SetTriangles(wrapGroupTriangles, 0);
        mesh.SetTriangles(pageEdgesGroupTriangles, 1);
        mesh.RecalculateBounds();

        SetBookSize(this.pageCount);

        FullBookWrapRegion fbwRegions = BuildRegions();

        RemapWrapUVs(mesh, faceIndexGroups, ref fbwRegions);
        SetBookCover(bookInfo, mesh, meshRenderer, fbwRegions.coverRegion);
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

    private void RemapWrapUVs(Mesh mesh, int[][] faceIndexGroups, ref FullBookWrapRegion fullBookWrapRegion)
    {
        Dictionary<int[], UVRect> faceRegionMap = new Dictionary<int[], UVRect>
        {
            { faceIndexGroups[4], fullBookWrapRegion.backRegion },
            { faceIndexGroups[2], fullBookWrapRegion.spineRegion },
            { faceIndexGroups[5], fullBookWrapRegion.coverRegion }
        };

        Vector2[] uvs = mesh.uv;
        Dictionary<int, Vector2> oldVertUVDict = new Dictionary<int, Vector2>();
        Dictionary<int, Vector2> newVertUVDict = new Dictionary<int, Vector2>();
        foreach (KeyValuePair<int[], UVRect> face in faceRegionMap)
        {
            foreach (int vertexIndex in face.Key)
            {
                Vector2 oldUV = uvs[vertexIndex];
                oldVertUVDict.Add(vertexIndex, oldUV);
                float newU = Mathf.Lerp(face.Value.uMin, face.Value.uMax, oldUV.x);
                float newV = Mathf.Lerp(face.Value.vMin, face.Value.vMax, oldUV.y);
                uvs[vertexIndex] = new Vector2(newU, newV);
                newVertUVDict.Add(vertexIndex, uvs[vertexIndex]);
            }
        }
        mesh.uv = uvs;
        Debug.Log("UVs rewrapped!");
    }

    private int GetPageCount(EpubMetadataModel bookInfo)
    {
        if (int.TryParse(bookInfo.PageCount, out int intPageCount))
        {
            if (intPageCount == 0)
            {
                return 200; // if you get a page count of 0, default the value to 200
            }
            else
            {
                return intPageCount;
            }
        }
        else
        {
            return 200; // if you fail to parse a page count, default to 200
        }
    }

    private float ComputeSpineFraction(int pageCount)
    {
        float rawPage = pageCount * pageCountScalar;
        return Mathf.Clamp(rawPage, minSpineValue, maxSpineValue);
    }

    private FullBookWrapRegion BuildRegions()
    {
        spineWidthFraction = ComputeSpineFraction(this.pageCount);
        backWidthFraction = frontWidthFraction = (1.0f - spineWidthFraction) / 2.0f;

        UVRect backRegion = new UVRect(umin: 0.0f, umax: backWidthFraction, vmin: 0.0f, vmax: 1.0f);
        UVRect spineRegion = new UVRect(umin: backRegion.uMax, umax: backRegion.uMax + spineWidthFraction, vmin: 0.0f, vmax: 1.0f);
        UVRect coverRegion = new UVRect(umin: spineRegion.uMax, umax: 1.0f, vmin: 0.0f, vmax: 1.0f);

        return new FullBookWrapRegion(backRegion, spineRegion, coverRegion);
    }
    public Texture2D LoadCoverTexture(string coverImageHref)
    {
        if (File.Exists(coverImageHref))
        {
            byte[] bytes = File.ReadAllBytes(coverImageHref);
            Texture2D tex = new Texture2D(2, 2);
            if (!tex.LoadImage(bytes))
            {
                Debug.LogError($"Unity could not load image: {coverImageHref}");
                Destroy(tex);
                return null;
            }

                Debug.Log($"Successfully loaded cover: {tex.width}x{tex.height}");

            return tex;
        }
        else
        {
            return null;
        }
    }
    public Texture2D BuildAtlasTexture(Texture2D coverTexture, int atlasWidth, int atlasHeight, UVRect coverRegion, Color fillerColor)
    {
        RenderTexture rt = new RenderTexture(atlasWidth, atlasHeight, depth: 0);
        rt.Create();
        RenderTexture.active = rt;

        GL.Clear(clearDepth: true, clearColor: true, backgroundColor: fillerColor);

        if (coverTexture != null)
        {
            Rect destRect = new Rect(
                x: coverRegion.uMin * atlasWidth,
                y: coverRegion.vMin * atlasHeight,
                width: (coverRegion.uMax - coverRegion.uMin) * atlasWidth,
                height: (coverRegion.vMax - coverRegion.vMin) * atlasHeight
                );
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, atlasWidth, atlasHeight, 0);
            Graphics.DrawTexture(destRect, coverTexture);
            GL.PopMatrix();
        }

        // read the RenderTexture back into an actual Texture2D
        Texture2D resultTexture = new Texture2D(atlasWidth, atlasHeight);
        resultTexture.ReadPixels(new Rect(0, 0, atlasWidth, atlasHeight), 0, 0);
        resultTexture.Apply();

        RenderTexture.active = null; // deactivate the RenderTexture to manage the GPU resource
        rt.Release(); // have to release the GPU resources so we don't leak memory
        Destroy(rt);
        if (coverTexture != null)
        {
            Destroy(coverTexture);
        }
        return resultTexture;
    }

    public void ApplyAtlasToBookWrapSubmesh(MeshRenderer meshRenderer, Texture2D atlas)
    {
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(propBlock, materialIndex: 0); // preserve anyything already set
        propBlock.SetTexture("_BaseMap", atlas); // so it uses URP's Lit/Unlit shaders
        meshRenderer.SetPropertyBlock(propBlock, materialIndex: 0);
    }

    public void SetBookCover(EpubMetadataModel bookInfo, Mesh mesh, MeshRenderer mr, UVRect coverRegion) // i bet we could change this to passing in the full wrap later?
    {
        float totalWrapWidth = (2 * mesh.bounds.size.z) + mesh.bounds.size.x; // back + cover + spine
        float atlasPhysicalHeight = mesh.bounds.size.y;

        int atlasWidth = Mathf.RoundToInt(totalWrapWidth * this.texelsPerUnit);
        int atlasHeight = Mathf.RoundToInt(atlasPhysicalHeight * this.texelsPerUnit);

        Texture2D coverTexture = LoadCoverTexture(bookInfo.CoverImageHref);
        Color defaultFillColor = GetDominantColor(coverTexture, sampleStep, bucketSize);
        Texture2D atlasTexture = BuildAtlasTexture(coverTexture, atlasWidth, atlasHeight, coverRegion, defaultFillColor);
        ApplyAtlasToBookWrapSubmesh(mr, atlasTexture);
    }
    /// <summary>
    /// Get's the main color of a given section of the image and puts it into buckets; instead of sampling every color, we're doing it based on the sampleStep
    /// i.e. if the sampleStep is 4, you read 1 out of every 16 pixels (so where every 4th column and 4th row intersect); increase for higher performance but lower accuracy
    /// </summary>
    /// <param name="coverTexture"></param>
    /// <param name="sampleStep"></param>
    /// <param name="bucketSize"></param>
    /// <returns></returns>
    public Color GetDominantColor(Texture2D coverTexture, int sampleStep, int bucketSize) // currently finding a problem where there are some images that the most common color isn't picked...
    {
        Dictionary<Color, int> buckets = new Dictionary<Color, int>();
        for (int y = 0; y < coverTexture.height; y += sampleStep)
        {
            for (int x = 0; x < coverTexture.width; x += sampleStep)
            {
                Color pixel = coverTexture.GetPixel(x, y);
                Color bucketedColor = GeneralizePixelsToColorBuckets(pixel, bucketSize);

                if (buckets.ContainsKey(bucketedColor))
                {
                    buckets[bucketedColor] += 1;
                }
                else
                {
                    buckets[bucketedColor] = 1;
                }
            }
        }
        return buckets.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
    }
    /// <summary>
    /// Generally, we're asking roughly which bucket would this pixel fall into? and sorting based on that instead of a unique bin every time
    /// Each bin is represented by its lowest boundary
    /// </summary>
    /// <param name="pixel"></param>
    /// <param name="bucketSize"></param>
    /// <returns></returns>
    public Color GeneralizePixelsToColorBuckets(Color pixel, int bucketSize)
    {
        var r = Mathf.Floor(pixel.r * 255 / bucketSize) * bucketSize / 255;
        var g = Mathf.Floor(pixel.g * 255 / bucketSize) * bucketSize / 255;
        var b = Mathf.Floor(pixel.b * 255 / bucketSize) * bucketSize / 255;
        return new Color(r, g, b);
    }
}
