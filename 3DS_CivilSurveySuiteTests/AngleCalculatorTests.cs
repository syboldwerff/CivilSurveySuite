﻿// Copyright Scott Whitney. All Rights Reserved.
// Reproduction or transmission in whole or in part, any form or by any
// means, electronic, mechanical or otherwise, is prohibited without the
// prior written consent of the copyright owner.

using System;
using _3DS_CivilSurveySuite.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace _3DS_CivilSurveySuiteTests
{
    [TestClass]
    public class AngleCalculatorTests
    {
        [TestMethod]
        public void Add_TwoAngleLessThan360Degrees_ReturnSum()
        {
            var angle1 = new Angle { Degrees = 50, Minutes = 10, Seconds = 10 };
            var angle2 = new Angle { Degrees = 50, Minutes = 10, Seconds = 10 };

            var result = Angle.Add(angle1, angle2);

            var expected = new Angle { Degrees = 100, Minutes = 20, Seconds = 20 };

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Add_TwoAngleGreaterThan360Degrees_ReturnSum()
        {
            var angle1 = new Angle { Degrees = 360, Minutes = 0, Seconds = 0 };
            var angle2 = new Angle { Degrees = 100, Minutes = 0, Seconds = 0 };

            var result = Angle.Add(angle1, angle2);

            var expected = new Angle { Degrees = 460, Minutes = 0, Seconds = 0 };

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Subtract_TwoAngleLessThan0Degrees_ReturnSum()
        {
            var angle1 = new Angle { Degrees = 10, Minutes = 0, Seconds = 0 };
            var angle2 = new Angle { Degrees = 100, Minutes = 0, Seconds = 0 };

            var result = Angle.Subtract(angle1, angle2);

            var expected = new Angle { Degrees = -90, Minutes = 0, Seconds = 0 };

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Add_TwoAngleWithNegative()
        {
            var angle1 = new Angle { Degrees = 10, Minutes = 0, Seconds = 0 };
            var angle2 = new Angle { Degrees = -100, Minutes = 0, Seconds = 0 };

            var result = Angle.Add(angle1, angle2);

            var expected = new Angle { Degrees = -90, Minutes = 0, Seconds = 0};

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Test_PlusMinusAngle_PositiveIntoNegative()
        {
            var angle1 = new Angle { Degrees = 10, Minutes = 0, Seconds = 0 };
            var result = PlusMinusAngle(angle1);
            var expected = new Angle { Degrees = -10, Minutes = 0, Seconds = 0 };

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Test_PlusMinusAngle_NegativeIntoPositive()
        {
            var angle1 = new Angle { Degrees = -10, Minutes = 0, Seconds = 0 };
            var result = PlusMinusAngle(angle1);
            var expected = new Angle { Degrees = 10, Minutes = 0, Seconds = 0 };

            Assert.AreEqual(expected, result);
        }

        private static Angle PlusMinusAngle(Angle angle)
        {
            if (angle.Degrees > 0)
                return new Angle { Degrees = angle.Degrees * -1, Minutes = angle.Minutes, Seconds = angle.Seconds };
            else
                return new Angle { Degrees = Math.Abs(angle.Degrees), Minutes = angle.Minutes, Seconds = angle.Seconds };
        }
    }
}