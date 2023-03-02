using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Web;
using System.Net;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Web.Services.Description;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Security.Cryptography;

namespace Genersoft.GS.HBYHCWCommon
{
    /// <summary>
    /// 通用类，客户、服务端都需要部署该dll
    /// </summary>
    public class CommonMgr
    {
        #region 格式转换

        #region Xml2Json

        public static string Xml2Json(string sxml)
        {
            DataSet ds = Xml2DataSet(sxml);
            string v = DataSet2Json(ds);
            return v;
        }

        #endregion Xml2Json

        #region DataSet2Json

        public static string DataSet2Json(DataSet ds)
        {
            var json = JsonConvert.SerializeObject(ds);
            return json;
        }

        #endregion DataSet2Json

        #region DataSet2XML

        public static string DataSet2XML(DataSet ds)
        {
            MemoryStream stream = null;
            XmlTextWriter writer = null;
            string r = "";
            try
            {
                stream = new MemoryStream();
                //从stream装载到XmlTextReader  
                writer = new XmlTextWriter(stream, Encoding.Unicode);

                //用WriteXml方法写入文件.  
                ds.WriteXml(writer);
                int count = (int)stream.Length;
                byte[] arr = new byte[count];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(arr, 0, count);

                UnicodeEncoding utf = new UnicodeEncoding();
                r = utf.GetString(arr).Trim();

                if (r.IndexOf("<NewDataSet>") >= 0)
                    r = r.Substring(r.IndexOf("<NewDataSet>"), r.Length - r.IndexOf("<NewDataSet>"));

                r = r.Replace("<NewDataSet>", "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><Data>")
                    .Replace("</NewDataSet>", "</Data>");
                r = r.Replace("<NewDataSet />", "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><Data/>");
                return r;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        }

        #endregion DataSet2XML

        #region Xml2DataSet

        public static DataSet Xml2DataSet(string sxml)
        {
            #region 处理xml，获得dataset

            StringReader stream = null;
            XmlTextReader reader = null;
            DataSet xmlds = new DataSet();
            try
            {
                //inXML = HttpUtility.UrlDecode(inXML, Encoding.UTF8);

                stream = new StringReader(sxml.Replace("\n", "").Replace("\r", ""));
                reader = new XmlTextReader(stream);
                xmlds.ReadXml(reader);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            #endregion

            return xmlds;
        }

        #endregion Xml2DataSet

        #region Json2DataSet

        public static DataSet Json2DataSet(string sjson)
        {
            //return (DataSet)JsonConvert.DeserializeObject(sjson, typeof(DataSet));
            DataSet ds = new DataSet();
            try
            {
                JObject jo = JObject.Parse(sjson);
                //遍历每一个属性是否是json数组，如果不是则反序列化为DataSet时会报错
                foreach (JProperty jp in jo.Properties())
                {
                    if (jo[jp.Name].GetType().Name != "JArray")
                    {
                        throw new Exception("该Json格式无法反序列化为数据集");
                    }
                }

                ds = JsonConvert.DeserializeObject<DataSet>(sjson);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ds;
        }

        #endregion Json2DataSet

        #endregion 格式转换

        #region ReturnStr

        /// <summary>
        /// 返回字符串
        /// </summary>
        /// <param name="state">0不通过1通过</param>
        /// <param name="mess">信息</param>
        /// <param name="type">XML;JSON</param>
        /// <returns></returns>
        public static string ReturnStrOLD(string state, string mess, string type)
        {
            string r = GetErrorStr(mess);
            switch (type)
            {
                case "XML":
                    r = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                        "<ResultInfo>" +
                        "<SourceBillCode></SourceBillCode>" +
                        "<BillID></BillID>" +
                        "<BillCode></BillCode>" +
                        "<State>" + state + "</State>" +
                        "<CreateDate>" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "</CreateDate>" +
                        "<FailReason>" + mess + "</FailReason>" +
                        "</ResultInfo>";
                    break;
                case "JSON":
                    r =
                        "{\"ResultInfo\": [{\"SourceBillCode\": \"\",\"BillID\": \"\",\"BillCode\": \"\",\"State\": \"" +
                        state + "\",\"CreateDate\": \"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                        "\",\"FailReason\": \"" + mess + "\"}]}";
                    break;
                default:
                    r =
                        "{\"ResultInfo\": [{\"SourceBillCode\": \"\",\"BillID\": \"\",\"BillCode\": \"\",\"State\": \"" +
                        state + "\",\"CreateDate\": \"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                        "\",\"FailReason\": \"" + mess + "\"}]}";
                    break;
            }

            return r;
        }

        /// <summary>
        /// 返回字符串
        /// </summary>
        /// <param name="sourcebillcode">原始单据号</param>
        /// <param name="billid">生成单据内码</param>
        /// <param name="billcode">生成单据编号</param>
        /// <param name="state">状态0失败1成功</param>
        /// <param name="mess">错误信息</param>
        /// <param name="type">XML;JSON</param>
        /// <returns></returns>
        public static string ReturnStrOLD(string sourcebillcode, string billid, string billcode, string state,
            string mess, string type)
        {
            string r = mess;
            switch (type)
            {
                case "XML":
                    r = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                        "<ResultInfo>" +
                        "<SourceBillCode>" + sourcebillcode + "</SourceBillCode>" +
                        "<BillID>" + billid + "</BillID>" +
                        "<BillCode>" + billcode + "</BillCode>" +
                        "<State>" + state + "</State>" +
                        "<CreateDate>" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "</CreateDate>" +
                        "<FailReason>" + mess + "</FailReason>" +
                        "</ResultInfo>";
                    break;
                case "JSON":
                    r = "{\"ResultInfo\": [{\"SourceBillCode\": \"" + sourcebillcode + "\",\"BillID\": \"" + billid +
                        "\",\"BillCode\": \"" + billcode + "\",\"State\": \"" + state + "\",\"CreateDate\": \"" +
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",\"FailReason\": \"" + mess + "\"}]}";
                    break;
                default:
                    r = "{\"ResultInfo\": [{\"SourceBillCode\": \"" + sourcebillcode + "\",\"BillID\": \"" + billid +
                        "\",\"BillCode\": \"" + billcode + "\",\"State\": \"" + state + "\",\"CreateDate\": \"" +
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",\"FailReason\": \"" + mess + "\"}]}";
                    break;
            }

            return r;
        }

        #endregion ReturnStr

        #region 构建返回结果

        public static string GetErrorStr(string M)
        {
            string R = M;
            int A = M.IndexOf("$$");
            int B = M.LastIndexOf("$$");
            if (A != B && A >= 0 && B >= 0)
                R = M.Substring(A + 2, B - A - 2);

            return R;
        }

        public static DataSet GetReturnDataSet()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            dt.TableName = "ResultInfo";
            dt.Columns.Add("PKID", typeof(string));
            dt.Columns.Add("BillID", typeof(string));
            dt.Columns.Add("BillCode", typeof(string));
            dt.Columns.Add("State", typeof(string));
            dt.Columns.Add("FailReason", typeof(string));
            dt.Columns.Add("BillItemID", typeof(string));
            dt.Columns["PKID"].DefaultValue = "";
            dt.Columns["BillID"].DefaultValue = "";
            dt.Columns["BillCode"].DefaultValue = "";
            dt.Columns["State"].DefaultValue = "0";
            dt.Columns["FailReason"].DefaultValue = "";
            dt.Columns["BillItemID"].DefaultValue = "";
            ds.Tables.Add(dt);

            return ds;
        }

        public static DataSet GetReturnDataSet(string pkid, string billid, string billcode, string state,
            string failreason)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            dt.TableName = "ResultInfo";
            dt.Columns.Add("PKID", typeof(string));
            dt.Columns.Add("BillID", typeof(string));
            dt.Columns.Add("BillCode", typeof(string));
            dt.Columns.Add("State", typeof(string));
            dt.Columns.Add("FailReason", typeof(string));
            dt.Columns.Add("BillItemID", typeof(string));
            DataRow dr = dt.NewRow();
            dr["PKID"] = pkid;
            dr["BillID"] = billid;
            dr["BillCode"] = billcode;
            dr["State"] = state;
            dr["FailReason"] = failreason;
            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            return ds;
        }

        public static string GetReturnDataString(string pkid, string billid, string billcode, string state,
            string failreason, string format)
        {
            DataSet ds = GetReturnDataSet(pkid, billid, billcode, state, failreason);
            string r = "";
            switch (format)
            {
                case "XML":
                    r = CommonMgr.DataSet2XML(ds);
                    break;
                case "JSON":
                    r = CommonMgr.DataSet2Json(ds);
                    break;
                default:
                    r = CommonMgr.DataSet2Json(ds);
                    break;
            }

            return GetErrorStr(r);
        }

        #region 返回字段增加分录ID字段

        public static DataSet GetReturnDataSet(string pkid, string billid, string billcode, string state,
            string failreason, string billitemid)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            dt.TableName = "ResultInfo";
            dt.Columns.Add("PKID", typeof(string));
            dt.Columns.Add("BillID", typeof(string));
            dt.Columns.Add("BillCode", typeof(string));
            dt.Columns.Add("State", typeof(string));
            dt.Columns.Add("FailReason", typeof(string));
            dt.Columns.Add("BillItemID", typeof(string));
            DataRow dr = dt.NewRow();
            dr["PKID"] = pkid;
            dr["BillID"] = billid;
            dr["BillCode"] = billcode;
            dr["State"] = state;
            dr["FailReason"] = failreason;
            dr["BillItemID"] = billitemid;
            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            return ds;
        }

