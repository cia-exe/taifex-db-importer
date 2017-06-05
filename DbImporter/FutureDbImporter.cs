using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using System.Data.Linq;
using System.Data.Linq.Mapping;
using GeneralUtil;

namespace DbImporter
{
    namespace Future
    {
        using System.Threading;

        using Database;
        using DbContext = Database.DbDataContext;

        class DbImporter
        {
            Dictionary<string, byte> mapContractID = new Dictionary<string, byte>(20);
            DbContext database;

            protected HashSet<string> mapIgnoreContractID= new HashSet<string>();
            string[] ignoreContractIDstr = {"CAF","CBF","CCF","CDF","CEF","CFF","CGF","CHF","CJF","CKF","CLF","CNF","CQF","CRF","CSF","CTF","CVF","CZF","DAF","DCF","DEF","DFF",
                                            "DGF","DHF","DIF","DJF","DKF","DMF","DNF","DOF","DPF","DQF","DRF","CAF","CBF","CCF","CDF","CEF","CFF","CGF","CHF","CJF","CKF","CLF","CNF",
                                            "CQF","CRF","CSF","CTF","CVF","CZF","DAF","DCF","DEF","DFF","DGF","DHF","DIF","DJF","DKF","DMF","DNF","DOF","DPF","DQF","DRF","CAF","CBF",
                                            "CCF","CDF","CEF","CFF","CGF","CHF","CJF","CKF","CLF","CNF","CQF","CRF","CSF","CTF","CVF","CZF","DAF","DCF","DEF","DFF","DGF","DHF","DIF",
                                            "DJF","DKF","DLF","DMF","DNF","DOF","DPF","DQF","DRF","CAF","CBF","CCF","CDF","CEF","CFF","CGF","CHF","CJF","CKF","CLF","CNF","CQF","CRF","CSF",
                                            "CTF","CVF","CZF","DAF","DCF","DEF","DFF","DGF","DHF","DIF","DJF","DKF","DMF","DNF","DOF","DPF","DQF","DRF","CAF","CBF","CCF","CDF","CEF",
                                            "CFF","CGF","CHF","CJF","CKF","CLF","CNF","CQF","CRF","CSF","CTF","CVF","CZF","DAF","DCF","DEF","DFF","DGF","DHF","DIF","DJF","DKF","DMF",
                                            "DNF","DOF","DPF","DQF","DRF","DSF","DTF","DUF","DVF","DWF","DXF","DG1","DO1","CV1","DU1","股票期貨"}; 
            //股票期貨前兩碼代表股票期貨之標的證券，南亞為「CA」，第三碼為該標的股票之版本，未經契約調整之代號為F， 為期貨(Futures)之意，代表標準契約。

            public DbImporter()
            {
                //string s= global::DbImporter.Properties.Settings.Default.STOCK_TWConnectionString;
                database = new DbContext();

                foreach (var c in database.RowContract_IDs)
                {
                    mapContractID[c.Name.Trim()] = c.ID;
                    //TxtLog.showLog(c.Name + " : " + c.ID);
                }

                foreach (var s in ignoreContractIDstr)
                {
                    if (mapContractID.ContainsKey(s)) throw new Exception("The Key is in both ContractID and IgnoreContractID!!! --> " +s);
                    mapIgnoreContractID.Add(s);
                    if (s.Length == 3 && (s[2] == 'f' || s[2] == 'F'))
                    {
                        string s2 = s.Substring(0, 2); // "CAF" ==> "CA"
                        if (mapContractID.ContainsKey(s2)) throw new Exception("The Key is in both ContractID and IgnoreContractID!!! --> " + s);
                        mapIgnoreContractID.Add(s2);
                    }
                }

                /*
                var q = from c in database.RowContract_IDs
                        orderby c.ID ascending
                        select c;

                foreach (var c in q)
                {
                    mapContractID[c.Name.Trim()] = c.ID;
                    //TxtLog.showLog(c.Name + " : " + c.ID);
                }
                */
            }


            abstract class CSVParser
            {
                //abstract public int ParseCSVFile(StreamReader csv);

                /*
          // date str format is yyyy/mm/dddd
          static int DateStr2Int(string dateStr)
          {
              string[] strs = dateStr.Split('/');
              int yy = int.Parse(strs[0]);
              int mm = int.Parse(strs[1]);
              int dd = int.Parse(strs[2]);
              return ((yy * 100 + mm) * 100) + dd;
          }

          static DateTime DateStr2DateTime(string dateStr)
          {
              string[] strs = dateStr.Split('/');
              int yy = int.Parse(strs[0]);
              int mm = int.Parse(strs[1]);
              int dd = int.Parse(strs[2]);
              return new DateTime(yy, mm, dd);
          }

          static string Int2DateStr(int iDate)
          {
              int dd = iDate % 100,
                  mm = (iDate / 100) % 100,
                  yy = iDate / 10000;
              return yy.ToString() + '/' + mm.ToString() + '/' + dd.ToString();
          }
          */

                protected static double? parseDouble(string[] sArr, object idx)
                {
                    string s = sArr[(int)idx];
                    if (s == "-") return null;
                    return double.Parse(s);
                }

                protected static int? parseInt(string[] sArr, object idx)
                {
                    string s = sArr[(int)idx];
                    if (s == "-") return null;
                    return int.Parse(s);
                }

                protected byte parseID(string[] sArr, object idx) // return 0 to ignore this row.
                {
                    string s = sArr[(int)idx];

                    if (mapIgnoreContractID.Contains(s)) return 0;

#if true
                    byte x;
                    if (mapContractID.TryGetValue(s, out x))
                    {
                        System.Diagnostics.Trace.Assert(x != 0);
                        return x;
                    }
                    else
                        return 0;                    
#else

                    try
                    {
                        var x = mapContractID[s];
                        System.Diagnostics.Trace.Assert(x != 0);
                        return x;
                    }
                    catch (KeyNotFoundException)// e)
                    {
                        //TxtLog.showLog("Key was Not Found => "+ s);
                        //throw e;
                        return 0;
                    }
#endif
                }

                //public const int NULL_VAL = (int)-0x80000000;

                /*
                void parseS(string[] sArr, object idx, out double lVal)
                {
                    string s = sArr[(int)idx];
                    if (s == "-") lVal=0.0;
                    lVal=double.Parse(s);
                }
            
                void parseS(string[] sArr, object idx, out int lVal)
                {
                    string s = sArr[(int)idx];
                    if (s == "-") lVal = 0;
                    lVal = int.Parse(s);
                }
                */

                protected Dictionary<string, byte> mapContractID;
                protected DbDataContext database;
                protected HashSet<string> mapIgnoreContractID;

                public CSVParser(DbImporter importer)
                {
                    mapContractID = importer.mapContractID;
                    mapIgnoreContractID = importer.mapIgnoreContractID;
                    database = importer.database;
                }

                /*
                bool IsComplete(object sourceEntity)
                {
                    //get entity members
                    IEnumerable<MetaDataMember> dataMembers = from mem in database.Mapping.GetTable(sourceEntity.GetType()).RowType.DataMembers
                                                              where mem.IsAssociation == false
                                                              select mem;        //go through the list of members and compare values

                    foreach (MetaDataMember mem in dataMembers)
                        if (mem.StorageAccessor.GetBoxedValue(sourceEntity) == null) return false;

                    return true;
                }
                */

                virtual protected bool IsOptional(string columnName)
                {
                    return false;
                }

                protected bool Union(object targetEntity, object sourceEntity) //return true if all field completed.
                {
                    bool complete = true;
                    //get entity members
                    IEnumerable<MetaDataMember> dataMembers = from mem in database.Mapping.GetTable(sourceEntity.GetType()).RowType.DataMembers
                                                              where mem.IsAssociation == false
                                                              select mem;        //go through the list of members and compare values

                    foreach (MetaDataMember mem in dataMembers)
                    {
                        object originalValue = mem.StorageAccessor.GetBoxedValue(targetEntity);
                        object newValue = mem.StorageAccessor.GetBoxedValue(sourceEntity);            //check if the value has changed

                        if (newValue == null)
                        {
                            if (originalValue == null && !IsOptional(mem.Name))
                                complete = false;

                            continue;
                        }

                        if (originalValue != null)
                        {
                            if (newValue.Equals(originalValue)) continue;
                            else TxtLog.showLog("duplicate input  data!! org= " + originalValue + " new= " + newValue );
                        }
                        //use reflection to update the target
                        System.Reflection.PropertyInfo propInfo = targetEntity.GetType().GetProperty(mem.Name);
                        propInfo.SetValue(targetEntity, propInfo.GetValue(sourceEntity, null), null);                //setboxedvalue bypasses change tracking - otherwise mem.StorageAccessor.SetBoxedValue(ref targetEntity, newValue); could be used instead of reflection
                    }

                    return complete;
                }

