using System;
using Genersoft.GS.HBYHQYSCore.QYS.impl;
using Genersoft.Platform.Core.Error;

namespace Genersoft.GS.HBYHQYSCore.QYS
{
    public class Push2QYS
    {
        public void CreateContractAndDocument_Company(string billcode, string categoryID, string GaiZhangName,
            string GaiZhangContact, string auditstate)
        {
            try
            {
                if (auditstate == "1")
                {
                    string result =
                        ConAndDocuImpl.SetDoubleContract_Company(billcode, categoryID, GaiZhangName, GaiZhangContact);
                    if (result == "SUCCESS")
                    {
                        HBYHCWCommon.CommonMgr.WriteLogFile("推送成功");
                    }
                    else
                    {
                        throw new GSPException("出错信息：" + ErrorLevel.Info);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("发生错误： " + e);
                throw;
            }
        }

        public void CreateContractAndDocument_Personal(string billcode, string categoryID, string GaiZhangName,
            string GaiZhangContact, string auditstate)
        {
            try
            {
                if (auditstate == "1")
                {
                    string result =
                        ConAndDocuImpl.SetDoubleContract_Personal(billcode, categoryID, GaiZhangName, GaiZhangContact);
                    if (result == "SUCCESS")
                    {
                        HBYHCWCommon.CommonMgr.WriteLogFile("推送成功");
                    }
                    else
                    {
                        throw new GSPException("出错信息：" + ErrorLevel.Info);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("发生错误： " + e);
                throw;
            }
        }

        public void CreateHRContractAndDocument_Company(string billcode, string categoryID, string GaiZhangName,
            string GaiZhangContact, string ProcName, string auditstate)
        {
            try
            {
                if (auditstate == "1")
                {
                    string result =
                        ConAndDocuImpl.SetDouble_Personal(billcode, categoryID, GaiZhangName, GaiZhangContact,
                            ProcName);
                    if (result == "SUCCESS")
                    {
                        HBYHCWCommon.CommonMgr.WriteLogFile("推送成功");
                    }
                    else
                    {
                        throw new GSPException("出错信息：" + ErrorLevel.Info);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("发生错误： " + e);
                throw;
            }
        }

        public void DownLoadAllDocument()
        {
            ConAndDocuImpl.Get_Document();
        }

        public void Synchronize_Comp_Auth()
        {
            ConAndDocuImpl.Sync_CompAuth();
        }
    }
}