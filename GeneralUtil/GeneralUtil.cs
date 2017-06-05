using System;
using System.Collections.Generic;
using System.Text;
//using System.Windows.Forms;
using System.Windows.Controls;

using System.IO;
using System.Diagnostics;
using System.Globalization;

using System.Windows.Threading;
using System.Threading;

using System.Text.RegularExpressions;

using System.Reflection;
using System.Linq;
using System.Data.Linq.Mapping;

using real = System.Double;

namespace GeneralUtil
{
    public sealed class TxtLog
    {
        static public RichTextBox logBox = null;

        public static void err(Exception e)
        {
            logSource("Except", e.Message);
        }

        public static void err()
        {
            err("!!!!!");
        }

        public static void err(string msg)
        {
            logSource("Error", msg);
        }

        public static void report(string msg)
        {
            log("report", msg);
        }

        public static void showLog(string msg)
        {
            log("info", msg, false);
        }

        private static void log(string logType, string msg)
        {
            log(logType, msg, true);
        }

        static int i = 0;
        static void PutsLogBox(string str)
        {
            //str = i++.ToString();
            if (logBox.ExtentHeight > 20*3000) logBox.Document.Blocks.Clear();

            bool bEnd = logBox.VerticalOffset>=(logBox.ExtentHeight-logBox.ViewportHeight);
            logBox.AppendText(str + "\r");
            if(bEnd) logBox.ScrollToEnd(); // scroll to end automatically when current position is at end.
            //logBox.SelectionStart = logBox.Text.Length;
            //logBox.ScrollToCaret();
        }

        //delegate void DeleFnStr(string str);
        //static DeleFnStr delePutsLogBox = new DeleFnStr(PutsLogBox);
        static Action<string> delePutsLogBox = PutsLogBox;

        private static void log(string logType, string msg, bool save)
        {
            //string str = string.Format("{0}@{1}({2}): {3}\r\n", DateTime.Now.ToString("yy/MM/dd/ddd-HH:mm:ss",
            string str = string.Format("{0}@{1}({2}): {3}", DateTime.Now.ToString("yy/MM/dd/ddd-HH:mm:ss",
                DateTimeFormatInfo.InvariantInfo),
                System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(),
                logType, msg);

            if (save)
            {
                try
                {
                    using (StreamWriter w = File.AppendText("log.txt"))
                    {
                        w.WriteLine(str);
                        //w.WriteLine("{0} ({1}): {2}", DateTime.Now.ToString("yyyy/MM/dd/ddd - HH:mm:ss", DateTimeFormatInfo.InvariantInfo),logType, msg);
                        //w.WriteLine("{0} ({1} {2}) : {3}", logType, DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), msg);
                        // Update the underlying file.
                        w.Flush();
                        // Close the writer and underlying file.
                        w.Close();
                    }
                }
                catch (Exception e)
                {
                    showLog("Error!: using log.txt fail ! " + e);
                }
            }

            if (logBox != null)
            {
                logBox.Dispatcher.BeginInvoke(delePutsLogBox, str);
                //logBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,delePutsLogBox, str);
                //logBox.BeginInvoke(delePutsLogBox,str);
            }
            Debug.WriteLine(str);
        }

        private static void logSource(string logType, string msg)
        {
            // Get the frame one step up the call tree
            StackFrame CallStack = new StackFrame(2, true);

            // These will now show the file and line number of the caller
            msg = string.Format("{0} [{1} / {2} / {3}]", msg, CallStack.GetMethod(), CallStack.GetFileName(), CallStack.GetFileLineNumber());
            log(logType, msg);
        }
    }

    public class PerformanceMessage
    {
        int perfIter = 0, perfTick = 0;

        public void Reset()
        {
            perfIter = perfTick = 0;
        }

        public void Tick()
        {
            int tNow = Environment.TickCount; perfIter++;
            if ((tNow - perfTick) > 15 * 1000)
            {
                //GeneralUtil.TxtLog.showLog("iter per sec=" + (((double)perfIter) / new System.TimeSpan(tNow - perfTick).TotalSeconds).ToString("N6"));
                GeneralUtil.TxtLog.showLog("iter per sec=" + ((1000.0 * perfIter) / (tNow - perfTick)).ToString("N6"));
                perfTick = tNow;
                perfIter = 0;
            }
        }
    }

    static public class Helper //static class is sealed
    {
        public static bool Is3rdWednesday(DateTime date)
        {
            // how to find 3rd Wednesday:
            // if day1 of month is first Wednesday, day15 is 3rd Wednesday.
            // if day1 of month is first Thursday, day7 must be 1st Wednesday and day21 must be the 3rd Wednesday. 
            // thus, if the day of month is between 15~21 & the day is Wednesday, the day is 3rd Wednesday.

            return date.DayOfWeek == DayOfWeek.Wednesday && date.Day >= 15 && date.Day <= 21;
        }