                //abstract object NewRow();
                //abstract void setEmpty(object row);
                //abstract object FindRowByKey(object keyRow, IQueryable table);

                //abstract protected bool IsEmpty(object row, Enum rowPart);
                //abstract protected bool Union(object dest, object src);
                //abstract protected System.Linq.Expressions.Expression getWhereKey(object row);

                // init in derived classs
                
                //Enum UnknownEnum;

                abstract protected object FindDbRowByKey(object r);
                abstract protected object FindRowByKey(object r, IQueryable<object> q);

                abstract protected bool getParsedRow(string[] arrS, out object row); // return true if row is completed.
                protected string strEndOfFile;
                protected Type enumCSVFieldsType;
                protected ITable DbTable;
                //protected IList<object> buffer;

                //{ return Queryable.SingleOrDefault<RowBigTraderUnclosedVol>(q as IQueryable<RowBigTraderUnclosedVol>, getWhereKey(r)); }

                /*
                void Copy(object dest, Object src)
                {
                    foreach (Enum e in Enum.GetValues(enumRowPartType))
                        if (UnknownEnum != e) Copy(dest, src, e);
                }*/

                bool _bRun;
                public bool bRun
                {
                    get { return _bRun; }
                    set { _bRun = value; }
                }

                public int ParseCSVFile(StreamReader csv)
                {
                    //var dict= database.RowTradeInfs.ToDictionary(p=>p.);
                    //UnknownEnum = (Enum)Enum.ToObject(enumRowPartType, 0);
                    
                    List<object> uncompletedRowBuffer = new List<object>();
                    int parseCount = 0;

                    bRun = true;
                    int parsedLine = Helper.ParseCSVFile(csv, enumCSVFieldsType, strEndOfFile, (string[] s) =>
                        {
                            if (++parseCount % 100 == 0)  TxtLog.showLog("parsing line:"+parseCount);

                            object r;

                            if (getParsedRow(s, out r))
                            {// row is completed

                                if (r == null) return bRun; // ignore it.

                                var rDb = FindDbRowByKey(r);

                                if (rDb == null)
                                {
                                    TxtLog.showLog("1 InsertOnSubmit:" + s[0]);
                                    DbTable.InsertOnSubmit(r); //bug
                                }
                                else
                                    Union(rDb, r);

                                return bRun;
                            }

                            var r0 = FindRowByKey(r, uncompletedRowBuffer.AsQueryable());
                            if (r0 == null)
                                uncompletedRowBuffer.Add(r);
                            else if (Union(r0, r))
                            {// row is completed

                                var rDb = FindDbRowByKey(r0);

                                if (rDb == null)
                                {
                                    TxtLog.showLog("2 InsertOnSubmit:" + s[0]);
                                    DbTable.InsertOnSubmit(r0);
                                }
                                else
                                    Union(rDb, r0);

                                uncompletedRowBuffer.Remove(r0);
                            }
                            return bRun;
                        });

                    foreach (var t in uncompletedRowBuffer)
                    {
                        DbTable.InsertOnSubmit(t);
                        TxtLog.err("unCompleted entity:" + t);
                    }

                    //if (parsedLine > 0){
                    try
                    {
                        database.SubmitChanges(ConflictMode.FailOnFirstConflict);
                        //database.SubmitChanges(ConflictMode.ContinueOnConflict);
                        //database.SubmitChanges();
                    }
                    catch (ChangeConflictException e)
                    {
                        TxtLog.err("ChangeConflictException: " + e);

                        // Make some adjustments and Try again.
                        //database.SubmitChanges();                         
                    }
                    catch (DuplicateKeyException e)
                    {
                        TxtLog.err("DuplicateKeyException: " +  e);

                        // Make some adjustments and Try again.
                        //database.SubmitChanges();                         
                    }
                    //}

                    if (!VerifyDB()) parsedLine*=-1;
                    return parsedLine;
                }

                protected static bool VerifyLastRecordDate(DateTime lastDay)
                {
                    if (DateTime.Today - lastDay > new TimeSpan(38, 0, 0))
                    {
                        TxtLog.report("########## Warning: last Data date is not close Today ##########");
                        return false;
                    }
                    else
                    {
                        TxtLog.showLog("Verify OK !");
                        return true;
                    }
                }

                virtual protected bool VerifyDB() { return true; }

            }

            class TradeInfParser : CSVParser
            {

                enum CSVField // must match all fields and order in csv file.
                { 日期 = 0, 契約, 交割月份, 開盤價, 最高價, 最低價, 收盤價, 漲跌價, 漲跌P, 成交量, 結算價, 未沖銷契約數, 最後最佳買價, 最後最佳賣價, 歷史最高價, 歷史最低價, 是否因訊息面暫停交易, 交易時段 };
                //日期 = 0, 契約, 交割月份, 開盤價, 最高價, 最低價, 收盤價, 漲跌價, 漲跌P, 成交量, 結算價, 未沖銷契約數, 最後最佳買價, 最後最佳賣價, 歷史最高價, 歷史最低價 /*,暫停交易*/
                //交易日期 ,契約,交割月份,開盤價,最高價,最低價,收盤價,漲跌價,漲跌%,*成交量,結算價,*未沖銷契約數,最後最佳買價,最後最佳賣價,歷史最高價,歷史最低價

                //{ 日期 = 0, 契約, 交割月份, 開盤價, 最高價, 最低價, 收盤價, 漲跌價, 漲跌P, 成交量, 結算價, 未沖銷契約數, 最後買進價格, 最後賣出價格 };
                //日期 ,契約,交割月份,開盤價,最高價,最低價,收盤價,漲跌價,漲跌%,*成交量,結算價,*未沖銷契約數,最後買進價格,最後賣出價格


                public TradeInfParser(DbImporter importer)
                    : base(importer)
                {
                    this.enumCSVFieldsType = typeof(CSVField);

                    this.DbTable = database.RowTradeInfs;
                    this.strEndOfFile = @"價差商品";
                }

                //override protected object getParsedRow(string[] arrS)
                override protected bool getParsedRow(string[] arrS, out object row) // return true if row is completed.
                {
                    var id = parseID(arrS, CSVField.契約);
                    if (id == 0)
                    {
                        row = null; // output row as null to ignore it.
                        return true;
                    }

                    string strPayMonth = arrS[(int)CSVField.交割月份];

                    { // "價差商品"
                        if (strPayMonth.Length == 13) //e.g. 201501/201502
                        {
                            if (strPayMonth[6] == '/')
                            {
                                row = null;
                                return true;
                            }
                        }

                        if (strPayMonth.Length == 15) // e.g. 201510W1/201510
                        {
                            if(((strPayMonth[6]=='w') || (strPayMonth[6]=='W')) && (strPayMonth[8] =='/'))
                            {
                                row = null;
                                return true;
                            }
                        }
                    }

                    RowTradeInf dest = new RowTradeInf();
                    row = dest;

                    dest.contract_ID = id;// parseID(arrS, CSVField.契約);
                    dest.trade_date = DateTime.Parse(arrS[(int)CSVField.日期]);

                    //dest.pay_month = DateTime.ParseExact(arrS[(int)CSVField.交割月份], "yyyyMM", null);                    
                    int idx_w = strPayMonth.ToLower().IndexOf('w');
                    if (idx_w < 0)
                        dest.pay_month = DateTime.ParseExact(strPayMonth, "yyyyMM", null);
                    else
                    {
                        Trace.Assert(idx_w < (strPayMonth.Length - 1));
                        {
                            var ch = strPayMonth.Last();
                            Trace.Assert(ch >= '0' && ch <= '9');
                        }
                        dest.pay_month = DateTime.ParseExact(strPayMonth, "yyyyMMWd", null);
                        dest.pay_month = dest.pay_month.AddDays(10.0);
                    }

                    dest.price_open = parseDouble(arrS, CSVField.開盤價);
                    dest.price_hi = parseDouble(arrS, CSVField.最高價);
                    dest.price_low = parseDouble(arrS, CSVField.最低價);
                    dest.price_close = parseDouble(arrS, CSVField.收盤價);
                    dest.volume = parseInt(arrS, CSVField.成交量);
                    dest.price_settle = parseDouble(arrS, CSVField.結算價);
                    dest.unclosed_volume = parseInt(arrS, CSVField.未沖銷契約數);
                    dest.last_buy_price = parseDouble(arrS, CSVField.最後最佳買價);//最後買進價格);
                    dest.last_sale_price = parseDouble(arrS, CSVField.最後最佳賣價);//最後賣出價格);

                    return true;
                }

