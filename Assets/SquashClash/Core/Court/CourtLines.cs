using System.Collections.Generic;
using UnityEngine;

namespace SquashClash.Core.Court
{
    public static class CourtLines
    {
        public static IReadOnlyList<LineBand> Build(CourtDimensions d)
        {
            var bands = new List<LineBand>(12);
            void Add(LineId id, SurfaceId surface, Vector2 a, Vector2 b, Vector2 growth) =>
                bands.Add(new LineBand(id, surface, a, b, growth, d.LineWidth));

            var up = Vector2.up;
            Add(LineId.FrontOut, SurfaceId.FrontWall, new Vector2(0, d.FrontOutLower), new Vector2(d.Width, d.FrontOutLower), up);
            Add(LineId.Service, SurfaceId.FrontWall, new Vector2(0, d.ServiceLower), new Vector2(d.Width, d.ServiceLower), up);
            Add(LineId.Tin, SurfaceId.FrontWall, new Vector2(0, d.TinTop - d.LineWidth), new Vector2(d.Width, d.TinTop - d.LineWidth), up);
            Add(LineId.BackOut, SurfaceId.BackWall, new Vector2(0, d.BackOutLower), new Vector2(d.Width, d.BackOutLower), up);
            var sideStart = new Vector2(0, d.FrontOutLower);
            var sideEnd = new Vector2(d.Length, d.BackOutLower);
            var sideGrowth = new Vector2(d.FrontOutLower - d.BackOutLower, d.Length).normalized;
            Add(LineId.LeftSideOut, SurfaceId.LeftWall, sideStart, sideEnd, sideGrowth);
            Add(LineId.RightSideOut, SurfaceId.RightWall, sideStart, sideEnd, sideGrowth);

            var shortFar = d.Length - d.ShortLineNearBackEdgeFromBack;
            Add(LineId.ShortLine, SurfaceId.Floor, new Vector2(0, shortFar - d.LineWidth), new Vector2(d.Width, shortFar - d.LineWidth), up);
            Add(LineId.HalfCourt, SurfaceId.Floor, new Vector2(d.Width / 2 - d.LineWidth / 2, shortFar), new Vector2(d.Width / 2 - d.LineWidth / 2, d.Length), Vector2.right);
            var boxBack = shortFar + d.ServiceBoxInner;
            var rightSide = d.Width - d.ServiceBoxInner - d.LineWidth;
            Add(LineId.LeftBoxSide, SurfaceId.Floor, new Vector2(d.ServiceBoxInner, shortFar), new Vector2(d.ServiceBoxInner, boxBack + d.LineWidth), Vector2.right);
            Add(LineId.LeftBoxBack, SurfaceId.Floor, new Vector2(0, boxBack), new Vector2(d.ServiceBoxInner + d.LineWidth, boxBack), up);
            Add(LineId.RightBoxSide, SurfaceId.Floor, new Vector2(rightSide, shortFar), new Vector2(rightSide, boxBack + d.LineWidth), Vector2.right);
            Add(LineId.RightBoxBack, SurfaceId.Floor, new Vector2(rightSide, boxBack), new Vector2(d.Width, boxBack), up);
            return bands.AsReadOnly();
        }
    }
}