        public static int WordCount(this String str)
        {
            return str.Split(new char[] { ' ', '.', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }


        public static DateTime getDateAfter(this DateTime[] dateArr, DateTime date, int numDay)
        {
            return dateArr.getDate(date, numDay, false);
        }

        public static DateTime getDateBefore(this DateTime[] dateArr, DateTime date, int numDay)
        {
            return dateArr.getDate(date, numDay, true);
        }

        // pass numDay=0 to get the first Date before/after dateArr.
        public static DateTime getDate(this DateTime[] dateArr,   DateTime date, int numDay, bool before)
        {
            int idx;
                        
            idx = Array.BinarySearch(dateArr, date);

            if (before)
            {
                if (idx < 0) idx = ~idx - 1;
                idx -= numDay;
            }
            else // after
            {
                if (idx < 0) idx = ~idx;
                idx += numDay;
            }

            //if (idx < 0 || idx>= s.tradeDate.Length) throw new Exception("get Trade Date fail !");
            if (idx < 0) return DateTime.MinValue; else if (idx >= dateArr.Length) return DateTime.MaxValue;
            return dateArr[idx];
        }

        public static T[] FillWith<T>(this T[] arr, T value)
        {
            Trace.Assert(arr != null);
            for (int i = 0; i < arr.Length; i++) arr[i] = value;
            return arr;
        }

        public static T[] FillWith<T>(this T[] arr, T value, int beginIdx, int count)
        {
            Trace.Assert(arr != null);
            int endIdx = Math.Min(beginIdx + count, arr.Length);
            for (int i = beginIdx; i < endIdx; i++) arr[i] = value;
            return arr;
        }

        public static T[] Clip<T>(this T[] arr, int beginIdx, int count)
        {
            Trace.Assert(arr != null);
            Trace.Assert(beginIdx >= 0);
            var l = arr.Length;
            var c = l - beginIdx;
            if (count > c) count = c;
            //Trace.Assert(beginIdx + count <= l);

            T[] r = new T[count];
            Array.Copy(arr, beginIdx, r, 0, count);
            return r;
        }

        public static void Normalize(this real[] arr, real valMin, real valMax, real multiplier)
        {
            Trace.Assert(valMax != valMin);
            if (valMax < valMin)
            {
                var tmp = valMin;
                valMin = valMax;
                valMax = tmp;
            }

            real range = valMax - valMin;
            for (int i = 0; i < arr.Length; i++)
            {
                var x = (arr[i] - valMin) / range;
                arr[i] = (x > 1.0 ? 1.0 : x < 0.0 ? 0.0 : x)*multiplier;
            }

        }

        // normalize as +1~- 1 * multiplier
        public static void NormalizeBase0(this real[] arr, real range, real multiplier)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                var x = arr[i] / range ;
                arr[i] = (x > 1.0 ? 1.0 : x < -1.0 ? -1.0 : x)*multiplier;
            }

        }

        public static void Mul(this real[] arr, real multiplier)
        {
            for (int i = 0; i < arr.Length; i++)
                arr[i] *= multiplier;
        }

        public static real Magnitude(this real[] arr)
        {
            real s = 0.0;
            for (int i = 0; i < arr.Length; i++)
            {
                var x = arr[i];
                s += x * x;
            }

            return Math.Sqrt(s);
        }

        public static void NormalizeColumn(this List<real[]> nArr, real multiplier)
        {
            int l = nArr[0].Length;
            Trace.Assert(nArr.All(x => { return x.Length == l; }));
            real[] lenArr = new real[l];
            lenArr.FillWith(0.0);

            for (int i = 0; i < nArr.Count; i++)
                for (int j = 0; j < l; j++)
                {
                    var x = nArr[i][j];
                    lenArr[j] += (x * x);
                }

            for (int i = 0; i < l; i++)
            {
                real len = 0.0;
                for (int j = 0; j < nArr.Count; j++)
                {
                    var x = nArr[j][i];
                    len += (x * x);
                }

                len = Math.Sqrt(len) / multiplier;
                for (int j = 0; j < nArr.Count; j++)
                    nArr[j][i] /= len;
            }
        }

        public static int LastIndexOfMax<T>(this T[] arr) where T : IComparable
        {
            Trace.Assert(arr != null);
            int maxIdx = 0;
            T max = arr[0];

            int i = 0;
            while (++i < arr.Length)
            {
                if (max.CompareTo(arr[i]) <= 0)
                {
                    maxIdx = i;
                    max = arr[i];
                }
            }
            return maxIdx;
        }

        public static int IndexOfMax<T>(this T[] arr) where T : IComparable
        {
            Trace.Assert(arr != null);
            int maxIdx = 0;
            T max = arr[0];

            int i = 0;
            while (++i < arr.Length)
            {
                if (max.CompareTo(arr[i]) < 0)
                {
                    maxIdx = i;
                    max = arr[i];
                }
            }

            return maxIdx;
        }

        public static int LastIndexOfMin<T>(this T[] arr) where T : IComparable
        {
            Trace.Assert(arr != null);
            int minIdx = 0;
            T min = arr[0];

            int i = 0;
            while (++i < arr.Length)
            {
                if (min.CompareTo(arr[i]) >= 0)
                {
                    minIdx = i;
                    min = arr[i];
                }
            }

            return minIdx;
        }

        public static int IndexOfMin<T>(this T[] arr) where T : IComparable
        {
            Trace.Assert(arr != null);
            int minIdx = 0;
            T min = arr[0];

            int i = 0;
            while (++i < arr.Length)
            {
                if (min.CompareTo(arr[i]) > 0)
                {
                    minIdx = i;
                    min = arr[i];
                }
            }

            return minIdx;
        }

        public static string ElementsToString<T>(this T[] src)
        {
            Trace.Assert(src != null);
            string s="(" + src + ") = ";
            int i;
            for(i=0;i<src.Length-1;i++)
                s += src[i].ToString()+", ";
            s += src[i] + ".";
            return s;
        }

        public static T[] Add1Element<T>(this T[] src, T value)
        {
            Trace.Assert(src != null);

            int size = src.Length + 1;

            var ret = new T[size];
            src.CopyTo(ret, 0);
            ret[size - 1] = value;
            return ret;
        }

        public static string PublicFieldsToString(this object o)
        {
            string s="( " +o.ToString() + " )\r";
            FieldInfo[] fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo fi in fields)
                s = s+fi.FieldType +" " + fi.Name +"  = "+ fi.GetValue(o) + "\r";

            return s;
        }

        public static void InitAllArrayFields(this object o, int size)
        {
            FieldInfo[] fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (FieldInfo fi in fields)
                if (fi.FieldType.IsArray) fi.SetValue(o, Array.CreateInstance(fi.FieldType.GetElementType(), size));
        }

