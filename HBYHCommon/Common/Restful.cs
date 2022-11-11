using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;
using System.Security.Cryptography;

namespace Genersoft.GS.HBYHQYSCommon
{
    public class Restful
    {
        static JavaScriptSerializer jss = new JavaScriptSerializer();
        
        /*
         * 
         */
        public static QYS_RtnMessege Post(string PostURL, string PostData)
        {
            QYS_RtnMessege cResult;
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(PostURL);
                request.ContentType = "application/json";
                request.Method = "POST";
                request.Headers.Add("x-qys-accesstoken",ServerAddr.QYS_ACCESSTOKEN);
                request.Headers.Add("x-qys-timestamp",Gettimestamp());
                request.Headers.Add("x-qys-signature",GetSignature());
                Stream reqStream = null;
                byte[] postData = Encoding.UTF8.GetBytes(PostData);
                reqStream = request.GetRequestStream();
                //写入流
                reqStream.Write(postData, 0, postData.Length);
                //获取响应，即发送请求
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                string responseHtml = streamReader.ReadToEnd();
                cResult = jss.Deserialize<QYS_RtnMessege>(responseHtml);
            }
            catch (Exception ex)
            {
                cResult = new QYS_RtnMessege();
                cResult.code = "-1";
                cResult.message = "发生错误：" + ex.Message;
            }
            return cResult;
        }
        
        public static QYS_RtnMessege Post_document(string PostURL, string PostData)
        {
            QYS_RtnMessege cResult;
            try
            {
                HttpClient client = new HttpClient();
                //添加契约锁要求的headers
                //由于创建合同用的Content-type为multipart/form-data，所以要定义一个MultipartFormDataContent来放入文件
                MultipartFormDataContent formContent = new MultipartFormDataContent();
                formContent.Add(new StringContent("123"), "title");
                formContent.Add(new StringContent("123"), "fileType");
                var result = client.PostAsync(PostURL, formContent).Result.ToString();
                cResult = jss.Deserialize<QYS_RtnMessege>(result);
            }
            catch (Exception ex)
            {
                cResult = new QYS_RtnMessege();
                cResult.code = "-1";
                cResult.message = "发生错误：" + ex.Message;
            }
            return cResult;
        }

        public struct ServerAddr
        {
            /// <summary>
            /// 测试地址
            /// </summary>
            public const string TEST_SERVER_IP = @"10.138.6.65";
            /// <summary>
            /// 正式地址
            /// </summary>
            //public const string OUTTER_SERVER_IP = @"61.136.145.239";

            public const string QYS_ACCESSTOKEN = "8Dqny0wg82";
            public const string QYS_SECRETKEY = "fm3gNdPjD1kjPbBDVTjlRBNGWMM54o";
        }

        public struct RestAddr
        {
            /// <summary>
            /// 契约锁文档创建接口地址-测试地址
            /// </summary>
            public const string CREATEBYFILE_ADDR_TEST = @"http://" + ServerAddr.TEST_SERVER_IP + ":9182/v2/document/createbyfile";
            /// <summary>
            /// 物料同步接口地址
            /// </summary>
            public const string MATERIAL_RESTFUL_ADDR = @"http://" + ServerAddr.TEST_SERVER_IP + ":8094/yhApi/erp/material/saveOneMaterial";
            

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
        
        public static string Gettimestamp()
        {
            long timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds(); // 相差秒数
            return timeStamp.ToString();
        }

        /**
         * MD5加密
         */
        public static string GetMD5(string str)
        {
            //创建MD5对象
            MD5 md5 = MD5.Create();
            //将str转换为数组
            Byte[] buffer = Encoding.Default.GetBytes(str);
            //将数组转为加密数组
            Byte[] MD5buffer = md5.ComputeHash(buffer);
            //加密数组转为字符串
            string MD5Str = Encoding.Default.GetString(MD5buffer);
            return MD5Str;
        }
    }

}