﻿// Copyright Scott Whitney. All Rights Reserved.
// Reproduction or transmission in whole or in part, any form or by any
// means, electronic, mechanical or otherwise, is prohibited without the
// prior written consent of the copyright owner.

using System;
using System.Globalization;
using _3DS_CivilSurveySuite.Core;
using Autodesk.AutoCAD.DatabaseServices;

namespace _3DS_CivilSurveySuite.ACAD2017
{
    public static class TextUtils
    {
        /// <summary>
        /// Creates a selection set with just TEXT and MTEXT entities.
        /// </summary>
        /// <param name="objectIds"><see cref="ObjectIdCollection"/> containing the selected entities.</param>
        /// <param name="message">The selection message.</param>
        /// <returns>True if the selection was successful, otherwise false.</returns>
        private static bool SelectText(out ObjectIdCollection objectIds, string message)
        {
            var typedValue = new TypedValue[1];
            typedValue.SetValue(new TypedValue((int)DxfCode.Start, "TEXT,MTEXT"), 0);

            objectIds = new ObjectIdCollection();

            if (!EditorUtils.GetSelection(out var selectedTextIds, typedValue, message))
                return false;

            objectIds = selectedTextIds;
            return true;
        }

        private static void UpdateText<T>(T textEntity, string updateText) where T : Entity
        {
            if (textEntity.ObjectId.IsType<DBText>())
            {
                var textObj = textEntity as DBText;
                if (textObj != null &&
                    textObj.TextString != updateText &&
                    textObj.IsWriteEnabled)
                {
                    textObj.TextString = updateText;
                }
            }

            if (textEntity.ObjectId.IsType<MText>())
            {
                var textObj = textEntity as MText;
                if (textObj != null &&
                    textObj.Contents != updateText &&
                    textObj.IsWriteEnabled)
                {
                    textObj.Contents = updateText;
                }
            }
        }

