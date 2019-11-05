/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Util.cs,v 1.1 2010/11/19 15:40:51 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

//namespace Poderosa
namespace Telnet
{
    /// <summary>
    /// <ja>
    /// •W€“I‚È¬Œ÷^Ž¸”s‚ðŽ¦‚µ‚Ü‚·B
    /// </ja>
    /// <en>
    /// A standard success/failure is shown. 
    /// </en>
    /// </summary>
    public enum GenericResult
    {
        /// <summary>
        /// <ja>¬Œ÷‚µ‚Ü‚µ‚½</ja>
        /// <en>Succeeded</en>
        /// </summary>
        Succeeded,
        /// <summary>
        /// <ja>Ž¸”s‚µ‚Ü‚µ‚½</ja>
        /// <en>Failed</en>
        /// </summary>
        Failed
    }

    //Debug.WriteLineIf‚ ‚½‚è‚ÅŽg—p
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public static class DebugOpt
    {
#if DEBUG
        public static bool BuildToolBar = false;
        public static bool CommandPopup = false;
        public static bool DrawingPerformance = false;
        public static bool DumpDocumentRelation = false;
        public static bool IntelliSense = false;
        public static bool IntelliSenseMenu = false;
        public static bool LogViewer = false;
        public static bool Macro = false;
        public static bool MRU = false;
        public static bool PromptRecog = false;
        public static bool Socket = false;
        public static bool SSH = false;
        public static bool ViewManagement = false;
        public static bool WebBrowser = false;
#else //RELEASE
        public static bool BuildToolBar = false;
        public static bool CommandPopup = false;
        public static bool DrawingPerformance = false;
        public static bool DumpDocumentRelation= false;
        public static bool IntelliSense = false;
        public static bool IntelliSenseMenu = false;
        public static bool LogViewer = false;
        public static bool Macro = false;
        public static bool MRU = false;
        public static bool PromptRecog = false;
        public static bool Socket = false;
        public static bool SSH = false;
        public static bool ViewManagement = false;
        public static bool WebBrowser = false;
#endif
    }


    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public static class RuntimeUtil
    {
        public static void ReportException(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);

            string errorfile = ReportExceptionToFile(ex);

            //ƒƒbƒZ[ƒWƒ{ƒbƒNƒX‚Å’Ê’mB
            //‚¾‚ª‚±‚Ì’†‚Å—áŠO‚ª”­¶‚·‚é‚±‚Æ‚ªSP1‚Å‚Í‚ ‚é‚ç‚µ‚¢B‚µ‚©‚à‚»‚¤‚È‚é‚ÆƒAƒvƒŠ‚ª‹­§I—¹‚¾B
            //Win32‚ÌƒƒbƒZ[ƒWƒ{ƒbƒNƒX‚ðo‚µ‚Ä‚à“¯‚¶BƒXƒe[ƒ^ƒXƒo[‚È‚ç‘åä•v‚Ì‚æ‚¤‚¾
            try
            {
                Console.WriteLine(errorfile + " " + ex.Message);
            }
            catch (Exception ex2)
            {
                Debug.WriteLine("(MessageBox.Show() failed) " + ex2.Message);
                Debug.WriteLine(ex2.StackTrace);
            }
        }
        public static void SilentReportException(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
            ReportExceptionToFile(ex);
        }
        public static void DebuggerReportException(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
        }
        //ƒtƒ@ƒCƒ‹–¼‚ð•Ô‚·
        private static string ReportExceptionToFile(Exception ex)
        {
            string errorfile = null;
            //ƒGƒ‰[ƒtƒ@ƒCƒ‹‚É’Ç‹L
            StreamWriter sw = null;
            try
            {
                sw = GetErrorLog(ref errorfile);
                ReportExceptionToStream(ex, sw);
            }
            finally
            {
                if (sw != null) sw.Close();
            }
            return errorfile;
        }
        private static void ReportExceptionToStream(Exception ex, StreamWriter sw)
        {
            sw.WriteLine(DateTime.Now.ToString());
            sw.WriteLine(ex.Message);
            sw.WriteLine(ex.StackTrace);
            //inner exception‚ð‡ŽŸ
            Exception i = ex.InnerException;
            while (i != null)
            {
                sw.WriteLine("[inner] " + i.Message);
                sw.WriteLine(i.StackTrace);
                i = i.InnerException;
            }
        }
        private static StreamWriter GetErrorLog(ref string errorfile)
        {
            errorfile = AppDomain.CurrentDomain.BaseDirectory + "error.log";
            return new StreamWriter(errorfile, true/*append!*/, Encoding.Default);
        }

