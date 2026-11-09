#if UNITY_EDITOR
using System;
using System.Reflection;
namespace SquashClash.AutoQA
{
    // UnityEditor-only reflection, required by the existing GameView resolution fixture.
    internal sealed class NativeGameView : IDisposable
    {
        private readonly object group;private readonly UnityEditor.EditorWindow view;
        private readonly PropertyInfo selected;private readonly int previous,index;
        internal NativeGameView()
        {
            var a=typeof(UnityEditor.Editor).Assembly;var t=a.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(t);
            var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null);
            group=t.GetProperty("currentGroup").GetValue(sizes);var g=group.GetType();
            index=(int)g.GetMethod("GetTotalCount").Invoke(group,null);
            var kind=a.GetType("UnityEditor.GameViewSizeType");
            var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),Enum.Parse(kind,"FixedResolution"),1206,2622,"AutoQA MVP");
            g.GetMethod("AddCustomSize").Invoke(group,new[]{size});
            var vt=a.GetType("UnityEditor.GameView");view=UnityEditor.EditorWindow.GetWindow(vt);
            selected=vt.GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            previous=(int)selected.GetValue(view);selected.SetValue(view,index);view.Repaint();
        }
        public void Dispose()
        {
            if(view!=null)selected.SetValue(view,previous);
            group.GetType().GetMethod("RemoveCustomSize").Invoke(group,new object[]{index});
        }
    }
}
#endif
