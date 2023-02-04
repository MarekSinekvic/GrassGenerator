using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GrassBladeGenerator : MonoBehaviour
{
    [Header("Blade parameters")]
    public float bladeWidth;
    public float bendingPower;
    public float bladeLength;
    public float rotation = 0;
    public Vector3 offset = Vector3.zero;
    [Header("Blade bias")]
    public float biasPower;
    public float biasIntens;

    [Header("Mesh parameters")]
    public int verticesCount;

    public bool regen = false;
    
    private void Update() {
        if (regen) {
            Mesh m = GenerateBlade();
            GetComponent<MeshFilter>().mesh = m;
            regen = false;
        }
    }
    private float biasFunc(float x) {
        return biasIntens*Mathf.Pow(x,biasPower);
        return (1-Mathf.Pow(biasPower,x))*biasIntens;
        return 1-Mathf.Pow(biasPower,-x);
    }
    private float bendingLevel(float y) {
        // return (1-Mathf.Pow(y*bendingPower,2))*biasIntens;
        // return 1/(1+Mathf.Pow(y*bendingPower,2));
        return Mathf.Pow(bendingPower,-y/bladeLength);
    }
    public Mesh GenerateBlade() {
        Mesh _mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();

        for (int i = 0; i < verticesCount; i++) {
            float y = (float)i/(verticesCount+1) * bladeLength;
            float x = bendingLevel(y)*bladeWidth;
            float z = biasFunc((float)i/(verticesCount+1));
            // vertices.Add(new Vector3(-x*bladeWidth/2+offset.x,y+offset.y,z+offset.z));
            // vertices.Add(new Vector3(x*bladeWidth/2+offset.x,y+offset.y,z+offset.z));
            Vector3 pos = new Vector3(Mathf.Cos(rotation),0,Mathf.Sin(rotation))*x/2;
            vertices.Add(-pos+new Vector3(0,y,0)+offset+new Vector3(-Mathf.Sin(rotation),0,Mathf.Cos(rotation))*z);
            vertices.Add(pos+new Vector3(0,y,0)+offset+new Vector3(-Mathf.Sin(rotation),0,Mathf.Cos(rotation))*z);

            uv.Add(new Vector2(0,y));
            uv.Add(new Vector2(1,y));
        }
        vertices.Add(new Vector3(-Mathf.Sin(rotation)*biasFunc(1),bladeLength,Mathf.Cos(rotation)*biasFunc(1))+offset);
        uv.Add(new Vector2(0.5f,1));
        
        for (int i = 0; i < vertices.Count-2; i++) {
            if (((i+1)%2) == 0) continue;
            // if (i > vertices.Count-2) continue;
        
            triangles.AddRange(new int[] {
                i,
                i+1,
                i+2
            });
            
            // triangles.AddRange(new int[] {
            //     i+1,
            //     i,
            //     i+2
            // });
            if (i==vertices.Count-3) continue;
            triangles.AddRange(new int[] {
                i+2,
                i+1,
                i+3
            });
            // triangles.AddRange(new int[] {
            //     i+3,
            //     i+2,
            //     i+1,
            // });
        }


        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.uv = uv.ToArray();

        _mesh.RecalculateNormals();
        _mesh.RecalculateTangents();

        return _mesh;
    }
}
