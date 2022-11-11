using System;
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
                String key = Restful.GetSignature();
                Console.WriteLine("ceshi");
            }
            catch (Exception ex)
            {
                throw new GSPException("1发生错误:" + ex.Message, ErrorLevel.Error);
            }
        }
    }


}