        public static void InitAllArrayFields<T>(this T o, T src, int idxSrcBegin, int size)
        {
            if (size < 0) size = 0;
            FieldInfo[] fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (FieldInfo fi in fields)
                if (fi.FieldType.IsArray)
                {
                    var a=Array.CreateInstance(fi.FieldType.GetElementType(), size);
                    Array.ConstrainedCopy(fi.GetValue(src) as Array,idxSrcBegin,a,0,size);
                    fi.SetValue(o, a);
                }
        }

        public static void Fibonacci(int[] fib)
        {
            fib[0] = 0;
            fib[1] = 1;
            for (int i = 2; i < fib.Length; i++)
                fib[i] = fib[i - 1] + fib[i - 2];
        }

        // find idxBegin of the obj that >=objBegin and idxEnd of the obj that <= objEnd.
        // return false if no data between objBegin & objEnd.
        public static bool IndexesBetween(Array sortedArr, Object objBegin, Object objEnd, out int idxBegin, out int idxEnd)
        {
            //  int begIdx= s_rawDataPool.CountTo(new CSVFutureReader.CombinedRawData(tBegin))-needDataLength+1;
            idxBegin = Array.BinarySearch(sortedArr, objBegin);
            if (idxBegin < 0) idxBegin = ~idxBegin; //==array.Length if all data less then tBegin

            idxEnd = Array.BinarySearch(sortedArr, objEnd);
            if (idxEnd < 0) idxEnd = (~idxEnd) - 1; // ==-1 if all data greater then tEnd

            if (idxBegin > idxEnd)
            {
                throw new Exception("no Data");
                // return false;
            }
            return true;
        }

        public static string getRootDirPath(string rootFolder)
        {
            string path = Directory.GetCurrentDirectory();
            int w_idx = path.IndexOf(rootFolder, System.StringComparison.CurrentCultureIgnoreCase);
            if (w_idx < 0)
            {
                TxtLog.err("wrong root folder name!");
                return null;
            }

            return path.Remove(w_idx + rootFolder.Length + 1);
        }

        public static void checkPoint()
        {
            StackFrame CallStack = new StackFrame(1, true);
            Debug.WriteLine(
                string.Format("[V]{0}({1}){2}",
                DateTime.Now.ToString("HH:mm:ss", DateTimeFormatInfo.InvariantInfo)
                , CallStack.GetFileLineNumber(), CallStack.GetMethod())
            );
        }

        public static int CountSubString(string srcStr, string substring)
        {
            int count = 0;
            int foundAtIndex;
            int searchFromIndex = 0;
            while ((foundAtIndex = srcStr.IndexOf(substring, searchFromIndex)) != -1)
            {
                searchFromIndex = foundAtIndex + substring.Length;
                count++;
            }
            return count;
        }

        //public static K[] NewConvertArr<T, K>(this T[] arr) where T : float where K : double
        //public static float[] NewConvertArr<T>(this T[] arr)
        public static To[] NewConvertArrX<Ti, To>(this Ti[] arr)
        {
            //Convert.ToSingle()
            // will convert fail ???
            return Array.ConvertAll(arr, x=>(To)(object)x);
        }

        public static float[] NewSingleArr<Ti>(this Ti[] arr)
        {
            return Array.ConvertAll(arr, x => Convert.ToSingle(x));
            //return Array.ConvertAll(arr, x => (float)(object)x);
        }

        public static double[] NewDoubleArr<Ti>(this Ti[] arr)
        {
            return Array.ConvertAll(arr, x => Convert.ToDouble(x));
//          return Array.ConvertAll(arr, x => (double)(object)x);
        }

        //static void test()
        //{
        //    double[] d = new double[100];
        //    float[] f = d.NewConvertArrX<float>();
        //}

#if false
        public static float[] NewConvertArrF(double[] arr)
        {
            return Array.ConvertAll(arr, x => (float)x);
            //return Array.ConvertAll<double, float>(arr, x => (float)x);
        }

        public static float[] NewConvertArrF(int[] arr)
        {
            return Array.ConvertAll<int, float>(arr, x => (float)x);
        }

        public static double[] NewConvertArr(float[] arr)
        {
            return Array.ConvertAll<float, double>(arr, x => (double)x);
        }

        public static double[] NewConvertArr(int[] arr)
        {
            return Array.ConvertAll<int, double>(arr, x => (double)x);
        }
#endif

        // CSV file parser
        // e.g. xxxx,xxx,xxxx,xxx"xxx""yy""yy""yy""yy"

