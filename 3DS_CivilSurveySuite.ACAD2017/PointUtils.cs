﻿// Copyright Scott Whitney. All Rights Reserved.
// Reproduction or transmission in whole or in part, any form or by any
// means, electronic, mechanical or otherwise, is prohibited without the
// prior written consent of the copyright owner.

using System;
using _3DS_CivilSurveySuite.Core;
using _3DS_CivilSurveySuite.Model;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace _3DS_CivilSurveySuite.ACAD2017
{
    public static class PointUtils
    {
        /// <summary>
        /// Creates a <see cref="DBPoint"/> from an angle and distance.
        /// </summary>
        public static void Create_At_Angle_And_Distance(Action<Transaction, Point3d> createAction)
        {
            if (!EditorUtils.GetPoint(out Point3d basePoint, "\n3DS> Select a base point: "))
                return;

            if (!EditorUtils.GetAngle(out Angle angle, "\n3DS> Enter bearing (Format: DDD.MMSS): ", basePoint, "\n3DS> Pick bearing on screen: "))
                return;

            if (!EditorUtils.GetDistance(out double dist, "\n3DS> Distance: ", basePoint))
                return;

            AcadApp.Editor.WriteMessage($"\n3DS> Bearing: {angle}");
            AcadApp.Editor.WriteMessage($"\n3DS> Distance: {dist}");

            var pko = new PromptKeywordOptions("\n3DS> Flip bearing? ") { AppendKeywordsToMessage = true };
            pko.Keywords.Add(Keywords.ACCEPT);
            pko.Keywords.Add(Keywords.CANCEL);
            pko.Keywords.Add(Keywords.FLIP);

            Point point = PointHelpers.AngleAndDistanceToPoint(angle, dist, basePoint.ToPoint());

            using (var graphics = new TransientGraphics())
            {
                graphics.DrawPlus(basePoint, Settings.GraphicsSize);
                graphics.DrawX(point.ToPoint3d(), Settings.GraphicsSize);
                graphics.DrawLine(basePoint, point.ToPoint3d());

                var cancelled = false;
                PromptResult prResult;
                do
                {
                    prResult = AcadApp.Editor.GetKeywords(pko);

                    if (prResult.Status != PromptStatus.Keyword &&
                        prResult.Status != PromptStatus.OK)
                        continue;

                    switch (prResult.StringResult)
                    {
                        case Keywords.ACCEPT:
                            using (var tr = AcadApp.StartTransaction())
                            {
                                createAction(tr, point.ToPoint3d());
                                tr.Commit();
                            }

                            cancelled = true;
                            break;
                        case Keywords.CANCEL:
                            cancelled = true;
                            break;
                        case Keywords.FLIP:
                            angle = angle.Flip();
                            point = PointHelpers.AngleAndDistanceToPoint(angle, dist, basePoint.ToPoint());
                            graphics.ClearGraphics();
                            graphics.DrawPlus(basePoint, Settings.GraphicsSize);
                            graphics.DrawX(point.ToPoint3d(), Settings.GraphicsSize);
                            graphics.DrawLine(basePoint, point.ToPoint3d());
                            break;
                    }
                } while (prResult.Status != PromptStatus.Cancel &&
                         prResult.Status != PromptStatus.Error && !cancelled);
            }
        }

        /// <summary>
        /// Creates at angle distance and slope.
        /// </summary>
        public static void Create_At_Angle_Distance_And_Slope(Action<Transaction, Point3d> createAction)
        {
            var graphics = new TransientGraphics();
            try
            {
                if (!EditorUtils.GetPoint(out Point3d basePoint, "\n3DS> Pick starting point: "))
                    return;

                graphics.DrawX(basePoint, Settings.GraphicsSize);

                if (!EditorUtils.GetAngle(out Angle angle, "\n3DS> Enter angle: ", basePoint))
                    return;

                if (!EditorUtils.GetDistance(out double distance, "\n3DS> Enter distance: ", basePoint))
                    return;

                var point = PointHelpers.AngleAndDistanceToPoint(angle, distance, basePoint.ToPoint());
                graphics.DrawX(point.ToPoint3d(), Settings.GraphicsSize);
                graphics.DrawLine(basePoint, point.ToPoint3d());

                if (!EditorUtils.GetDouble(out double slope, "\n3DS> Enter slope (%): "))
                    return;

                var newPoint = new Point(point.X, point.Y, basePoint.Z + distance * (slope / 100.0));

                using (var tr = AcadApp.StartTransaction())
                {
                    createAction(tr, newPoint.ToPoint3d());
                    tr.Commit();
                }
            }
            catch (Exception e)
            {
                AcadApp.WriteErrorMessage(e);
            }
            finally
            {
                graphics.Dispose();
            }
        }

        /// <summary>
        /// Create a point at a picked location with elevation calculated at designated slope.
        /// </summary>
        public static void Create_At_Slope_At_Point(Action<Transaction, Point3d> createAction)
        {
            using (var graphics = new TransientGraphics())
            using (var tr = AcadApp.StartTransaction())
            {
                if (!EditorUtils.GetPoint(out Point3d firstPoint, "\n3DS> Base point: "))
                    return;

                graphics.DrawPlus(firstPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetPoint(out Point3d secondPoint, "\n3DS> New point location: "))
                    return;

                graphics.DrawPlus(secondPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetDouble(out double slope, "\n3DS> Percent slope: ", allowZero: false))
                    return;

                double distance = PointHelpers.GetDistanceBetweenPoints(firstPoint.ToPoint(), secondPoint.ToPoint());
                double elevation = firstPoint.Z + distance * (slope / 100.0);
                var newPoint = new Point3d(secondPoint.X, secondPoint.Y, elevation);

                createAction(tr, newPoint);

                tr.Commit();
            }
        }

        /// <summary>
        /// Places a point at the intersection of two bearings defined by four points.
        /// </summary>
        public static void Create_At_Intersection_Of_Four_Points(Action<Transaction, Point3d> createAction)
        {
            var graphics = new TransientGraphics();
            try
            {
                if (!EditorUtils.GetPoint(out Point3d firstPoint, "\n3DS> Pick first point: "))
                    return;

                graphics.DrawPlus(firstPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetPoint(out Point3d secondPoint, "\n3DS> Pick first point: "))
                    return;

                graphics.DrawPlus(secondPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetPoint(out Point3d thirdPoint, "\n3DS> Pick first point: "))
                    return;

                graphics.DrawPlus(thirdPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetPoint(out Point3d fourthPoint, "\n3DS> Pick first point: "))
                    return;

                graphics.DrawPlus(fourthPoint, Settings.GraphicsSize);
                graphics.DrawLine(firstPoint, secondPoint);
                graphics.DrawLine(thirdPoint, fourthPoint);

                var canIntersect = PointHelpers.FourPointIntersection(firstPoint.ToPoint(), secondPoint.ToPoint(), thirdPoint.ToPoint(), fourthPoint.ToPoint(), out Point intersectionPoint);

                if (!canIntersect)
                {
                    AcadApp.WriteMessage("No intersection point found!");
                    return;
                }

                graphics.DrawDot(intersectionPoint.ToPoint3d(), Settings.GraphicsSize);

                using (var tr = AcadApp.StartTransaction())
                {
                    createAction(tr, intersectionPoint.ToPoint3d());
                    tr.Commit();
                }
            }
            catch (Exception e)
            {
                AcadApp.WriteErrorMessage(e);
            }
            finally
            {
                graphics.Dispose();
            }
        }

        /// <summary>
        /// Creates a point at the intersection of two bearings from two base points.
        /// </summary>
        public static void Create_At_Intersection_Two_Bearings(Action<Transaction, Point3d> createAction)
        {
            var graphics = new TransientGraphics();
            try
            {
                if (!EditorUtils.GetPoint(out Point3d firstPoint, "\n3DS> Pick first point: "))
                    return;

                graphics.DrawPlus(firstPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetAngle(out Angle firstAngle, "\n3DS> Enter first bearing: ", firstPoint))
                    return;

                var endPoint1 = PointHelpers.AngleAndDistanceToPoint(firstAngle, 1000, firstPoint.ToPoint());
                graphics.DrawLine(firstPoint, endPoint1.ToPoint3d());

                if (!EditorUtils.GetPoint(out Point3d secondPoint, "\n3DS> Pick second point: "))
                    return;

                graphics.DrawPlus(secondPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetAngle(out Angle secondAngle, "\n3DS> Enter second bearing: ", secondPoint))
                    return;

                var endPoint2 = PointHelpers.AngleAndDistanceToPoint(secondAngle, 1000, secondPoint.ToPoint());
                graphics.DrawLine(secondPoint, endPoint2.ToPoint3d());

                var canIntersect = PointHelpers.AngleAngleIntersection(firstPoint.ToPoint(), firstAngle, secondPoint.ToPoint(), secondAngle, out Point intersectionPoint);

                if (!canIntersect)
                {
                    AcadApp.Editor.WriteMessage("\n3DS> No intersection found! ");
                    return;
                }

                AcadApp.Editor.WriteMessage($"\n3DS> Intersection found at X:{Math.Round(intersectionPoint.X, 4)} Y:{Math.Round(intersectionPoint.Y, 4)}");

                graphics.DrawX(intersectionPoint.ToPoint3d(), Settings.GraphicsSize);

                using (var tr = AcadApp.StartTransaction())
                {
                    createAction(tr, intersectionPoint.ToPoint3d());
                    tr.Commit();
                }
            }
            catch (Exception e)
            {
                AcadApp.Editor.WriteMessage($"3DS> Command Exception: {e.Message}");
            }
            finally
            {
                graphics.Dispose();
            }
        }

        /// <summary>
        /// Creates a point at intersection of two distances.
        /// </summary>
        public static void Create_At_Intersection_Two_Distances(Action<Transaction, Point3d> createAction)
        {
            var graphics = new TransientGraphics();
            try
            {
                if (!EditorUtils.GetPoint(out Point3d firstPoint, "\n3DS> Pick first point: "))
                    return;

                graphics.DrawPlus(firstPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetDistance(out double dist1, "\n3DS> Enter first distance: ", firstPoint))
                    return;

                graphics.DrawCircle(firstPoint, dist1);

                if (!EditorUtils.GetPoint(out Point3d secondPoint, "\n3DS> Pick second point: "))
                    return;

                graphics.DrawPlus(secondPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetDistance(out double dist2, "\n3DS> Enter second distance: ", secondPoint))
                    return;

                graphics.DrawCircle(secondPoint, dist2);

                var canIntersect = PointHelpers.DistanceDistanceIntersection(firstPoint.ToPoint(), dist1, secondPoint.ToPoint(), dist2, out Point firstInt, out Point secondInt);

                if (!canIntersect)
                {
                    AcadApp.Editor.WriteMessage("\n3DS> No intersection found! ");
                    return;
                }

                graphics.DrawDot(firstInt.ToPoint3d(), Settings.GraphicsSize/2);
                graphics.DrawDot(secondInt.ToPoint3d(), Settings.GraphicsSize/2);
                AcadApp.Editor.WriteMessage($"\n3DS> First intersection found at X:{Math.Round(firstInt.X, 4)} Y:{Math.Round(firstInt.Y, 4)}");
                AcadApp.Editor.WriteMessage($"\n3DS> Second intersection found at X:{Math.Round(secondInt.X, 4)} Y:{Math.Round(secondInt.Y, 4)}");

                if (!EditorUtils.GetPoint(out Point3d pickedPoint, "\n3DS> Pick near desired intersection: "))
                    return;

                using (var tr = AcadApp.StartTransaction())
                {
                    graphics.ClearGraphics();
                    if (PointHelpers.GetDistanceBetweenPoints(pickedPoint.ToPoint(), firstInt) <= PointHelpers.GetDistanceBetweenPoints(pickedPoint.ToPoint(), secondInt))
                    {
                        //use first point
                        //CreatePoint(tr, firstInt.ToPoint3d());
                        graphics.DrawDot(firstInt.ToPoint3d(), Settings.GraphicsSize/2);
                        createAction(tr, firstInt.ToPoint3d());
                    }
                    else
                    {
                        //use second point
                        graphics.DrawDot(secondInt.ToPoint3d(), Settings.GraphicsSize/2);
                        createAction(tr, secondInt.ToPoint3d());
                    }

                    tr.Commit();
                }
            }
            catch (Exception e)
            {
                AcadApp.Editor.WriteMessage($"3DS> Command Exception: {e.Message}");
            }
            finally
            {
                graphics.Dispose();
            }
        }

        /// <summary>
        /// Creates a point at the offset two lines with given distance.
        /// </summary>
        public static void Create_At_Offset_Two_Lines(Action<Transaction, Point3d> createAction)
        {
            var graphics = new TransientGraphics();
            try
            {
                using (Transaction tr = AcadApp.StartTransaction())
                {
                    AcadApp.Editor.WriteMessage("\n3DS> Select first line to offset.");
                    Line firstLineToOffset = LineUtils.GetLineOrPolylineSegment(tr);

                    if (firstLineToOffset == null)
                        return;

                    // Highlight line.
                    graphics.DrawLine(firstLineToOffset, TransientDrawingMode.Highlight);

                    AcadApp.Editor.WriteMessage("\n3DS> Select second line to offset.");
                    Line secondLineToOffset = LineUtils.GetLineOrPolylineSegment(tr);

                    if (secondLineToOffset == null)
                        return;

                    // Highlight line.
                    graphics.DrawLine(secondLineToOffset, TransientDrawingMode.Highlight);

                    // Prompt for offset distance.
                    if (!EditorUtils.GetDistance(out double dist, "\n" + ResourceStrings.Offset_Distance))
                        return;

                    // Pick offset side.
                    if (!EditorUtils.GetPoint(out Point3d offsetPoint, "\n" + ResourceStrings.Pick_Offset_Side))
                        return;

                    Line firstOffsetLine = LineUtils.Offset(firstLineToOffset, dist, offsetPoint);
                    Line secondOffsetLine = LineUtils.Offset(secondLineToOffset, dist, offsetPoint);
                    Point intersectionPoint = LineUtils.FindIntersectionPoint(firstOffsetLine, secondOffsetLine);

                    var pko = new PromptKeywordOptions("\n" + ResourceStrings.Accept_Position) { AppendKeywordsToMessage = true, AllowNone = true };
                    pko.Keywords.Add(Keywords.ACCEPT);
                    pko.Keywords.Add(Keywords.CANCEL);
                    pko.Keywords.Default = Keywords.ACCEPT;

                    graphics.ClearGraphics();
                    graphics.DrawPlus(intersectionPoint.ToPoint3d(), Settings.GraphicsSize);

                    var cancelled = false;
                    do
                    {
                        PromptResult prResult = AcadApp.Editor.GetKeywords(pko);

                        switch (prResult.Status)
                        {
                            case PromptStatus.Cancel:
                            case PromptStatus.None:
                            case PromptStatus.Error:
                                cancelled = true;
                                break;
                            case PromptStatus.OK:
                            case PromptStatus.Keyword:
                                switch (prResult.StringResult)
                                {
                                    case Keywords.ACCEPT:
                                        createAction(tr, intersectionPoint.ToPoint3d());
                                        cancelled = true;
                                        break;
                                    case Keywords.CANCEL:
                                        cancelled = true;
                                        break;
                                }
                                break;
                        }

                    } while (!cancelled);
                    tr.Commit();
                }
            }
            catch (Exception e)
            {
                AcadApp.Editor.WriteMessage(e.Message);
            }
            finally
            {
                graphics.Dispose();
            }
        }

        /// <summary>
        /// Creates a point at the intersection of a bearing from one point and distance from a second.
        /// </summary>
        public static void Create_At_Intersection_Of_Angle_And_Distance(Action<Transaction, Point3d> createAction)
        { 
            var graphics = new TransientGraphics();
            try
            {
                if (!EditorUtils.GetPoint(out Point3d firstPoint, "\n3DS> Pick first point: "))
                    return;

                graphics.DrawPlus(firstPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetAngle(out Angle angle1, "\n3DS> Enter angle: ", firstPoint))
                    return;

                var constructionPoint = PointHelpers.AngleAndDistanceToPoint(angle1, 32000, firstPoint.ToPoint());
                graphics.DrawLine(firstPoint, constructionPoint.ToPoint3d());

                if (!EditorUtils.GetPoint(out Point3d secondPoint, "\n3DS> Pick second point: "))
                    return;

                graphics.DrawPlus(secondPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetDistance(out double dist, "\n3DS> Enter distance: ", secondPoint))
                    return;

                graphics.DrawCircle(secondPoint, dist);

                var canIntersect = PointHelpers.AngleDistanceIntersection(firstPoint.ToPoint(), angle1, secondPoint.ToPoint(), dist, out Point firstInt, out Point secondInt);

                if (!canIntersect)
                {
                    AcadApp.Editor.WriteMessage("\n3DS> No intersection found! ");
                    return;
                }

                graphics.DrawDot(firstInt.ToPoint3d(), Settings.GraphicsSize/2);
                graphics.DrawDot(secondInt.ToPoint3d(), Settings.GraphicsSize/2);
                AcadApp.Editor.WriteMessage($"\n3DS> First intersection found at X:{Math.Round(firstInt.X, 4)} Y:{Math.Round(firstInt.Y, 4)}");
                AcadApp.Editor.WriteMessage($"\n3DS> Second intersection found at X:{Math.Round(secondInt.X, 4)} Y:{Math.Round(secondInt.Y, 4)}");

                if (!EditorUtils.GetPoint(out Point3d pickedPoint, "\n3DS> Pick near desired intersection: "))
                    return;

                using (var tr = AcadApp.StartTransaction())
                {
                    graphics.ClearGraphics();
                    if (PointHelpers.GetDistanceBetweenPoints(pickedPoint.ToPoint(), firstInt) <= PointHelpers.GetDistanceBetweenPoints(pickedPoint.ToPoint(), secondInt))
                    {
                        //use first point
                        //CreatePoint(tr, firstInt.ToPoint3d());
                        graphics.DrawDot(firstInt.ToPoint3d(), Settings.GraphicsSize/2);
                        createAction(tr, firstInt.ToPoint3d());
                    }
                    else
                    {
                        //use second point
                        graphics.DrawDot(secondInt.ToPoint3d(), Settings.GraphicsSize/2);
                        createAction(tr, secondInt.ToPoint3d());
                    }

                    tr.Commit();
                }
            }
            catch (Exception e)
            {
                AcadApp.Editor.WriteMessage($"3DS> Command Exception: {e.Message}");
            }
            finally
            {
                graphics.Dispose();
            }
        }

        /// <summary>
        /// Creates a point at the production of a line and distance.
        /// </summary>
        public static void Create_At_Production_Of_Line_And_Distance(Action<Transaction, Point3d> createAction)
        {
            var graphics = new TransientGraphics();
            using (Transaction tr = AcadApp.StartTransaction())
            {
                try
                {
                    Line line = LineUtils.GetNearestPointOfLineOrPolylineSegment(tr, out Point3d basePoint);

                    if (line == null)
                        return;

                    graphics.DrawLine(line, TransientDrawingMode.Highlight);
                    graphics.DrawPlus(basePoint, Settings.GraphicsSize);

                    Angle angle = LineUtils.GetAngleOfLine(line);

                    // If the basePoint is equal to the lines StartPoint, we want the angle to go in the
                    // opposite direction. So we Flip().
                    if (basePoint == line.StartPoint)
                        angle = angle.Flip();

                    if (!EditorUtils.GetDistance(out double dist, "\n" + ResourceStrings.Offset_Distance, basePoint))
                        return;

                    var pko = new PromptKeywordOptions("\n" + ResourceStrings.Accept_Position) { AppendKeywordsToMessage = true, AllowNone = true };
                    pko.Keywords.Add(Keywords.ACCEPT);
                    pko.Keywords.Add(Keywords.CANCEL);
                    pko.Keywords.Add(Keywords.FLIP);
                    pko.Keywords.Default = Keywords.ACCEPT;

                    Point point = PointHelpers.AngleAndDistanceToPoint(angle, dist, basePoint.ToPoint());

                    graphics.ClearGraphics();
                    graphics.DrawPlus(basePoint, Settings.GraphicsSize);
                    graphics.DrawX(point.ToPoint3d(), Settings.GraphicsSize);
                    graphics.DrawLine(basePoint, point.ToPoint3d());

                    var cancelled = false;
                    PromptResult prResult;
                    do
                    {
                        prResult = AcadApp.Editor.GetKeywords(pko);

                        if (prResult.Status != PromptStatus.Keyword &&
                            prResult.Status != PromptStatus.OK)
                            continue;

                        switch (prResult.StringResult)
                        {
                            case Keywords.NONE: // If user doesn't enter anything.
                            case Keywords.ACCEPT:
                                createAction(tr, point.ToPoint3d());
                                cancelled = true;
                                break;
                            case Keywords.CANCEL:
                                cancelled = true;
                                break;
                            case Keywords.FLIP:
                                angle = angle.Flip();
                                point = PointHelpers.AngleAndDistanceToPoint(angle, dist, basePoint.ToPoint());
                                graphics.ClearGraphics();
                                graphics.DrawPlus(basePoint, Settings.GraphicsSize);
                                graphics.DrawX(point.ToPoint3d(), Settings.GraphicsSize);
                                graphics.DrawLine(basePoint, point.ToPoint3d());
                                break;
                        }
                    } while (prResult.Status != PromptStatus.Cancel && prResult.Status != PromptStatus.Error && !cancelled);

                    tr.Commit();
                }
                catch (Exception e)
                {
                    AcadApp.Editor.WriteMessage(e.ToString());
                }
                finally
                {
                    graphics.Dispose();
                }
            }
        }

        /// <summary>
        /// Creates a point at text location.
        /// </summary>
        public static void Create_At_Label_Location(Action<Transaction, Point3d> createAction, bool useTextAsElevation = false)
        {
            //TODO: Better method for building TypedValues ?
            //var typedValue = new TypedValue[1];
            //typedValue.SetValue(new TypedValue((int)DxfCode.Start, "TEXT,MTEXT"), 0);

            if (!EditorUtils.GetSelectionOfType<MText, DBText>(out var selectedTextIds, "\n3DS> Select text entities: "))
                return;

            using (var tr = AcadApp.StartTransaction())
            {
                foreach (ObjectId objectId in selectedTextIds)
                {
                    var textEnt = tr.GetObject(objectId, OpenMode.ForRead) as Entity;
                    Point3d point = Point3d.Origin;

                    if (objectId.IsType<DBText>())
                    {
                        var text = textEnt as DBText;

                        if (text == null)
                            throw new ArgumentNullException(nameof(text));

                        if (useTextAsElevation)
                        {
                            //add way to use text contents as elevation.
                            var elevText = StringHelpers.ExtractDoubleFromString(text.TextString);
                            point = new Point3d(text.Position.X, text.Position.Y, elevText);
                        }
                        else
                        {
                            point = new Point3d(text.Position.X, text.Position.Y, text.Position.Z);
                        }
                    }

                    if (objectId.IsType<MText>())
                    {
                        var text = textEnt as MText;

                        if (text == null)
                            throw new ArgumentNullException(nameof(text));

                        if (useTextAsElevation)
                        {
                            var elevText = StringHelpers.ExtractDoubleFromString(text.Contents);
                            point = new Point3d(text.Location.X, text.Location.Y, elevText);
                        }
                        else
                        {
                            point = new Point3d(text.Location.X, text.Location.Y, text.Location.Z);
                        }

                    }
                    createAction(tr, point);
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// Creates points at an offset between points.
        /// </summary>
        public static void Create_At_Offset_Between_Points(Action<Transaction, Point3d> createAction)
        {
            var graphics = new TransientGraphics();
            try
            {
                if (!EditorUtils.GetPoint(out Point3d firstPoint, "\n3DS> Pick first point: "))
                    return;

                graphics.DrawX(firstPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetPoint(out Point3d secondPoint, "\n3DS> Pick second point: "))
                    return;

                graphics.DrawX(secondPoint, Settings.GraphicsSize);
                graphics.DrawLine(firstPoint, secondPoint);

                var baseLine = AngleHelpers.GetAngleBetweenPoints(firstPoint.ToPoint(), secondPoint.ToPoint());

                do
                {
                    using (var tr = AcadApp.StartTransaction())
                    {
                        if (!EditorUtils.GetDistance(out double horizontalDist, "\n3DS> Enter distance along line: ", firstPoint))
                            break;

                        var basePoint = PointHelpers.AngleAndDistanceToPoint(baseLine, horizontalDist, firstPoint.ToPoint());

                        if (!EditorUtils.GetDistance(out double leftOffsetDist, "\n3DS> Enter left offset: ", basePoint.ToPoint3d()))
                            break;

                        var leftOffsetPt = PointHelpers.AngleAndDistanceToPoint(baseLine - 90, leftOffsetDist, basePoint);
                        graphics.DrawDot(leftOffsetPt.ToPoint3d(), Settings.GraphicsSize);
                        createAction(tr, leftOffsetPt.ToPoint3d());

                        if (!EditorUtils.GetDistance(out double rightOffsetDist, "\n3DS> Enter right offset: ", basePoint.ToPoint3d()))
                            break;

                        var rightOffsetPt = PointHelpers.AngleAndDistanceToPoint(baseLine + 90, rightOffsetDist, basePoint);
                        graphics.DrawDot(rightOffsetPt.ToPoint3d(), Settings.GraphicsSize);
                        createAction(tr, rightOffsetPt.ToPoint3d());

                        tr.Commit();

                        graphics.ClearGraphics();
                        //redraw base graphics.
                        graphics.DrawX(firstPoint, Settings.GraphicsSize);
                        graphics.DrawX(secondPoint, Settings.GraphicsSize);
                        graphics.DrawLine(firstPoint, secondPoint);
                    }
                } while (true);
            }
            catch (Exception e)
            {
                AcadApp.WriteErrorMessage(e);
            }
            finally
            {
                graphics.Dispose();
            }

 


        }

        /// <summary>
        /// Creates at distance between points.
        /// </summary>
        /// <remarks>
        /// After picking the two points to define the line, the point numbers, slope, horizontal,
        /// and vertical distances are displayed. The points created do not have to lie between the
        /// chosen points. You can enter a negative distance to create a point back from the first
        /// point or a distance greater than the distance between the points to create a point beyond
        /// the second point.
        /// </remarks>
        public static void Create_At_Distance_Between_Points(Action<Transaction, Point3d> createAction)
        {
            using (var graphics = new TransientGraphics())
            using (var tr = AcadApp.StartTransaction())
            {
                if (!EditorUtils.GetPoint(out Point3d firstPoint, "\n3DS> First point: "))
                    return;

                graphics.DrawPlus(firstPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetPoint(out Point3d secondPoint, "\n3DS> Second point: "))
                    return;

                var deltaZ = secondPoint.Z - firstPoint.Z;
                var angle = AngleHelpers.GetAngleBetweenPoints(firstPoint.ToPoint(), secondPoint.ToPoint());
                var distBetween = PointHelpers.GetDistanceBetweenPoints(firstPoint.ToPoint(), secondPoint.ToPoint());
                var midPoint = PointHelpers.GetMidpointBetweenPoints(firstPoint.ToPoint(), secondPoint.ToPoint());

                graphics.DrawPlus(secondPoint, Settings.GraphicsSize);
                graphics.DrawLine(firstPoint, secondPoint);
                graphics.DrawArrow(midPoint.ToPoint3d(), angle, Settings.GraphicsSize);

                AcadApp.Editor.WriteMessage($"\n3DS> Total distance: {Math.Round(distBetween, 3)}");

                do
                {
                    if (!EditorUtils.GetDouble(out double distance, "\n3DS> Enter distance: ", allowZero: false))
                        break;

                    var point = PointHelpers.AngleAndDistanceToPoint(angle, distance, firstPoint.ToPoint());
                    var elevation = firstPoint.Z + deltaZ * (distance / distBetween);

                    var newPoint = new Point3d(point.X, point.Y, elevation);
                    graphics.DrawPlus(newPoint, Settings.GraphicsSize);
                    createAction(tr, newPoint);

                } while (true);

                tr.Commit();
            }
        }

        /// <summary>
        /// Add multiple points (with interpolated elevation) between two points.
        /// </summary>
        public static void Create_Between_Points(Action<Transaction, Point3d> createAction)
        {
            var graphics = new TransientGraphics();
            try
            {
                if (!EditorUtils.GetPoint(out Point3d firstPoint, "\n3DS> Pick first point: "))
                    return;

                graphics.DrawPlus(firstPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetPoint(out Point3d secondPoint, "\n3DS> Pick second point: "))
                    return;
                
                graphics.DrawPlus(secondPoint, Settings.GraphicsSize);
                graphics.DrawLine(firstPoint, secondPoint);

                graphics.DrawChainage(firstPoint, secondPoint);

                // Calculate angle and distances from picked points.
                Angle angleBetweenPoints = AngleHelpers.GetAngleBetweenPoints(firstPoint.ToPoint(), secondPoint.ToPoint());
                double distanceBetweenPoints = PointHelpers.GetDistanceBetweenPoints(firstPoint.ToPoint(), secondPoint.ToPoint());
                double elevationDifference = secondPoint.Z - firstPoint.Z;

                using (var tr = AcadApp.StartTransaction())
                {
                    bool cancelled = false;
                    do
                    {
                        //TODO: Implement way to show point moving along line relative to mouse position for point creation.
                        /*
                        var pointerGraphics = new TransientGraphics();
                        AcadApp.Editor.PointMonitor += CreatePointBetweenPoints_PointMonitor;
                        Having brain wave moment. Can use methods like intersect 2 bearings to calculate point
                        on the line relative to the mouse position. if we take the line and add 90°? depending
                        on which side of the line the mouse is. we can use the IsLeft() method.
                        need to pass in new graphics object so we can clear it each move.
                        write own event and handler. to pass points etc.
                        */

                        if (!EditorUtils.GetDistance(out double distance, "\n3DS> Enter distance: ", firstPoint))
                            cancelled = true;

                        var newPoint = PointHelpers.AngleAndDistanceToPoint(angleBetweenPoints, distance, firstPoint.ToPoint());
                        var point3d = new Point3d(newPoint.X, newPoint.Y, firstPoint.Z + elevationDifference * (distance / distanceBetweenPoints));
                    
                        graphics.DrawDot(newPoint.ToPoint3d(), Settings.GraphicsSize);
                        
                        createAction(tr, point3d);
                        
                    } while (!cancelled);
                    tr.Commit();
                }
            }
            catch (Exception e)
            {
                AcadApp.Editor.WriteMessage($"3DS> Command Exception: {e.Message}");
            }
            finally
            {
                //AcadApp.Editor.PointMonitor -= CreatePointBetweenPoints_PointMonitor;
                graphics.Dispose();
            }
        }


        //TODO: Hookup with create_between_points when I figure it out.

        private static void CreatePointBetweenPoints_PointMonitor(object sender, PointMonitorEventArgs e)
        {
            
        }

        public static void Inverse(Point3d firstPoint, Point3d secondPoint)
        {
            var angle = AngleHelpers.GetAngleBetweenPoints(firstPoint.ToPoint(), secondPoint.ToPoint());
            var distance = PointHelpers.GetDistanceBetweenPoints(firstPoint.ToPoint(), secondPoint.ToPoint(), true, SystemVariables.LUPREC);
            var delta = MathHelpers.DeltaPoint(firstPoint.ToPoint(), secondPoint.ToPoint());
            var slope = Math.Round(Math.Abs(delta.Z / distance * 100), 3);

            AcadApp.Editor.WriteMessage($"\n3DS> Angle: {angle} ({angle.Flip()})");
            AcadApp.Editor.WriteMessage($"\n3DS> Distance: {distance}");
            AcadApp.Editor.WriteMessage($"\n3DS> dX:{delta.X} dY:{delta.Y} dZ:{delta.Z}");
            AcadApp.Editor.WriteMessage($"\n3DS> Slope:{slope}%");
        }


        /// <summary>
        /// Inverses between points (pick), echoes coordinates, 
        /// azimuths, bearings, horz/vert distance and slope.
        /// </summary>
        public static void Inverse_Pick()
        {
            var graphics = new TransientGraphics();
            try
            {
                // Pick first point.
                if (!EditorUtils.GetPoint(out Point3d firstPoint, "\n3DS> Pick first point: "))
                    return;

                // Highlight first point.
                graphics.DrawX(firstPoint, Settings.GraphicsSize);

                // Pick second point.
                if (!EditorUtils.GetPoint(out Point3d secondPoint, "\n3DS> Pick second point: "))
                    return;

                Inverse(firstPoint, secondPoint);
            }
            catch (Exception e)
            {
                AcadApp.Editor.WriteMessage(e.ToString());
            }
            finally
            {
                graphics.Dispose();
            }
        }

        /// <summary>
        /// Does the same as <see cref="Inverse_Pick"/> but displays the information on the screen.
        /// </summary>
        public static void Inverse_Pick_Display()
        {
            var graphics = new TransientGraphics();
            try
            {
                while (true)
                {
                    bool loopPick = EditorUtils.GetPoint(out Point3d firstPoint, "\n3DS> Pick first point: ");

                    if (!loopPick)
                        break;

                    // Highlight first point.
                    graphics.DrawX(firstPoint, Settings.GraphicsSize);

                    // Pick second point.
                    if (!EditorUtils.GetPoint(out Point3d secondPoint, "\n3DS> Pick second point: "))
                        return;

                    var angle = AngleHelpers.GetAngleBetweenPoints(firstPoint.ToPoint(), secondPoint.ToPoint());
                    var distance = PointHelpers.GetDistanceBetweenPoints(firstPoint.ToPoint(), secondPoint.ToPoint());
                    var delta = MathHelpers.DeltaPoint(firstPoint.ToPoint(), secondPoint.ToPoint());
                    var slope = Math.Round(Math.Abs(delta.Z / distance * 100), 3);

                    var midPoint = PointHelpers.GetMidpointBetweenPoints(firstPoint.ToPoint(), secondPoint.ToPoint());
                    graphics.ClearGraphics();
                    graphics.DrawX(firstPoint, Settings.GraphicsSize);
                    graphics.DrawX(secondPoint, Settings.GraphicsSize);
                    graphics.DrawLine(firstPoint, secondPoint);
                    graphics.DrawText(midPoint.ToPoint3d(), $"bearing: {angle} \\P dist: {distance} \\P dX:{delta.X} dY:{delta.Y} dZ:{delta.Z} \\P Slope:{slope}%", 1.0, angle.GetOrdinaryAngle());
                }
            }
            catch (Exception e)
            {
                AcadApp.Editor.WriteMessage(e.ToString());
            }
            finally
            {
                graphics.Dispose();
            }
        }

        /// <summary>
        /// Inverses the perpendicular.
        /// </summary>
        public static void Inverse_Pick_Perpendicular()
        {
            var graphics = new TransientGraphics();
            try
            {
                if (!EditorUtils.GetPoint(out Point3d firstPoint, "\n3DS> Pick first point: "))
                    return;

                graphics.DrawPlus(firstPoint, Settings.GraphicsSize);

                if (!EditorUtils.GetPoint(out Point3d secondPoint, "\n3DS> Pick second point: "))
                    return;

                graphics.DrawPlus(secondPoint, Settings.GraphicsSize);
                graphics.DrawLine(firstPoint, secondPoint);

                do
                {
                    if (!EditorUtils.GetPoint(out Point3d pickedPoint, "\n3DS> Pick offset point: "))
                        break;

                    var canIntersect = PointHelpers.PerpendicularIntersection(firstPoint.ToPoint(), secondPoint.ToPoint(), pickedPoint.ToPoint(), out Point intersectionPoint);

                    if (!canIntersect)
                    {
                        AcadApp.Editor.WriteMessage("\n3DS> No intersection found. ");
                        continue;
                    }

                    graphics.DrawX(intersectionPoint.ToPoint3d(), Settings.GraphicsSize);
                    graphics.DrawX(pickedPoint, Settings.GraphicsSize);
                    graphics.DrawLine(pickedPoint, intersectionPoint.ToPoint3d());

                    var distance = PointHelpers.GetDistanceBetweenPoints(pickedPoint.ToPoint(), intersectionPoint, true);
                    var angle = AngleHelpers.GetAngleBetweenPoints(pickedPoint.ToPoint(), intersectionPoint);

                    var midPt = PointHelpers.GetMidpointBetweenPoints(pickedPoint.ToPoint(), intersectionPoint);
                    
                    graphics.DrawText(midPt.ToPoint3d(), $"bearing: {angle} \\P dist: {distance}", 1, angle);

                    AcadApp.Editor.WriteMessage($"\n3DS> Angle: {angle}");
                    AcadApp.Editor.WriteMessage($"\n3DS> Distance: {distance}");

                } while (true);
            }
            catch (Exception e)
            {
                AcadApp.WriteErrorMessage(e);
            }
            finally
            {
                graphics.Dispose();
            }
        }


        /// <summary>
        /// Creates a <see cref="DBPoint"/> at the specified location.
        /// </summary>
        /// <param name="tr">The existing transaction.</param>
        /// <param name="position">The position to create the point at.</param>
        /// <remarks>Don't forget to Commit(); the transaction after using.</remarks>
        public static void CreatePoint(Transaction tr, Point3d position)
        {
            // Open the Block table for read
            var bt = tr.GetObject(AcadApp.ActiveDocument.Database.BlockTableId, OpenMode.ForRead) as BlockTable;

            if (bt == null)
                return;

            // Open the Block table record Model space for write
            var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            if (btr == null)
                return;

            DBPoint acPoint = new DBPoint(position);
            acPoint.SetDatabaseDefaults();

            // Add the new object to the block table record and the transaction
            btr.AppendEntity(acPoint);
            tr.AddNewlyCreatedDBObject(acPoint, true);

            // Save the new object to the database
            // Don't commit, leave it up to the calling method.
        }

    }
}