        public static string GetReturnDataString(string pkid, string billid, string billcode, string state,
            string failreason, string format, string billitemid)
        {
            DataSet ds = GetReturnDataSet(pkid, billid, billcode, state, failreason, billitemid);
            string r = "";
            switch (format)
            {
                case "XML":
                    r = CommonMgr.DataSet2XML(ds);
                    break;
                case "JSON":
                    r = CommonMgr.DataSet2Json(ds);
                    break;
                default:
                    r = CommonMgr.DataSet2Json(ds);
                    break;
            }

            return GetErrorStr(r);
        }

        #endregion


        public static string GetReturnDataString(string state, string failreason, string format)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            dt.TableName = "ResultInfo";
            dt.Columns.Add("State", typeof(string));
            dt.Columns.Add("FailReason", typeof(string));
            dt.Columns.Add("DataList", typeof(string));

            DataRow dr = dt.NewRow();
            dr["State"] = state;
            dr["FailReason"] = failreason;
            dr["DataList"] = "@@@@";
            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            string r = "";
            switch (format)
            {
                case "XML":
                    r = CommonMgr.DataSet2XML(ds);
                    break;
                case "JSON":
                    r = CommonMgr.DataSet2Json(ds);
                    break;
                default:
                    r = CommonMgr.DataSet2Json(ds);
                    break;
            }