        // (lineBegin or ,) ( XXX, or XXXX" )   or " (XXXX ("" yyyy*)*) " ==> "xxxx""yyy""yy""y""yy" ....
        // \G: match must occur at the point where the previous match ended.
        // (^|,) :  match must occur at the beginning of the string or the beginning of the line.  OR ","
        // (not " not , ) 1~n
        //
        static Regex m_rgxCSVFields = new Regex(@"
            \G(^|,)(
            (?<field> [^"",]+ )
            |
            ""(?<field> [^""]+("""" [^""]* )* )""
            )?
            ", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        static int m_fieldGroupIndex = m_rgxCSVFields.GroupNumberFromName("field");

        //public delegate bool OnParse1CSVLine(string[] sVals);
        //public static int ParseCSVFile(StreamReader excludeHeaderCSV, Type enumTypeFields, string strEndOfFile, OnParse1CSVLine onParse1CSVLine)
        public static int ParseCSVFile(StreamReader excludeHeaderCSV, Type enumTypeFields, string strEndOfFile, Func<string[], bool> onParse1CSVLine)
        {
            int totalParsedLine = 0;
            string s;
            int line = 1;

            Array eVals = Enum.GetValues(enumTypeFields);
            while ((s = excludeHeaderCSV.ReadLine()) != null)
            {
                s = s.Trim();
                line++;
                if (s == "") continue;
                if (s.Last() == ',') // remove end null field
                {
                    char[] charsToTrim = { ',' };
                    s = s.TrimEnd(charsToTrim);
                }

                MatchCollection m = m_rgxCSVFields.Matches(s);
                if (m.Count != eVals.Length)
                {
                    if (strEndOfFile != null && s.IndexOf(strEndOfFile) >= 0) break;
                    TxtLog.showLog("csv read not match (" + line + ") : " + s);
                    continue;
                }

                string[] sArrRet = new string[eVals.Length];

                foreach (int idx in eVals)
                    sArrRet[idx] = m[idx].Groups[m_fieldGroupIndex].Value.Trim();

                if (!onParse1CSVLine(sArrRet))
                {
                    TxtLog.showLog("Parse break(" + line + ") : " + s);
                    break;
                }
                totalParsedLine++;

            }// end while

            return totalParsedLine;

        }

        public static int ParseCSVFile(string fileName, Encoding encoding, Type enumTypeFields, string strEndOfFile, Func<string[], bool> onParse1CSVLine)
        {
            int totalParsedLine = 0;
            using (StreamReader csv = new StreamReader(fileName, encoding))
            {
                string s;
                while ((s = csv.ReadLine()) != null && s.Trim() == "") ; //skip fields header
                totalParsedLine = ParseCSVFile(csv, enumTypeFields, strEndOfFile, onParse1CSVLine);
            }// end useing
            return totalParsedLine;
        }

        //======================================================================================


        static public object[][] getFieldValues(object obj) // public and non-public field
        {
            return (
                from p in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                select new object[] { p.Name, p.GetValue(obj) }
                ).ToArray();
        }

        static public object[][] getPropertyValues(object obj) // get fields with get_xxxx()/set_xxxx() method.
        {
            return (
                from p in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                select new object[] { p.Name, p.GetValue(obj, null) }
                ).ToArray();
        }

        static public object[][] getPrimaryKeyValues(object row) // get primary key fields of SQL.
        {
            //object[] customP;
            return (
                from p in row.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                let customP = p.GetCustomAttributes(typeof(ColumnAttribute), false)
                where customP.Length == 1 && ((ColumnAttribute)customP[0]).IsPrimaryKey
                select new object[] { p.Name, p.GetValue(row, null) }
                         ).ToArray();
        }

        public static string Arr2Str(object obj)
        {
            return toString(obj, true);
        }

        static string toString(object obj, bool firstPos)
        {
            string s;

            if (obj == null) s = "null";
            else
            {

                if (obj.GetType().IsArray)//  if (obj is object[])
                {
                    object[] arrObj = obj as object[];

                    s = "(";
                    if (arrObj.Length > 0)
                        s += toString(arrObj[0], true);
                    for (int i = 1; i < arrObj.Length; i++)
                        s += toString(arrObj[i], false);
                    return s + ")";
                }
                else s = obj.ToString();
            }
            return firstPos ? s : ", " + s;
        }

        // find the num that is divided by 4 and closest to a source num. (may greater or less then source num)
        public static int RoundDividedBy4(double val)
        {
            return 4 * (int)(val / 4.0 + 0.5);
        }
        //============================
        // it only set data member, not use Get() Set().
        static public void MemberwiseCopy(object dest, object src)
        {
            // src may be desc's base class. thus src's fields should be subset of desc's.
            //var fields = src.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            // GetFields() does not get base class's non public fields!!! it only could get current class's non public fields.

            Type typeS = src.GetType();
            //System.Collections.ArrayList fields= new System.Collections.ArrayList();
            var fields = new System.Collections.Generic.List<FieldInfo>();
            fields.AddRange(typeS.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));

            while ((typeS= typeS.BaseType) != typeof(object))
            {
                fields.AddRange(typeS.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static));
            }

            foreach (FieldInfo fi in fields)
            {
                fi.SetValue(dest, fi.GetValue(src));
            }
            
            //if(src.GetType().BaseType!=typeof(object))
        }
               

        #region HtmlRetriever

        // Yes, using a thread would be an approach.  Unfortunately, it doesn't work.  Internet Explorer is a COM object, it has thread affinity.
        // Its Document property can only be read by the thread that called the Navigate() method.
        // That doesn't leave a lot of desirable alternatives.  Calling Application.DoEvents() would be a solution but that has many dangerous side effects. 
        // For one, the user could close the app's main form and the app will bomb when navigation is complete. 
        // One possible trick is to block execution with Form.ShowDialog(), using a dialog that isn't visible to the user.  This class uses that trick:

        //Call it like this: var doc = HtmlRetriever.GetDocument("google.com");
        // It is not exactly perfect, you'll notice the side-effect of calling ShowDialog().  By far the best approach is to integrate the WebBrowser into the client app.

        private static System.Windows.Forms.WebBrowser mWb;
        private static System.Windows.Forms.Form mForm;

        public static System.Windows.Forms.HtmlDocument GetHtmlDocument(string url)
        {
            using (mForm = new System.Windows.Forms.Form())
            {
                mForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                mForm.TransparencyKey = mForm.BackColor;
                mForm.ShowInTaskbar = false;
                mForm.MinimumSize = new System.Drawing.Size(0, 0);
                mForm.Size = mForm.MinimumSize;
                System.Windows.Forms.HtmlDocument retval;
                using (mWb = new System.Windows.Forms.WebBrowser())
                {
                    mWb.DocumentCompleted += mWb_DocumentCompleted;
                    mWb.Navigate(url);
                    mForm.ShowDialog();
                    retval = mWb.Document;
                }
                return retval;
            }
        }

        static void mWb_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url == mWb.Url) mForm.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
        #endregion //HtmlRetriever

    }// end of Helper

    //==================================================
    /// <summary>
    /// A fast random number generator for .NET
    /// Colin Green, January 2005
    /// 
    /// September 4th 2005
    ///	 Added NextBytesUnsafe() - commented out by default.
    ///	 Fixed bug in Reinitialise() - y,z and w variables were not being reset.
    /// 
    /// Key points:
    ///  1) Based on a simple and fast xor-shift pseudo random number generator (RNG) specified in: 
    ///  Marsaglia, George. (2003). Xorshift RNGs.
    ///  http://www.jstatsoft.org/v08/i14/xorshift.pdf
    ///  
    ///  This particular implementation of xorshift has a period of 2^128-1. See the above paper to see
    ///  how this can be easily extened if you need a longer period. At the time of writing I could find no 
    ///  information on the period of System.Random for comparison.
    /// 
    ///  2) Faster than System.Random. Up to 8x faster, depending on which methods are called.
    /// 
    ///  3) Direct replacement for System.Random. This class implements all of the methods that System.Random 
    ///  does plus some additional methods. The like named methods are functionally equivalent.
    ///  
    ///  4) Allows fast re-initialisation with a seed, unlike System.Random which accepts a seed at construction
    ///  time which then executes a relatively expensive initialisation routine. This provides a vast speed improvement
    ///  if you need to reset the pseudo-random number sequence many times, e.g. if you want to re-generate the same
    ///  sequence many times. An alternative might be to cache random numbers in an array, but that approach is limited
    ///  by memory capacity and the fact that you may also want a large number of different sequences cached. Each sequence
    ///  can each be represented by a single seed value (int) when using FastRandom.
    ///  
    ///  Notes.
    ///  A further performance improvement can be obtained by declaring local variables as static, thus avoiding 
    ///  re-allocation of variables on each call. However care should be taken if multiple instances of
    ///  FastRandom are in use or if being used in a multi-threaded environment.
    /// 
    /// </summary>
    public class FastRandom
    {
        // The +1 ensures NextDouble doesn't generate 1.0
        const double REAL_UNIT_INT = 1.0 / ((double)int.MaxValue + 1.0);
        const double REAL_UNIT_UINT = 1.0 / ((double)uint.MaxValue + 1.0);
        const uint Y = 842502087, Z = 3579807591, W = 273326509;
        // The NextDouble DOES generate 1.0
        const double REAL_UNIT_INT1 = 1.0 / ((double)int.MaxValue);
        const double REAL_UNIT_UINT1 = 1.0 / ((double)uint.MaxValue);

        uint x, y, z, w;

        #region Constructors

        /// <summary>
        /// Initialises a new instance using time dependent seed.
        /// </summary>
        public FastRandom()
        {
            // Initialise using the system tick count.
            Reinitialise((int)Environment.TickCount);
        }

        /// <summary>
        /// Initialises a new instance using an int value as seed.
        /// This constructor signature is provided to maintain compatibility with
        /// System.Random
        /// </summary>
        public FastRandom(int seed)
        {
            Reinitialise(seed);
        }

        #endregion

        #region Public Methods [Reinitialisation]

        /// <summary>
        /// Reinitialises using an int value as a seed.
        /// </summary>
        /// <param name="seed"></param>
        public void Reinitialise(int seed)
        {
            // The only stipulation stated for the xorshift RNG is that at least one of
            // the seeds x,y,z,w is non-zero. We fulfill that requirement by only allowing
            // resetting of the x seed
            x = (uint)seed;
            y = Y;
            z = Z;
            w = W;
        }

        #endregion

        #region Public Methods [System.Random functionally equivalent methods]

        /// <summary>
        /// Generates a random int over the range 0 to int.MaxValue-1.
        /// MaxValue is not generated in order to remain functionally equivalent to System.Random.Next().
        /// This does slightly eat into some of the performance gain over System.Random, but not much.
        /// For better performance see:
        /// 
        /// Call NextInt() for an int over the range 0 to int.MaxValue.
        /// 
        /// Call NextUInt() and cast the result to an int to generate an int over the full Int32 value range
        /// including negative values. 
        /// </summary>
        /// <returns></returns>
        public int Next()
        {
            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;
            w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));

            // Handle the special case where the value int.MaxValue is generated. This is outside of 
            // the range of permitted values, so we therefore call Next() to try again.
            uint rtn = w & 0x7FFFFFFF;
            if (rtn == 0x7FFFFFFF)
                return Next();
            return (int)rtn;
        }

        /// <summary>
        /// Generates a random int over the range 0 to upperBound-1, and not including upperBound.
        /// </summary>
        /// <param name="upperBound"></param>
        /// <returns></returns>
        public int Next(int upperBound)
        {
            if (upperBound < 0)
                throw new ArgumentOutOfRangeException("upperBound", upperBound, "upperBound must be >=0");

            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;

            // The explicit int cast before the first multiplication gives better performance.
            // See comments in NextDouble.
            return (int)((REAL_UNIT_INT * (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))))) * upperBound);
        }

        public int Next1(int upperBound)
        {
            if (upperBound < 0)
                throw new ArgumentOutOfRangeException("upperBound", upperBound, "upperBound must be >=0");

            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;

            // The explicit int cast before the first multiplication gives better performance.
            // See comments in NextDouble.
            return (int)((REAL_UNIT_INT1 * (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))))) * upperBound);
        }

        /// <summary>
        /// Generates a random int over the range lowerBound to upperBound-1, and not including upperBound.
        /// upperBound must be >= lowerBound. lowerBound may be negative.
        /// </summary>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <returns></returns>
        public int Next(int lowerBound, int upperBound)
        {
            if (lowerBound > upperBound)
                throw new ArgumentOutOfRangeException("upperBound", upperBound, "upperBound must be >=lowerBound");

            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;

            // The explicit int cast before the first multiplication gives better performance.
            // See comments in NextDouble.
            int range = upperBound - lowerBound;
            if (range < 0)
            {	// If range is <0 then an overflow has occured (e.g. int.max ~ int.min ) and must resort to using long integer arithmetic instead (slower).
                // We also must use all 32 bits of precision, instead of the normal 31, which again is slower.	
                return lowerBound + (int)((REAL_UNIT_UINT * (double)(w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))) * (double)((long)upperBound - (long)lowerBound));
            }

            // 31 bits of precision will suffice if range<=int.MaxValue. This allows us to cast to an int and gain
            // a little more performance.
            return lowerBound + (int)((REAL_UNIT_INT * (double)(int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))))) * (double)range);
        }

        public int Next1(int lowerBound, int upperBound)
        {
            if (lowerBound > upperBound)
                throw new ArgumentOutOfRangeException("upperBound", upperBound, "upperBound must be >=lowerBound");

            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;

            // The explicit int cast before the first multiplication gives better performance.
            // See comments in NextDouble.
            int range = upperBound - lowerBound;
            if (range < 0)
            {	// If range is <0 then an overflow has occured (e.g. int.max ~ int.min ) and must resort to using long integer arithmetic instead (slower).
                // We also must use all 32 bits of precision, instead of the normal 31, which again is slower.	
                return lowerBound + (int)((REAL_UNIT_UINT1 * (double)(w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))) * (double)((long)upperBound - (long)lowerBound));
            }

            // 31 bits of precision will suffice if range<=int.MaxValue. This allows us to cast to an int and gain
            // a little more performance.
            return lowerBound + (int)((REAL_UNIT_INT1 * (double)(int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))))) * (double)range);
        }

        /// <summary>
        /// Generates a random double. Values returned are from 0.0 up to but not including 1.0.
        /// </summary>
        /// <returns></returns>
        public double NextDouble()
        {
            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;

            // Here we can gain a 2x speed improvement by generating a value that can be cast to 
            // an int instead of the more easily available uint. If we then explicitly cast to an 
            // int the compiler will then cast the int to a double to perform the multiplication, 
            // this final cast is a lot faster than casting from a uint to a double. The extra cast
            // to an int is very fast (the allocated bits remain the same) and so the overall effect 
            // of the extra cast is a significant performance improvement.
            //
            // Also note that the loss of one bit of precision is equivalent to what occurs within 
            // System.Random.
            return (REAL_UNIT_INT * (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))));
        }

        public double NextDouble1()
        {
            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;

            // Here we can gain a 2x speed improvement by generating a value that can be cast to 
            // an int instead of the more easily available uint. If we then explicitly cast to an 
            // int the compiler will then cast the int to a double to perform the multiplication, 
            // this final cast is a lot faster than casting from a uint to a double. The extra cast
            // to an int is very fast (the allocated bits remain the same) and so the overall effect 
            // of the extra cast is a significant performance improvement.
            //
            // Also note that the loss of one bit of precision is equivalent to what occurs within 
            // System.Random.
            return (REAL_UNIT_INT1 * (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))));
        }

        /// <summary>
        /// Fills the provided byte array with random bytes.
        /// This method is functionally equivalent to System.Random.NextBytes(). 
        /// </summary>
        /// <param name="buffer"></param>
        public void NextBytes(byte[] buffer)
        {
            // Fill up the bulk of the buffer in chunks of 4 bytes at a time.
            uint x = this.x, y = this.y, z = this.z, w = this.w;
            int i = 0;
            uint t;
            for (int bound = buffer.Length - 3; i < bound; )
            {
                // Generate 4 bytes. 
                // Increased performance is achieved by generating 4 random bytes per loop.
                // Also note that no mask needs to be applied to zero out the higher order bytes before
                // casting because the cast ignores thos bytes. Thanks to Stefan Troschütz for pointing this out.
                t = (x ^ (x << 11));
                x = y; y = z; z = w;
                w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));

                buffer[i++] = (byte)w;
                buffer[i++] = (byte)(w >> 8);
                buffer[i++] = (byte)(w >> 16);
                buffer[i++] = (byte)(w >> 24);
            }

            // Fill up any remaining bytes in the buffer.
            if (i < buffer.Length)
            {
                // Generate 4 bytes.
                t = (x ^ (x << 11));
                x = y; y = z; z = w;
                w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));

                buffer[i++] = (byte)w;
                if (i < buffer.Length)
                {
                    buffer[i++] = (byte)(w >> 8);
                    if (i < buffer.Length)
                    {
                        buffer[i++] = (byte)(w >> 16);
                        if (i < buffer.Length)
                        {
                            buffer[i] = (byte)(w >> 24);
                        }
                    }
                }
            }
            this.x = x; this.y = y; this.z = z; this.w = w;
        }


        //		/// <summary>
        //		/// A version of NextBytes that uses a pointer to set 4 bytes of the byte buffer in one operation
        //		/// thus providing a nice speedup. The loop is also partially unrolled to allow out-of-order-execution,
        //		/// this results in about a x2 speedup on an AMD Athlon. Thus performance may vary wildly on different CPUs
        //		/// depending on the number of execution units available.
        //		/// 
        //		/// Another significant speedup is obtained by setting the 4 bytes by indexing pDWord (e.g. pDWord[i++]=w)
        //		/// instead of adjusting it dereferencing it (e.g. *pDWord++=w).
        //		/// 
        //		/// Note that this routine requires the unsafe compilation flag to be specified and so is commented out by default.
        //		/// </summary>
        //		/// <param name="buffer"></param>
        //		public unsafe void NextBytesUnsafe(byte[] buffer)
        //		{
        //			if(buffer.Length % 8 != 0)
        //				throw new ArgumentException("Buffer length must be divisible by 8", "buffer");
        //
        //			uint x=this.x, y=this.y, z=this.z, w=this.w;
        //			
        //			fixed(byte* pByte0 = buffer)
        //			{
        //				uint* pDWord = (uint*)pByte0;
        //				for(int i=0, len=buffer.Length>>2; i < len; i+=2) 
        //				{
        //					uint t=(x^(x<<11));
        //					x=y; y=z; z=w;
        //					pDWord[i] = w = (w^(w>>19))^(t^(t>>8));
        //
        //					t=(x^(x<<11));
        //					x=y; y=z; z=w;
        //					pDWord[i+1] = w = (w^(w>>19))^(t^(t>>8));
        //				}
        //			}
        //
        //			this.x=x; this.y=y; this.z=z; this.w=w;
        //		}

        #endregion

        #region Public Methods [Methods not present on System.Random]

        /// <summary>
        /// Generates a uint. Values returned are over the full range of a uint, 
        /// uint.MinValue to uint.MaxValue, inclusive.
        /// 
        /// This is the fastest method for generating a single random number because the underlying
        /// random number generator algorithm generates 32 random bits that can be cast directly to 
        /// a uint.
        /// </summary>
        /// <returns></returns>
        public uint NextUInt()
        {
            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;
            //x = w; // for speed up. 405 vs 403 ops/sec
            return (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)));
        }

        /*
        private uint[] uintBuff = new uint[64*1024*1024];
        private int uintBuffSize = 0;
        public uint NextUInt() //buffered
        {

            if (uintBuffSize == 0)
            {
                TxtLog.showLog("fill rand buffer...");
                uintBuffSize = uintBuff.Length;
                //System.Threading.Parallel.For(0, uintBuffSize, delegate(int i)
                for(int i=0;i<uintBuffSize;i++)
                {
                    uintBuff[i] = _NextUInt();
                }//);
            }

            return uintBuff[--uintBuffSize]; //performance is 400 ops per sec. unbuffered is 404 ops per sec.
        }
         */

        /// <summary>
        /// Generates a random int over the range 0 to int.MaxValue, inclusive. 
        /// This method differs from Next() only in that the range is 0 to int.MaxValue
        /// and not 0 to int.MaxValue-1.
        /// 
        /// The slight difference in range means this method is slightly faster than Next()
        /// but is not functionally equivalent to System.Random.Next().
        /// </summary>
        /// <returns></returns>
        public int NextInt()
        {
            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;
            return (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))));
        }


        // Buffer 32 bits in bitBuffer, return 1 at a time, keep track of how many have been returned
        // with bitBufferIdx.
        uint bitBuffer;
        uint bitMask = 1;

        /// <summary>
        /// Generates a single random bit.
        /// This method's performance is improved by generating 32 bits in one operation and storing them
        /// ready for future calls.
        /// </summary>
        /// <returns></returns>
        public bool NextBool()
        {
            if (bitMask == 1)
            {
                // Generate 32 more bits.
                uint t = (x ^ (x << 11));
                x = y; y = z; z = w;
                bitBuffer = w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));

                // Reset the bitMask that tells us which bit to read next.
                bitMask = 0x80000000;
                return (bitBuffer & bitMask) == 0;
            }

            return (bitBuffer & (bitMask >>= 1)) == 0;
        }

        #endregion
    }

    //=================================
#if false
    /*  You can leave off the interface, or change to IEnumerable.  See below.*/
    class ReadOnlyArray<T> : IEnumerable<T>
    {
        private readonly T[] array;
        public ReadOnlyArray(T[] a_array) {        array = a_array;    }
        // read-only because no `set'
        public T this[int i]    { get { return array[i]; } }
        public int Length    { get { return array.Length; } }
        /*  You can comment this method out if you don't implement IEnumerable<T>.       Casting array.GetEnumerator to IEnumerator<T> will not work.    */
        public IEnumerator<T> GetEnumerator()    {        foreach(T el in array)        {            yield return el;        }    }
        /*  If you don't implement any interface, change this to:  public IEnumerator GetEnumerator()
         * Or you can implement only IEnumerable (rather than IEnerable<T>) and keep "IEnumerator IEnumerable.GetEnumerator()"  */
        //IEnumerator IEnumerable<T>.GetEnumerator()    {        return array.GetEnumerator();    }
    }

    class ReadOnlyArray2
    {
        Array
    }
#endif


#if false
    //=================================
    // user class must be a non-abstract type with a public parameterless constructor
    public class Singleton<T> where T : new()
    {
        Singleton() { }

        // it will return null when we get Instance in client class's default constructor first time, but i don't know why. it may has unstable/undetermined result.
        public static T Instance
        {
            get { return SingletonCreator.instance; }
        }

        class SingletonCreator
        {
            // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit.

            /*(The C# specification implies that no types with static constructors should be marked with the beforefieldinit flag.
             * 1. If marked BeforeFieldInit then the type's initializer method is executed at, or sometime before, first access to any static field defined for that type.
             * 2.If not marked BeforeFieldInit then that type's initializer method is executed at (i.e., is triggered by): 
             * a. first access to any static or instance field of that type, or 
             * b. first invocation of any static, instance or virtual method of that type */

            static SingletonCreator() { }

            internal static readonly T instance = new T();
        }
    }
 
#else
    //================================================================
    public class Object<T> // for boxing a value type Variable. 
    {
        public T Value;
        public Object(T Value) { this.Value = Value; }
    }

    public static class Singleton<T> where T : class
    {
        static volatile T _instance;
        static object _lock = new object();
        static Singleton(){}

        // public static bool HasInstance { get { return _instance != null; } }
        public static T Instance
        {
            get
            {
                if (_instance == null)
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            // Call Default Constructor 
                            // BindingFlags.Static The static constructor for a class executes at most once in a given application domain.
                            // The execution of a static constructor is triggered by the first of the following events to occur within an application domain:
                            // An instance of the class is created.Any of the static members of the class are referenced.
                            ConstructorInfo constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                            if (constructor == null /*|| constructor.IsAssembly /*Also exclude internal constructors.*/) throw new Exception(string.Format("A constructor is missing for '{0}'.", typeof(T).Name));
                            //avoide infinite recursion when get Instance in default constructor of client class.
                            //if(! new System.Diagnostics.StackTrace().GetFrames().Any(o => o.GetMethod().ReflectedType == constructor.ReflectedType )) _instance = (T)constructor.Invoke(null);
                            _instance = (T)constructor.Invoke(null);
                        }
                    }
                return _instance;
            }
        }
    }
#endif
    // ****  useage example: ***
    //
    //    public class TestClass
    //   {
    //       private string _createdTimestamp;

    //       public TestClass () { _createdTimestamp = DateTime.Now.ToString(); }
    //       public void Write() { Debug.WriteLine(_createdTimestamp); }
    //   }

    ////The class is used with the SingletonProvider as follows
    //Singleton<TestClass>.Instance.Write();
    //// This setup means that no matter which thread uses this class in the Singleton Pattern, whenever that public method is called, it should output the same value.
    //*************************


    //===================================================================================
    // Derivable Singleton class base
    // fields and base class's fields will just init 1 time. (every base class has 1 instance and derived class copy fields from it)
    // (constructor will be called many times because derived cons will call base cons.)
    public class SingletonBase
    {
        // it is constructor
        virtual protected void Constructor() 
        {
            //BaseConstructor<BaseClassType>(); // derived  class should invoke it at begin of this method.
        }

        public static T Instance<T>() where T : SingletonBase, new()
        {
            return Singleton<T>.Instance;
        }
                
        protected void BaseConstructor<T>() where T : SingletonBase, new()
        {
            GeneralUtil.Helper.MemberwiseCopy(this, Instance<T>());
        }

        protected class Singleton<T> where T : SingletonBase, new()
        {
            public static T Instance { get; private set; }
            static Singleton()
            {
                Instance = new T();

                System.Diagnostics.Debug.WriteLine("Construct " + Instance);
                Instance.Constructor();

                // how to get base class singleton instance and copy it ??
                //var v = typeof(T).BaseType.Name;
            }
        }

    }


#if false // example
    class DataA : SingletonBase
    {
        int f1a, f2a, f3a;
        override protected void Constructor()
        {
            BaseConstructor<DeriveSingleton>();
            f1a = 1; f2a = 2; f3a = 3;
        }

        //public DataA() { System.Diagnostics.Debug.WriteLine(this.GetType() + " cons"); }
    }

    class DataB : DataA
    {
        public int f1b, f2b, f3b;

        override protected void Constructor()
        {
            BaseConstructor<DataA>();
            f1b = 4; f2b = 5; f3b = 6;
        }

        //public DataB(){System.Diagnostics.Debug.WriteLine(this.GetType()+" cons");}

    }

    class DataC : DataB
    {
        public int f1c, f2c, f3c;

        override protected void Constructor()
        {
            BaseConstructor<DataB>();
            f1c = 14; f2c = 15; f3c = 16;
        }

        //public DataC(){System.Diagnostics.Debug.WriteLine(this.GetType()+" cons");}

    }

    // get  instance 
    var g1 = SingletonBase.Instance<DataC>();
    var g2 = SingletonBase.Instance<DataA>();
    var g3 = SingletonBase.Instance<DataB>();

#endif
    //==============================================

    // *** With a call to InitOnLoad.Initialise() added to your main method. ***
    public class InitOnLoad : Attribute
    {
        public static void Initialise()
        {
            // get a list of types which are marked with the InitOnLoad attribute
            var types = from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()) where t.GetCustomAttributes(typeof(InitOnLoad), false).Count() > 0 select t;
            // process each type to force initialise it
            foreach (var type in types)
            {
#if false
                // try to find a static field which is of the same type as the declaring class, and then evaluate the static field if found.
                var field = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(f => f.FieldType == type).FirstOrDefault();
                if (field != null) field.GetValue(null);
#else
                // Call Default Constructor 
                // BindingFlags.Static The static constructor for a class executes at most once in a given application domain.
                // The execution of a static constructor is triggered by the first of the following events to occur within an application domain:
                // An instance of the class is created.Any of the static members of the class are referenced.
                ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (constructor == null /*|| constructor.IsAssembly /*Also exclude internal constructors.*/) throw new Exception(string.Format("A constructor is missing for '{0}'.", type.Name));
                var obj = constructor.Invoke(null);
#endif
            }
        }
    }



}//namespace

/*


    static class TxtLog
    {
        static public RichTextBox logBox;

        public static void err(string msg)
        {
            logSource("Error", msg);
        }

        public static void err(Exception e)
        {
            logSource("Except", e.Message);
        }

        public static void report(string msg)
        {
            log("report", msg);
        }

        private static void log(string logType, string msg)
        {
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                string str=string.Format("{0} ({1}): {2}", DateTime.Now.ToString("yyyy/MM/dd/ddd - HH:mm:ss", DateTimeFormatInfo.InvariantInfo),logType, msg);
                Debug.WriteLine(str);
                w.WriteLine(str);

                //w.WriteLine("{0} ({1}): {2}", DateTime.Now.ToString("yyyy/MM/dd/ddd - HH:mm:ss", DateTimeFormatInfo.InvariantInfo),logType, msg);
                //w.WriteLine("{0} ({1} {2}) : {3}", logType, DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), msg);
                // Update the underlying file.
                w.Flush();

                // Close the writer and underlying file.
                w.Close();
            }
        }

        private static void logSource(string logType, string msg)
        {
            // Get the frame one step up the call tree
            StackFrame CallStack = new StackFrame(2, true);

            // These will now show the file and line number of the caller
            msg = string.Format("{0} [{1} / {2} / {3}]", msg, CallStack.GetMethod(), CallStack.GetFileName(), CallStack.GetFileLineNumber());
            log(logType, msg);
        }
    }
*/

