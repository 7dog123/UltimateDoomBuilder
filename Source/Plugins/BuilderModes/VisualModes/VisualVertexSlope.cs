﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeImp.DoomBuilder.BuilderModes;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.VisualModes
{
	/// <summary>
	/// Stores the normalized angle (clockwise from the vertex) and if the original line's front points in a clockwise or counter clockwise direction
	/// </summary>
	internal struct LineAngleInfo
	{
		public double angle;
		public bool clockwise;

		public LineAngleInfo(Linedef ld, Vertex v, Sector s)
		{
			Line2D line;

			if (ld.Start == v)
			{
				line = ld.Line;
				clockwise = true;
			}
			else
			{
				line = new Line2D(ld.End.Position, ld.Start.Position);
				clockwise = false;
			}

			angle = line.GetAngle();

			if (ld.Front.Sector != s)
				clockwise = !clockwise;
		}
	}

	internal class VisualVertexSlope : BaseVisualSlope
	{
		#region ================== Variables

		private readonly Vertex vertex;
		private readonly Sector sector;
		private double angle;

		#endregion

		#region ================== Properties

		public Vertex Vertex { get { return vertex; } }
		public Sector Sector { get { return sector; } }

		#endregion

		#region ================== Constructor / Destructor

		public VisualVertexSlope(BaseVisualMode mode, SectorLevel level, Vertex vertex, Sector sector, bool up) : base(mode, level, up)
		{
			this.vertex = vertex;
			this.sector = sector;

			type = VisualSlopeType.Vertex;

			ComputeAngle();

			Update();

			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		private void ComputeAngle()
		{
			List<LineAngleInfo> lines = new List<LineAngleInfo>();
			List<double> angles = new List<double>();

			// Special case handling
			if (vertex.Linedefs.Count == 0) // should never happen
			{
				angle = 0.0;
				return;
			}
			else if (vertex.Linedefs.Count == 1)
			{
				angle = vertex.Linedefs.First().Angle + Angle2D.PIHALF;
				return;
			}

			// Get all lines that we have to take into account. Ignore linedefs where both sides
			// don't belong to the sector or don't belong to the sector
			foreach (Linedef ld in vertex.Linedefs)
			{
				if (ld.IsDisposed)
					continue;
				else
				{
					bool frontsame = false;
					bool backsame = false;

					if (ld.Front != null && ld.Front.Sector == sector)
						frontsame = true;

					if (ld.Back != null && ld.Back.Sector == sector)
						backsame = true;

					if (frontsame == backsame)
						continue;
				}

				lines.Add(new LineAngleInfo(ld, vertex, sector));
			}

			// Special case handling
			if(lines.Count == 0)
			{
				angle = lines[0].angle;
				return;
			}

			// Sort lines by their normalized angle
			lines.Sort((a, b) => a.angle.CompareTo(b.angle));

			// Get the other line we want to compute the angle between
			int other = 1;
			if (!lines[0].clockwise)
				other = lines.Count - 1;

			Vector2D v1 = Vector2D.FromAngle(lines[0].angle);
			Vector2D v2 = Vector2D.FromAngle(lines[other].angle);

			angle = lines[0].angle + (Math.Atan2(v2.y, v2.x) - Math.Atan2(v1.y, v1.x)) / 2.0;

			// If the first line is going clockwise we have to add 180°
			if (lines[0].clockwise)
				angle += Angle2D.PI;

			// Add 90° to get it in line with Doom's angles
			angle += Angle2D.PIHALF;

			// Also need to modify the angle for ceilings
			if (level.type == SectorLevelType.Ceiling)
				angle += Angle2D.PI;

			angle = Angle2D.Normalized(angle);
		}

		public override void Update()
		{
			plane = new Plane(level.plane.Normal, level.plane.Offset - 0.1f);

			if (!up)
				plane = plane.GetInverted();

			UpdatePosition();
		}

		public void UpdatePosition()
		{
			Vector2D av = Vector2D.FromAngle(angle);

			SetPosition(new Line2D(vertex.Position, vertex.Position + av), level.plane);
		}

		/// <summary>
		/// Finds a slope handle to pivot around. It takes the vertex that's furthest away from the given handle
		/// </summary>
		/// <param name="starthandle">The slope handle to start from (the one we need to find a pivot handle for)</param>
		/// <returns></returns>
		public static VisualVertexSlope GetSmartPivotHandle(VisualVertexSlope starthandle, BaseVisualMode mode)
		{
			VisualVertexSlope handle = starthandle;
			List<VisualVertexSlope> potentialhandles = new List<VisualVertexSlope>();
			List<IVisualEventReceiver> selectedsectors = mode.GetSelectedObjects(true, false, false, false, false);

			if (selectedsectors.Count == 0)
			{
				// No sectors selected, so find all handles that belong to the same level
				foreach (VisualVertexSlope checkhandle in mode.VertexSlopeHandles[starthandle.Sector])
				{
					if (checkhandle != starthandle && checkhandle.Level == starthandle.Level)
						potentialhandles.Add(checkhandle);
				}
			}
			else
			{
				// Sectors are selected, get all handles from those sectors that have the same level
				HashSet<Sector> sectors = new HashSet<Sector>();

				foreach (BaseVisualGeometrySector bvgs in selectedsectors)
					sectors.Add(bvgs.Sector.Sector);

				foreach (Sector s in sectors)
					foreach (VisualVertexSlope checkhandle in mode.VertexSlopeHandles[s])
					{
						if (checkhandle != starthandle)
							foreach (BaseVisualGeometrySector bvgs in selectedsectors)
								if (bvgs.Level == checkhandle.Level)
									potentialhandles.Add(checkhandle);
					}
			}

			foreach (KeyValuePair<Sector, List<VisualSlope>> kvp in mode.AllSlopeHandles)
				foreach (VisualSlope checkhandle in kvp.Value)
					checkhandle.SmartPivot = false;

			handle = potentialhandles.OrderByDescending(h => Vector2D.Distance(h.Vertex.Position, starthandle.vertex.Position)).First();

			if (handle == starthandle)
				return null;

			return handle;
		}

		/// <summary>
		/// This is called when the thing must be tested for line intersection. This should reject
		/// as fast as possible to rule out all geometry that certainly does not touch the line.
		/// </summary>
		public override bool PickFastReject(Vector3D from, Vector3D to, Vector3D dir)
		{
			if (vertex.IsDisposed || sector.IsDisposed)
				return false;

			RectangleF bbox = sector.BBox;

			if ((up && plane.Distance(from) > 0.0f) || (!up && plane.Distance(from) < 0.0f))
			{
				if (plane.GetIntersection(from, to, ref pickrayu))
				{
					if (pickrayu > 0.0f)
					{
						pickintersect = from + (to - from) * pickrayu;

						return ((pickintersect.x >= bbox.Left) && (pickintersect.x <= bbox.Right) &&
								(pickintersect.y >= bbox.Top) && (pickintersect.y <= bbox.Bottom));
					}
				}
			}

			return false;
		}

		/// <summary>
		/// This is called when the thing must be tested for line intersection. This should perform
		/// accurate hit detection and set u_ray to the position on the ray where this hits the geometry.
		/// </summary>
		public override bool PickAccurate(Vector3D from, Vector3D to, Vector3D dir, ref double u_ray)
		{
			u_ray = pickrayu;

			Sidedef sd = MapSet.NearestSidedef(sector.Sidedefs, pickintersect);
			Vertex v = MapSet.NearestVertex(new Vertex[] { sd.Line.Start, sd.Line.End }, pickintersect);
			if (v == vertex)
			{
				double side = sd.Line.SideOfLine(pickintersect);

				if ((side <= 0.0f && sd.IsFront) || (side > 0.0f && !sd.IsFront))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the pivor point for this slope handle
		/// </summary>
		/// <returns>The pivot point as Vector3D</returns>
		public override Vector3D GetPivotPoint()
		{
			return new Vector3D(vertex.Position, level.plane.GetZ(vertex.Position));
		}

		#endregion

		#region ================== Events

		public override void OnChangeTargetHeight(int amount)
		{
			VisualSlope pivothandle = null;
			List<IVisualEventReceiver> selectedsectors = mode.GetSelectedObjects(true, false, false, false, false);
			List<SectorLevel> levels = new List<SectorLevel>();

			if (selectedsectors.Count == 0)
				levels.Add(level);
			else
			{
				foreach (BaseVisualGeometrySector bvgs in selectedsectors)
					levels.Add(bvgs.Level);

				if (!levels.Contains(level))
					levels.Add(level);
			}

			// Try to find a slope handle the user set to be the pivot handle
			// TODO: doing this every time is kind of stupid. Maybe store the pivot handle in the mode?
			foreach (KeyValuePair<Sector, List<VisualSlope>> kvp in mode.AllSlopeHandles)
			{
				foreach (VisualSlope handle in kvp.Value)
				{
					if (handle.Pivot)
					{
						pivothandle = handle;
						break;
					}
				}
			}

			// User didn't set a pivot handle, try to find the smart pivot handle
			if (pivothandle == null)
				pivothandle = GetSmartPivotHandle(this, mode);

			// Still no pivot handle, cancle
			if (pivothandle == null)
				return;

			pivothandle.SmartPivot = true;

			mode.CreateUndo("Change slope");

			Plane originalplane = level.plane;

			// Build a new plane. Since we only got 2 points (the pivot point of the pivot handle and the vertex slope vertex) we need
			// to create a third point. That's done by getting the perpendicular of the line between the aforementioned 2 points, then
			// add the perpendicular to the vertex position of the vertex slope vertex
			Vector3D p3 = pivothandle.GetPivotPoint();
			Vector2D perp = new Line2D(vertex.Position, p3).GetPerpendicular();

			Vector3D p1 = new Vector3D(vertex.Position, originalplane.GetZ(vertex.Position) + amount);
			Vector3D p2 = new Vector3D(vertex.Position + perp, originalplane.GetZ(vertex.Position + perp) + amount);

			Plane plane = new Plane(p1, p2, p3, true);

			// Apply slope to surfaces
			foreach (SectorLevel l in levels)
				VisualSidedefSlope.ApplySlope(l, plane, mode);

			mode.SetActionResult("Changed slope.");
		}

		#endregion
	}
}
