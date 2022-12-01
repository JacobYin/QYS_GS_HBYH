using System.Data;
using Genersoft.Platform.Core.DataAccess;
using Genersoft.Platform.AppFramework.Service;
using System;

namespace Genersoft.GS.HBYHQYSCore.QYS
{
    public class DocumentInfo
    {
        #region MyRegion
        //合同内码（存储文件夹名称）
        public string djnm;
        //合同附件文件名称
        public string filename;
        //合同附件文件扩展名
        public string filetype;

        #endregion

        public static DocumentInfo GetDocumentInfo(String HTNM)
        {
            DocumentInfo di = null;
            IGSPDatabase db = GSPContext.Current.Database;
            IDbDataParameter[] param = new IDbDataParameter[1];
            param[0] = db.MakeInParam("HTNM", GSPDbDataType.VarChar, 36, HTNM);
            DataSet ds = db.RunProcGetDataSet("HBYHINTERFACE_QYS_DOCUMENT_DOCUMENTINFO", param, 1);

            if (ds != null && ds.Tables.Count == 1 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                di = new DocumentInfo();
                di.djnm = row["DJNM"].ToString();
                di.filename = row["FILENAME"].ToString();
                di.filetype = row["FILETYPE"].ToString();
            }

            return di;
        }
    }
}