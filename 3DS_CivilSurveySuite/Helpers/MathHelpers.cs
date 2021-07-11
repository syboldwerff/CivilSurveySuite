﻿// Copyright Scott Whitney. All Rights Reserved.
// Reproduction or transmission in whole or in part, any form or by any
// means, electronic, mechanical or otherwise, is prohibited without the
// prior written consent of the copyright owner.

using System;
using System.Collections.Generic;
using _3DS_CivilSurveySuite.Abstraction;
using _3DS_CivilSurveySuite.Model;
using Autodesk.AutoCAD.Geometry;

namespace _3DS_CivilSurveySuite.Helpers
{
    public static class MathHelpers
    {
        /// <summary>
        /// Converts links to meters
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public static double ConvertLinkToMeters(double link)
        {
            const double linkConversion = 0.201168;

            return Math.Round(link * linkConversion, 4);
        }

        /// <summary>
        /// Converts feet and inches to meters
        /// </summary>
        /// <param name="feetAndInches">
        /// Feet and inches represented as decimal. 5feet 2inch 5.02.
        /// Inches less than 10 must have a preceding 0. 
        /// </param>
        /// <returns></returns>
        public static double ConvertFeetToMeters(double feetAndInches)
        {
            const double feetConversion = 0.3048;
            const double inchConversion = 0.0254;

            var feet = Math.Truncate(feetAndInches) * feetConversion;
            var inch1 = feetAndInches - Math.Truncate(feetAndInches);
            var inch2 = (inch1 * 100) * inchConversion;

            return Math.Round(feet + inch2, 4);
        }

        /// <summary>
        /// Converts <see cref="Angle"/> object to decimal degrees
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static double AngleToDecimalDegrees(Angle angle)
        {
            if (angle == null)
                return 0;

            double minutes = (double) angle.Minutes / 60;
            double seconds = (double) angle.Seconds / 3600;

            double decimalDegree = angle.Degrees + minutes + seconds;

            return decimalDegree;
        }

        /// <summary>
        /// Converts decimal degrees to radians
        /// </summary>
        /// <param name="decimalDegrees"></param>
        /// <returns>A double value containing the decimal degrees in radians.</returns>
        public static double DecimalDegreesToRadians(double decimalDegrees)
        {
            return decimalDegrees * (Math.PI / 180);
        }

        /// <summary>
        /// Converts decimal degrees to <see cref="Angle"/> object
        /// </summary>
        /// <param name="decimalDegrees"></param>
        /// <returns></returns>
        public static Angle DecimalDegreesToDMS(double decimalDegrees)
        {
            var degrees = Math.Floor(decimalDegrees);
            var minutes = Math.Floor((decimalDegrees - degrees) * 60);
            var seconds = Math.Round(((decimalDegrees - degrees) * 60 - minutes) * 60, 0);

            return new Angle { Degrees = (int) degrees, Minutes = (int) minutes, Seconds = (int) seconds };
        }

        /// <summary>
        /// Gets distance between two coordinates
        /// </summary>
        /// <param name="x1">Easting of first coordinate</param>
        /// <param name="x2">Easting of second coordinate</param>
        /// <param name="y1">Northing of first coordinate</param>
        /// <param name="y2">Northing of second coordinate</param>
        /// <returns>double</returns>
        public static double DistanceBetweenPoints(double x1, double x2, double y1, double y2)
        {
            double x = Math.Abs(x1 - x2);
            double y = Math.Abs(y1 - y2);

            double distance = Math.Round(Math.Sqrt(x * x + y * y), 4);

            return distance;
        }

        /// <summary>
        /// Gets angle/bearing between two coordinates
        /// </summary>
        /// <param name="x1">Easting of first coordinate</param>
        /// <param name="x2">Easting of second coordinate</param>
        /// <param name="y1">Northing of first coordinate</param>
        /// <param name="y2">Northing of second coordinate</param>
        /// <returns><see cref="Angle"/></returns>
        public static Angle AngleBetweenPoints(double x1, double x2, double y1, double y2)
        {
            double rad = Math.Atan2(x2 - x1, y2 - y1);

            if (rad < 0)
                rad += 2 * Math.PI; // if radians is less than 0 add 2PI

            double decDeg = Math.Abs(rad) * 180 / Math.PI;
            return DecimalDegreesToDMS(decDeg);
        }

        /// <summary>
        /// Converts a list of <see cref="Angle"/> objects into a list of <see cref="Point2d"/> objects.
        /// </summary>
        /// <param name="bearingList"></param>
        /// <param name="basePoint"></param>
        /// <returns>collection of <see cref="Point2d"/></returns>
        public static List<Point2d> BearingAndDistanceToCoordinates(IEnumerable<TraverseObject> bearingList, Point2d basePoint)
        {
            var pointList = new List<Point2d>();
            pointList.Add(basePoint);

            var i = 0;
            foreach (TraverseObject item in bearingList)
            {
                double dec = AngleToDecimalDegrees(item.Angle);
                double rad = DecimalDegreesToRadians(dec);

                double departure = item.Distance * Math.Sin(rad);
                double latitude = item.Distance * Math.Cos(rad);

                double newX = Math.Round(pointList[i].X + departure, 4);
                double newY = Math.Round(pointList[i].Y + latitude, 4);

                pointList.Add(new Point2d(newX, newY));
                i++;
            }

            return pointList;
        }

        /// <summary>
        /// Converts a <see cref="IEnumerable{T}"/> of <see cref="TraverseAngleObject"/> to a List of <see cref="Point2d"/>.
        /// </summary>
        /// <param name="angleList">A enumerable list containing the <see cref="TraverseAngleObject"/>'s.</param>
        /// <param name="basePoint">The base point.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="Point2d"/>.</returns>
        public static List<Point2d> AngleAndDistanceToCoordinates(IEnumerable<TraverseAngleObject> angleList, Point2d basePoint)
        {
            var newPointList = new List<Point2d> { basePoint };
            var lastBearing = new Angle();
            var i = 0;
            foreach (TraverseAngleObject item in angleList)
            {
                Angle nextBearing = lastBearing;

                if (!item.Angle.IsEmpty)
                {
                    switch (item.ReferenceDirection)
                    {
                        case AngleReferenceDirection.Backward:
                            nextBearing = lastBearing - new Angle(180);
                            break;
                        case AngleReferenceDirection.Forward:
                            nextBearing = lastBearing;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    switch (item.RotationDirection)
                    {
                        case AngleRotationDirection.Negative:
                            nextBearing -= item.Angle;
                            break;
                        case AngleRotationDirection.Positive:
                            nextBearing += item.Angle;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                newPointList.Add(AngleAndDistanceToPoint(nextBearing, item.Distance, newPointList[i]));
                lastBearing = nextBearing;
                i++;
            }
            return newPointList;
        }

        private static Point2d AngleAndDistanceToPoint(Angle angle, double distance, Point2d basePoint)
        {
            double dec = AngleToDecimalDegrees(angle);
            double rad = DecimalDegreesToRadians(dec);

            double departure = distance * Math.Sin(rad);
            double latitude = distance * Math.Cos(rad);

            double newX = Math.Round(basePoint.X + departure, 4);
            double newY = Math.Round(basePoint.Y + latitude, 4);

            return new Point2d(newX, newY);
        }
    }
}