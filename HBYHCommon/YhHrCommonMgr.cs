using Genersoft.Platform.AppFramework.Service;
using Genersoft.Platform.BizComponent.BasicLib;
using Genersoft.Platform.Core.DataAccess;
using Genersoft.Platform.Core.Error;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genersoft.GS.HBYHCWCommon
{
    public class YhHrCommonMgr : BaseBizComponent
	{
		/**
		 * 根据HR类型和HR类型对应编码，获取相应数据
		 */
		public string getAssetsListPr(string hrType, string hrCode)
		{
			IGSPDatabase db = GSPContext.Current.Database;
			//HBYHCWCommon.CommonMgr.WriteLogFile("根据资产编号获取资产信息，直接存储过程:（assetNo=" + assetNo+ ",userLogId=" + userLogId + ")");
			IDbDataParameter[] param = new IDbDataParameter[2];
			param[0] = db.MakeInParam("hrType", GSPDbDataType.VarChar, 36, hrType);
			param[1] = db.MakeInParam("hrCode", GSPDbDataType.VarChar, 36, hrCode);
			DataSet ds = null;
			try
			{
				ds = db.RunProcGetDataSet("HBYH_P_HR_CommonMgr_GETHRINFO", param, 1);
			}
			catch (Exception ex)
			{
				throw new GSPException("查询时！：" + ex.Message + "\n", (ErrorLevel)1);
			}

			if (ds != null && ds.Tables.Count > 0)
			{
				return JsonConvert.SerializeObject((object)ds);
			}
			else
			{
				return null;
			}

		}
	}
}