               protected override bool VerifyDB()
               {

                   //var Id = mapContractID["TX"];     
                   var table = database.RowTradeInfs;
                   var lastDay = table.Max(p => p.trade_date);
                   var res = from p in table
                             where p.trade_date == lastDay
                             orderby p.contract_ID descending, p.pay_month descending
                             select p;

                   foreach (var query in res)
                       TxtLog.showLog(Helper.Arr2Str(Helper.getPropertyValues(query)));

                   return VerifyLastRecordDate(lastDay) && base.VerifyDB() && res.Count()>0;
               }

                override protected object FindDbRowByKey(object row)
                {
                    RowTradeInf r = row as RowTradeInf;
                    return database.RowTradeInfs.SingleOrDefault(g => g.contract_ID == r.contract_ID && g.trade_date == r.trade_date && g.pay_month==r.pay_month);
                }

                override protected object FindRowByKey(object row, IQueryable<object> q)
                {
                    RowTradeInf r = row as RowTradeInf;

                    return q.SingleOrDefault(g =>
                            ((RowTradeInf)g).contract_ID == r.contract_ID && ((RowTradeInf)g).trade_date == r.trade_date &&  ((RowTradeInf)g).pay_month == r.pay_month);

                    //return Queryable.SingleOrDefault<RowBigTraderUnclosedVol>(q as IQueryable<RowBigTraderUnclosedVol> , g => g.contract_ID == r.contract_ID && g.trade_date == r.trade_date);
                }

            }

            class BigTraderParser : CSVParser
            {
                //日期 商品 商品名稱 月份類別	交易人類別	前五大交易人買方	前五大交易人賣方	前十大交易人買方	前十大交易人賣方	全市場未沖銷部位數
                enum CSVField // must match all fields and order in csv file.
                { 日期 = 0, 商品, 商品名稱, 月份類別, 交易人類別, 前五大交易人買方, 前五大交易人賣方, 前十大交易人買方, 前十大交易人賣方, 全市場未沖銷部位數 };

                public BigTraderParser(DbImporter importer)
                    : base(importer)
                {
                    this.enumCSVFieldsType = typeof(CSVField);

                    this.DbTable = database.RowBigTraderUnclosedVols;
                    this.strEndOfFile = @"月份類別格式";
                }

                //override protected object getParsedRow(string[] arrS)
                override protected bool getParsedRow(string[] arrS, out object row) // return true if row is completed.
                {
                    var id=parseID(arrS,CSVField.商品);
                    string strPayMonth = arrS[(int)CSVField.月份類別];

                    if (id==0 || strPayMonth=="-")
                    {
                        row = null; // output row as null to ignore it.
                        return true;
                    }

                    RowBigTraderUnclosedVol dest = new RowBigTraderUnclosedVol();
                    row = dest;

                    dest.trade_date = DateTime.Parse(arrS[(int)CSVField.日期]);
                    dest.contract_ID = id;                    

                    if (TryFillAll(dest, arrS, strPayMonth) || TryFillNear(dest, arrS, strPayMonth) || TryFillWeek(dest, arrS, strPayMonth))
                        return false;
                    else
                    {
                        TxtLog.err("TryFillXXXX all failed!!!!");
                        return true;
                    }
                }

                override protected bool IsOptional(string columnName)
                {
                    return columnName.ToLower().IndexOf("week") >= 0;                    
                }

                private static bool TryFillAll(RowBigTraderUnclosedVol dest, string[] arrS, string strPayMonth)
                {
                    {// verify

                        if (strPayMonth != "999999") //月份類別格式:yyyymm為近月，999999 為所有月份
                            return false;
                    }

                    //交易人類別格式： 0 為部位排序前五大或前十大交易人，1 為部位排序前五大或前十大交易人中，屬於特定法人者									
                    if (arrS[(int)CSVField.交易人類別] == "1")
                    {
                        dest.top5corp_buy_vol = parseInt(arrS, CSVField.前五大交易人買方);
                        dest.top5corp_sell_vol = parseInt(arrS, CSVField.前五大交易人賣方);
                        dest.top10corp_buy_vol = parseInt(arrS, CSVField.前十大交易人買方);
                        dest.top10corp_sell_vol = parseInt(arrS, CSVField.前十大交易人賣方);
                    }
                    else
                    {
                        dest.top5_buy_vol = parseInt(arrS, CSVField.前五大交易人買方);
                        dest.top5_sell_vol = parseInt(arrS, CSVField.前五大交易人賣方);
                        dest.top10_buy_vol = parseInt(arrS, CSVField.前十大交易人買方);
                        dest.top10_sell_vol = parseInt(arrS, CSVField.前十大交易人賣方);
                    }
                    dest.market_vol = parseInt(arrS, CSVField.全市場未沖銷部位數);

                    return true;
                }

                private static bool TryFillNear(RowBigTraderUnclosedVol dest, string[] arrS, string strPayMonth)
                {
                    {// verify

                        if (strPayMonth == "999999") //月份類別格式:yyyymm為近月，999999 為所有月份
                            return false;

                        foreach (var c in strPayMonth)
                        {
                            if (c < '0' || c > '9')
                                return false;
                        }
                    }

                    //交易人類別格式： 0 為部位排序前五大或前十大交易人，1 為部位排序前五大或前十大交易人中，屬於特定法人者
                    if (arrS[(int)CSVField.交易人類別] == "1")
                    {
                        dest.near_top5corp_buy_vol = parseInt(arrS, CSVField.前五大交易人買方);
                        dest.near_top5corp_sell_vol = parseInt(arrS, CSVField.前五大交易人賣方);
                        dest.near_top10corp_buy_vol = parseInt(arrS, CSVField.前十大交易人買方);
                        dest.near_top10corp_sell_vol = parseInt(arrS, CSVField.前十大交易人賣方);
                    }
                    else
                    {
                        dest.near_top5_buy_vol = parseInt(arrS, CSVField.前五大交易人買方);
                        dest.near_top5_sell_vol = parseInt(arrS, CSVField.前五大交易人賣方);
                        dest.near_top10_buy_vol = parseInt(arrS, CSVField.前十大交易人買方);
                        dest.near_top10_sell_vol = parseInt(arrS, CSVField.前十大交易人賣方);
                    }
                    dest.near_market_vol = parseInt(arrS, CSVField.全市場未沖銷部位數);

                    return true;
                }

                private static bool TryFillWeek(RowBigTraderUnclosedVol dest, string[] arrS, string strPayMonth)
                {
                    {// verify

                        int idx_w = strPayMonth.ToLower().IndexOf('w');
                        if (idx_w <= 0) // w at first char or not found.
                            return false;

                        if(idx_w == (strPayMonth.Length - 1)) // w at last char
                            return false;

                        var ch = strPayMonth.Last(); // w is not digit
                        if (ch < '0' && ch > '9')
                            return false;
                        
                    }

                    //交易人類別格式： 0 為部位排序前五大或前十大交易人，1 為部位排序前五大或前十大交易人中，屬於特定法人者
                    if (arrS[(int)CSVField.交易人類別] == "1")
                    {
                        dest.week_top5corp_buy_vol = parseInt(arrS, CSVField.前五大交易人買方);
                        dest.week_top5corp_sell_vol = parseInt(arrS, CSVField.前五大交易人賣方);
                        dest.week_top10corp_buy_vol = parseInt(arrS, CSVField.前十大交易人買方);
                        dest.week_top10corp_sell_vol = parseInt(arrS, CSVField.前十大交易人賣方);
                    }
                    else
                    {
                        dest.week_top5_buy_vol = parseInt(arrS, CSVField.前五大交易人買方);
                        dest.week_top5_sell_vol = parseInt(arrS, CSVField.前五大交易人賣方);
                        dest.week_top10_buy_vol = parseInt(arrS, CSVField.前十大交易人買方);
                        dest.week_top10_sell_vol = parseInt(arrS, CSVField.前十大交易人賣方);
                    }
                    dest.week_market_vol = parseInt(arrS, CSVField.全市場未沖銷部位數);

                    return true;
                }


