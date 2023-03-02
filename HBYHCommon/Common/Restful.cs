using System;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Genersoft.GS.HBYHQYSCommon
{
    public class Restful
    {
        /// <summary>
        /// 网页端POST请求方法
        /// </summary>
        /// <param name="PostURL">请求的URL地址</param>
        /// <param name="PostData">发送请求的postdata</param>
        /// <returns>返回QYS返回的json数据</returns>
        /// <exception cref="Exception"></exception>
        public static string Post(string PostURL, string PostData)
        {
            HttpWebRequest request = null;
            Stream reqStream = null;

            try
            {
                request = (HttpWebRequest)HttpWebRequest.Create(PostURL);
                request.Method = "POST";
                request.Accept = "text/plain,application/json";
                request.UserAgent = "privateapp-csharp-api-client";
                request.ContentType = "application/json";

                request.Headers.Add("x-qys-accesstoken", ServerAddr.QYS_ACCESSTOKEN);
                request.Headers.Add("x-qys-timestamp", Gettimestamp());
                request.Headers.Add("x-qys-signature", GetSignature());
                request.SendChunked = true;

                byte[] postData = Encoding.UTF8.GetBytes(PostData);
                reqStream = request.GetRequestStream();
                //写入流
                reqStream.Write(postData, 0, postData.Length);
                //获取响应，即发送请求
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                string responseHtml = streamReader.ReadToEnd();
                HBYHCWCommon.CommonMgr.WriteLogFile("所写内容：" + responseHtml);
                return responseHtml;
            }
            catch (Exception ex)
            {
                HBYHCWCommon.CommonMgr.WriteLogFile("发生错误：错误原因" + ex.Message);
                throw ex;
            }
        }


        /// <summary>
        /// 创建文件的POST请求方法，用来调用创建合同附件的接口
        /// </summary>
        /// <param name="PostURL">发送请求的POST地址</param>
        /// <param name="filename">需要上传的文件名</param>
        /// <param name="filetype">需要上传的文件拓展名</param>
        /// <param name="djnm">浪潮单据内码</param>
        /// <returns>返回契约锁的结果JSON字符串</returns>
        /// <exception cref="Exception"></exception>
        public static string Post_document(string PostURL, string filename, string filetype,
            string djnm)
        {
            HttpWebRequest request = null;
            Stream stream = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(PostURL);
                request.Method = "POST";
                request.Accept = "text/plain,application/json";
                request.UserAgent = "privateapp-csharp-api-client";

                request.Headers.Add("x-qys-accesstoken", ServerAddr.QYS_ACCESSTOKEN);
                request.Headers.Add("x-qys-timestamp", Gettimestamp());
                request.Headers.Add("x-qys-signature", GetSignature());
                request.ContentType = "multipart/form-data; boundary=" + ServerAddr.Boundary;
                request.SendChunked = true;
                stream = request.GetRequestStream();

                //路径为浪潮附加158ftp文件服务器的地址
                var path = $@"ftp://10.138.6.158:21/HTFiles/{djnm}/{filename}.{filetype}";
                //WriteMultipart(ref stream, "file", filename, $"application/{filetype}", new FileInfo(path));
                WriteMultipart(ref stream, "file", filename, $"application/{filetype}", path);
                WriteMultipart(ref stream, "fileType", filetype);
                WriteMultipart(ref stream, "title", filename);

                Byte[] data = Encoding.UTF8.GetBytes(new StringBuilder().Append(ServerAddr.Dash)
                    .Append(ServerAddr.Boundary).Append(ServerAddr.Dash)
                    .Append(ServerAddr.Newline).ToString());
                stream.Write(data, 0, data.Length);
                stream.Flush();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                string responseHtml = streamReader.ReadToEnd();
                HBYHCWCommon.CommonMgr.WriteLogFile("返回结果：" + responseHtml);
                return responseHtml;
            }
            catch (Exception e)
            {
                HBYHCWCommon.CommonMgr.WriteLogFile("发生错误：错误原因" + e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 获取文件下载链接的GET方法
        /// </summary>
        /// <param name="contractId">契约锁的contractid</param>
        /// <param name="htnm">浪潮的合同内码字段</param>
        /// <returns></returns>
        public static string Get_document(string contractId, string htnm)
        {
            HttpWebRequest request = null;
           
            try
            {
                StringBuilder urlBuilder = new StringBuilder(NormalRestAddr.CONTRACT_DOWNLOAD_ADDR_TEST);
                urlBuilder.Append("?");
                urlBuilder.Append("contractId").Append("=").Append(UrlEncodeUTF8(contractId));
                urlBuilder.Append("&");
                urlBuilder.Append("downloadItems").Append("=").Append(UrlEncodeUTF8("NORMAL"));
                urlBuilder.Append("&");
                urlBuilder.Append("needCompressForOneFile").Append("=").Append(UrlEncodeUTF8("true"));

                HBYHCWCommon.CommonMgr.WriteLogFile("调用下载合同URL地址" + urlBuilder.ToString());

                request = (HttpWebRequest)WebRequest.Create(urlBuilder.ToString());
                request.Method = "GET";
                request.Accept = "text/plain,application/json";
                request.UserAgent = "privateapp-csharp-api-client";
                request.Headers.Add("x-qys-accesstoken", ServerAddr.QYS_ACCESSTOKEN);
                request.Headers.Add("x-qys-timestamp", Gettimestamp());
                request.Headers.Add("x-qys-signature", GetSignature());

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();


                if ((int)response.StatusCode == 200)
                {
                    Stream inputStream = null;
                    try
                    {
                        string head = response.GetResponseHeader("Content-Disposition");
                        int headst = head.IndexOf("fileName=");
                        string filenames = head.Substring(headst + 9, head.Length - 20);
                        string decode_filename = HttpUtility.UrlDecode(filenames, Encoding.UTF8);

                        HBYHCWCommon.CommonMgr.WriteLogFile(decode_filename);
                        FtpWebRequest ftprequest =
                            (FtpWebRequest)WebRequest.Create(
                                $@"ftp://10.138.6.158:21/HTFiles/{htnm}/{decode_filename}");
                        ftprequest.Credentials =
                            new NetworkCredential(ServerAddr.FTPUSER, ServerAddr.FTPPASSWORD); // 需要认证的填写用户名、密码
                        ftprequest.Method = WebRequestMethods.Ftp.UploadFile;
                        ftprequest.KeepAlive = true;
                        ftprequest.UseBinary = true;
                        ftprequest.UsePassive = true;
                        ftprequest.ContentLength = response.ContentLength;

                        byte[] buff = new byte[51200];
                        int contentLen;
                        Stream inputftpstrm = response.GetResponseStream();

                        try
                        {
                            Stream outputftpstrm = ftprequest.GetRequestStream();
                            while ((contentLen = inputftpstrm.Read(buff, 0, buff.Length)) > 0)
                            {
                                outputftpstrm.Write(buff, 0, contentLen);
                            }

                            outputftpstrm.Close();
                            inputftpstrm.Close();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }

                        return decode_filename;
                    }
                    catch (Exception e)
                    {
                        // 处理异常情况
                        HBYHCWCommon.CommonMgr.WriteLogFile("下载附件出错，出错原因" + e.Message);
                    }
                    finally
                    {
                        if (inputStream != null)
                        {
                            inputStream.Close();
                        }
                    }
                }
                else
                {
                    // 处理异常情况
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return null;
        }

        /// <summary>
        /// 获取外部法人单位的详细信息，返回名称，认证时间、认证情况等信息
        /// </summary>
        /// <returns></returns>
        public static string Get_Organization_Auth()
        {
            HttpWebRequest request = null;
            
            try
            {
                StringBuilder urlBuilder = new StringBuilder(NormalRestAddr.QUERY_ALL_COMPANY_Auth);
                urlBuilder.Append("?");
                urlBuilder.Append("tenantType").Append("=").Append(UrlEncodeUTF8("COMPANY"));

                HBYHCWCommon.CommonMgr.WriteLogFile("调用查询公司认证状态列表接口：" + urlBuilder.ToString());

                request = (HttpWebRequest)WebRequest.Create(urlBuilder.ToString());
                request.Method = "GET";
                request.Accept = "text/plain,application/json";
                request.UserAgent = "privateapp-csharp-api-client";
                request.Headers.Add("x-qys-accesstoken", ServerAddr.QYS_ACCESSTOKEN);
                request.Headers.Add("x-qys-timestamp", Gettimestamp());
                request.Headers.Add("x-qys-signature", GetSignature());

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if ((int)response.StatusCode == 200)
                {
                    Stream responseStream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    string responseHtml = streamReader.ReadToEnd();
                    return responseHtml;
                }
                else
                {
                    HBYHCWCommon.CommonMgr.WriteLogFile("状态码错误:" + response.StatusCode);
                }
            }
            catch (Exception e)
            {
                HBYHCWCommon.CommonMgr.WriteLogFile(e.Message);
                throw;
            }

            return "COMPANY_AUTH_QUERY_FAILED";
        }
        
        /// <summary>
        /// 接口配置信息
        /// </summary>
        public struct ServerAddr
        {
            public const string Dash = "--";
            public const string Newline = "\r\n";
            public const string TEST_SERVER_IP = @"10.138.6.65";
            public const string NORMAL_SERVER_IP = @"10.138.6.61";
            public const string QYS_ACCESSTOKEN = "9onu6TJeLE";
            public const string QYS_SECRETKEY = "vCUvelNy7f7QtxBTeF4JlAKqc0xJ3R";
            public static string Boundary = GenerateBoundary();
            public const string FTPUSER = "yhftp";
            public const string FTPPASSWORD = "shift@6457086";
        }

        /// <summary>
        /// 测试服各个接口调用地址
        /// </summary>
        public struct RestAddr
        {
            /// <summary>
            /// 契约锁文档创建接口地址-测试地址
            /// </summary>
            public const string ADD_CONTRACTFILE_ADDR_TEST =
                @"http://" + ServerAddr.TEST_SERVER_IP + ":9182/v2/document/createbyfile";

            public const string CREATE_BYCATEGORY_ADDR_TEST =
                @"http://" + ServerAddr.TEST_SERVER_IP + ":9182/contract/createbycategory";

            public const string ADD_SIGNATORY_DYNAMIC_TEST =
                @"http://" + ServerAddr.TEST_SERVER_IP + ":9182/contract/addsignatories";

            public const string CONTRACT_DOWNLOAD_ADDR_TEST =
                @"http://" + ServerAddr.TEST_SERVER_IP + ":9182/contract/download";

            public const string CONTRACT_SEND_ADDR_TEST =
                @"http://" + ServerAddr.TEST_SERVER_IP + ":9182/contract/send";

            public const string CONTRACT_RESEND_ADDR_TEST =
                @"http://" + ServerAddr.TEST_SERVER_IP + ":9182/contract/resend";

            public const string CONTRACT_RECALL_ADDR_TEST =
                @"http://" + ServerAddr.TEST_SERVER_IP + ":9182/contract/recall";

            public const string CONTRACT_DELETE_ADDR_TEST =
                @"http://" + ServerAddr.TEST_SERVER_IP + ":9182/contract/delete";
        }

        /// <summary>
        /// 正式服各个接口调用地址
        /// </summary>
        public struct NormalRestAddr
        {
            /// <summary>
            /// 契约锁文档创建接口地址-测试地址
            /// </summary>
            public const string ADD_CONTRACTFILE_ADDR_TEST =
                @"http://" + ServerAddr.NORMAL_SERVER_IP + ":9182/v2/document/createbyfile";

            public const string CREATE_BYCATEGORY_ADDR_TEST =
                @"http://" + ServerAddr.NORMAL_SERVER_IP + ":9182/contract/createbycategory";

            public const string ADD_SIGNATORY_DYNAMIC_TEST =
                @"http://" + ServerAddr.NORMAL_SERVER_IP + ":9182/contract/addsignatories";

            public const string CONTRACT_DOWNLOAD_ADDR_TEST =
                @"http://" + ServerAddr.NORMAL_SERVER_IP + ":9182/contract/download";

            public const string CONTRACT_SEND_ADDR_TEST =
                @"http://" + ServerAddr.NORMAL_SERVER_IP + ":9182/contract/send";

            public const string CONTRACT_RESEND_ADDR_TEST =
                @"http://" + ServerAddr.NORMAL_SERVER_IP + ":9182/contract/resend";

            public const string CONTRACT_RECALL_ADDR_TEST =
                @"http://" + ServerAddr.NORMAL_SERVER_IP + ":9182/contract/recall";

            public const string CONTRACT_DELETE_ADDR_TEST =
                @"http://" + ServerAddr.NORMAL_SERVER_IP + ":9182/contract/delete";

            public const string QUERY_ALL_COMPANY_Auth =
                @"http://" + ServerAddr.NORMAL_SERVER_IP + ":9182/company/list";
        }

        /// <summary>
        /// 生成时间戳，返回linux时间戳格式
        /// </summary>
        /// <returns></returns>
        public static string Gettimestamp()
        {
            long timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds(); // 相差秒数
            return timeStamp.ToString();
        }

        /**
         * MD5加密结果32位小写，yindp2022-11-25更新原先王老师MD5算法，已测试验证与契约锁MD5加密结果一致
         */
        public static string GetMD5(string ConvertString)
        {
            //创建MD5对象
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string t2 = BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(ConvertString)));
            t2 = t2.Replace("-", "");
            return t2.ToLower();
        }

        /**
         * 获取签名值
         */
        public static string GetSignature()
        {
            long timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds(); // 相差秒数
            string key = GetMD5(ServerAddr.QYS_ACCESSTOKEN + ServerAddr.QYS_SECRETKEY + timeStamp);
            return key;
        }

        public static string GenerateBoundary()
        {
            return "----" + Guid.NewGuid().ToString("N");
        }

        public static string UrlEncodeUTF8(string source)
        {
            return HttpUtility.UrlEncode(source, System.Text.Encoding.UTF8);
        }
        
        /// <summary>
        /// 读取文件流方法
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="name"></param>
        /// <param name="fileInfo"></param>
        public static void WriteMultipart(ref Stream stream, string name, FileInfo fileInfo)
        {
            string filename = fileInfo.Name;
            String encodedFilename = UrlEncodeUTF8(filename);

            StringBuilder header = new StringBuilder().Append(ServerAddr.Dash).Append(ServerAddr.Boundary)
                .Append(ServerAddr.Newline)
                .Append("Content-Disposition: form-data; name=").Append(name).Append("; filename=")
                .Append(encodedFilename).Append(ServerAddr.Newline)
                .Append("Content-Type:").Append("application/octet-stream").Append(ServerAddr.Newline)
                .Append(ServerAddr.Newline);
            Byte[] data = Encoding.UTF8.GetBytes(header.ToString());
            stream.Write(data, 0, data.Length);

            byte[] buffer = new byte[51200];

            FileStream fileStream = null;

            try
            {
                fileStream = fileInfo.OpenRead();

                int count;
                while ((count = fileStream.Read(buffer, 0, Convert.ToInt32(buffer.Length))) != -1)
                {
                    stream.Write(buffer, 0, count);
                }

                data = Encoding.UTF8.GetBytes(ServerAddr.Newline);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                // 异常处理
                HBYHCWCommon.CommonMgr.WriteLogFile("错误原因： " + e.Message);
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }

        public static void WriteMultipart(ref Stream stream, string name, string filename, string contentType,
            FileInfo fileInfo)
        {
            filename = string.IsNullOrWhiteSpace(filename) ? fileInfo.Name : filename;
            String encodedFilename = UrlEncodeUTF8(filename);

            StringBuilder header = new StringBuilder().Append(ServerAddr.Dash).Append(ServerAddr.Boundary)
                .Append(ServerAddr.Newline)
                .Append("Content-Disposition: form-data; name=").Append(name).Append("; filename=")
                .Append(encodedFilename).Append(ServerAddr.Newline)
                .Append("Content-Type:")
                .Append(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType)
                .Append(ServerAddr.Newline)
                .Append(ServerAddr.Newline);
            Byte[] data = Encoding.UTF8.GetBytes(header.ToString());
            stream.Write(data, 0, data.Length);

            byte[] buffer = new byte[51200];

            FileStream fileStream = null;

            try
            {
                fileStream = fileInfo.OpenRead();

                int count;
                while ((count = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, count);
                }


                data = Encoding.UTF8.GetBytes(ServerAddr.Newline);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                // 异常处理
                HBYHCWCommon.CommonMgr.WriteLogFile(e.ToString() + e.Data.ToString() + e.HResult.ToString() +
                                                    e.Message + e.Source);
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }

        public static void WriteMultipart(ref Stream stream, string name, string value)
        {
            StringBuilder content = new StringBuilder().Append(ServerAddr.Dash).Append(ServerAddr.Boundary)
                .Append(ServerAddr.Newline)
                .Append("Content-Disposition: form-data; name=").Append(name).Append(ServerAddr.Newline)
                .Append("Content-Type: text/plain; charset=utf-8").Append(ServerAddr.Newline)
                .Append(ServerAddr.Newline)
                .Append(value).Append(ServerAddr.Newline);

            Byte[] data = Encoding.UTF8.GetBytes(content.ToString());
            stream.Write(data, 0, data.Length);
        }

        public static void WriteMultipart(ref Stream stream, string name, string filename, string contentType,
            String ftpUrl)
        {
            String encodedFilename = UrlEncodeUTF8(filename);

            StringBuilder header = new StringBuilder().Append(ServerAddr.Dash).Append(ServerAddr.Boundary)
                .Append(ServerAddr.Newline)
                .Append("Content-Disposition: form-data; name=").Append(name).Append("; filename=")
                .Append(encodedFilename).Append(ServerAddr.Newline)
                .Append("Content-Type:")
                .Append(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType)
                .Append(ServerAddr.Newline)
                .Append(ServerAddr.Newline);
            Byte[] data = Encoding.UTF8.GetBytes(header.ToString());
            stream.Write(data, 0, data.Length);

            byte[] buffer = new byte[51200];

            FtpWebResponse ftpResponse = null;
            Stream ftpStream = null;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Credentials =
                    new NetworkCredential(ServerAddr.FTPUSER, ServerAddr.FTPPASSWORD); // 需要认证的填写用户名、密码
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.KeepAlive = true;
                request.UseBinary = true;
                request.UsePassive = true;
                request.Timeout = 3000;

                ftpResponse = (FtpWebResponse)request.GetResponse();
                ftpStream = ftpResponse.GetResponseStream();

                int count;
                while ((count = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, count);
                }

                data = Encoding.UTF8.GetBytes(ServerAddr.Newline);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                HBYHCWCommon.CommonMgr.WriteLogFile("读取附件错误，错误原因：" + e.Message);
            }
            finally
            {
                if (ftpStream != null)
                {
                    ftpStream.Close();
                }

                if (ftpResponse != null)
                {
                    ftpResponse.Close();
                }
            }
        }
    }
}