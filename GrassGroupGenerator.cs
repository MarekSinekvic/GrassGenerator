using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GrassGroupGenerator : MonoBehaviour
{
    public GameObject grassBladePrefab;
    public float generateRadius = 1f;
    public int grassCount = 1;

    public Texture2D grassSpread;
    public float placingBound = 0.5f;
    public float placingPower = 0.1f;
    public bool regen = false;

    [Header("Grass parameters")]
    public Vector2 biasRange;
    public float biasZeroFrequency=3;
    public AnimationCurve bladeLengthRandomFunc;
    public float LODs = 0.1f, LODsPower = 1;


    void Update()
    {
        if (regen) {
            GenerateGroup();
            regen = false;
        }
    }

    float biasRandom() {
        float v = 1-Mathf.Pow(2,-Mathf.Pow(Random.value/biasZeroFrequency,2));
        return v;
    }
    public void GenerateGroup() {
        var bladeGenerator = GetComponent<GrassBladeGenerator>();

        List<CombineInstance> combine = new List<CombineInstance>();
        for (int i = 0; i < grassCount; i++) {
            Vector2 a = new Vector2(-1+Random.value*2,-1+Random.value*2);//Random.insideUnitCircle*generateRadius
            Vector3 pos = new Vector3(a.x,0,a.y)*generateRadius/2;

            // if (grassSpread.GetPixel((int)((a.x+1)/2*grassSpread.width),(int)((a.y+1)/2*grassSpread.height)).r < placingBound) continue;
            if (Mathf.Pow(grassSpread.GetPixel((int)((a.x+1)/2*grassSpread.width),(int)((a.y+1)/2*grassSpread.height)).r*Random.value,placingPower) < placingBound) continue;
            
            bladeGenerator.rotation = Random.value*2*Mathf.PI;
            bladeGenerator.offset = pos;
            bladeGenerator.biasIntens = (biasRandom()+biasRange.x)*(biasRange.y-biasRange.x);
            bladeGenerator.bladeLength = bladeLengthRandomFunc.Evaluate(Random.value);
            Mesh _m = bladeGenerator.GenerateBlade();
            
            CombineInstance c = new CombineInstance();
            c.mesh = _m;
            c.transform = Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one);
            combine.Add(c);
        }
        var m = new Mesh();
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.CombineMeshes(combine.ToArray());
        m.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = m;
        // return;
        // for (int i = 0; i < grassCount; i++) {
        //     Vector3 pos = new Vector3(-1+Random.value*2,0,-1+Random.value*2).normalized*Random.value*generateRadius;
        //     GameObject o = Instantiate(grassBladePrefab,transform.position+pos,Quaternion.Euler(0,Random.value*360,0));
            
        //     var bladeGenerator = o.GetComponent<GrassBladeGenerator>();
        //     bladeGenerator.biasIntens = (biasRandom()+biasRange.x)*(biasRange.y-biasRange.x);

        //     o.GetComponent<MeshFilter>().mesh = bladeGenerator.GenerateBlade();
        //     o.transform.parent = transform;
        // }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position,new Vector3(generateRadius,0.01f,generateRadius));
    }
}