            return GetErrorStr(r);
        }

        #endregion 构建返回结果

        #region WriteLogFile

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="txt"></param>
        public static void WriteLogFile(string txt)
        {
            string vsPath = @"C:\HBYH\QYS\";
            if (!Directory.Exists(vsPath))
                Directory.CreateDirectory(vsPath);
            try
            {
                DateTime currentTime = System.DateTime.Now;
                vsPath = vsPath + "GS_InterFace_Log" + currentTime.ToString("yyyyMMdd") + ".txt";
                StreamWriter sw = new StreamWriter(vsPath, true, System.Text.Encoding.Default);
                sw.WriteLine(currentTime + "      :      " + txt + "\r\n");
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void WriteLogFile(string txt, string vsPath)
        {
            if (!Directory.Exists(vsPath))
                Directory.CreateDirectory(vsPath);
            try
            {
                DateTime currentTime = System.DateTime.Now;
                vsPath = vsPath + "GS_Log" + currentTime.ToString("yyyyMMdd") + ".txt";
                StreamWriter sw = new StreamWriter(vsPath, true, System.Text.Encoding.Default);
                sw.WriteLine(currentTime + "      :      " + txt + "\r\n");
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion WriteLogFile

        #region 动态调用webservices

        public static object InvokeWebService(string url, string methodname, object[] args)
        {
            return InvokeWebService(url, null, methodname, args);
        }

        public static object InvokeWebService(string url, string classname, string methodname, object[] args)
        {
            string @namespace = "EnterpriseServerBase.WebService.DynamicWebCalling";
            if (classname == null || classname == "")
            {
                classname = GetClassName(url);
            }

            try
            {
                //获取WSDL
                WebClient wc = new WebClient();
                Stream stream = wc.OpenRead(url + "?WSDL");
                ServiceDescription sd = ServiceDescription.Read(stream);
                ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
                sdi.AddServiceDescription(sd, "", "");
                CodeNamespace cn = new CodeNamespace(@namespace);

                //生成客户端代理类代码
                CodeCompileUnit ccu = new CodeCompileUnit();
                ccu.Namespaces.Add(cn);
                sdi.Import(cn, ccu);
                CSharpCodeProvider csc = new CSharpCodeProvider();
                //ICodeCompiler icc = csc.CreateCompiler();

                //设定编译参数
                CompilerParameters cplist = new CompilerParameters();
                cplist.GenerateExecutable = false;
                cplist.GenerateInMemory = true;
                cplist.ReferencedAssemblies.Add("System.dll");
                cplist.ReferencedAssemblies.Add("System.XML.dll");
                cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
                cplist.ReferencedAssemblies.Add("System.Data.dll");

                //编译代理类
                CompilerResults cr = csc.CompileAssemblyFromDom(cplist, ccu);
                if (true == cr.Errors.HasErrors)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (System.CodeDom.Compiler.CompilerError ce in cr.Errors)
                    {
                        sb.Append(ce.ToString());
                        sb.Append(System.Environment.NewLine);
                    }

                    throw new Exception(sb.ToString());
                }

                //生成代理实例，并调用方法
                System.Reflection.Assembly assembly = cr.CompiledAssembly;
                Type t = assembly.GetType(@namespace + "." + classname, true, true);
                object obj = Activator.CreateInstance(t);
                System.Reflection.MethodInfo mi = t.GetMethod(methodname);

                return mi.Invoke(obj, args);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message, new Exception(ex.InnerException.StackTrace));
            }
        }

        private static string GetClassName(string url)
        {
            string[] parts = url.Split('/');
            string[] pps = parts[parts.Length - 1].Split('.');
            return pps[0];
        }

        #endregion 动态调用webservices

        #region HTTP API接口返回

        public static string GetJson(string url, string parms, Encoding Encode)
        {
            HttpWebRequest objWebRequest = (HttpWebRequest)WebRequest.Create(url);

            objWebRequest.Method = "POST";
            objWebRequest.ContentType = "application/x-www-form-urlencoded";

            byte[] byteArray = Encode.GetBytes(parms);

            objWebRequest.ContentLength = byteArray.Length;
            Stream newStream = objWebRequest.GetRequestStream();
            newStream.Write(byteArray, 0, byteArray.Length); //写入参数 
            newStream.Close();
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)objWebRequest.GetResponse();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
            }

            StreamReader sr = new StreamReader(response.GetResponseStream(), Encode);
            return sr.ReadToEnd(); // 返回的数据
        }

