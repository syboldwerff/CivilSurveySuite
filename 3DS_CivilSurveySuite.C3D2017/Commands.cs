﻿// Copyright Scott Whitney. All Rights Reserved.
// Reproduction or transmission in whole or in part, any form or by any
// means, electronic, mechanical or otherwise, is prohibited without the
// prior written consent of the copyright owner.

using _3DS_CivilSurveySuite.ACAD2017;
using _3DS_CivilSurveySuite.UI.Views;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;

[assembly: CommandClass(typeof(_3DS_CivilSurveySuite.C3D2017.Commands))]
namespace _3DS_CivilSurveySuite.C3D2017
{
    public static class Commands
    {
        [CommandMethod("3DS", "_3DSCptBrgDist", CommandFlags.Modal)]
        public static void CptBrgDist()
        {
            PointUtils.Create_At_Angle_And_Distance(CogoPointUtils.CreatePoint);
        }

        [CommandMethod("3DS", "_3DSCptProdDist", CommandFlags.Modal)]
        public static void CptProdDist()
        {
            PointUtils.Create_At_Production_Of_Line_And_Distance(CogoPointUtils.CreatePoint);
        }

        [CommandMethod("3DS", "_3DSCptOffsetLn", CommandFlags.Modal)]
        public static void CptOffsetLn()
        {
            PointUtils.Create_At_Offset_Two_Lines(CogoPointUtils.CreatePoint);
        }

        [CommandMethod("3DS", "_3DSCptIntBrg", CommandFlags.Modal)]
        public static void CptIntBrg()
        {
            PointUtils.Create_At_Intersection_Two_Bearings(CogoPointUtils.CreatePoint);
        }

        [CommandMethod("3DS", "_3DSCptIntDist", CommandFlags.Modal)]
        public static void CptIntDist()
        {
            PointUtils.Create_At_Intersection_Two_Distances(CogoPointUtils.CreatePoint);
        }

        [CommandMethod("3DS", "_3DSCptIntBrd", CommandFlags.Modal)]
        public static void CptIntBearingDist()
        {
            PointUtils.Create_At_Intersection_Of_Angle_And_Distance(CogoPointUtils.CreatePoint);
        }

        [CommandMethod("3DS", "_3DSCptIntFour", CommandFlags.Modal)]
        public static void CptIntFourPoint()
        {
            PointUtils.Create_At_Intersection_Of_Four_Points(CogoPointUtils.CreatePoint);
        }

        [CommandMethod("3DS", "_3DSCptMidBetweenPoly", CommandFlags.Modal)]
        public static void CptMidBetweenPoly()
        {
            PolylineUtils.MidPointBetweenPolylines(CogoPointUtils.CreatePoint);
        }

        [CommandMethod("3DS", "_3DSCptSlope", CommandFlags.Modal)]
        public static void PtSlope()
        {
            PointUtils.Create_At_Slope_At_Point(CogoPointUtils.CreatePoint);
        }

        [CommandMethod("3DS", "_3DSCptLabelIns", CommandFlags.Modal)]
        public static void CptAtLabelIns()
        {
            PointUtils.Create_At_Label_Location(CogoPointUtils.CreatePoint);
        }

        [CommandMethod("3DS", "_3DSCptLabelInsText", CommandFlags.Modal)]
        public static void CptAtLabelInsText()
        {
            PointUtils.Create_At_Label_Location(CogoPointUtils.CreatePoint, true);
        }

