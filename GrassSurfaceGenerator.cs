using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GrassSurfaceGenerator : MonoBehaviour
{
    public GameObject grassGrouoPrefab;
    public float groupsSize;
    public int LODsCount = 0;
    public AnimationCurve grassDensity;
    public AnimationCurve bladeVerticesCount;
    public float grassDensityIntens;
    [Header("Spread")]
    public Texture2D grassSpread;
    public float spreadBounds = 0.4f;
    public float spreadPower = 0.1f;

    public bool combineMeshes = true;
    public bool regen = false;
    void Start()
    {
        
    }

    void Update()
    {
        if (regen) {
            for (int i = transform.childCount; i >= 0 && transform.childCount > 0; --i) {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
            GenerateGrassQuads();
            if (combineMeshes) {
                List<CombineInstance> combine = new List<CombineInstance>();
                for (int i = 0; i < transform.childCount; i++) {
                    GameObject g = transform.GetChild(i).gameObject;
                    CombineInstance c = new CombineInstance();
                    c.mesh = g.GetComponent<MeshFilter>().sharedMesh;
                    c.transform = Matrix4x4.TRS(g.transform.localPosition,Quaternion.identity,Vector3.one);
                    combine.Add(c);
                }
                var m = new Mesh();
                m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                m.CombineMeshes(combine.ToArray());
                m.RecalculateNormals();
                GetComponent<MeshFilter>().sharedMesh = m;
                for (int i = transform.childCount; i >= 0 && transform.childCount > 0; --i) {
                    DestroyImmediate(transform.GetChild(0).gameObject);
                } 
            }
            regen = false;
        }
    }
    Texture2D getTextureArea(Texture2D tex, RectInt copyArea) {
        Color[] pixels = tex.GetPixels(copyArea.x,copyArea.y,copyArea.width,copyArea.height);
        Texture2D newTex = new Texture2D(copyArea.width,copyArea.height);
        
        newTex.SetPixels(0,0,newTex.width,newTex.height,pixels);

        return newTex;
    }
    void GenerateGrassQuads() {
        GrassGroupGenerator grassGroup = grassGrouoPrefab.GetComponent<GrassGroupGenerator>();
        for (float i = 0; i < LODsCount; i++) {
            for (float j = 0; j < LODsCount; j++) {
                Vector2 offset = new Vector2(j-LODsCount/2,i-LODsCount/2)*groupsSize;
                var obj = Instantiate(grassGrouoPrefab,transform.position+new Vector3(offset.x,0,offset.y),Quaternion.identity);
                var grassBladeGenerator = obj.GetComponent<GrassBladeGenerator>();
                float newVerts = bladeVerticesCount.Evaluate(offset.magnitude/LODsCount)*grassBladeGenerator.verticesCount;
                grassBladeGenerator.verticesCount = (int)newVerts;

                var groupGenerator = obj.GetComponent<GrassGroupGenerator>();
                groupGenerator.grassSpread = getTextureArea(grassSpread,new RectInt((int)(j/LODsCount*grassSpread.width),(int)(i/LODsCount*grassSpread.height),(int)((float)grassSpread.width/LODsCount),(int)((float)grassSpread.height/LODsCount)));
                groupGenerator.placingBound = spreadBounds;
                groupGenerator.placingPower = spreadPower;
                
                groupGenerator.grassCount = (int)(grassDensity.Evaluate(offset.magnitude/LODsCount)*grassDensityIntens);
                groupGenerator.generateRadius = groupsSize;
                groupGenerator.GenerateGroup();

                obj.transform.parent = transform;
            }
        }
    }
}
