using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class EchoSonarOcclusionCheck
{
    [InitializeOnLoadMethod]
    private static void Resume()
    {
        const string request = "Temp/EchoSonarOcclusionCheck.request";
        if(!File.Exists(request)) return;
        EditorApplication.delayCall += () =>
        {
            File.Delete(request);
            try { Validate(); } catch(Exception e) { Debug.LogException(e); }
        };
    }

    [MenuItem("Echo/Validate Sonar Shadows and Reflection")]
    public static void Validate()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        var previousScene = SceneManager.GetActiveScene();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        var root = new GameObject("Acoustic test geometry");
        SceneManager.MoveGameObjectToScene(root, scene);
        var material = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/3D/Environment/Underwater/Materials/Basalt.mat"));
        var ringMaterial = new Material(Resources.Load<Material>("SonarRing"));
        var atlas = new Texture2D(SonarPropagation.Samples, WorldSonarPulse.MaxPulses, TextureFormat.RGBAFloat, false, true)
        { filterMode = FilterMode.Point, wrapModeU = TextureWrapMode.Repeat, wrapModeV = TextureWrapMode.Clamp };
        var target = new RenderTexture(256,256,24);
        var readback = new Texture2D(256,256,TextureFormat.RGB24,false);
        var previous = RenderTexture.active;
        // Both probes lie inside ONE flat triangle, with identical normals and material.
        var roof = new Mesh { name = "Sonar partial single triangle" };
        roof.vertices = new[] {
            new Vector3(-0.5f,0,-0.5f),new Vector3(-0.5f,0,0.5f),new Vector3(0.5f,0,0)
        };
        roof.triangles = new[]{0,1,2};
        roof.RecalculateNormals(); roof.RecalculateBounds();
        GameObject Cube(string name, Vector3 position, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(root.transform, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            go.GetComponent<MeshFilter>().sharedMesh = roof;
            return go;
        }
        var wall = Cube("Blocking wall",Vector3.zero,new Vector3(1,4,8));
        var hidden = Cube("Object behind wall",new Vector3(4,0,0),new Vector3(2,2,2));
        var side = Cube("Object outside shadow",new Vector3(0,0,7),new Vector3(2,2,2));
        var cameraObject = new GameObject("Acoustic check camera",typeof(Camera));
        SceneManager.MoveGameObjectToScene(cameraObject,scene);
        var camera = cameraObject.GetComponent<Camera>();
        camera.scene=scene;
        camera.transform.SetPositionAndRotation(new Vector3(0,25,0),Quaternion.Euler(90,0,0));
        camera.orthographic=true; camera.orthographicSize=12;
        camera.backgroundColor=new Color(2/255f,12/255f,19/255f,1);
        camera.clearFlags=CameraClearFlags.SolidColor; camera.allowHDR=false; camera.allowMSAA=false;
        camera.targetTexture=target;
        Color32[] Render()
        {
            camera.Render(); RenderTexture.active=target;
            readback.ReadPixels(new Rect(0,0,256,256),0,0); readback.Apply();
            return readback.GetPixels32();
        }
        Color32 Pixel(Color32[] pixels, Vector3 world)
        {
            Vector3 screen=camera.WorldToScreenPoint(world);
            return pixels[Mathf.FloorToInt(screen.y)*256+Mathf.FloorToInt(screen.x)];
        }
        try
        {
            var physics=scene.GetPhysicsScene();
            Physics.SyncTransforms();
            Vector3 origin=new Vector3(-6,0,0);
            var profile=SonarPropagation.Trace(physics,origin,20,4,obstacleFilter:c=>c.transform.IsChildOf(root.transform));
            if(Mathf.Abs(profile.mask[0].r-5.5f)>0.02f) throw new Exception("Direct wave did not stop at the near wall face.");
            if(profile.IsVisible(hidden.transform.position,SonarPropagation.ObjectId(hidden.GetComponent<Collider>())))
                throw new Exception("Object behind wall is acoustically visible.");
            if(!profile.IsVisible(side.transform.position,SonarPropagation.ObjectId(side.GetComponent<Collider>())))
                throw new Exception("Unobstructed side object was hidden.");
            if(!profile.IsVisible(wall.transform.position,SonarPropagation.ObjectId(wall.GetComponent<Collider>())))
                throw new Exception("The hit object itself became unreadable.");
            float wallId = SonarPropagation.ObjectId(wall.GetComponent<Collider>());
            if(profile.SurfaceVisibility(new Vector3(-0.25f,0,0),wallId) < 0.99f
                || profile.SurfaceVisibility(new Vector3(0.25f,0,0),wallId) > 0)
                throw new Exception("Spatial coverage must reveal only the near portion of the struck object.");
            float transition = profile.SurfaceVisibility(new Vector3(-0.1f,0,0),wallId);
            if(transition <= 0 || transition >= 1)
                throw new Exception("Spatial boundary did not feather within the same flat face.");
            if(profile.reflections.Count==0) throw new Exception("No reflection was planned.");
            var reflected = new SonarPropagation.Reflection
            { origin = new Vector3(-0.58f,0,0), direction = SonarPropagation.Reflect(Vector3.right,Vector3.left), distance = 5.5f };
            if(reflected.direction.x>-0.99f)
                throw new Exception("Normal-incidence reflection did not return toward the source.");
            Vector3 incident=new Vector3(1,0,0.5f).normalized;
            Vector3 oblique=SonarPropagation.Reflect(incident,Vector3.left);
            if(Mathf.Abs(oblique.x+incident.x)>0.001f || Mathf.Abs(oblique.z-incident.z)>0.001f)
                throw new Exception("Reflection did not preserve the angle of incidence.");
            var echo=SonarPropagation.Trace(physics,reflected.origin,20-reflected.distance,0,reflected.direction,12,c=>c.transform.IsChildOf(root.transform));
            if(!echo.IsVisible(new Vector3(-8,0,0)) || echo.IsVisible(new Vector3(4,0,0)))
                throw new Exception("Reflected wave leaked to the far side of the wall.");

            Shader.SetGlobalInt("_EchoSonarCount",0); Shader.SetGlobalInt("_EchoOcclusionEnabled",0);
            Color32[] background=Render();
            var origins=new Vector4[WorldSonarPulse.MaxPulses];
            var waves=new Vector4[WorldSonarPulse.MaxPulses];
            var colors=new Vector4[WorldSonarPulse.MaxPulses];
            var sectors=new Vector4[WorldSonarPulse.MaxPulses];
            var settings=new WorldSonarPulse.Settings { range=20,duration=2 };
            origins[0]=new Vector4(origin.x,origin.z,20,0); waves[0]=settings.Wave(1.4f);
            colors[0]=new Vector4(0,0,1,1); sectors[0]=new Vector4(0,0,-1,0);
            atlas.SetPixels(0,0,SonarPropagation.Samples,1,profile.mask); atlas.Apply();
            Shader.SetGlobalVectorArray("_EchoSonarOrigins",origins); Shader.SetGlobalVectorArray("_EchoSonarWaves",waves);
            Shader.SetGlobalVectorArray("_EchoSonarColors",colors); Shader.SetGlobalVectorArray("_EchoSonarSectors",sectors);
            Shader.SetGlobalTexture("_EchoSonarOcclusion",atlas); Shader.SetGlobalInt("_EchoSonarCount",1);
            Color32[] unblocked=Render();
            Vector3 behindPixel=new Vector3(3.5f,0,0),wallPixel=new Vector3(-0.25f,0,0),sidePixel=new Vector3(-0.5f,0,7);
            Vector3 backPixel = new Vector3(0.25f,0,0);
            if(Pixel(unblocked,behindPixel).Equals(Pixel(background,behindPixel))) throw new Exception("Behind-wall render probe was invalid.");
            Shader.SetGlobalInt("_EchoOcclusionEnabled",1);
            Color32[] blocked=Render();
            if(!Pixel(blocked,behindPixel).Equals(Pixel(background,behindPixel))) throw new Exception("Shader revealed the object behind the wall.");
            if(Pixel(blocked,wallPixel).Equals(Pixel(background,wallPixel))) throw new Exception("Shader failed to reveal the struck wall.");
            if(Pixel(blocked,sidePixel).Equals(Pixel(background,sidePixel))) throw new Exception("Shader hid the unobstructed side object.");
            if(!Pixel(blocked,backPixel).Equals(Pixel(background,backPixel)))
                throw new Exception("The far pixels of a single flat triangle lit up with its near pixels.");

            // Even a line drawn above the wall must disappear in its acoustic shadow.
            material.SetFloat("_SonarObject",0);
            var line=new GameObject("Blocked sonar line",typeof(LineRenderer)).GetComponent<LineRenderer>();
            line.transform.SetParent(root.transform,false); line.sharedMaterial=ringMaterial;
            line.useWorldSpace=true; line.widthMultiplier=0.2f; line.positionCount=2;
            line.SetPositions(new[]{new Vector3(4,4,-1),new Vector3(4,4,1)});
            line.startColor=line.endColor=Color.red;
            Color32[] blockedLine=Render();
            for(int i=0;i<blockedLine.Length;i++) if(!blockedLine[i].Equals(background[i]))
                throw new Exception("Visible sonar line continued through the wall.");
            Shader.SetGlobalInt("_EchoOcclusionEnabled",0);
            Color32[] freeLine=Render(); int changed=0;
            for(int i=0;i<freeLine.Length;i++) if(!freeLine[i].Equals(background[i])) changed++;
            if(changed<10) throw new Exception("Line clipping render probe was invalid.");

            // A tilted reflector provides an indirect path around a shorter blocking wall.
            // The target is dark directly, dark before the echo arrives, then lit by the echo alone.
            line.enabled = false;
            material.SetFloat("_SonarObject",1);
            wall.transform.localScale = new Vector3(1,4,4);
            var mirror = Cube("Angled reflecting surface",new Vector3(-2,0,5),new Vector3(5,4,0.25f));
            Vector3 incoming = (mirror.transform.position-origin).normalized;
            Vector3 outgoing = (hidden.transform.position-mirror.transform.position).normalized;
            Vector3 mirrorNormal = (incoming-outgoing).normalized;
            mirror.transform.rotation = Quaternion.LookRotation(mirrorNormal,Vector3.up);
            Physics.SyncTransforms();
            var directProfile = SonarPropagation.Trace(physics,origin,20,8,obstacleFilter:c=>c.transform.IsChildOf(root.transform));
            float hiddenId = SonarPropagation.ObjectId(hidden.GetComponent<Collider>());
            if(directProfile.IsVisible(hidden.transform.position,hiddenId))
                throw new Exception("Indirect target has an unintended direct path.");
            SonarPropagation.Reflection path = null;
            SonarPropagation.Profile reflectedProfile = null;
            foreach(var candidate in directProfile.reflections)
            {
                var candidateProfile = SonarPropagation.Trace(physics,candidate.origin,20-candidate.distance,0,
                    candidate.direction,settings.reflectionHalfAngle,c=>c.transform.IsChildOf(root.transform));
                if(candidate.targetId==hiddenId && candidateProfile.IsVisible(hidden.transform.position,hiddenId))
                { path=candidate; reflectedProfile=candidateProfile; break; }
            }
            if(path==null) throw new Exception("No reflected path discovered the hidden target.");
            atlas.SetPixels(0,0,SonarPropagation.Samples,1,directProfile.mask);
            atlas.SetPixels(0,1,SonarPropagation.Samples,1,reflectedProfile.mask); atlas.Apply();
            waves[0] = settings.Wave(1.8f);
            Shader.SetGlobalVectorArray("_EchoSonarWaves",waves);
            Shader.SetGlobalInt("_EchoSonarCount",1); Shader.SetGlobalInt("_EchoOcclusionEnabled",1);
            Color32[] directOnly = Render();
            if(!Pixel(directOnly,behindPixel).Equals(Pixel(background,behindPixel)))
                throw new Exception("Target was revealed without the indirect echo.");
            origins[1] = new Vector4(path.origin.x,path.origin.z,20-path.distance,0);
            colors[1] = colors[0]*settings.reflectionStrength;
            sectors[1] = new Vector4(path.direction.x,path.direction.z,Mathf.Cos(settings.reflectionHalfAngle*Mathf.Deg2Rad),0);
            float fullPath = path.distance+Vector3.Distance(path.origin,hidden.transform.position);
            waves[1] = settings.Wave((fullPath-0.5f)/10,path.distance);
            Shader.SetGlobalVectorArray("_EchoSonarOrigins",origins); Shader.SetGlobalVectorArray("_EchoSonarColors",colors);
            Shader.SetGlobalVectorArray("_EchoSonarSectors",sectors); Shader.SetGlobalVectorArray("_EchoSonarWaves",waves);
            Shader.SetGlobalInt("_EchoSonarCount",2);
            if(!Pixel(Render(),behindPixel).Equals(Pixel(background,behindPixel)))
                throw new Exception("Reflected echo revealed its target before traversing the full path.");
            waves[1] = settings.Wave(1.8f,path.distance);
            Shader.SetGlobalVectorArray("_EchoSonarWaves",waves);
            if(Pixel(Render(),behindPixel).Equals(Pixel(background,behindPixel)))
                throw new Exception("Reflected echo failed to reveal the indirectly reachable target.");
            if(!Pixel(Render(),new Vector3(4.5f,0,0)).Equals(Pixel(background,new Vector3(4.5f,0,0))))
                throw new Exception("Reflected echo lit the far portion of its target.");
            Debug.Log("SONAR PARTIAL TRIANGLE PASSED: near pixels visible and far pixels exactly hidden on ONE flat triangle, including indirect reflected illumination.");
            Debug.Log("SONAR INDIRECT REVEAL PASSED: target blocked directly, discovered by an angled reflection, dark before total travel time, visible through the reflected shader pulse.");
            Debug.Log("SONAR OCCLUSION PASSED: first-hit blocking, visible struck object, hidden object behind, open side visibility, correct normal/oblique reflection, no forward echo leak, shader shadowing and clipped travelling line.");
        }
        finally
        {
            Shader.SetGlobalInt("_EchoSonarCount",0); Shader.SetGlobalInt("_EchoOcclusionEnabled",0);
            RenderTexture.active=previous; camera.targetTexture=null;
            EditorSceneManager.CloseScene(scene,true);
            if(previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
            Object.DestroyImmediate(material); Object.DestroyImmediate(ringMaterial); Object.DestroyImmediate(atlas);
            Object.DestroyImmediate(target); Object.DestroyImmediate(readback);
            Object.DestroyImmediate(roof);
        }
    }
}
