using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

/// <summary>Builds saved visual assets only. Never changes player colliders or gameplay components.</summary>
public static class EchoSubmarineVisualBuilder
{
    private const string Root = "Assets/Art/3D/Players/Submarine";

    [MenuItem("Echo/Build Submarine Visual Assets")]
    public static void Build()
    {
        System.IO.Directory.CreateDirectory(Root+"/Meshes");
        System.IO.Directory.CreateDirectory(Root+"/Materials");
        System.IO.Directory.CreateDirectory(Root+"/Prefabs");
        AssetDatabase.Refresh();
        var vertices=new List<Vector3>();
        var triangles=new List<int>();
        var colors=new List<Color>();
        void Vertex(Vector3 v,float accent) { vertices.Add(v);colors.Add(new Color(accent,0,0,1)); }
        void Hull(float[] stations,float[] radii,Vector3 center,float verticalScale,float accent)
        {
            const int sides=32;
            int start=vertices.Count;
            for(int j=0;j<stations.Length;j++) for(int i=0;i<sides;i++)
            {
                float angle=i*Mathf.PI*2/sides;
                Vertex(center+new Vector3(Mathf.Cos(angle)*radii[j],stations[j],Mathf.Sin(angle)*radii[j]*verticalScale),accent);
            }
            for(int j=0;j<stations.Length-1;j++) for(int i=0;i<sides;i++)
            {
                int a=start+j*sides+i,b=a+sides,c=start+j*sides+(i+1)%sides,d=c+sides;
                triangles.AddRange(new[]{a,b,c,c,b,d});
            }
            int nose=vertices.Count;Vertex(center+new Vector3(0,stations[0],0),accent);
            int tail=vertices.Count;Vertex(center+new Vector3(0,stations[stations.Length-1],0),accent);
            for(int i=0;i<sides;i++)
            {
                int next=(i+1)%sides;
                triangles.AddRange(new[]{nose,start+i,start+next,tail,start+(stations.Length-1)*sides+next,start+(stations.Length-1)*sides+i});
            }
        }
        Hull(new[]{-1.05f,-0.98f,-0.8f,-0.45f,0f,0.4f,0.7f,0.95f,1.08f},
            new[]{0.025f,0.2f,0.35f,0.43f,0.44f,0.36f,0.23f,0.13f,0.05f},Vector3.zero,0.72f,0);
        // Local Y points aft; the existing Body rotation maps the bow to player forward.
        Hull(new[]{-0.34f,-0.26f,0f,0.26f,0.34f},new[]{0.015f,0.09f,0.12f,0.09f,0.015f},new Vector3(0,-0.1f,0.34f),1.6f,0.25f);
        void Fin(float side)
        {
            var shape=new[]{new Vector3(side*0.16f,0.55f,0),new Vector3(side*0.6f,0.88f,0),new Vector3(side*0.5f,1.09f,0),new Vector3(side*0.12f,0.95f,0)};
            int start=vertices.Count;
            Vector3 center=(shape[0]+shape[1]+shape[2]+shape[3])/4;
            for(int i=0;i<4;i++)Vertex(shape[i]+Vector3.back*0.025f,0.65f);
            for(int i=0;i<4;i++)Vertex(Vector3.Lerp(center,shape[i],0.8f)+Vector3.forward*0.06f,0.65f);
            void Tri(int a,int b,int c)
            {
                if(side<0)triangles.AddRange(new[]{start+a,start+c,start+b});
                else triangles.AddRange(new[]{start+a,start+b,start+c});
            }
            Tri(4,5,6);Tri(4,6,7);Tri(0,2,1);Tri(0,3,2);
            for(int i=0;i<4;i++){int n=(i+1)%4;Tri(i,n,i+4);Tri(n,n+4,i+4);}
        }
        Fin(1);Fin(-1);
        var mesh=new Mesh{name="Sonar Submarine Hull"};
        mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetColors(colors);mesh.RecalculateNormals();mesh.RecalculateBounds();
        string meshPath=Root+"/Meshes/SonarSubmarine.asset";
        var savedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if(savedMesh){EditorUtility.CopySerialized(mesh,savedMesh);Object.DestroyImmediate(mesh);mesh=savedMesh;}
        else AssetDatabase.CreateAsset(mesh,meshPath);
        string materialPath=Root+"/Materials/SonarSubmarine.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(!material){material=new Material(Shader.Find("Echo/Submarine Unlit"));AssetDatabase.CreateAsset(material,materialPath);}
        var model=new GameObject("Sonar Submarine",typeof(MeshFilter),typeof(MeshRenderer));
        model.transform.rotation=Quaternion.Euler(-90,0,0);
        model.GetComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=model.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;
        renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
        PrefabUtility.SaveAsPrefabAsset(model,Root+"/Prefabs/SonarSubmarine.prefab");
        Object.DestroyImmediate(model);AssetDatabase.SaveAssets();
        Debug.Log("SUBMARINE VISUAL ASSETS SAVED: mesh, material and preview prefab; colliders unchanged.");
    }
}
