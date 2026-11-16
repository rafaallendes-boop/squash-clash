using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.App
{
    // Device robustness: keeps the HUD and every full-screen layer inside the safe area of the phone it runs on
    // (notch, Dynamic Island, home indicator). It reads Screen.safeArea, turns the insets into panel units and applies
    // them as padding: the header starts right under the top inset (or at a small margin on a phone without one) and
    // each fitted layer keeps its content between the insets while its background still covers the whole screen.
    // Checked every frame, touched only when the screen, the insets or the panel size change. No allocation.
    public sealed class SafeAreaLayout : MonoBehaviour
    {
        // Distance from the top edge to the HUD on a screen without a top inset.
        public const float MinTop=12f;
        private const int MaxLayers=8;
        private static bool simulating;private static Vector2 simulatedScreen;private static Rect simulatedSafe;
        // Tests and captures: lay the HUD out as a given phone would (pixels, origin at the bottom left, like Screen.safeArea).
        public static void Simulate(Vector2 screenPixels,Rect safePixels){simulating=true;simulatedScreen=screenPixels;simulatedSafe=safePixels;}
        public static void StopSimulating(){simulating=false;}
        private VisualElement root;
        private readonly VisualElement[] layers=new VisualElement[MaxLayers];
        private readonly float[] extraTop=new float[MaxLayers],extraBottom=new float[MaxLayers];
        private int count;private Rect shownSafe;private Vector2 shownScreen,shownPanel=new Vector2(-1,-1);
        // Insets in panel units.
        public float Top {get;private set;}
        public float Bottom {get;private set;}
        public float Left {get;private set;}
        public float Right {get;private set;}
        // Where the HUD starts, from the top of the panel.
        public float TopEdge=>Inward(Mathf.Max(MinTop,Top));
        // Whole panel units, never short of the inset (a hair over a whole unit does not cost another one).
        private static float Inward(float units)=>Mathf.Ceil(units-.05f);
        public event Action Changed;
        public void Initialize(VisualElement root){this.root=root;Apply();}
        // Keeps a full-screen layer's content inside the safe area: padding = inset + extra. A NaN extra leaves that side alone.
        public void Fit(VisualElement layer,float top,float bottom)
        {
            if(layer==null||count>=MaxLayers)return;
            layers[count]=layer;extraTop[count]=top;extraBottom[count]=bottom;Place(count);count++;
        }
        private void Place(int index)
        {
            if(!float.IsNaN(extraTop[index]))layers[index].style.paddingTop=TopEdge+extraTop[index];
            if(!float.IsNaN(extraBottom[index]))layers[index].style.paddingBottom=Inward(Bottom+extraBottom[index]);
        }
        // True when something changed and the layout was applied again.
        public bool Apply()
        {
            if(root==null)return false;
            var panel=new Vector2(root.layout.width,root.layout.height);
            if(float.IsNaN(panel.x)||float.IsNaN(panel.y)||panel.x<=0||panel.y<=0)return false;
            var screen=simulating?simulatedScreen:new Vector2(Screen.width,Screen.height);var safe=simulating?simulatedSafe:Screen.safeArea;
            if(screen.x<=0||screen.y<=0)return false;
            if(safe==shownSafe&&screen==shownScreen&&panel==shownPanel)return false;
            shownSafe=safe;shownScreen=screen;shownPanel=panel;
            float ux=panel.x/screen.x,uy=panel.y/screen.y;
            Left=Mathf.Max(0,safe.xMin*ux);Right=Mathf.Max(0,(screen.x-safe.xMax)*ux);
            Bottom=Mathf.Max(0,safe.yMin*uy);Top=Mathf.Max(0,(screen.y-safe.yMax)*uy);
            root.style.paddingTop=TopEdge;root.style.paddingLeft=Inward(Left);root.style.paddingRight=Inward(Right);
            for(int i=0;i<count;i++)Place(i);
            Changed?.Invoke();
            return true;
        }
        private void Update()=>Apply();
    }
}