                protected override bool VerifyDB()
                {

                    //var Id = mapContractID["TX"];     
                    var table =database.RowBigTraderUnclosedVols;
                    var lastDay = table.Max(p=>p.trade_date);
                    var res = from p in table
                              where p.trade_date == lastDay
                              orderby p.contract_ID descending
                              select p;

                    foreach (var query in res)
                        TxtLog.showLog(Helper.Arr2Str(Helper.getPropertyValues(query)));

                    return VerifyLastRecordDate(lastDay) && base.VerifyDB() && res.Count() > 0;
                }

                override protected object FindDbRowByKey(object row)
                {
                    RowBigTraderUnclosedVol r = row as RowBigTraderUnclosedVol;
                    return database.RowBigTraderUnclosedVols.SingleOrDefault(g => g.contract_ID == r.contract_ID && g.trade_date == r.trade_date);
                }

                override protected object FindRowByKey(object row, IQueryable<object> q)
                {
                    RowBigTraderUnclosedVol r = row as RowBigTraderUnclosedVol;

                    return q.SingleOrDefault(g =>
                            ((RowBigTraderUnclosedVol)g).contract_ID == r.contract_ID && ((RowBigTraderUnclosedVol)g).trade_date == r.trade_date);

                    //return Queryable.SingleOrDefault<RowBigTraderUnclosedVol>(q as IQueryable<RowBigTraderUnclosedVol> , g => g.contract_ID == r.contract_ID && g.trade_date == r.trade_date);
                }

            }

            class CorpX3Parser : CSVParser
            {
                //日期	商品名稱	身份別	多方交易口數	多方交易契約金額(千元)	空方交易口數	空方交易契約金額(千元)	多空交易口數淨額	多空交易契約金額淨額(千元)
                //多方未平倉口數	多方未平倉契約金額(千元)	空方未平倉口數	空方未平倉契約金額(千元)	多空未平倉口數淨額	多空未平倉契約金額淨額(千元)
                enum CSVField // must match all fields and order in csv file.
                {
                    日期 = 0, 商品名稱, 身份別, 多方交易口數, 多方交易契約金額k, 空方交易口數, 空方交易契約金額k, 多空交易口數淨額, 多空交易契約金額淨額k,
                    多方未平倉口數, 多方未平倉契約金額k, 空方未平倉口數, 空方未平倉契約金額k, 多空未平倉口數淨額, 多空未平倉契約金額淨額k
                };

                public CorpX3Parser(DbImporter importer)
                    : base(importer)
                {
                    this.enumCSVFieldsType = typeof(CSVField);

                    this.DbTable = database.RowCorpX3s;
                    this.strEndOfFile = null;
                }

                //override protected object getParsedRow(string[] arrS)
                override protected bool getParsedRow(string[] arrS, out object row) // return true if row is completed.
                {
                    var id = parseID(arrS, CSVField.商品名稱);
                    if (id == 0)
                    {
                        row = null; // output row as null to ignore it.
                        return true;
                    }

                    RowCorpX3 dest = new RowCorpX3();
                    row = dest;

                    dest.trade_date = DateTime.Parse(arrS[(int)CSVField.日期]);
                    dest.contract_ID = id;// parseID(arrS, CSVField.商品名稱);

                    string sCorp = arrS[(int)CSVField.身份別];
                    if (sCorp.IndexOf(@"外資") >= 0)
                    {
                        dest.c1_long_vol = parseInt(arrS, CSVField.多方交易口數);
                        dest.c1_long_moneyK = parseInt(arrS, CSVField.多方交易契約金額k);
                        dest.c1_short_vol = parseInt(arrS, CSVField.空方交易口數);
                        dest.c1_short_moneyK = parseInt(arrS, CSVField.空方交易契約金額k);
                        dest.c1_unclosed_long_vol = parseInt(arrS, CSVField.多方未平倉口數);
                        dest.c1_unclosed_long_moneyK = parseInt(arrS, CSVField.多方未平倉契約金額k);
                        dest.c1_unclosed_short_vol = parseInt(arrS, CSVField.空方未平倉口數);
                        dest.c1_unclosed_short_moneyK = parseInt(arrS, CSVField.空方未平倉契約金額k);
                    }
                    else if (sCorp.IndexOf(@"投信") >= 0)
                    {
                        dest.c2_long_vol = parseInt(arrS, CSVField.多方交易口數);
                        dest.c2_long_moneyK = parseInt(arrS, CSVField.多方交易契約金額k);
                        dest.c2_short_vol = parseInt(arrS, CSVField.空方交易口數);
                        dest.c2_short_moneyK = parseInt(arrS, CSVField.空方交易契約金額k);
                        dest.c2_unclosed_long_vol = parseInt(arrS, CSVField.多方未平倉口數);
                        dest.c2_unclosed_long_moneyK = parseInt(arrS, CSVField.多方未平倉契約金額k);
                        dest.c2_unclosed_short_vol = parseInt(arrS, CSVField.空方未平倉口數);
                        dest.c2_unclosed_short_moneyK = parseInt(arrS, CSVField.空方未平倉契約金額k);
                    }
                    else if (sCorp.IndexOf(@"自營商") >= 0)
                    {
                        dest.c3_long_vol = parseInt(arrS, CSVField.多方交易口數);
                        dest.c3_long_moneyK = parseInt(arrS, CSVField.多方交易契約金額k);
                        dest.c3_short_vol = parseInt(arrS, CSVField.空方交易口數);
                        dest.c3_short_moneyK = parseInt(arrS, CSVField.空方交易契約金額k);
                        dest.c3_unclosed_long_vol = parseInt(arrS, CSVField.多方未平倉口數);
                        dest.c3_unclosed_long_moneyK = parseInt(arrS, CSVField.多方未平倉契約金額k);
                        dest.c3_unclosed_short_vol = parseInt(arrS, CSVField.空方未平倉口數);
                        dest.c3_unclosed_short_moneyK = parseInt(arrS, CSVField.空方未平倉契約金額k);
                    }

                    return false;
                }

                protected override bool VerifyDB()
                {

                    //var Id = mapContractID["TX"];     
                    var table = database.RowCorpX3s;
                    var lastDay = table.Max(p => p.trade_date);
                    var res = from p in table
                              where p.trade_date == lastDay
                              orderby p.contract_ID descending
                              select p;

                    foreach (var query in res)
                        TxtLog.showLog(Helper.Arr2Str(Helper.getPropertyValues(query)));

                    return VerifyLastRecordDate(lastDay) && base.VerifyDB() && res.Count() > 0;
                }
                
                override protected object FindDbRowByKey(object row)
                {
                    RowCorpX3 r = row as RowCorpX3;
                    return database.RowCorpX3s.SingleOrDefault(g => g.contract_ID == r.contract_ID && g.trade_date == r.trade_date);
                }

                override protected object FindRowByKey(object row, IQueryable<object> q)
                {
                    RowCorpX3 r = row as RowCorpX3;

                    return q.SingleOrDefault(g =>
                            ((RowCorpX3)g).contract_ID == r.contract_ID && ((RowCorpX3)g).trade_date == r.trade_date);

                    //return Queryable.SingleOrDefault<RowBigTraderUnclosedVol>(q as IQueryable<RowBigTraderUnclosedVol> , g => g.contract_ID == r.contract_ID && g.trade_date == r.trade_date);
                }

            }

#if (false)
            class TradeInfParser : CSVParser
            {
                public TradeInfParser(DbImporter importer) : base(importer) { }
                //======================================================================

                //日期 ,契約,交割月份,開盤價,最高價,最低價,收盤價,漲跌價,漲跌%,*成交量,結算價,*未沖銷契約數,最後買進價格,最後賣出價格
                enum CSVField // must match all fields and order in csv file.
                { 日期 = 0, 契約, 交割月份, 開盤價, 最高價, 最低價, 收盤價, 漲跌價, 漲跌P, 成交量, 結算價, 未沖銷契約數, 最後買進價格, 最後賣出價格 };

