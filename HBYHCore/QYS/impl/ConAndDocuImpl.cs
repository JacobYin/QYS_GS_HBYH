using System;
using System.Collections.Generic;
using System.Data;
using Genersoft.GS.HBYHQYSCommon.Bean.requestBean;
using Genersoft.GS.HBYHQYSCommon.Bean.responseBean;
using Genersoft.GS.HBYHQYSCommon.utils;
using Genersoft.Platform.AppFramework.Service;
using Genersoft.Platform.Core.DataAccess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Genersoft.GS.HBYHQYSCore.QYS.impl
{
    public class ConAndDocuImpl
    {
        /**
         * 首先，通过存储过程获取到要上传的多个附件（或一个），然后把这些返回的id存在一个list里，之后创建合同需要用到这个包含文档id的list
         */
        public static string SetContract(string HTNM)
        {
            List<string> documents = new List<string>();
            IGSPDatabase db = GSPContext.Current.Database;
            IDbDataParameter[] param = new IDbDataParameter[1];
            param[0] = db.MakeInParam("HTNM", GSPDbDataType.VarChar, 36, HTNM);
            DataSet ds = db.RunProcGetDataSet("HBYHINTERFACE_QYS_DOCUMENT_DOCUMENTINFO", param, 1);
            

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow row = ds.Tables[0].Rows[i];
                    string postresult = HBYHQYSCommon.Restful.Post_document(HBYHQYSCommon.Restful.RestAddr.ADD_CONTRACTFILE_ADDR_TEST,
                        row["FILENAME"].ToString(), row["FILETYPE"].ToString(), row["DJNM"].ToString());
                    JObject oresult = (JObject)JsonConvert.DeserializeObject(postresult);
                    if ((int)oresult["code"] == 0)
                    {
                        documents.Add(oresult["result"]["documentId"].ToString());
                        byte[] byteArray = Guid.NewGuid().ToByteArray();
                        uint id = BitConverter.ToUInt32(byteArray, 0);
                        string s = oresult["result"]["documentId"].ToString();
                        long documentid = Convert.ToInt64(s);
                        DateTime now = DateTime.Now;
                        string date = now.Year.ToString() + now.Month.ToString("00") + now.Day.ToString("00");
                        string time = now.ToLongTimeString();
                        HBYHCWCommon.CommonMgr.WriteLogFile("修改数据库数据断点");
                        string sql = $@"insert into uf_es_record@OADB (ID, YYLCID, YYLCMC, LCDJZJID, GZQFJID, MODEDATACREATERTYPE, MODEDATACREATEDATE, MODEDATACREATETIME)values ({id}, 3036553591784023031, '股份公司合同测试签章流程', '{HTNM}', {documentid}, 3, '{date}', '{time}')";
                        int sqlStatement = db.ExecSqlStatement(sql);
                        if (sqlStatement > 0)
                        {
                            HBYHCWCommon.CommonMgr.WriteLogFile("插入数据成功，影像数据：" + sqlStatement);
                        }
                        else
                        {
                            HBYHCWCommon.CommonMgr.WriteLogFile("插入数据失败，影响结果：" + sqlStatement);
                        }
                    }   
                    else
                    {
                        HBYHCWCommon.CommonMgr.WriteLogFile("推送数据失败，查看日志。");
                    }
                }
            }
            
            //当一个合同所有该盖章的附件全部上传后，之后就要创建合同，调用创建合同接口
            ContractRequest contract = null;
            List<Signatory> signatories = null;
            IDbDataParameter[] nparams = new IDbDataParameter[1];
            nparams[0] = db.MakeInParam("HTNM", GSPDbDataType.VarChar, 36, HTNM);
            DataSet nds = db.RunProcGetDataSet("HBYHINTERFACE_QYS_CONTRACT_CONTRACTINFO", nparams, 1);

            if (nds != null && nds.Tables.Count > 0 && nds.Tables[0].Rows.Count > 0)
            {
                DataRow row = nds.Tables[0].Rows[0];
                contract = new ContractRequest();
                contract.subject = row["SUBJECT"].ToString();
                contract.ordinal = JSONUtils.GetBoolean(row["ORDINAL"].ToString());
                contract.sn = row["SN"].ToString();
                contract.categoryId = "3036553591784023031";
                contract.send = JSONUtils.GetBoolean(row["SEND"].ToString());
                contract.creatorName = row["CREATORNAME"].ToString();
                contract.creatorContact = row["CREATORCONTACT"].ToString();
                contract.extraSign = JSONUtils.GetBoolean(row["EXTRASIGN"].ToString());
                contract.mustSign = JSONUtils.GetBoolean(row["MUSTSIGN"].ToString());
                contract.autoCreateCounterSign = JSONUtils.GetBoolean(row["AUTOCREATECOUNTERSIGN"].ToString());
                contract.msgCode = Convert.ToInt32(row["MSGCODE"].ToString());
                signatories = new List<Signatory>();
                Signatory fqsignatory = new Signatory();
                fqsignatory.tenantType = "CORPORATE";
                fqsignatory.tenantName = "湖北宜化集团有限责任公司";
                signatories.Add(fqsignatory);
                Signatory khsignatory = new Signatory();
                khsignatory.tenantType = "CORPORATE";
                khsignatory.tenantName = row["KHF"].ToString();
                khsignatory.receiverName = row["RECEIVERNAME"].ToString();
                khsignatory.contact = row["RECEIVERCONTACT"].ToString();
                khsignatory.language = row["HTLANGUAGE"].ToString();
                signatories.Add(khsignatory);
                contract.signatories = signatories;
                contract.documents = documents;
            }

            string serializecontract = JsonConvert.SerializeObject(contract);
            HBYHCWCommon.CommonMgr.WriteLogFile("创建JSON内容：" + serializecontract);
            string Contract_result = HBYHQYSCommon.Restful.Post(HBYHQYSCommon.Restful.RestAddr.CREATE_BYCATEGORY_ADDR_TEST, serializecontract);
            JObject contract_object = (JObject)JsonConvert.DeserializeObject(Contract_result);
            HBYHCWCommon.CommonMgr.WriteLogFile("执行创建合同接口");
            if ((int) contract_object["code"] == 0)
            {
                string contract_id = contract_object["contractId"].ToString();
                long contract_sz = Convert.ToInt64(contract_id);
                string sql_2 =
                    $"UPDATE uf_es_record@OADB s SET s.QYSHTID = {contract_sz},s.QYSHTZTZ = 'draft' WHERE s.LCDJZJID = '{HTNM}'";
                int sqlStatement_result = db.ExecSqlStatement(sql_2);
                if (sqlStatement_result > 0)
                {
                    HBYHCWCommon.CommonMgr.WriteLogFile("修改数据成功，影像数据：" + sqlStatement_result);
                }
                else
                {
                    HBYHCWCommon.CommonMgr.WriteLogFile("修改数据失败，影响结果：" + sqlStatement_result);
                }
            }

            db.Commit();
            return (string) contract_object["message"];
        }
    }
}