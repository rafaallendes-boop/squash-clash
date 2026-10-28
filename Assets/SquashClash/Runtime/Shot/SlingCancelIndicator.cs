using System.Collections.Generic;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Shot
{
    // View only. The actual limit is drawn at its screen coordinate, clipped around the
    // front wall/HUD. The explanatory card chooses the nearest unobstructed space.
    public sealed class SlingCancelIndicator
    {
        private readonly VisualElement root;
        private readonly Label card;
        private readonly List<VisualElement> lines=new List<VisualElement>();
        private readonly List<Rect> blockers=new List<Rect>(8),pieces=new List<Rect>(16),scratch=new List<Rect>(16);
        public SlingCancelIndicator(VisualElement root)
        {
            this.root=root;card=new Label {name="sling-cancel-local",pickingMode=PickingMode.Ignore};
            card.style.position=Position.Absolute;card.style.marginLeft=card.style.marginRight=card.style.marginTop=card.style.marginBottom=0;card.style.fontSize=11;card.style.whiteSpace=WhiteSpace.Normal;
            card.style.paddingLeft=card.style.paddingRight=4;card.style.paddingTop=3;
            card.style.backgroundColor=new Color(.02f,.05f,.08f,.94f);card.style.color=Color.white;
            card.style.borderLeftWidth=card.style.borderRightWidth=card.style.borderTopWidth=card.style.borderBottomWidth=1;
            root.Insert(0,card);Hide();
        }
        private Rect ScreenRect(Rect r)
        {
            float x=root.layout.width/Screen.width,y=root.layout.height/Screen.height;
            return Rect.MinMaxRect(r.xMin*x,(Screen.height-r.yMax)*y,r.xMax*x,(Screen.height-r.yMin)*y);
        }
        private void AddBlocker(string name)
        {
            var e=root.Q(name);if(e==null||e.resolvedStyle.display==DisplayStyle.None||e.worldBound.width<=0||e.worldBound.height<=0)return;
            var b=e.worldBound;b.position-=root.worldBound.position;blockers.Add(b);
        }
        private static void Subtract(Rect source,Rect cut,List<Rect> output)
        {
            if(!source.Overlaps(cut)){output.Add(source);return;}
            float left=Mathf.Max(source.xMin,cut.xMin),right=Mathf.Min(source.xMax,cut.xMax);
            float top=Mathf.Max(source.yMin,cut.yMin),bottom=Mathf.Min(source.yMax,cut.yMax);
            if(top>source.yMin)output.Add(Rect.MinMaxRect(source.xMin,source.yMin,source.xMax,top));
            if(bottom<source.yMax)output.Add(Rect.MinMaxRect(source.xMin,bottom,source.xMax,source.yMax));
            if(left>source.xMin)output.Add(Rect.MinMaxRect(source.xMin,top,left,bottom));
            if(right<source.xMax)output.Add(Rect.MinMaxRect(right,top,source.xMax,bottom));
        }
        private void Clip(Rect source)
        {
            pieces.Clear();pieces.Add(source);
            foreach(var cut in blockers)
            {
                scratch.Clear();foreach(var p in pieces)Subtract(p,cut,scratch);
                pieces.Clear();pieces.AddRange(scratch);
            }
        }
        private static void Place(VisualElement e,Rect rect)
        {e.style.left=rect.x;e.style.top=rect.y;e.style.width=rect.width;e.style.height=rect.height;e.style.display=DisplayStyle.Flex;}
        public void Show(SlingModel model,ParamSet config,CourtDimensions court,Rect area,float mmPerPx)
        {
            if(model==null||!model.Active||!model.Locked||root.layout.width<=0||root.layout.height<=0||UnityEngine.Camera.main==null){Hide();return;}
            blockers.Clear();var cam=UnityEngine.Camera.main;
            float minX=float.PositiveInfinity,minY=minX,maxX=float.NegativeInfinity,maxY=maxX;
            foreach(var p in new[]{new Vector3(0,0,0),new Vector3(court.Width,0,0),new Vector3(0,court.FrontOutLower,0),new Vector3(court.Width,court.FrontOutLower,0)})
            {
                var v=cam.WorldToViewportPoint(p);float x=v.x*root.layout.width,y=(1-v.y)*root.layout.height;
                minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);
            }
            // A small rendering clearance is view-only; it never changes touch geometry.
            blockers.Add(Rect.MinMaxRect(minX-1,minY-1,maxX+1,maxY+1));
            foreach(var name in new[]{"boot-header","dev-toggle","dev-bar","sling-status","shot-overlay"})AddBlocker(name);
            var safe=ScreenRect(area);float stroke=6f*root.layout.width/Screen.width;
            float limit=model.Region==SlingCancelRegion.Above?(float)config.Get("gesture.slingCancelAboveMm"):(float)config.Get("gesture.slingCancelLateralMm");
            string arrow=model.Region==SlingCancelRegion.Above?"↑":model.Region==SlingCancelRegion.Left?"←":"→";
            Vector2 anchor;Rect boundary;
            if(model.Region==SlingCancelRegion.Above)
            {
                float y=(Screen.height-model.LockPositionPx.y-limit/mmPerPx)/Screen.height*root.layout.height;
                anchor=new Vector2(model.LockPositionPx.x/Screen.width*root.layout.width,y);
                boundary=new Rect(safe.xMin,Mathf.Clamp(y-stroke/2,safe.yMin,safe.yMax-stroke),safe.width,stroke);
            }
            else
            {
                float sign=model.Region==SlingCancelRegion.Left?-1:1;
                float x=(model.LockPositionPx.x+sign*limit/mmPerPx)/Screen.width*root.layout.width;
                anchor=new Vector2(x,(Screen.height-model.LockPositionPx.y)/Screen.height*root.layout.height);
                boundary=new Rect(Mathf.Clamp(x-stroke/2,safe.xMin,safe.xMax-stroke),safe.yMin,stroke,safe.height);
            }
            bool inside=model.InsideCancel(model.LastPositionPx);var color=inside?Color.yellow:Color.cyan;
            Clip(boundary);
            for(int i=0;i<pieces.Count;i++)
            {
                if(i>=lines.Count)
                {
                    var line=new VisualElement {name="sling-cancel-limit-"+i,pickingMode=PickingMode.Ignore};line.style.position=Position.Absolute;root.Insert(0,line);lines.Add(line);
                }
                Place(lines[i],pieces[i]);lines[i].style.backgroundColor=color;
            }
            for(int i=pieces.Count;i<lines.Count;i++)lines[i].style.display=DisplayStyle.None;
            Clip(safe);const float width=96,height=92;float best=float.PositiveInfinity;Rect selected=default;bool found=false;
            foreach(var room in pieces)
            {
                if(room.width<width+4||room.height<height+4)continue;
                var candidate=new Rect(Mathf.Clamp(anchor.x-width/2,room.xMin+2,room.xMax-width-2),Mathf.Clamp(anchor.y-height/2,room.yMin+2,room.yMax-height-2),width,height);
                float distance=(candidate.center-anchor).sqrMagnitude;if(distance>=best)continue;best=distance;selected=candidate;found=true;
            }
            if(found)
            {
                card.text=$"✕ Cancelar {arrow}\nLímite {limit:F0} mm\ndesde donde fijaste\n{(inside?"DENTRO: suelta":"FUERA: cruza el límite")}";
                card.style.borderLeftColor=card.style.borderRightColor=card.style.borderTopColor=card.style.borderBottomColor=color;
                Place(card,selected);
            }
            else card.style.display=DisplayStyle.None;
        }
        public void Hide(){card.style.display=DisplayStyle.None;foreach(var line in lines)line.style.display=DisplayStyle.None;}
        public void Dispose(){card.RemoveFromHierarchy();foreach(var line in lines)line.RemoveFromHierarchy();lines.Clear();}
    }
}