        public static string ConcatStrArray(string[] values, char delimiter)
        {
            StringBuilder bld = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) bld.Append(delimiter);
                bld.Append(values[i]);
            }
            return bld.ToString();
        }

        //min–¢–ž‚Ímin, maxˆÈã‚ÍmaxA‚»‚êˆÈŠO‚Ívalue‚ð•Ô‚·
        public static int AdjustIntRange(int value, int min, int max)
        {
            Debug.Assert(min <= max);
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        public static bool IsZeroLength(string str)
        {
            return str == null || str.Length == 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public static class ParseUtil
    {
        public static bool ParseBool(string value, bool defaultvalue)
        {
            try
            {
                if (value == null || value.Length == 0) return defaultvalue;
                return Boolean.Parse(value);
            }
            catch (Exception)
            {
                return defaultvalue;
            }
        }
        public static byte ParseByte(string value, byte defaultvalue)
        {
            try
            {
                if (value == null || value.Length == 0) return defaultvalue;
                return Byte.Parse(value);
            }
            catch (Exception)
            {
                return defaultvalue;
            }
        }
        public static int ParseInt(string value, int defaultvalue)
        {
            try
            {
                if (value == null || value.Length == 0) return defaultvalue;
                return Int32.Parse(value);
            }
            catch (Exception)
            {
                return defaultvalue;
            }
        }
        public static float ParseFloat(string value, float defaultvalue)
        {
            try
            {
                if (value == null || value.Length == 0) return defaultvalue;
                return Single.Parse(value);
            }
            catch (Exception)
            {
                return defaultvalue;
            }
        }
        public static int ParseHexInt(string value, int defaultvalue)
        {
            try
            {
                if (value == null || value.Length == 0) return defaultvalue;
                return Int32.Parse(value, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception)
            {
                return defaultvalue;
            }
        }
        public static Color ParseColor(string t, Color defaultvalue)
        {
            if (t == null || t.Length == 0)
                return defaultvalue;
            else
            {
                if (t.Length == 8)
                { //16i‚Å•Û‘¶‚³‚ê‚Ä‚¢‚é‚±‚Æ‚à‚ ‚éB‹‡—]‚Ìô‚Å‚±‚Ì‚æ‚¤‚É
                    int v;
                    if (Int32.TryParse(t, System.Globalization.NumberStyles.HexNumber, null, out v))
                        return Color.FromArgb(v);
                }
                else if (t.Length == 6)
                {
                    int v;
                    if (Int32.TryParse(t, System.Globalization.NumberStyles.HexNumber, null, out v))
                        return Color.FromArgb((int)((uint)v | 0xFF000000)); //'A'—v‘f‚Í0xFF‚É
                }
                Color c = Color.FromName(t);
                return c.ToArgb() == 0 ? defaultvalue : c; //‚Ö‚ñ‚È–¼‘O‚¾‚Á‚½‚Æ‚«AARGB‚Í‘S•”0‚É‚È‚é‚ªAIsEmpty‚ÍfalseB‚È‚Ì‚Å‚±‚ê‚Å”»’è‚·‚é‚µ‚©‚È‚¢
            }
        }

        public static T ParseEnum<T>(string value, T defaultvalue)
        {
            try
            {
                if (value == null || value.Length == 0)
                    return defaultvalue;
                else
                    return (T)Enum.Parse(typeof(T), value, false);
            }
            catch (Exception)
            {
                return defaultvalue;
            }
        }

        //TODO Generics‰»
        public static ValueType ParseMultipleEnum(Type enumtype, string t, ValueType defaultvalue)
        {
            try
            {
                int r = 0;
                foreach (string a in t.Split(','))
                    r |= (int)Enum.Parse(enumtype, a, false);
                return r;
            }
            catch (FormatException)
            {
                return defaultvalue;
            }
        }
    }
}