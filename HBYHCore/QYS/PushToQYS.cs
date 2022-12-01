using System;
using System.Web.Script.Serialization;
using Genersoft.Platform.Core.Error;

namespace Genersoft.GS.HBYHQYSCore.QYS
{
    public class PushToQYS
    {
        private static JavaScriptSerializer jss = new JavaScriptSerializer();

        public void createbyfile(string billcode, string auditstate)
        {
            try
            {
                if (auditstate == "1")
                {
                    DocumentInfo documentInfo = DocumentInfo.GetDocumentInfo(billcode);
                    if (documentInfo != null)
                    {
                        string jsona = HBYHQYSCommon.Restful.Post_document(
                            HBYHQYSCommon.Restful.RestAddr.CREATEBYFILE_ADDR_TEST,
                            documentInfo.filename, documentInfo.filetype, documentInfo.djnm);
                        HBYHCWCommon.CommonMgr.WriteLogFile("状态：" + jsona);
                    }
                    else
                    {
                        throw new GSPException("推送结果：", ErrorLevel.Info);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("发生错误：" + e.Message);
                throw;
            }
        }
    }
}