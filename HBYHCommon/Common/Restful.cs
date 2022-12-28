using System;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace Genersoft.GS.HBYHQYSCommon
{
    public class Restful
    {
        public static string Post(string PostURL, string PostData)
        {
            HttpWebRequest request = null;
            Stream reqStream = null;

            try
            {
                request = (HttpWebRequest) HttpWebRequest.Create(PostURL);
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
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                string responseHtml = streamReader.ReadToEnd();
                HBYHCWCommon.CommonMgr.WriteLogFile("所写内容：" + responseHtml);
                return responseHtml;
                response.Close();
            }
            catch (Exception ex)
            {
                HBYHCWCommon.CommonMgr.WriteLogFile("发生错误：错误原因" + ex.Message);
                throw ex;
            }

            return null;
        }


        public static string Post_document(string PostURL, string filename, string filetype,
            string djnm)
        {
            HttpWebRequest request = null;
            Stream stream = null;
            try
            {
                request = (HttpWebRequest) WebRequest.Create(PostURL);
                request.Method = "POST";
                request.Accept = "text/plain,application/json";
                request.UserAgent = "privateapp-csharp-api-client";

                request.Headers.Add("x-qys-accesstoken", ServerAddr.QYS_ACCESSTOKEN);
                request.Headers.Add("x-qys-timestamp", Gettimestamp());
                request.Headers.Add("x-qys-signature", GetSignature());
                request.ContentType = "multipart/form-data; boundary=" + ServerAddr.Boundary;
                request.SendChunked = true;
                stream = request.GetRequestStream();

                var path = $@"\\Ser158\ht_fj\{djnm}\{filename}.{filetype}";
                WriteMultipart(ref stream, "file", filename, $"application/{filetype}", new FileInfo(path));
                WriteMultipart(ref stream, "fileType", filetype);
                WriteMultipart(ref stream, "title", filename);

                Byte[] data = Encoding.UTF8.GetBytes(new StringBuilder().Append(ServerAddr.Dash)
                    .Append(ServerAddr.Boundary).Append(ServerAddr.Dash)
                    .Append(ServerAddr.Newline).ToString());
                stream.Write(data, 0, data.Length);
                stream.Flush();

                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                string responseHtml = streamReader.ReadToEnd();
                HBYHCWCommon.CommonMgr.WriteLogFile("所写内容：" + responseHtml);
                return responseHtml;
                response.Close();
            }
            catch (Exception e)
            {
                HBYHCWCommon.CommonMgr.WriteLogFile("发生错误：错误原因" + e.Message);
                throw e;
            }

            return null;
        }


        public struct ServerAddr
        {
            public const string Dash = "--";
            public const string Newline = "\r\n";
            public const string TEST_SERVER_IP = @"10.138.6.65";
            public const string QYS_ACCESSTOKEN = "8Dqny0wg82";
            public const string QYS_SECRETKEY = "fm3gNdPjD1kjPbBDVTjlRBNGWMM54o";
            public static string Boundary = GenerateBoundary();
        }

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
    }
}