        public static string GetUrlEncodeStr(string s, Encoding Encode)
        {
            return HttpUtility.UrlEncode(s, Encode);
        }

        public static string GetUrlDecodeStr(string s, Encoding Encode)
        {
            return HttpUtility.UrlDecode(s, Encode);
        }

        //decode后+号会被替换成空格，特殊处理一下
        public static string GetSpecialStr(string s)
        {
            return HttpUtility.UrlDecode(s);
        }

        #endregion

        #region GenerateStringID

        public static string GenerateStringID()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }

            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

        #endregion GenerateStringID

        #region 两表合并，得到新的合并结构

        /// <summary>
        /// 两表合并，得到新的合并结构
        /// </summary>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <param name="KeyColName">关联字段</param>
        /// <returns></returns>
        public static DataTable MergeDataTable(DataTable dt1, DataTable dt2)
        {
            //定义临时变量
            DataTable dtReturn = new DataTable();

            //构建表dtReturn结构
            for (int i = 0; i < dt1.Columns.Count; i++)
            {
                dtReturn.Columns.Add(dt1.Columns[i].ColumnName);
            }

            for (int j = 0; j < dt2.Columns.Count; j++)
            {
                if (!dtReturn.Columns.Contains(dt2.Columns[j].ColumnName))
                    dtReturn.Columns.Add(dt2.Columns[j].ColumnName);
            }

            return dtReturn;
        }

        #endregion 两表合并，得到新的合并结构

        /// <summary>
        /// 将excel导入到datatable
        /// </summary>
        /// <param name="filePath">excel路径</param>
        /// <param name="isColumnName">是否有表头</param>
        /// <param name="ColumnCount">表头行数</param>
        /// <returns>返回datatable</returns>
        public static DataTable ExcelToDataTable(string filePath, bool isColumnName, int ColumnCount)
        {
            DataTable dataTable = null;
            FileStream fs = null;
            DataColumn column = null;
            DataRow dataRow = null;
            IWorkbook workbook = null;
            ISheet sheet = null;
            IRow row = null;
            ICell cell = null;
            int startRow = 0;
            try
            {
                using (fs = File.OpenRead(filePath))
                {
                    // 2007版本
                    if (filePath.IndexOf(".xlsx") > 0)
                        workbook = new XSSFWorkbook(fs);
                    // 2003版本
                    else if (filePath.IndexOf(".xls") > 0)
                        workbook = new HSSFWorkbook(fs);

                    if (workbook != null)
                    {
                        sheet = workbook.GetSheetAt(0); //读取第一个sheet，当然也可以循环读取每个sheet
                        dataTable = new DataTable();
                        if (sheet != null)
                        {
                            int rowCount = sheet.LastRowNum; //总行数
                            if (rowCount > 0)
                            {
                                IRow firstRow = sheet.GetRow(0); //第一行
                                int cellCount = firstRow.LastCellNum; //列数

                                //构建datatable的列
                                if (isColumnName)
                                {
                                    startRow = ColumnCount; //有表头，从表头行数以下开始读取
                                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        cell = firstRow.GetCell(i);
                                        if (cell != null)
                                        {
                                            if (cell.StringCellValue != null)
                                            {
                                                column = new DataColumn(cell.StringCellValue);
                                                dataTable.Columns.Add(column);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        column = new DataColumn("column" + (i + 1));
                                        dataTable.Columns.Add(column);
                                    }
                                }

                                //填充行
                                for (int i = startRow; i <= rowCount; ++i)
                                {
                                    row = sheet.GetRow(i);
                                    if (row == null) continue;

                                    dataRow = dataTable.NewRow();
                                    for (int j = row.FirstCellNum; j < cellCount; ++j)
                                    {
                                        cell = row.GetCell(j);
                                        if (cell == null)
                                        {
                                            dataRow[j] = "";
                                        }
                                        else
                                        {
                                            //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)
                                            switch (cell.CellType)
                                            {
                                                case CellType.Blank:
                                                    dataRow[j] = "";
                                                    break;
                                                case CellType.Numeric:
                                                    short format = cell.CellStyle.DataFormat;
                                                    //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理
                                                    if (format == 14 || format == 31 || format == 57 || format == 58)
                                                        dataRow[j] = cell.DateCellValue;
                                                    else
                                                        dataRow[j] = cell.NumericCellValue;
                                                    break;
                                                case CellType.String:
                                                    dataRow[j] = cell.StringCellValue;
                                                    break;
                                            }
                                        }
                                    }

                                    dataTable.Rows.Add(dataRow);
                                }
                            }
                        }
                    }
                }

                return dataTable;
            }
            catch (Exception ex)
            {
                if (fs != null)
                {
                    fs.Close();
                }

                throw ex;
            }
        }
    }
}