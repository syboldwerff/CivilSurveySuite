﻿// Copyright Scott Whitney. All Rights Reserved.
// Reproduction or transmission in whole or in part, any form or by any
// means, electronic, mechanical or otherwise, is prohibited without the
// prior written consent of the copyright owner.

using System;
using _3DS_CivilSurveySuite.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace _3DS_CivilSurveySuite_ACADBase21
{
    public class Polylines
    {
        public static Polyline Square(Point2d basePoint, double squareSize, int lineWidth = 0)
        {
            var pointTopLeft = new Point2d(basePoint.X - squareSize, basePoint.Y + squareSize);
            var pointTopRight = new Point2d(basePoint.X + squareSize, basePoint.Y + squareSize);
            var pointBottomRight = new Point2d(basePoint.X + squareSize, basePoint.Y - squareSize);
            var pointBottomLeft = new Point2d(basePoint.X - squareSize, basePoint.Y - squareSize);

            var pLine = new Polyline();
            pLine.AddVertexAt(0, pointTopLeft, 0, lineWidth, lineWidth);
            pLine.AddVertexAt(1, pointTopRight, 0, lineWidth, lineWidth);
            pLine.AddVertexAt(2, pointBottomRight, 0, lineWidth, lineWidth);
            pLine.AddVertexAt(3, pointBottomLeft, 0, lineWidth, lineWidth);
            pLine.AddVertexAt(4, pointTopLeft, 0, lineWidth, lineWidth);
            pLine.AddVertexAt(5, pointBottomRight, 0, lineWidth, lineWidth);
            pLine.AddVertexAt(6, pointBottomLeft, 0, lineWidth, lineWidth);
            pLine.AddVertexAt(7, pointTopRight, 0, lineWidth, lineWidth);

            return pLine;
        }

        /// <summary>
        /// Gets the angle of the segment closest to the picked point from a polyline
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="pickedPoint"></param>
        /// <returns>A double representing the angle of the polyline segment.</returns>
        //FIXED: Make option to return readable angle (page-up like in Civil 3D). //Not an option.
        //FIXED: When polyline selected is the first segement, the angle is incorrect.
        //FIXED: Debug this and find out what's happening at start/end of polylines.
        public static double GetPolylineSegmentAngle(Polyline polyline, Point3d pickedPoint)
        {
            var segmentStart = 0;

            Point3d closestPoint = polyline.GetClosestPointTo(pickedPoint, false);
            double len = polyline.GetDistAtPoint(closestPoint);

            for (var i = 1; i < polyline.NumberOfVertices - 1; i++)
            {
                Point3d pt1 = polyline.GetPoint3dAt(i);
                double l1 = polyline.GetDistAtPoint(pt1);

                Point3d pt2 = polyline.GetPoint3dAt(i + 1);
                double l2 = polyline.GetDistAtPoint(pt2);

                if (len > l1 && len < l2)
                {
                    segmentStart = i;
                    break;
                }
            }

            LineSegment2d segment = polyline.GetLineSegment2dAt(segmentStart);

            if (!MathHelpers.IsOrdinaryAngle(segment.StartPoint.ToPoint(), segment.EndPoint.ToPoint()))
            {
                // if it isn't an ordinary angle, we flip it.
                return MathHelpers.RadiansToAngle(segment.Direction.Angle).Flip().ToRadians();
            }

            return segment.Direction.Angle;
        }

        /// <summary>
        /// Gets the angle of the segment closest to the picked point from a 3d polyline
        /// </summary>
        /// <param name="polyline3d"></param>
        /// <param name="pickedPoint"></param>
        /// <returns>angle of line segment as double</returns>
        public static double GetPolyline3dSegmentAngle(Polyline3d polyline3d, Point3d pickedPoint)
        {
            // Take the 3d Polyline and convert it to 2d.
            var polyline = new Polyline();
            for (int j = 0; j <= polyline3d.EndParam; j++)
            {
                Point3d point = polyline3d.GetPointAtParameter(j);
                polyline.AddVertexAt(j, new Point2d(point.X, point.Y), 0, 0, 0);
            }

            return GetPolylineSegmentAngle(polyline, pickedPoint);
        }

        public static void DrawPolyline3d(Transaction tr, BlockTableRecord btr, Point3dCollection points, string layerName)
        {
            var pLine3d = new Polyline3d(Poly3dType.SimplePoly, points, false) { Layer = layerName };
            btr.AppendEntity(pLine3d);
            tr.AddNewlyCreatedDBObject(pLine3d, true);
        }

        public static void DrawPolyline2d(Transaction tr, BlockTableRecord btr, Point3dCollection points, string layerName)
        {
            var pLine2d = new Polyline2d(Poly2dType.SimplePoly, points, 0, false, 0, 0, null);
            var pLine = new Polyline();
            pLine.ConvertFrom(pLine2d, false);
            pLine.Layer = layerName;
            pLine.Elevation = 0;
            btr.AppendEntity(pLine);
            tr.AddNewlyCreatedDBObject(pLine, true);
        }

        /// <summary>
        /// Gets the polyline segment.
        /// </summary>
        /// <param name="polyline">The polyline.</param>
        /// <param name="nestedEntity">The nested entity.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="ArgumentNullException">polyline</exception>
        /// <exception cref="ArgumentNullException">nestedEntity</exception>
        /// TODO Edit XML Comment Template for GetPolylineSegment
        public static int GetPolylineSegment(Polyline polyline, PromptNestedEntityResult nestedEntity)
        {
            if (polyline == null)
                throw new ArgumentNullException(nameof(polyline));

            if (nestedEntity == null)
                throw new ArgumentNullException(nameof(nestedEntity));

            // Transform picked point from current UCS to WCS.
            Point3d wcsPickedPoint = nestedEntity.PickedPoint.TransformBy(AutoCADActive.Editor.CurrentUserCoordinateSystem);

            // Get the closest point to picked point on the polyline.
            // If the polyline is nested, it's needed to transform the picked point using the 
            // the transformation matrix that is applied to the polyline by its containers.
            var pointOnPolyline = nestedEntity.GetContainers().Length == 0 ? 
                polyline.GetClosestPointTo(wcsPickedPoint, false) : // Not nested polyline.
                polyline.GetClosestPointTo(wcsPickedPoint.TransformBy(nestedEntity.Transform.Inverse()), false); // Nested polyline

            // Get the selected segment index.
            return (int)polyline.GetParameterAtPoint(pointOnPolyline);
        }
    }
}