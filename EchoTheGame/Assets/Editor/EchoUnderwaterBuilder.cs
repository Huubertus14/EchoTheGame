using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Project.Echo.Environment;
using Project.Echo.Spawner;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

/// <summary>Authored once in the editor. All meshes/materials/prefabs remain inspectable assets.</summary>
public static class EchoUnderwaterBuilder
{
    private const string Root = "Assets/Art/3D/Environment/Underwater";
    private static readonly List<Vector3> vertices = new();
    private static readonly List<int> triangles = new();
    [InitializeOnLoadMethod]
    private static void BuildRequestedMap()
    {
        const string request = "Temp/EchoUnderwaterBuild.request";
        if (!File.Exists(request)) return;
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(request);
            for (int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
                if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                { Debug.LogError("Save your scene before building the underwater map."); return; }
            try { Build(); } catch(Exception e) { Debug.LogException(e); }
        };
    }
    private static readonly Vector3[] Spawns = {
        new(-27,0,-12), new(25,0,13), new(-22,0,18), new(22,0,-18),
        new(0,0,25), new(0,0,-25), new(-31,0,4), new(31,0,-3),
        new(-12,0,-8), new(12,0,8), new(-10,0,11), new(10,0,-11)
    };

    [MenuItem("Echo/Build Underwater Test Map")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before building the map.");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(Root + "/Meshes");
        Directory.CreateDirectory(Root + "/Materials");
        Directory.CreateDirectory(Root + "/Prefabs");
        AssetDatabase.Refresh();
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "Environment" || go.name == "Underwater Arena") Object.DestroyImmediate(go);
        var arenaObject = new GameObject("Underwater Arena");
        var arena = arenaObject.AddComponent<UnderwaterArena>();
        // Convex hull of an asymmetric elliptical basin keeps boundary projection well-defined.
        var points = new List<Vector2>();
        for (int i = 0; i < 48; i++)
        {
            float a = i * Mathf.PI * 2 / 48;
            float r = 1 + 0.045f * Mathf.Sin(3*a + 0.4f) + 0.025f * Mathf.Cos(5*a);
            points.Add(new Vector2(Mathf.Cos(a)*42*r, Mathf.Sin(a)*34*r));
        }
        arena.shoreline = Hull(points).ToArray();
        var rockMat = Material("Basalt", new Color(0.055f,0.18f,0.21f), 0.035f);
        var floorMat = Material("Silt", new Color(0.018f,0.065f,0.085f), 0.012f);
        floorMat.SetFloat("_SonarObject", 0);
        EditorUtility.SetDirty(floorMat);
        var wreckMat = Material("Wreck", new Color(0.10f,0.24f,0.25f), 0.022f);
        var rocks = new GameObject[4];
        for (int i = 0; i < rocks.Length; i++)
        {
            Mesh mesh = RockMesh(i);
            var go = MeshObject("Basalt " + (i+1), mesh, rockMat, null, true);
            rocks[i] = Prefab(go, "Basalt_0" + (i+1));
        }
        var floor = MeshObject("Rippled seabed", FloorMesh(), floorMat, arenaObject.transform, false);
        PrefabUtility.SaveAsPrefabAsset(floor, Root + "/Prefabs/Seabed.prefab");
        var rim = new GameObject("Continuous reef boundary").transform;
        rim.SetParent(arenaObject.transform, false);
        for (int i = 0; i < arena.shoreline.Length; i++)
        {
            Vector2 a = arena.shoreline[i], b = arena.shoreline[(i+1)%arena.shoreline.Length];
            Vector2 edge = b-a, outward = new Vector2(edge.y,-edge.x).normalized;
            // Overlapping invisible collision slabs complement the hard gameplay boundary.
            var wall = new GameObject("Reef collision " + i, typeof(BoxCollider));
            wall.transform.SetParent(rim, false);
            Vector2 mid = (a+b)*0.5f + outward*1.4f;
            wall.transform.position = new Vector3(mid.x,0,mid.y);
            wall.transform.rotation = Quaternion.Euler(0,-Mathf.Atan2(edge.y,edge.x)*Mathf.Rad2Deg,0);
            wall.GetComponent<BoxCollider>().size = new Vector3(edge.magnitude+1,10,2.8f);
            int n = Mathf.CeilToInt(edge.magnitude/3);
            for (int j = 0; j < n; j++)
            {
                Vector2 p = Vector2.Lerp(a,b,(j+0.5f)/n) + outward * 2.3f;
                Place(rocks[(i+j)%4], rim, new Vector3(p.x,-0.8f,p.y),
                    new Vector3(3.8f+(i%3)*0.4f,4+(i%4)*0.55f,3.6f), i*37+j*51);
            }
        }
        var islands = new GameObject("Scattered basalt outcrops").transform;
        islands.SetParent(arenaObject.transform,false);
        var rng = new System.Random(24062026);
        int placed = 0;
        for (int attempt = 0; attempt < 1500 && placed < 42; attempt++)
        {
            Vector3 p = new Vector3((float)rng.NextDouble()*74-37, -0.7f, (float)rng.NextDouble()*58-29);
            if (!arena.Contains(p,6) || Spawns.Any(s => Vector2.Distance(new Vector2(s.x,s.z),new Vector2(p.x,p.z)) < 6)) continue;
            if (Vector2.Distance(new Vector2(p.x,p.z), new Vector2(2,-1)) < 10) continue;
            if (islands.Cast<Transform>().Any(t => Vector2.Distance(new Vector2(t.position.x,t.position.z), new Vector2(p.x,p.z)) < 6)) continue;
            float size = 1.5f+(float)rng.NextDouble()*1.4f;
            Place(rocks[placed%4],islands,p,new Vector3(size,2+(float)rng.NextDouble()*1.8f,size*0.8f),rng.Next(360));
            placed++;
        }
        var wreck = Wreck(wreckMat);
        Place(wreck,arenaObject.transform,new Vector3(2,-0.65f,-1),Vector3.one,28);
        var spawner = Object.FindAnyObjectByType<PlayerSpawner>();
        foreach (Transform child in spawner.transform.Cast<Transform>().ToArray()) Object.DestroyImmediate(child.gameObject);
        spawner.transform.position = Vector3.zero;
        for (int i = 0; i < Spawns.Length; i++)
        {
            var spawn = new GameObject("Dive point " + (i+1).ToString("00"),typeof(SpawnPoint));
            spawn.transform.SetParent(spawner.transform,false);
            spawn.transform.position = Spawns[i];
            spawn.transform.rotation = Quaternion.LookRotation(-Spawns[i].normalized,Vector3.up);
        }
        var camera = Camera.main;
        camera.backgroundColor = new Color(2/255f,12/255f,19/255f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        RenderSettings.skybox = null;
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene);
        Validate();
        Preview();
    }

    private static Material Material(string name, Color color, float contour)
    {
        string path = Root + "/Materials/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!mat) { mat = new Material(Shader.Find("Echo/Underwater Unlit")); AssetDatabase.CreateAsset(mat,path); }
        mat.SetColor("_RockColor",color); mat.SetFloat("_Contour",contour);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static GameObject MeshObject(string name, Mesh mesh, Material mat, Transform parent, bool collider)
    {
        var go = new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
        go.transform.SetParent(parent,false);
        go.GetComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = mat; renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
        if (collider) { go.AddComponent<MeshCollider>().sharedMesh = mesh; go.AddComponent<SonarObject>(); }
        return go;
    }

    private static GameObject Prefab(GameObject go, string name)
    {
        var asset = PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+name+".prefab");
        Object.DestroyImmediate(go); return asset;
    }

    private static void Place(GameObject prefab, Transform parent, Vector3 p, Vector3 scale, float angle)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);
        go.transform.localPosition = p; go.transform.localScale = scale; go.transform.localRotation = Quaternion.Euler(0,angle,0);
    }

    private static void Tri(Vector3 a, Vector3 b, Vector3 c)
    {
        int n = vertices.Count; vertices.Add(a); vertices.Add(b); vertices.Add(c);
        triangles.Add(n); triangles.Add(n+1); triangles.Add(n+2);
    }
    private static void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d) { Tri(a,b,c); Tri(a,c,d); }
    private static Mesh SaveMesh(string name)
    {
        var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
        mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
        string path = Root+"/Meshes/"+name+".asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing) { EditorUtility.CopySerialized(mesh,existing); Object.DestroyImmediate(mesh); mesh = existing; }
        else AssetDatabase.CreateAsset(mesh,path);
        vertices.Clear(); triangles.Clear(); return mesh;
    }
    private static Mesh RockMesh(int seed)
    {
        vertices.Clear(); triangles.Clear();
        const int sides = 11, rings = 5;
        var v = new Vector3[rings,sides];
        for (int r = 0; r < rings; r++) for (int i = 0; i < sides; i++)
        {
            float a = i*Mathf.PI*2/sides;
            float shape = new[]{0.62f,1f,0.92f,0.65f,0.22f}[r];
            float noise = 1+Mathf.Sin(i*8.1f+r*3.7f+seed*5.3f)*0.19f;
            v[r,i] = new Vector3(Mathf.Cos(a)*shape*noise, r*0.43f-0.3f + Mathf.Sin(i*3+seed)*0.12f,Mathf.Sin(a)*shape*noise);
        }
        for (int r=0;r<rings-1;r++) for(int i=0;i<sides;i++)
            Quad(v[r,i],v[r+1,i],v[r+1,(i+1)%sides],v[r,(i+1)%sides]);
        for(int i=0;i<sides;i++) { Tri(new Vector3(0,-0.3f,0),v[0,i],v[0,(i+1)%sides]); Tri(new Vector3(0,1.5f,0),v[4,(i+1)%sides],v[4,i]); }
        return SaveMesh("Basalt_0"+(seed+1));
    }
    private static Mesh FloorMesh()
    {
        vertices.Clear(); triangles.Clear();
        Vector3 V(int x,int z) => new Vector3(x,-2.6f + Mathf.Sin(x*0.21f+Mathf.Sin(z*0.13f))*0.24f+Mathf.Cos(z*0.27f+x*0.09f)*0.18f,z);
        for (int x=-66;x<66;x+=2) for(int z=-58;z<58;z+=2) Quad(V(x,z),V(x,z+2),V(x+2,z+2),V(x+2,z));
        return SaveMesh("Rippled_Seabed");
    }
    private static GameObject Wreck(Material mat)
    {
        vertices.Clear(); triangles.Clear();
        // Open, broken hull: two side strips with an exposed keel, no solid deck.
        for(int side=-1;side<=1;side+=2) for(int i=0;i<8;i++)
        {
            if(i==4 && side==1) continue;
            float z0=-6+i*1.5f,z1=z0+1.5f;
            float W(float z) => 0.45f+1.6f*Mathf.Sin((z+6)/12*Mathf.PI);
            Vector3 a=new Vector3(side*W(z0)*0.45f,-1,z0),b=new Vector3(side*W(z1)*0.45f,-1,z1);
            Vector3 c=new Vector3(side*W(z1),1.4f+Mathf.Sin(i*4)*0.35f,z1),d=new Vector3(side*W(z0),1.2f,z0);
            Quad(a,b,c,d); Quad(d,c,b,a);
        }
        var root = new GameObject("Broken survey vessel");
        MeshObject("Open fractured hull",SaveMesh("Wreck_Hull"),mat,root.transform,true);
        // Unity cube is copied to a named mesh asset so even the beams are findable.
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var beam = Object.Instantiate(cube.GetComponent<MeshFilter>().sharedMesh); beam.name="Wreck_Beam";
        Object.DestroyImmediate(cube);
        string beamPath=Root+"/Meshes/Wreck_Beam.asset";
        var existing=AssetDatabase.LoadAssetAtPath<Mesh>(beamPath);
        if(existing) { Object.DestroyImmediate(beam); beam=existing; } else AssetDatabase.CreateAsset(beam,beamPath);
        void Beam(string name,Vector3 p,Vector3 scale,float angle)
        {
            var part=MeshObject(name,beam,mat,root.transform,true);
            part.transform.localPosition=p;part.transform.localScale=scale;part.transform.localRotation=Quaternion.Euler(0,0,angle);
        }
        Beam("Exposed keel",new Vector3(0,-0.9f,0),new Vector3(0.3f,0.35f,11.8f),0);
        for(int i=0;i<6;i++)
        {
            float z=-4.5f+i*1.8f;
            Beam("Port rib "+i,new Vector3(-1.2f,0,z),new Vector3(0.18f,2.4f,0.23f),-25);
            Beam("Starboard rib "+i,new Vector3(1.2f,0,z),new Vector3(0.18f,2.4f,0.23f),25);
        }
        Beam("Fallen mast",new Vector3(2.8f,-0.4f,2),new Vector3(5,0.25f,0.25f),12);
        return Prefab(root,"Broken_Survey_Vessel");
    }

    private static float Cross(Vector2 a,Vector2 b,Vector2 c) => (b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x);
    private static List<Vector2> Hull(List<Vector2> p)
    {
        p=p.OrderBy(v=>v.x).ThenBy(v=>v.y).ToList(); var h=new List<Vector2>();
        foreach(var v in p) { while(h.Count>=2 && Cross(h[^2],h[^1],v)<=0) h.RemoveAt(h.Count-1); h.Add(v); }
        int lower=h.Count;
        for(int i=p.Count-2;i>=0;i--) { while(h.Count>lower && Cross(h[^2],h[^1],p[i])<=0) h.RemoveAt(h.Count-1);h.Add(p[i]); }
        h.RemoveAt(h.Count-1);return h;
    }

    [MenuItem("Echo/Validate Underwater Test Map")]
    public static void Validate()
    {
        var arena=Object.FindAnyObjectByType<UnderwaterArena>();
        if(!arena) throw new Exception("Open the underwater Game scene first.");
        Physics.SyncTransforms();
        var spawns=Object.FindObjectsByType<SpawnPoint>();
        foreach(var spawn in spawns)
        {
            if(!arena.Contains(spawn.transform.position,3)) throw new Exception("Spawn outside safe boundary: "+spawn.name);
            if(Physics.OverlapSphere(spawn.transform.position,2).Any(c=>c.transform.IsChildOf(arena.transform))) throw new Exception("Blocked spawn: "+spawn.name);
            foreach(var other in spawns) if(other!=spawn && Vector3.Distance(spawn.transform.position,other.transform.position)<8) throw new Exception("Spawn spacing too small");
        }
        for(int i=0;i<720;i++)
        {
            float a=i*Mathf.PI/360;
            Vector3 outside=new Vector3(Mathf.Cos(a)*500,0,Mathf.Sin(a)*500);
            if(!arena.Contains(arena.Constrain(outside))) throw new Exception("Boundary projection failed");
            if(!arena.Contains(arena.Move(Vector3.zero,outside))) throw new Exception("Movement escaped boundary");
        }
        int obstacleProbes = 0;
        var outcrops = arena.transform.Find("Scattered basalt outcrops");
        foreach(Transform rock in outcrops)
        {
            Vector3 center = new Vector3(rock.position.x,0,rock.position.z);
            for(int side=0;side<4;side++)
            {
                Vector3 direction = Quaternion.Euler(0,side*90,0)*Vector3.forward;
                Vector3 start = center-direction*6;
                if(!arena.IsSpawnClear(start)) continue;
                Vector3 end = arena.Move(start,center+direction*6);
                if(Vector3.Dot(end-center,direction)>0) throw new Exception("Movement crossed an outcrop: "+rock.name);
                obstacleProbes++;
            }
        }
        if(obstacleProbes<20) throw new Exception("Insufficient obstacle probes");
        foreach(var renderer in arena.GetComponentsInChildren<MeshRenderer>())
        {
            if(!AssetDatabase.Contains(renderer.GetComponent<MeshFilter>().sharedMesh)) throw new Exception("Unsaved mesh");
            if(UnityEditor.ShaderUtil.ShaderHasError(renderer.sharedMaterial.shader)) throw new Exception("Environment shader failed");
        }
        Debug.Log("UNDERWATER VALIDATION PASSED: "+spawns.Length+" clear distributed spawns, 720 boundary/movement probes, "+obstacleProbes+" swept obstacle probes, persistent meshes, unlit shader.");
    }
    [MenuItem("Echo/Preview Underwater Test Map")]
    public static void Preview()
    {
        var go=new GameObject("Preview camera",typeof(Camera));var cam=go.GetComponent<Camera>();
        cam.transform.position=new Vector3(0,100,0);cam.transform.rotation=Quaternion.Euler(90,0,0);
        cam.orthographic=true;cam.orthographicSize=42;cam.backgroundColor=new Color(2/255f,12/255f,19/255f);cam.clearFlags=CameraClearFlags.SolidColor;
        cam.cullingMask=~(1<<5);
        var rt=new RenderTexture(1600,1200,24); cam.targetTexture=rt;
        cam.Render(); var previous=RenderTexture.active;RenderTexture.active=rt;
        var image=new Texture2D(1600,1200,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1200),0,0);image.Apply();
        Directory.CreateDirectory("../docs");File.WriteAllBytes("../docs/underwater-map-preview.png",image.EncodeToPNG());
        RenderTexture.active=previous;cam.targetTexture=null;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);Object.DestroyImmediate(go);
    }
}