                private void ParseRow(string[] s, RowTradeInf dest)
                {
                    dest.contract_ID = mapContractID[s[(int)CSVField.契約]];
                    dest.trade_date = DateTime.Parse(s[(int)CSVField.日期]);
                    dest.pay_month = DateTime.ParseExact(s[(int)CSVField.交割月份], "yyyyMM", null);

                    dest.price_open = parseDouble(s, CSVField.開盤價);
                    dest.price_hi = parseDouble(s, CSVField.最高價);
                    dest.price_low = parseDouble(s, CSVField.最低價);
                    dest.price_close = parseDouble(s, CSVField.收盤價);
                    dest.volume = parseInt(s, CSVField.成交量);
                    dest.price_settle = parseDouble(s, CSVField.結算價);
                    dest.unclosed_volume = parseInt(s, CSVField.未沖銷契約數);
                    dest.last_buy_price = parseDouble(s, CSVField.最後買進價格);
                    dest.last_sale_price = parseDouble(s, CSVField.最後賣出價格);
                }

                private RowTradeInf FindRowByKey(RowTradeInf r)
                {
                    return database.RowTradeInfs.SingleOrDefault(g => g.contract_ID == r.contract_ID && g.trade_date == r.trade_date && g.pay_month == r.pay_month);
                }

                static bool Equals(RowTradeInf a, RowTradeInf b)
                {
                    return
                        a.contract_ID == b.contract_ID &&
                        a.trade_date == b.trade_date &&
                        a.pay_month == b.pay_month &&
                        a.price_open == b.price_open &&
                        a.price_hi == b.price_hi &&
                        a.price_low == b.price_low &&
                        a.price_close == b.price_close &&
                        a.volume == b.volume &&
                        a.price_settle == b.price_settle &&
                        a.unclosed_volume == b.unclosed_volume &&
                        a.last_buy_price == b.last_buy_price &&
                        a.last_sale_price == b.last_sale_price;
                }

                static void Copy(RowTradeInf dest, RowTradeInf src)
                {
                    dest.contract_ID = src.contract_ID;
                    dest.trade_date = src.trade_date;
                    dest.pay_month = src.pay_month;
                    dest.price_open = src.price_open;
                    dest.price_hi = src.price_hi;
                    dest.price_low = src.price_low;
                    dest.price_close = src.price_close;
                    dest.volume = src.volume;
                    dest.price_settle = src.price_settle;
                    dest.unclosed_volume = src.unclosed_volume;
                    dest.last_buy_price = src.last_buy_price;
                    dest.last_sale_price = src.last_sale_price;
                }

                override public int ParseCSVFile(StreamReader csv)
                {
                    return Helper.ParseCSVFile(csv, typeof(CSVField), @"價差商品",
                        new Helper.OnParse1CSVLine((string[] s) =>
                            {
                                RowTradeInf r = new RowTradeInf();
                                ParseRow(s, r);

                                var r0 = FindRowByKey(r);
                                if (r0 == null)
                                    this.database.RowTradeInfs.InsertOnSubmit(r);
                                else if (!Equals(r0, r))
                                {
                                    string ss = ""; foreach (string str in s) ss += "|" + str.ToString();
                                    TxtLog.showLog("warning: dtat base entity exists and not equas new one " + ss);
                                    Copy(r0, r);
                                }
                                return true;
                            }));
                }
                //====================================================================== 

            }

            class BigTraderParser : CSVParser
            {
                public BigTraderParser(DbImporter importer) : base(importer) { }
                //======================================================================

                //======================================================================
                //日期 商品 商品名稱 月份類別	交易人類別	前五大交易人買方	前五大交易人賣方	前十大交易人買方	前十大交易人賣方	全市場未沖銷部位數
                enum CSVField // must match all fields and order in csv file.
                { 日期 = 0, 商品, 商品名稱, 月份類別, 交易人類別, 前五大交易人買方, 前五大交易人賣方, 前十大交易人買方, 前十大交易人賣方, 全市場未沖銷部位數 };

                enum RowPart { Unknown = 0, Near, All, NearCorp, AllCorp }

                private RowPart ParseRow(string[] s, RowBigTraderUnclosedVol dest)
                {
                    RowPart rowPart = RowPart.Unknown;

                    dest.trade_date = DateTime.Parse(s[(int)CSVField.日期]); ;
                    dest.contract_ID = mapContractID[s[(int)CSVField.商品]];

                    if (s[(int)CSVField.月份類別] == "999999") //月份類別格式:yyyymm為近月，999999 為所有月份									
                    {
                        //交易人類別格式： 0 為部位排序前五大或前十大交易人，1 為部位排序前五大或前十大交易人中，屬於特定法人者									
                        if (s[(int)CSVField.交易人類別] == "1")
                        {
                            dest.top5corp_buy_vol = parseInt(s, CSVField.前五大交易人買方);
                            dest.top5corp_sell_vol = parseInt(s, CSVField.前五大交易人賣方);
                            dest.top10corp_buy_vol = parseInt(s, CSVField.前十大交易人買方);
                            dest.top10corp_sell_vol = parseInt(s, CSVField.前十大交易人賣方);
                            rowPart = RowPart.AllCorp;
                        }
                        else
                        {
                            dest.top5_buy_vol = parseInt(s, CSVField.前五大交易人買方);
                            dest.top5_sell_vol = parseInt(s, CSVField.前五大交易人賣方);
                            dest.top10_buy_vol = parseInt(s, CSVField.前十大交易人買方);
                            dest.top10_sell_vol = parseInt(s, CSVField.前十大交易人賣方);
                            rowPart = RowPart.All;
                        }
                        dest.market_vol = parseInt(s, CSVField.全市場未沖銷部位數);
                    }
                    else
                    {
                        //交易人類別格式： 0 為部位排序前五大或前十大交易人，1 為部位排序前五大或前十大交易人中，屬於特定法人者									
                        if (s[(int)CSVField.交易人類別] == "1")
                        {
                            dest.near_top5corp_buy_vol = parseInt(s, CSVField.前五大交易人買方);
                            dest.near_top5corp_sell_vol = parseInt(s, CSVField.前五大交易人賣方);
                            dest.near_top10corp_buy_vol = parseInt(s, CSVField.前十大交易人買方);
                            dest.near_top10corp_sell_vol = parseInt(s, CSVField.前十大交易人賣方);
                            rowPart = RowPart.NearCorp;
                        }
                        else
                        {
                            dest.near_top5_buy_vol = parseInt(s, CSVField.前五大交易人買方);
                            dest.near_top5_sell_vol = parseInt(s, CSVField.前五大交易人賣方);
                            dest.near_top10_buy_vol = parseInt(s, CSVField.前十大交易人買方);
                            dest.near_top10_sell_vol = parseInt(s, CSVField.前十大交易人賣方);
                            rowPart = RowPart.Near;
                        }
                        dest.near_market_vol = parseInt(s, CSVField.全市場未沖銷部位數);
                    }

                    return rowPart;
                }

                static private RowBigTraderUnclosedVol FindRowByKey(IQueryable<RowBigTraderUnclosedVol> q,RowBigTraderUnclosedVol r)
                {
                    return Queryable.SingleOrDefault<RowBigTraderUnclosedVol>(q,g => g.contract_ID == r.contract_ID && g.trade_date == r.trade_date);
                }

                private RowBigTraderUnclosedVol FindRowByKey(RowBigTraderUnclosedVol r)
                {
                    return FindRowByKey(database.RowBigTraderUnclosedVols, r);
                    //database.RowBigTraderUnclosedVols.SingleOrDefault(g => g.contract_ID == r.contract_ID && g.trade_date == r.trade_date);
                }

