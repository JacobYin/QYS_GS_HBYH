using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Genersoft.GS.HBYHQYSCommon;
using Genersoft.Platform.Core.Error;

namespace Genersoft.GS.HBYHQYSCore.QYS
{
    public class HBYHQysInterfaceMgr
    {
        public void QysCreateByFile()
        {
            try
            {
                //lcfile = File.Create(@"D:\Temp\png001.png");

                // Restful.Post(Restful.RestAddr.CREATEBYFILE_ADDR_TEST, "");

                //string json = jss.Serialize(GI);
                //HBYHPMSCommon.CommonMgr.WriteLogFile("json:" + json);

                //Common.WMS_RtnMessege rtn = Common.Restful.Post(Common.Restful.RestAddr.GIREQBILLS_RESTFUL_ADDR, json);
                //HBYHPMSCommon.CommonMgr.WriteLogFile("json1:" + Common.Restful.RestAddr.GIREQBILLS_RESTFUL_ADDR);
                // HBYHPMSCommon.CommonMgr.WriteLogFile("rtn:" + rtn.msg);
                String key = Restful.GetSignature();

            }
            catch (Exception ex)
            {
                throw new GSPException("1发生错误:" + ex.Message, ErrorLevel.Error);
            }
        }
    }


}