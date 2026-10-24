using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Object = UnityEngine.Object;

public static class EchoSonarVisibilityCheck
{
    [InitializeOnLoadMethod]
    private static void ResumeCheck()
    {
        if(File.Exists("Temp/EchoSonarLiveCheck.request")) EditorApplication.update += PollLiveCheck;
        const string request = "Temp/EchoSonarVisibilityCheck.request";
        if (!File.Exists(request)) return;
        EditorApplication.delayCall += () =>
        {
            File.Delete(request);
            try { Validate(); } catch (Exception e) { Debug.LogException(e); }
        };
    }

    private static float liveStarted = -1;
    private static int livePhase;
    private static Vector3 liveOrigin;
    private static PlayerSonarBehaviour livePlayer;
    private static NetworkObject liveImpact;

    [MenuItem("Echo/Validate Live Sonar Pulse")]
    public static void RequestLiveCheck()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Start a host session first.");
        File.WriteAllText("Temp/EchoSonarLiveCheck.request", "");
        EditorApplication.update -= PollLiveCheck;
        EditorApplication.update += PollLiveCheck;
    }

    private static void PollLiveCheck()
    {
        const string request = "Temp/EchoSonarLiveCheck.request";
        if (!EditorApplication.isPlaying) { liveStarted = -1; return; }
        try
        {
            if (liveStarted < 0)
            {
                if(!File.Exists(request)) return;
                livePlayer = Object.FindAnyObjectByType<PlayerSonarBehaviour>();
                if(!livePlayer || !livePlayer.Object || !livePlayer.HasInputAuthority) return;
                File.Delete(request);
                liveOrigin = livePlayer.transform.position;
                typeof(PlayerSonarBehaviour).GetMethod("RPC_PlayParticleEffect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(livePlayer, new object[]{liveOrigin});
                liveStarted = Time.time;
                livePhase = 0;
                return;
            }
            float age = Time.time - liveStarted;
            if(livePhase == 0 && age > 0.3f)
            {
                var manager = Object.FindAnyObjectByType<WorldSonarPulse>();
                if(!manager || Shader.GetGlobalInt("_EchoSonarCount") < 1) throw new Exception("Player RPC did not create a pulse.");
                var rings = manager.GetComponentsInChildren<LineRenderer>();
                if(rings.Length < 1 || !rings[0].enabled) throw new Exception("Player sonar ring missing.");
                var owner = livePlayer.GetComponentInParent<Project.Echo.Player.PlayerNetworkedController>();
                var sourceColor = owner.GetComponentInChildren<Project.Echo.Player.Visuals.PlayerVisualController>().PlayerColor;
                Color ringColor = rings[0].startColor;
                if(ringColor.r != sourceColor.r || ringColor.g != sourceColor.g || ringColor.b != sourceColor.b)
                    throw new Exception("Main sonar color differs from the submarine color.");
                var origin = Shader.GetGlobalVectorArray("_EchoSonarOrigins")[0];
                var wave = Shader.GetGlobalVectorArray("_EchoSonarWaves")[0];
                Vector3 point = rings[0].GetPosition(0);
                if(Vector2.Distance(new Vector2(origin.x,origin.y),new Vector2(liveOrigin.x,liveOrigin.z))>0.001f
                    || Mathf.Abs(Vector2.Distance(new Vector2(point.x,point.z),new Vector2(liveOrigin.x,liveOrigin.z))-wave.x)>0.001f)
                    throw new Exception("Visible ring and surface reveal disagree about origin/radius.");
                if(ShaderUtil.ShaderHasError(rings[0].sharedMaterial.shader)) throw new Exception("Ring shader failed.");
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Players/Sonar/NetworkedSonar.prefab");
                liveImpact = livePlayer.Runner.Spawn(prefab.GetComponent<NetworkObject>(), liveOrigin + new Vector3(3,0,3));
                liveImpact.GetComponent<SonarImpact>().RPC_UpdateColor(livePlayer.Object.InputAuthority, Vector3.up);
                livePhase = 1;
            }
            if(livePhase == 1 && age > 1.2f)
            {
                if(liveImpact) throw new Exception("Short-lived impact emitter did not despawn.");
                if(Shader.GetGlobalInt("_EchoSonarCount") < 2) throw new Exception("Pulse vanished with impact emitter.");
                var manager = Object.FindAnyObjectByType<WorldSonarPulse>();
                bool reflected = false;
                foreach(var ring in manager.GetComponentsInChildren<LineRenderer>()) if(!ring.loop) reflected = true;
                if(!reflected) throw new Exception("No reflected wave was created in the underwater map.");
                livePhase = 2;
            }
            if(livePhase == 2 && age > 7)
            {
                if(Shader.GetGlobalInt("_EchoSonarCount") != 0) throw new Exception("Sonar did not finish fading.");
                Debug.Log("SONAR LIVE CHECK PASSED: player RPC, matching ring/reveal radius, impact RPC, pulse survives emitter despawn, all pulses expire.");
                liveStarted = -1;
                EditorApplication.update -= PollLiveCheck;
                EditorApplication.isPlaying = false;
            }
        }
        catch(Exception e)
        {
            Debug.LogException(e);
            liveStarted = -1;
            EditorApplication.update -= PollLiveCheck;
            EditorApplication.isPlaying = false;
        }
    }

    [MenuItem("Echo/Validate Sonar Visibility")]
    public static void Validate()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before the isolated sonar render check.");
        // A referenced Resources material retains the ring shader in player builds.
        const string ringPath = "Assets/Resources/SonarRing.mat";
        if (!AssetDatabase.LoadAssetAtPath<Material>(ringPath))
        {
            var ringMaterial = new Material(Shader.Find("Echo/Sonar Ring"));
            AssetDatabase.CreateAsset(ringMaterial, ringPath);
            AssetDatabase.SaveAssets();
        }
        Shader.SetGlobalInt("_EchoSonarCount", 0);
        Shader.SetGlobalInt("_EchoOcclusionEnabled", 0);
        var scene = EditorSceneManager.NewPreviewScene();
        var root = new GameObject("Sonar render check");
        SceneManager.MoveGameObjectToScene(root, scene);
        var cameraObject = new GameObject("Check camera", typeof(Camera));
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
        var camera = cameraObject.GetComponent<Camera>();
        camera.scene = scene;
        camera.transform.SetPositionAndRotation(new Vector3(0,20,0), Quaternion.Euler(90,0,0));
        camera.orthographic = true;
        camera.orthographicSize = 10;
        camera.backgroundColor = new Color(2/255f,12/255f,19/255f,1);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.allowHDR = false;
        camera.allowMSAA = false;
        var target = new RenderTexture(128,128,24);
        camera.targetTexture = target;
        var readback = new Texture2D(128,128,TextureFormat.RGB24,false);
        var previous = RenderTexture.active;
        var materials = new Material[3];
        Mesh uvTestMesh = null;
        var origins = new Vector4[WorldSonarPulse.MaxPulses];
        var waves = new Vector4[WorldSonarPulse.MaxPulses];
        var colors = new Vector4[WorldSonarPulse.MaxPulses];
        var settings = new WorldSonarPulse.Settings();
        void SetWave(float age, int count = 1)
        {
            origins[0] = new Vector4(-9,0,settings.range,0);
            waves[0] = settings.Wave(age);
            colors[0] = new Vector4(0,0,1,1);
            origins[1] = new Vector4(9,0,settings.range,0);
            waves[1] = settings.Wave(age);
            colors[1] = new Vector4(1,0.2f,0.1f,1);
            Shader.SetGlobalVectorArray("_EchoSonarOrigins", origins);
            Shader.SetGlobalVectorArray("_EchoSonarWaves", waves);
            Shader.SetGlobalVectorArray("_EchoSonarColors", colors);
            Shader.SetGlobalInt("_EchoSonarCount", count);
        }
        Color32[] Render()
        {
            camera.Render();
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0,0,128,128),0,0);
            readback.Apply();
            return readback.GetPixels32();
        }
        try
        {
            Color32[] background = Render();
            string folder = "Assets/Art/3D/Environment/Underwater/";
            string[] names = { "Basalt", "Wreck", "Silt" };
            for (int i=0;i<3;i++)
            {
                materials[i] = new Material(AssetDatabase.LoadAssetAtPath<Material>(folder+"Materials/"+names[i]+".mat"));
                if (ShaderUtil.ShaderHasError(materials[i].shader)) throw new Exception("Sonar shader failed compilation.");
                var go = new GameObject(names[i], typeof(MeshFilter), typeof(MeshRenderer));
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3((i-1)*5,0,0);
                go.transform.localScale = new Vector3(2,2,2);
                go.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(folder+"Meshes/Basalt_01.asset");
                go.GetComponent<MeshRenderer>().sharedMaterial = materials[i];
            }
            Color32[] hidden = Render();
            for(int i=0;i<hidden.Length;i++)
                if(!hidden[i].Equals(background[i])) throw new Exception("Unhit geometry differs from the camera background at pixel "+i);
            SetWave(0.6f);
            Color32[] revealed = Render();
            int changed = 0;
            for(int i=0;i<revealed.Length;i++) if(!revealed[i].Equals(background[i])) changed++;
            if(changed<100) throw new Exception("Sonar did not reveal the geometry.");
            for(int i=0;i<revealed.Length;i++)
                if(revealed[i].r != background[i].r || revealed[i].g != background[i].g)
                    throw new Exception("Blue sonar changed hue on the revealed objects.");
            // A far part of the same scene must stay exactly dark until the original wave reaches it.
            for (int y=0;y<128;y++) for(int x=0;x<128;x++)
            {
                Vector2 world = new Vector2((x+0.5f)/128*20-10, (y+0.5f)/128*20-10);
                if(Vector2.Distance(world,new Vector2(-9,0)) > settings.Radius(0.6f)+0.2f
                    && !revealed[y*128+x].Equals(background[y*128+x]))
                    throw new Exception("Surface revealed ahead of the emitted wave.");
            }
            // Generated stones have no UVs. Add deliberately meaningless UVs; image must be identical.
            var filters = root.GetComponentsInChildren<MeshFilter>();
            uvTestMesh = Object.Instantiate(filters[0].sharedMesh);
            var uv = new Vector2[uvTestMesh.vertexCount];
            for(int i=0;i<uv.Length;i++) uv[i]=new Vector2(i*17.3f,-i*91.1f);
            uvTestMesh.uv = uv;
            foreach(var filter in filters) filter.sharedMesh = uvTestMesh;
            Color32[] withUvs = Render();
            for(int i=0;i<withUvs.Length;i++)
                if(!withUvs[i].Equals(revealed[i])) throw new Exception("Sonar depends on mesh UVs.");
            // Two waves must coexist rather than restarting/cancelling each object's animation.
            SetWave(0.6f, 2);
            Color32[] overlapping = Render();
            int combined = 0;
            for(int i=0;i<overlapping.Length;i++) if(!overlapping[i].Equals(background[i])) combined++;
            if(combined<=changed) throw new Exception("Second sonar pulse did not coexist with the first.");
            // The travelling line has finished, but hit rocks must still hold their full color.
            SetWave(2.1f);
            Color32[] held = Render();
            for(int y=0;y<128;y++) for(int x=0;x<50;x++)
                if(!held[y*128+x].Equals(revealed[y*128+x]))
                    throw new Exception("A hit rock faded before its hold duration ended.");
            // The reached seabed never leaves a filled disk behind the travelling ring.
            for(int y=0;y<128;y++) for(int x=85;x<128;x++)
                if(!held[y*128+x].Equals(background[y*128+x]))
                    throw new Exception("Seabed was filled by the sonar pulse.");
            settings.radiusCurve = AnimationCurve.EaseInOut(0,0,1,1);
            foreach(float age in new[]{0.2f,0.8f,1.4f})
            {
                SetWave(age);
                Color32[] curved=Render();
                for(int y=0;y<128;y++) for(int x=0;x<128;x++)
                {
                    Vector2 world=new Vector2((x+0.5f)/128*20-10,(y+0.5f)/128*20-10);
                    if(Vector2.Distance(world,new Vector2(-9,0))>settings.Radius(age)+0.2f
                        && !curved[y*128+x].Equals(background[y*128+x]))
                        throw new Exception("Curved wave revealed geometry ahead of its radius.");
                }
            }
            SetWave(settings.duration + settings.holdDuration + settings.fadeDuration);
            Color32[] faded = Render();
            for(int i=0;i<faded.Length;i++)
                if(!faded[i].Equals(background[i])) throw new Exception("Geometry did not fade back to the exact background.");
            Shader.SetGlobalInt("_EchoSonarCount", 0);
            Shader.SetGlobalInt("_EchoOcclusionEnabled", 0);
            Debug.Log("SONAR VISIBILITY PASSED: exact hidden background, matching source hue, no filled seabed, retained object color after the ring, world-space leading edge, linear/eased curves, UV independence, overlapping pulses and final fade. "+changed+" pixels revealed; "+combined+" with two waves.");
        }
        finally
        {
            RenderTexture.active = previous;
            camera.targetTexture = null;
            EditorSceneManager.ClosePreviewScene(scene);
            foreach(var material in materials) if(material) Object.DestroyImmediate(material);
            if(uvTestMesh) Object.DestroyImmediate(uvTestMesh);
            Shader.SetGlobalInt("_EchoSonarCount", 0);
            Object.DestroyImmediate(readback);
            Object.DestroyImmediate(target);
        }
    }
}