                static bool Equals(RowBigTraderUnclosedVol a, RowBigTraderUnclosedVol b, RowPart rowPart)
                {
                    switch (rowPart)
                    {
                        case RowPart.All:
                            return
                        a.trade_date == b.trade_date &&
                        a.contract_ID == b.contract_ID &&
                         a.market_vol == b.market_vol &&

                        a.top5_buy_vol == b.top5_buy_vol &&
                        a.top5_sell_vol == b.top5_sell_vol &&
                        a.top10_buy_vol == b.top10_buy_vol &&
                        a.top10_sell_vol == b.top10_sell_vol;

                        case RowPart.AllCorp:
                            return
                        a.trade_date == b.trade_date &&
                        a.contract_ID == b.contract_ID &&
                         a.market_vol == b.market_vol &&

                        a.top5corp_buy_vol == b.top5corp_buy_vol &&
                        a.top5corp_sell_vol == b.top5corp_sell_vol &&
                        a.top10corp_buy_vol == b.top10corp_buy_vol &&
                        a.top10corp_sell_vol == b.top10corp_sell_vol;

                        case RowPart.Near:
                            return
                        a.trade_date == b.trade_date &&
                        a.contract_ID == b.contract_ID &&
                        a.market_vol == b.near_market_vol &&

                        a.near_top5_buy_vol == b.near_top5_buy_vol &&
                        a.near_top5_sell_vol == b.near_top5_sell_vol &&
                        a.near_top10_buy_vol == b.near_top10_buy_vol &&
                        a.near_top10_sell_vol == b.near_top10_sell_vol;

                        case RowPart.NearCorp:
                            return
                        a.trade_date == b.trade_date &&
                        a.contract_ID == b.contract_ID &&
                        a.market_vol == b.near_market_vol &&

                        a.near_top5corp_buy_vol == b.near_top5corp_buy_vol &&
                        a.near_top5corp_sell_vol == b.near_top5corp_sell_vol &&
                        a.near_top10corp_buy_vol == b.near_top10corp_buy_vol &&
                        a.near_top10corp_sell_vol == b.near_top10corp_sell_vol;

                        default:
                            TxtLog.err("compare unknown type");
                            return false;
                    }

                }


                static void Copy(RowBigTraderUnclosedVol dest, RowBigTraderUnclosedVol src, RowPart rowPart)
                {
                    switch (rowPart)
                    {
                        case RowPart.All:
                            dest.trade_date = src.trade_date;
                            dest.contract_ID = src.contract_ID;
                            dest.market_vol = src.market_vol;

                            dest.top5_buy_vol = src.top5_buy_vol;
                            dest.top5_sell_vol = src.top5_sell_vol;
                            dest.top10_buy_vol = src.top10_buy_vol;
                            dest.top10_sell_vol = src.top10_sell_vol;
                            break;

                        case RowPart.AllCorp:
                            dest.trade_date = src.trade_date;
                            dest.contract_ID = src.contract_ID;
                            dest.market_vol = src.market_vol;

                            dest.top5corp_buy_vol = src.top5corp_buy_vol;
                            dest.top5corp_sell_vol = src.top5corp_sell_vol;
                            dest.top10corp_buy_vol = src.top10corp_buy_vol;
                            dest.top10corp_sell_vol = src.top10corp_sell_vol;
                            break;

                        case RowPart.Near:
                            dest.trade_date = src.trade_date;
                            dest.contract_ID = src.contract_ID;
                            dest.market_vol = src.near_market_vol;

                            dest.near_top5_buy_vol = src.near_top5_buy_vol;
                            dest.near_top5_sell_vol = src.near_top5_sell_vol;
                            dest.near_top10_buy_vol = src.near_top10_buy_vol;
                            dest.near_top10_sell_vol = src.near_top10_sell_vol;
                            break;

                        case RowPart.NearCorp:
                            dest.trade_date = src.trade_date;
                            dest.contract_ID = src.contract_ID;
                            dest.market_vol = src.near_market_vol;

                            dest.near_top5corp_buy_vol = src.near_top5corp_buy_vol;
                            dest.near_top5corp_sell_vol = src.near_top5corp_sell_vol;
                            dest.near_top10corp_buy_vol = src.near_top10corp_buy_vol;
                            dest.near_top10corp_sell_vol = src.near_top10corp_sell_vol;
                            break;

                        default:
                            TxtLog.err("copy unknown type");
                            break;

                    }


                    /*dest.trade_date = src.trade_date;
                    dest.contract_ID = src.contract_ID;
                    dest.top5_buy_vol = src.top5_buy_vol;
                    dest.top5_sell_vol = src.top5_sell_vol;
                    dest.top10_buy_vol = src.top10_buy_vol;
                    dest.top10_sell_vol = src.top10_sell_vol;
                    dest.top5corp_buy_vol = src.top5corp_buy_vol;
                    dest.top5corp_sell_vol = src.top5corp_sell_vol;
                    dest.top10corp_buy_vol = src.top10corp_buy_vol;
                    dest.top10corp_sell_vol = src.top10corp_sell_vol;
                    dest.near_top5_buy_vol = src.near_top5_buy_vol;
                    dest.near_top5_sell_vol = src.near_top5_sell_vol;

                     *    dest.near_top10_buy_vol = src.near_top10_buy_vol;
                    dest.near_top10_sell_vol = src.near_top10_sell_vol;
                    dest.near_top5corp_buy_vol = src.near_top5corp_buy_vol;
                    dest.near_top5corp_sell_vol = src.near_top5corp_sell_vol;
                    dest.near_top10corp_buy_vol = src.near_top10corp_buy_vol;
                    dest.near_top10corp_sell_vol = src.near_top10corp_sell_vol;
                    dest.market_vol = src.market_vol;
                    dest.near_market_vol = src.near_market_vol;*/
                }

                void setEmpty(RowBigTraderUnclosedVol r)
                {
                    r.top10_buy_vol = EMPTY_INT;
                    r.top10corp_buy_vol = EMPTY_INT;
                    r.near_top10_buy_vol = EMPTY_INT;
                    r.near_top10corp_buy_vol = EMPTY_INT;
                }


                //bool IsEmpty(RowBigTraderUnclosedVol r, RowPart rowPart)
                bool IsEmpty(Object row, Enum rowPart)
                {
                    RowBigTraderUnclosedVol r = row as RowBigTraderUnclosedVol;
                    switch ((RowPart)rowPart)
                    {
                        case RowPart.All: return r.top10_buy_vol == EMPTY_INT;
                        case RowPart.AllCorp: return r.top10corp_buy_vol == EMPTY_INT;
                        case RowPart.Near: return r.near_top10_buy_vol == EMPTY_INT;
                        case RowPart.NearCorp: return r.near_top10corp_buy_vol == EMPTY_INT;

                        default:
                            TxtLog.err("IsEmpty checks unknown part");
                            return false;
                    }

                }

                bool IsComplete(RowBigTraderUnclosedVol r)
                {
                    foreach (RowPart e in Enum.GetValues(typeof(RowPart)))
                        if (e!=RowPart.Unknown && IsEmpty(r, e)) return false;
                    return true;
                }

                void Copy(RowBigTraderUnclosedVol a, RowBigTraderUnclosedVol b)
                {
                    foreach (RowPart e in Enum.GetValues(typeof(RowPart)))
                        if (e != RowPart.Unknown) Copy(a,b,e);
                }

                override public int ParseCSVFile(StreamReader csv)
                {

                    List<RowBigTraderUnclosedVol> buffer = new List<RowBigTraderUnclosedVol>();

                    int parsedLine = Helper.ParseCSVFile(csv, typeof(CSVField), @"月份類別格式",
                        new Helper.OnParse1CSVLine((string[] s) =>
                        {
                            RowBigTraderUnclosedVol r = new RowBigTraderUnclosedVol();
                            setEmpty(r);

                            RowPart rowPart = ParseRow(s, r);

                            var r0 = FindRowByKey(buffer.AsQueryable(), r);
                            if (r0 == null)
                            {
                                buffer.Add(r);
                            }
                            else
                                if (!IsEmpty(r0, rowPart))
                                    TxtLog.report("warning: ignore duplicate key data on:" + r.trade_date);
                                else
                                {
                                    Copy(r0, r, rowPart);
                                    if(IsComplete(r0))
                                    {
                                        var rDb=FindRowByKey(r0);
                                        if(rDb==null)
                                            database.RowBigTraderUnclosedVols.InsertOnSubmit(r0);
                                        else
                                        {
                                            if(!Equals(rDb,r0))
                                            {
                                                string ss = ""; foreach (string str in s) ss += "|" + str.ToString();
                                                TxtLog.showLog("warning: dtat base entity exists and not equas new one " + ss);
                                                Copy(rDb, r0);
                                            }
                                        }
                                        buffer.Remove(r0);
                                    }
                                }
                            return true;
                        }));

                    foreach (var t in buffer)
                            TxtLog.err("unCompleted entity:" + t.trade_date);

                    return parsedLine;
                }