        [CommandMethod("3DS", "_3DSCptLabelsReset", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public static void CptResetLabels()
        {
            CogoPointUtils.LabelResetSelection();
        }

        [CommandMethod("3DS", "_3DSCptLabelsMove", CommandFlags.Modal)]
        public static void CptMoveLabels()
        {
            C3DService.ShowDialog<CogoPointMoveLabelView>();
        }

        [CommandMethod("3DS", "_3DSRawDesUpper", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public static void RawDesUpper()
        {
            CogoPointUtils.RawDescriptionToUpper();
        }

        [CommandMethod("3DS", "_3DSFullDesUpper", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public static void FullDesUpper()
        {
            CogoPointUtils.DescriptionFormatToUpper();
        }

        [CommandMethod("3DS", "_3DSCptMatchLblRot", CommandFlags.Modal)]
        public static void CptMatchLblRot()
        {
            CogoPointUtils.LabelRotateMatch();
        }

        [CommandMethod("3DS", "_3DSCptMatchMrkRot", CommandFlags.Modal)]
        public static void CptMatchMrkRot()
        {
            CogoPointUtils.MarkerRotateMatch();
        }

        [CommandMethod("3DS", "_3DSZoomToCpt", CommandFlags.Modal)]
        public static void ZoomToPt()
        {
            CogoPointUtils.ZoomPoint();
        }

        [CommandMethod("3DS", "_3DSCptInverse", CommandFlags.Modal)]
        public static void InverseCogoPoint()
        {
            CogoPointUtils.Inverse_ByPointNumber();
        }

        [CommandMethod("3DS", "_3DSCptUsedPts", CommandFlags.Modal)]
        public static void UsedPts()
        {
            CogoPointUtils.UsedPoints();
        }

        [CommandMethod("3DS", "_3DSCptSetNext", CommandFlags.Modal)]
        public static void CptSetNextPointNumber()
        {
            CogoPointUtils.SetNextPointNumber();
        }


        // Surfaces
        [CommandMethod("3DS", "_3DSSurfaceElAtPt", CommandFlags.Modal)]
        public static void SurfaceElevationAtPoint()
        {
            SurfaceUtils.GetSurfaceElevationAtPoint();
        }

        [CommandMethod("3DS", "3DSSurfaceAddBreaklines", CommandFlags.Modal)]
        public static void SurfaceAddBreaklines()
        {
            SurfaceUtils.AddBreaklineToSurface();
        }


        [CommandMethod("3DS", "3DSSurfaceRemoveBreaklines", CommandFlags.Modal)]
        public static void SurfaceRemoveBreaklines()
        {
            SurfaceUtils.RemoveBreaklinesFromSurface();
        }

        [CommandMethod("3DS", "3DSSurfaceSelAboveBelow", CommandFlags.Modal)]
        public static void SurfaceSelectAboveOrBelow()
        {
            SurfaceUtils.SelectPointsAboveOrBelowSurface();
        }


        // Palettes
        [CommandMethod("3DS", "3DSShowConnectLineworkWindow", CommandFlags.Modal)]
        public static void ShowConnectLinework()
        {
            C3DService.ShowDialog<ConnectLineworkView>();
        }

        [CommandMethod("3DS", "3DSShowCogoPointEditor", CommandFlags.Modal)]
        public static void ShowCogoPointEditor()
        {
            C3DService.ShowDialog<CogoPointEditorView>();
        }

        [CommandMethod("3DS", "3DSShowCogoPointFindReplace", CommandFlags.Modal)]
        public static void ShowCogoFindReplace()
        {
            C3DService.ShowDialog<CogoPointReplaceDuplicateView>();
        }


        // Labels
        [CommandMethod("3DS", "3DSLabelMaskOff", CommandFlags.Modal)]
        public static void LabelMaskOff()
        {
            CogoPointUtils.LabelMaskToggle(false);
        }

        [CommandMethod("3DS", "3DSLabelMaskOn", CommandFlags.Modal)]
        public static void LabelMaskOn()
        {
            CogoPointUtils.LabelMaskToggle(true);
        }

        [CommandMethod("3DS", "3DSLabelLineBreak", CommandFlags.Modal)]
        public static void LabelLineBreak()
        {
            CogoPointUtils.AddLineBreakToDescription();
        }

        [CommandMethod("3DS", "_3DSLabelStack", CommandFlags.Modal)]
        public static void StackLabels()
        {
            CogoPointUtils.LabelStack();
        }


        // Testing Command
        [CommandMethod("3DS", "3DSTest", CommandFlags.Modal)]
        public static void Test()
        {
            if (!EditorUtils.GetEntityOfType<CogoPoint>(out var cgId, ""))
            {
                return;
            }

            using (var tr = AcadApp.StartTransaction())
            {
                var cogoPoint = (CogoPoint)tr.GetObject(cgId, OpenMode.ForRead);

                var udp = CogoPointUtils.GetUDP("PointId");
                AcadApp.Editor.WriteMessage($"\n3DS> UDP: {cogoPoint.GetUDPValue(udp)}");

                tr.Commit();
            }



        }
    }
}