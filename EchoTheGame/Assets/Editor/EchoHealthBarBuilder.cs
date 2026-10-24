using Project.Echo.Menu;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class EchoHealthBarBuilder
{
    [MenuItem("Echo/Style Player Health Bar")]
    public static void Build()
    {
        const string path="Assets/Prefabs/Players/PlayerHealthbar.prefab";
        var theme=AssetDatabase.LoadAssetAtPath<EchoMenuTheme>("Assets/UI/Echo/EchoMenuTheme.asset");
        if(!theme)throw new System.InvalidOperationException("EchoMenuTheme is required.");
        var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            for(int i=root.transform.childCount-1;i>=0;i--)Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
            root.transform.SetPositionAndRotation(Vector3.zero,Quaternion.Euler(90,0,0));
            var canvasObject=new GameObject("Hull integrity / World canvas",typeof(RectTransform),typeof(Canvas));
            canvasObject.layer=5;canvasObject.transform.SetParent(root.transform,false);
            var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
            var canvasRect=canvasObject.GetComponent<RectTransform>();canvasRect.sizeDelta=new Vector2(180,16);canvasRect.localScale=Vector3.one*0.01f;
            RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 offsetMin,Vector2 offsetMax)
            {
                var go=new GameObject(name,typeof(RectTransform));go.layer=5;go.transform.SetParent(parent,false);
                var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=offsetMin;rect.offsetMax=offsetMax;return rect;
            }
            Image Image(string name,Transform parent,Vector2 min,Vector2 max,Vector2 offsetMin,Vector2 offsetMax,Color color)
            {
                var rect=Rect(name,parent,min,max,offsetMin,offsetMax);var image=rect.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;
            }
            var sliderRect=Rect("Integrity meter",canvasRect,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            var slider=sliderRect.gameObject.AddComponent<Slider>();slider.interactable=false;slider.transition=Selectable.Transition.None;
            slider.navigation=new Navigation{mode=Navigation.Mode.None};slider.minValue=0;slider.maxValue=100;slider.value=100;
            Image("Dark instrument backing",sliderRect,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,theme.panel);
            var fillArea=Rect("Fill area",sliderRect,new Vector2(0,0.25f),new Vector2(1,0.75f),new Vector2(4,0),new Vector2(-4,0));
            var fill=Image("Seafoam hull reserve",fillArea,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,theme.seafoam);
            slider.fillRect=fill.rectTransform;
            // Stationary gaps stay in place while the slider drains, including partial segments.
            for(int i=1;i<10;i++)Image("Division "+i,fillArea,new Vector2(i/10f,0),new Vector2(i/10f,1),new Vector2(-1,0),new Vector2(1,0),theme.abyss);
            var edge=theme.muted;edge.a=0.65f;
            Image("Lower instrument rule",sliderRect,new Vector2(0,0),new Vector2(1,0),new Vector2(0,1),new Vector2(0,2),edge);
            Image("Left bracket",sliderRect,Vector2.zero,new Vector2(0,1),Vector2.zero,new Vector2(1,0),theme.seafoam);
            Image("Right bracket",sliderRect,new Vector2(1,0),Vector2.one,new Vector2(-1,0),Vector2.zero,theme.seafoam);
            var behaviour=root.GetComponent<HealthBarPositionBehaviour>();
            var serialized=new SerializedObject(behaviour);
            serialized.FindProperty("_theme").objectReferenceValue=theme;
            serialized.FindProperty("_fill").objectReferenceValue=fill;
            serialized.FindProperty("_positionOffset").vector3Value=new Vector3(0,0.9f,-1.5f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root,path);
            Debug.Log("HEALTH BAR STYLED: persistent editable segmented navy/seafoam world UI using EchoMenuTheme.");
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