                public int ParseCSVFileX(StreamReader csv)
                {
                    List<RowBigTraderUnclosedVol> table = new List<RowBigTraderUnclosedVol>();
                    //IQueryable<RowBigTraderUnclosedVol> qTable = table.AsQueryable();

                    //System.Collections.Generic.HashSet
                    int parsedLine= Helper.ParseCSVFile(csv, typeof(CSVField), @"月份類別格式",
                        new Helper.OnParse1CSVLine((string[] s) =>
                        {
                            RowBigTraderUnclosedVol r = new RowBigTraderUnclosedVol();
                            setEmpty(r);

                            RowPart rowPart = ParseRow(s, r);

                            var r0 = FindRowByKey(table.AsQueryable(),r);
                            if (r0 == null)
                            {
                                //this.database.RowBigTraderUnclosedVols.InsertOnSubmit(r);
                                table.Add(r);
                            }
                            else if (!Equals(r0, r, rowPart))
                            {
                                if (!IsEmpty(r0, rowPart))
                                {
                                    string ss = ""; foreach (string str in s) ss += "|" + str.ToString();
                                    TxtLog.showLog("warning: dtat base entity exists and not equas new one " + ss);
                                }
                                Copy(r0, r, rowPart);
                            }
                            return true;
                        }));

                    foreach (var t in table)
                    {
                        if (!IsComplete(t))
                            TxtLog.err("unCompleted entity:"+t.trade_date);
                    }

                    return parsedLine;
                }
                //====================================================================== 


            }

            class CorpX3Parser : CSVParser
            {
                public CorpX3Parser(DbImporter importer) : base(importer) { }
                //======================================================================

                //======================================================================
                //日期	商品名稱	身份別	多方交易口數	多方交易契約金額(千元)	空方交易口數	空方交易契約金額(千元)	多空交易口數淨額	多空交易契約金額淨額(千元)
                //多方未平倉口數	多方未平倉契約金額(千元)	空方未平倉口數	空方未平倉契約金額(千元)	多空未平倉口數淨額	多空未平倉契約金額淨額(千元)
                enum CSVField // must match all fields and order in csv file.
                {
                    日期 = 0, 商品名稱, 身份別, 多方交易口數, 多方交易契約金額k, 空方交易口數, 空方交易契約金額k, 多空交易口數淨額, 多空交易契約金額淨額k,
                    多方未平倉口數, 多方未平倉契約金額k, 空方未平倉口數, 空方未平倉契約金額k, 多空未平倉口數淨額, 多空未平倉契約金額淨額k
                };

                enum RowPart { Unknown = 0, Corp1, Corp2, Corp3 }

                private RowPart ParseRow(string[] s, RowCorpX3 dest)
                {
                    RowPart rowPart = RowPart.Unknown;

                    dest.trade_date = DateTime.Parse(s[(int)CSVField.日期]);
                    dest.contract_ID = mapContractID[s[(int)CSVField.商品名稱]];

                    string sCorp = s[(int)CSVField.身份別];
                    if (sCorp.IndexOf(@"外資") >= 0)
                    {
                        dest.c1_long_vol = parseInt(s, CSVField.多方交易口數);
                        dest.c1_long_moneyK = parseInt(s, CSVField.多方交易契約金額k);
                        dest.c1_short_vol = parseInt(s, CSVField.空方交易口數);
                        dest.c1_short_moneyK = parseInt(s, CSVField.空方交易契約金額k);
                        dest.c1_unclosed_long_vol = parseInt(s, CSVField.多方未平倉口數);
                        dest.c1_unclosed_long_moneyK = parseInt(s, CSVField.多方未平倉契約金額k);
                        dest.c1_unclosed_short_vol = parseInt(s, CSVField.空方未平倉口數);
                        dest.c1_unclosed_short_moneyK = parseInt(s, CSVField.空方未平倉契約金額k);

                        rowPart = RowPart.Corp1;

                    }
                    else if (sCorp.IndexOf(@"投信") >= 0)
                    {
                        dest.c2_long_vol = parseInt(s, CSVField.多方交易口數);
                        dest.c2_long_moneyK = parseInt(s, CSVField.多方交易契約金額k);
                        dest.c2_short_vol = parseInt(s, CSVField.空方交易口數);
                        dest.c2_short_moneyK = parseInt(s, CSVField.空方交易契約金額k);
                        dest.c2_unclosed_long_vol = parseInt(s, CSVField.多方未平倉口數);
                        dest.c2_unclosed_long_moneyK = parseInt(s, CSVField.多方未平倉契約金額k);
                        dest.c2_unclosed_short_vol = parseInt(s, CSVField.空方未平倉口數);
                        dest.c2_unclosed_short_moneyK = parseInt(s, CSVField.空方未平倉契約金額k);

                        rowPart = RowPart.Corp2;

                    }
                    else if (sCorp.IndexOf(@"自營商") >= 0)
                    {
                        dest.c1_long_vol = parseInt(s, CSVField.多方交易口數);
                        dest.c3_long_moneyK = parseInt(s, CSVField.多方交易契約金額k);
                        dest.c3_short_vol = parseInt(s, CSVField.空方交易口數);
                        dest.c3_short_moneyK = parseInt(s, CSVField.空方交易契約金額k);
                        dest.c3_unclosed_long_vol = parseInt(s, CSVField.多方未平倉口數);
                        dest.c3_unclosed_long_moneyK = parseInt(s, CSVField.多方未平倉契約金額k);
                        dest.c3_unclosed_short_vol = parseInt(s, CSVField.空方未平倉口數);
                        dest.c3_unclosed_short_moneyK = parseInt(s, CSVField.空方未平倉契約金額k);

                        rowPart = RowPart.Corp3;

                    }

                    return rowPart;
                }

                private RowCorpX3 FindRowByKey(RowCorpX3 r)
                {
                    return database.RowCorpX3s.SingleOrDefault(g => g.contract_ID == r.contract_ID && g.trade_date == r.trade_date);
                }

                static bool Equals(RowCorpX3 a, RowCorpX3 b, RowPart rowPart)
                {
                    switch (rowPart)
                    {
                        case RowPart.Corp1:
                            return
                        a.trade_date == b.trade_date &&
                        a.contract_ID == b.contract_ID &&

                        a.c1_long_vol == b.c1_long_vol &&
                        a.c1_long_moneyK == b.c1_long_moneyK &&
                        a.c1_short_vol == b.c1_short_vol &&
                        a.c1_short_moneyK == b.c1_short_moneyK &&
                        a.c1_unclosed_long_vol == b.c1_unclosed_long_vol &&
                        a.c1_unclosed_long_moneyK == b.c1_unclosed_long_moneyK &&
                        a.c1_unclosed_short_vol == b.c1_unclosed_short_vol &&
                        a.c1_unclosed_short_moneyK == b.c1_unclosed_short_moneyK;

                        case RowPart.Corp2:
                            return
                        a.trade_date == b.trade_date &&
                        a.contract_ID == b.contract_ID &&

                        a.c2_long_vol == b.c2_long_vol &&
                        a.c2_long_moneyK == b.c2_long_moneyK &&
                        a.c2_short_vol == b.c2_short_vol &&
                        a.c2_short_moneyK == b.c2_short_moneyK &&
                        a.c2_unclosed_long_vol == b.c2_unclosed_long_vol &&
                        a.c2_unclosed_long_moneyK == b.c2_unclosed_long_moneyK &&
                        a.c2_unclosed_short_vol == b.c2_unclosed_short_vol &&
                        a.c2_unclosed_short_moneyK == b.c2_unclosed_short_moneyK;

                        case RowPart.Corp3:
                            return
                        a.trade_date == b.trade_date &&
                        a.contract_ID == b.contract_ID &&

                        a.c3_long_vol == b.c3_long_vol &&
                        a.c3_long_moneyK == b.c3_long_moneyK &&
                        a.c3_short_vol == b.c3_short_vol &&
                        a.c3_short_moneyK == b.c3_short_moneyK &&
                        a.c3_unclosed_long_vol == b.c3_unclosed_long_vol &&
                        a.c3_unclosed_long_moneyK == b.c3_unclosed_long_moneyK &&
                        a.c3_unclosed_short_vol == b.c3_unclosed_short_vol &&
                        a.c3_unclosed_short_moneyK == b.c3_unclosed_short_moneyK;

                        default:
                            TxtLog.err("compare unknown type");
                            return false;
                    }

                }