        private static string GetText<T>(T textEntity) where T : Entity
        {
            if (textEntity.ObjectId.IsType<DBText>())
            {
                var textObj = textEntity as DBText;
                if (textObj != null)
                {
                    return textObj.TextString;
                }
            }

            if (textEntity.ObjectId.IsType<MText>())
            {
                var textObj = textEntity as MText;
                if (textObj != null)
                {
                    return textObj.Contents;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Strips any alpha characters from text leaving only a number value (if it contains one)
        /// </summary>
        public static void RemoveAlphaCharactersFromText()
        {
            if (!SelectText(out var objectIds, "\n3DS> Select text entities: "))
                return;

            using (var tr = AcadApp.StartTransaction())
            {

                foreach (ObjectId objectId in objectIds)
                {
                    var textEnt = tr.GetObject(objectId, OpenMode.ForWrite) as Entity;

                    if (textEnt == null)
                        throw new InvalidOperationException("textEnd was null.");

                    string text = GetText(textEnt);
                    string cleanedString = StringHelpers.RemoveAlphaCharacters(text);

                    if (string.IsNullOrEmpty(cleanedString))
                        continue;

                    textEnt.UpgradeOpen();
                    UpdateText(textEnt, cleanedString);
                    textEnt.DowngradeOpen();
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// Adds a prefix to the selected text objects
        /// </summary>
        public static void AddPrefixToText()
        {
            //TODO: add space option?
            if (!EditorUtils.GetString(out string prefixText, "\n3DS> Enter prefix text: "))
                return;

            if (!SelectText(out var objectIds, "\n3DS> Select text entities: "))
                return;

            using (var tr = AcadApp.StartTransaction())
            {
                foreach (ObjectId objectId in objectIds)
                {
                    var textEnt = tr.GetObject(objectId, OpenMode.ForWrite) as Entity;

                    if (textEnt == null)
                        throw new InvalidOperationException("textEnd was null.");

                    textEnt.UpgradeOpen();
                    string text = prefixText + GetText(textEnt);
                    UpdateText(textEnt, text);
                    textEnt.DowngradeOpen();
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// Adds a suffix to the selected text objects.
        /// </summary>
        public static void AddSuffixToText()
        {
            if (!EditorUtils.GetString(out string suffixText, "\n3DS> Enter suffix text: "))
                return;

            if (!SelectText(out var objectIds, "\n3DS> Select text entities: "))
                return;

            using (var tr = AcadApp.StartTransaction())
            {
                foreach (ObjectId objectId in objectIds)
                {
                    var textEnt = tr.GetObject(objectId, OpenMode.ForWrite) as Entity;

                    if (textEnt == null)
                        throw new InvalidOperationException("textEnd was null.");

                    textEnt.UpgradeOpen();
                    string text = GetText(textEnt) + suffixText;
                    UpdateText(textEnt, text);
                    textEnt.DowngradeOpen();
                }
                tr.Commit();
            }


        }

        /// <summary>
        /// Converts the selected text to Uppercase
        /// </summary>
        public static void TextToUpper()
        {
            if (!SelectText(out var objectIds, "\n3DS> Select text entities: "))
                return;

            using (var tr = AcadApp.StartTransaction())
            {
                foreach (ObjectId objectId in objectIds)
                {
                    var textEnt = tr.GetObject(objectId, OpenMode.ForWrite) as Entity;

                    if (textEnt == null)
                        throw new InvalidOperationException("textEnd was null.");

                    string text = GetText(textEnt).ToUpper();
                    textEnt.UpgradeOpen();
                    UpdateText(textEnt, text);
                    textEnt.DowngradeOpen();
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// Converts the selected text to lowercase
        /// </summary>
        public static void TextToLower()
        {
            if (!SelectText(out var objectIds, "\n3DS> Select text entities: "))
                return;

            using (var tr = AcadApp.StartTransaction())
            {
                foreach (ObjectId objectId in objectIds)
                {
                    var textEnt = tr.GetObject(objectId, OpenMode.ForWrite) as Entity;

                    if (textEnt == null)
                        throw new InvalidOperationException("textEnd was null.");

                    string text = GetText(textEnt).ToLower();

                    textEnt.UpgradeOpen();
                    UpdateText(textEnt, text);
                    textEnt.DowngradeOpen();
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// Converts the selected text to sentence case
        /// </summary>
        public static void TextToSentence()
        {
            if (!SelectText(out var objectIds, "\n3DS> Select text entities: "))
                return;

            using (var tr = AcadApp.StartTransaction())
            {
                foreach (ObjectId objectId in objectIds)
                {
                    var textEnt = tr.GetObject(objectId, OpenMode.ForRead) as Entity;

                    if (textEnt == null)
                        throw new InvalidOperationException("textEnd was null.");

                    string text = GetText(textEnt).ToSentence();

                    textEnt.UpgradeOpen();
                    UpdateText(textEnt, text);
                    textEnt.DowngradeOpen();
                }
                tr.Commit();
            }

        }

        /// <summary>
        /// Adds a number to a text entity if it is a valid number
        /// </summary>
        public static void AddNumberToText()
        {
            if (!SelectText(out var objectIds, "\n3DS> Select text entities: "))
                return;

            if (!EditorUtils.GetDouble(out double addValue, "\n3DS> Enter amount to add to text: "))
                return;

            using (var tr = AcadApp.StartTransaction())
            {
                foreach (ObjectId objectId in objectIds)
                {
                    var textEnt = tr.GetObject(objectId, OpenMode.ForRead) as Entity;

                    if (textEnt == null)
                        throw new InvalidOperationException("textEnd was null.");

                    string text = GetText(textEnt);

                    if (text.IsNumeric())
                    {
                        textEnt.UpgradeOpen();
                        double mathValue = Convert.ToDouble(text) + addValue;
                        UpdateText(textEnt, mathValue.ToString(CultureInfo.InvariantCulture));
                        textEnt.DowngradeOpen();
                    }
                    else
                    {
                        var entExtent = textEnt.GeometricExtents;
                        double midpointX = Math.Round((entExtent.MaxPoint.X+entExtent.MinPoint.X)/2, SystemVariables.LUPREC);
                        double midpointY = Math.Round((entExtent.MaxPoint.Y+entExtent.MinPoint.Y)/2, SystemVariables.LUPREC);

                        // ignoring text at...
                        AcadApp.WriteMessage($"Ignoring text at X:{midpointX} Y:{midpointY}. Not a number.");
                    }
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// Subtracts a number from a text entity if it is a valid number
        /// </summary>
        public static void SubtractNumberFromText()
        {
            if (!SelectText(out var objectIds, "\n3DS> Select text entities: "))
                return;

            if (!EditorUtils.GetDouble(out double subtractValue, "\n3DS> Enter amount to subtract from text: "))
                return;

            using (var tr = AcadApp.StartTransaction())
            {
                foreach (ObjectId objectId in objectIds)
                {
                    var textEnt = tr.GetObject(objectId, OpenMode.ForRead) as Entity;

                    if (textEnt == null)
                        throw new InvalidOperationException("textEnd was null.");

                    string text = GetText(textEnt);

                    if (text.IsNumeric())
                    {
                        textEnt.UpgradeOpen();
                        double mathValue = Convert.ToDouble(text) - subtractValue;
                        UpdateText(textEnt, mathValue.ToString(CultureInfo.InvariantCulture));
                        textEnt.DowngradeOpen();
                    }
                    else
                    {
                        var entExtent = textEnt.GeometricExtents;
                        double midpointX = Math.Round((entExtent.MaxPoint.X+entExtent.MinPoint.X)/2, SystemVariables.LUPREC);
                        double midpointY = Math.Round((entExtent.MaxPoint.Y+entExtent.MinPoint.Y)/2, SystemVariables.LUPREC);

                        // ignoring text at...
                        AcadApp.WriteMessage($"Ignoring text at X:{midpointX} Y:{midpointY}. Not a number.");
                    }
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// Multiplies the text by a number
        /// </summary>
        public static void MultiplyTextByNumber()
        {
            if (!SelectText(out var objectIds, "\n3DS> Select text entities: "))
                return;

            if (!EditorUtils.GetDouble(out double multiplyValue, "\n3DS> Enter value to multiply by text: "))
                return;

            using (var tr = AcadApp.StartTransaction())
            {
                foreach (ObjectId objectId in objectIds)
                {
                    var textEnt = tr.GetObject(objectId, OpenMode.ForRead) as Entity;

                    if (textEnt == null)
                        throw new InvalidOperationException("textEnd was null.");

                    string text = GetText(textEnt);

                    if (text.IsNumeric())
                    {
                        textEnt.UpgradeOpen();
                        double mathValue = Convert.ToDouble(text) * multiplyValue;
                        UpdateText(textEnt, mathValue.ToString(CultureInfo.InvariantCulture));
                        textEnt.DowngradeOpen();
                    }
                    else
                    {
                        var entExtent = textEnt.GeometricExtents;
                        double midpointX = Math.Round((entExtent.MaxPoint.X+entExtent.MinPoint.X)/2, SystemVariables.LUPREC);
                        double midpointY = Math.Round((entExtent.MaxPoint.Y+entExtent.MinPoint.Y)/2, SystemVariables.LUPREC);

                        // ignoring text at...
                        AcadApp.WriteMessage($"Ignoring text at X:{midpointX} Y:{midpointY}. Not a number.");
                    }
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// Divides the text by a number
        /// </summary>
        public static void DivideTextByNumber()
        {
            if (!SelectText(out var objectIds, "\n3DS> Select text entities: "))
                return;

            if (!EditorUtils.GetDouble(out double divideValue, "\n3DS> Enter value to divide by text: "))
                return;

            using (var tr = AcadApp.StartTransaction())
            {
                foreach (ObjectId objectId in objectIds)
                {
                    var textEnt = tr.GetObject(objectId, OpenMode.ForRead) as Entity;

                    if (textEnt == null)
                        throw new InvalidOperationException("textEnd was null.");

                    string text = GetText(textEnt);

                    if (text.IsNumeric())
                    {
                        textEnt.UpgradeOpen();
                        double mathValue = Convert.ToDouble(text) / divideValue;
                        UpdateText(textEnt, mathValue.ToString(CultureInfo.InvariantCulture));
                        textEnt.DowngradeOpen();
                    }
                    else
                    {
                        var entExtent = textEnt.GeometricExtents;
                        double midpointX = Math.Round((entExtent.MaxPoint.X+entExtent.MinPoint.X)/2, SystemVariables.LUPREC);
                        double midpointY = Math.Round((entExtent.MaxPoint.Y+entExtent.MinPoint.Y)/2, SystemVariables.LUPREC);

                        // ignoring text at...
                        AcadApp.WriteMessage($"Ignoring text at X:{midpointX} Y:{midpointY}. Not a number.");
                    }
                }
                tr.Commit();
            }
        }
    }
}