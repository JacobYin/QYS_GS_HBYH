using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;

using Genersoft.GS.ZZY.OSSI.SCM.Com;
using Genersoft.Platform.AppFramework.Service;
using Genersoft.Platform.Core.DataAccess;
using Genersoft.Platform.Core.Error;

using Genersoft.GS.HBYHCWCommon;

namespace Genersoft.GS.HBYHCWCore
{
    public class HBYHInterfaceMgr
    {
        /// <summary>
        /// 生成单据
        /// </summary>
        /// <param name="classCode">类集编号，必传</param>
        /// <param name="configId">配置ID，可不传</param>
        /// <returns>接口平台返回的生成结果，json格式的字符串</returns>
        public string GenerateBill(string classCode, string configId)
        {
            string data = "";
            //调用接口平台生成标准单据
            string res = InterfaceMgr.OutSaveData(classCode, configId, data, "JSON", GSPContext.Current.Session.UserID);
            return res;
        }
       /// <summary>
       /// 用于做费用报销审批后构件
       /// </summary>
       /// <param name="billid">单据主键</param>
       /// <param name="spzt">审批状态</param>
        public void FYBXSPH(string billid, string spzt)
        {
            IGSPDatabase db = GSPContext.Current.Database;
            if (spzt == "2")//审批通过
            {
                string sql = "select BZJE from FYBXDKS where BXDNM={0}";
                IDbDataParameter[] param = new IDbDataParameter[1];
                param[0] = db.MakeInParam("BXDNM", GSPDbDataType.VarChar, 36, billid);
                DataSet ds = db.ExecuteDataSet(sql, param);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    string bzjeStr = ds.Tables[0].Rows[0]["BZJE"].ToString();
                    Decimal bzje = Convert.ToDecimal(bzjeStr);
                    if (bzje > 100)
                    {
                        throw new GSPException("报账金额" + bzjeStr + "超出标准限额100,不允许审批通过", ErrorLevel.Info);
                    }
                }

            }
        }
        public string PustData(string billID)
        {
            string resStr = string.Empty;
            string sql = "select BillID as \"BillID\",BillCode  as \"BillCode\",Creator as \"Creator\",Note as \"Note\" from FYBXDKS where BXDNM={0}";
            IGSPDatabase db = GSPContext.Current.Database;
            IDbDataParameter[] param = new IDbDataParameter[1];
            param[0] = db.MakeInParam("BXDNM", GSPDbDataType.VarChar, 36, billID);
            DataSet ds = db.ExecuteDataSet(sql, param);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                string url = "http://10.110.86.22:8088/fybxd/bill/generate";
                ds.Tables[0].TableName = "FYBXD";
                //dataset转json格式字符串
                string sendData =CommonMgr.DataSet2Json(ds);
                //构造接口入参，参数直接通过&连接
                string parm = "type=getmd5" + "&classsetcode=fybxd&format=json&param=" + sendData;
                Encoding Encode = Encoding.Default;
                string res=CommonMgr.GetJson(url, parm, Encode);
                //后面就可以解析res

            }
            else
            {
                resStr = "未找到符合条件的数据";
            }
            return resStr;

        }

    }
}