                static void Copy(RowCorpX3 a, RowCorpX3 b, RowPart rowPart)
                {

                    switch (rowPart)
                    {
                        case RowPart.Corp1:
                            a.trade_date = b.trade_date;
                            a.contract_ID = b.contract_ID;

                            a.c1_long_vol = b.c1_long_vol;
                            a.c1_long_moneyK = b.c1_long_moneyK;
                            a.c1_short_vol = b.c1_short_vol;
                            a.c1_short_moneyK = b.c1_short_moneyK;
                            a.c1_unclosed_long_vol = b.c1_unclosed_long_vol;
                            a.c1_unclosed_long_moneyK = b.c1_unclosed_long_moneyK;
                            a.c1_unclosed_short_vol = b.c1_unclosed_short_vol;
                            a.c1_unclosed_short_moneyK = b.c1_unclosed_short_moneyK;
                            break;

                        case RowPart.Corp2:
                            a.trade_date = b.trade_date;
                            a.contract_ID = b.contract_ID;

                            a.c2_long_vol = b.c2_long_vol;
                            a.c2_long_moneyK = b.c2_long_moneyK;
                            a.c2_short_vol = b.c2_short_vol;
                            a.c2_short_moneyK = b.c2_short_moneyK;
                            a.c2_unclosed_long_vol = b.c2_unclosed_long_vol;
                            a.c2_unclosed_long_moneyK = b.c2_unclosed_long_moneyK;
                            a.c2_unclosed_short_vol = b.c2_unclosed_short_vol;
                            a.c2_unclosed_short_moneyK = b.c2_unclosed_short_moneyK;
                            break;


                        case RowPart.Corp3:
                            a.trade_date = b.trade_date;
                            a.contract_ID = b.contract_ID;

                            a.c3_long_vol = b.c3_long_vol;
                            a.c3_long_moneyK = b.c3_long_moneyK;
                            a.c3_short_vol = b.c3_short_vol;
                            a.c3_short_moneyK = b.c3_short_moneyK;
                            a.c3_unclosed_long_vol = b.c3_unclosed_long_vol;
                            a.c3_unclosed_long_moneyK = b.c3_unclosed_long_moneyK;
                            a.c3_unclosed_short_vol = b.c3_unclosed_short_vol;
                            a.c3_unclosed_short_moneyK = b.c3_unclosed_short_moneyK;
                            break;

                        default:
                            TxtLog.err("copy unknown type");
                            break;
                    }

                }


                void setEmpty(RowCorpX3 r)
                {
                    r.c1_long_vol = EMPTY_INT;
                    r.c2_long_vol = EMPTY_INT;
                    r.c3_long_vol = EMPTY_INT;
                }

                bool IsEmpty(RowCorpX3 r, RowPart rowPart)
                {
                    switch (rowPart)
                    {
                        case RowPart.Corp1: return r.c1_long_vol == EMPTY_INT;
                        case RowPart.Corp2: return r.c2_long_vol == EMPTY_INT;
                        case RowPart.Corp3: return r.c3_long_vol == EMPTY_INT;

                        default:
                            TxtLog.err("IsEmpty checks unknown part");
                            return false;
                    }

                }


                override public int ParseCSVFile(StreamReader csv)
                {
                    return Helper.ParseCSVFile(csv, typeof(CSVField), null,
                        new Helper.OnParse1CSVLine((string[] s) =>
                        {
                            RowCorpX3 r = new RowCorpX3();
                            setEmpty(r);

                            RowPart rowPart = ParseRow(s, r);

                            var r0 = FindRowByKey(r);
                            if (r0 == null)
                                this.database.RowCorpX3s.InsertOnSubmit(r);
                            else if (!Equals(r0, r, rowPart))
                            {
                                if (!IsEmpty(r0, rowPart))
                                {
                                    string ss = ""; foreach (string str in s) ss += "|" + str.ToString();
                                    TxtLog.showLog("warning: dtat base entity exists and not equas new one " + ss);
                                }
                                Copy(r0, r, rowPart);
                            }
                            return true;
                        }));
                }
                //====================================================================== 
            }

#endif

            CSVParser parser;

            public void requestStop()
            {
                if (parser != null)
                    parser.bRun = false;
            }

            public bool IsBusy()
            {
                return (parser != null && parser.bRun == true);
            }


            Thread thread;

            public void ImportCSV(string fileName, Action taskComplete) // Async call
            {
                if(thread != null && thread.IsAlive) return;

                if (!File.Exists(fileName))
                {
                    if (taskComplete != null) taskComplete();
                    return;
                }

                    thread = new Thread(() =>
                    {
                        try
                        {
                            ImportCSV(fileName);
                        }
                        catch (Exception e)
                        {
                            TxtLog.report(e.ToString());
                        }
                        thread = null;
                        if (taskComplete != null) taskComplete();
                    });

                    thread.Start();
            }

            public int ImportCSV(string fileName)
            {
                if (parser != null) return -1;
                TxtLog.report(fileName);
                /*
                int? ni = 0;
                int? nni = null;
                bool b=ni==nni;  // b=false

                var a1 = new RowContract_ID();
                var a2 = new RowContract_ID();
                a1.ID = 1; a1.Name = "a";
                a2.ID = 1; a2.Name = "a";
                b = a1 == a2; // b=false
                 */

                int totalParsedLine = 0;

                //try{
                using (StreamReader csv = new StreamReader(fileName, Encoding.GetEncoding(950)))
                {
                    string sHeader;
                    while ((sHeader = csv.ReadLine()) != null && sHeader.Trim() == "") ; //find the fields header that is the first non-empty line.

                    // identify the file type and select correspond reader.
                    if (sHeader.IndexOf(@"開盤價") >= 0)
                    { // tradeInf
                        parser = new TradeInfParser(this);
                        totalParsedLine = parser.ParseCSVFile(csv);
                    }
                    else if (sHeader.IndexOf(@"前五大交易人") >= 0)
                    { //BigTrader
                        parser = new BigTraderParser(this);
                        totalParsedLine = parser.ParseCSVFile(csv);
                    }
                    else if (sHeader.IndexOf(@"多方交易口數") >= 0)
                    { //CorpX3
                        parser = new CorpX3Parser(this);
                        totalParsedLine = parser.ParseCSVFile(csv);
                    }
                    else
                    {
                        TxtLog.report("unknown file header : " + fileName);
                        totalParsedLine = -1;
                    }

                }// end useing
                //}catch (Exception e){TxtLog.report(e.ToString());}

                if (totalParsedLine > 0)
                {
                    //parser = null;
                    TxtLog.report("parse OK: line=" + totalParsedLine);
                }

                parser = null;
                return totalParsedLine;
            }

#if(false)
            [Serializable]
            public class CombinedRawData : System.IComparable//<CombinedRawData>
            {
                //public int CompareTo(CombinedRawData y) { return TradeDate.CompareTo(y.TradeDate); }
                public int CompareTo(Object y) { return TradeDate.CompareTo((y as CombinedRawData).TradeDate); }

                public CombinedRawData(DateTime date) { TradeDate = date; }
                public CombinedRawData() { }

                public UInt16 procFlag;
                public DateTime TradeDate;

                public int payMonth;
                //public double
                // maxVPriceOpen
                //,maxVPriceClose
                //,maxVolume
                //;

                #region  TradeInf

                public double
                    priceOpening
                    , priceHigh
                    , priceLow
                    , priceClosing
                    , priceClearing
                    , volume
                    , unclosedVolume
                    //, priceTrade
                    ;
                #endregion

                #region CorpsX3
                public double
                    c1LongVol
                    , c1LongMoneyK
                    , c1ShortVol
                    , c1ShortMoneyK
                    , c1UnClosedLongVol
                    , c1UnClosedLongMoneyK
                    , c1UnClosedShortVol
                    , c1UnClosedShortMoneyK

                    , c2LongVol
                    , c2LongMoneyK
                    , c2ShortVol
                    , c2ShortMoneyK
                    , c2UnClosedLongVol
                    , c2UnClosedLongMoneyK
                    , c2UnClosedShortVol
                    , c2UnClosedShortMoneyK

                    , c3LongVol
                    , c3LongMoneyK
                    , c3ShortVol
                    , c3ShortMoneyK
                    , c3UnClosedLongVol
                    , c3UnClosedLongMoneyK
                    , c3UnClosedShortVol
                    , c3UnClosedShortMoneyK
                    ;
                #endregion

                #region BigTrader
                public double
                    ucTotal
                    , ucBig5Buy
                    , ucBig5Sell
                    , ucBig10Buy
                    , ucBig10Sell
                    , ucBig5CorpBuy
                    , ucBig5CorpSell
                    , ucBig10CorpBuy
                    , ucBig10CorpSell
                    ;
                #endregion
            }
#endif
          

        }



    }
}
