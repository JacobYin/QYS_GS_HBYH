using System;
using System.Collections.Generic;
using System.Data;
using Genersoft.GS.HBYHQYSCommon.Bean.requestBean;
using Genersoft.GS.HBYHQYSCommon.utils;
using Genersoft.Platform.AppFramework.Service;
using Genersoft.Platform.Core.DataAccess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Action = Genersoft.GS.HBYHQYSCommon.Bean.requestBean.Action;

namespace Genersoft.GS.HBYHQYSCore.QYS.impl
{
    public class ConAndDocuImpl
    {
        /**
         * (双方签署场景)首先，通过存储过程获取到要上传的多个附件（或一个），然后把这些返回的id存在一个list里，之后创建合同需要用到这个包含文档id的list
         */
        public static string SetDoubleContract(string HTNM, string categoryID, string GaiZhangName,
            string GaiZhangContact)
        {
            // 定义要上传的附件列表
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
                    string htmc = row["HTMC"].ToString();
                    string postresult = HBYHQYSCommon.Restful.Post_document(
                        HBYHQYSCommon.Restful.NormalRestAddr.ADD_CONTRACTFILE_ADDR_TEST,
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
                        //将数据插入到中间表当中去
                        string sql =
                            $@"insert into uf_es_record@OADB (ID, YYLCMC, LCDJZJID, GZQFJID, GZHFJID, MODEDATACREATERTYPE, MODEDATACREATEDATE, MODEDATACREATETIME)values ({id},'{htmc}', '{HTNM}', {documentid},0, 3, '{date}', '{time}')";
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

                //开始定义双签合同的各项信息
                contract.subject = row["SUBJECT"].ToString();
                contract.sn = row["SN"].ToString();
                contract.description = row["DESCRIPTION"].ToString();
                contract.categoryId = categoryID;
                contract.send = JSONUtils.GetBoolean(row["SEND"].ToString());
                contract.tenantName = row["TENANTNAME"].ToString();
                contract.businessData = row["BUSINESSDATA"].ToString();
                contract.creatorName = row["CREATORNAME"].ToString();
                contract.autoCreateCounterSign = JSONUtils.GetBoolean(row["AUTOCREATECOUNTERSIGN"].ToString());
                contract.extraSign = JSONUtils.GetBoolean(row["EXTRASIGN"].ToString());
                contract.mustSign = JSONUtils.GetBoolean(row["MUSTSIGN"].ToString());
                contract.signAll = Convert.ToInt32(row["SIGNALL"].ToString());
                contract.msgCode = Convert.ToInt32(row["MSGCODE"].ToString());
                contract.documents = documents;

                //定义双方签署方的签署序列
                signatories = new List<Signatory>();

                //定义各签署方的信息
                Signatory fqsignatory = new Signatory();
                fqsignatory.tenantType = "COMPANY";
                fqsignatory.tenantName = row["TENANTNAME"].ToString();
                fqsignatory.serialNo = "1";
                fqsignatory.remind = true;
                fqsignatory.contact = row["WFYWDH"].ToString();
                fqsignatory.receiverName = row["WFYEWUYUAN"].ToString();
                fqsignatory.language = "zh_CN";

                List<Action> fqfActions = new List<Action>();

                //Action_1
                Action WoFangQianzi = new Action();
                WoFangQianzi.type = "PERSONAL";
                WoFangQianzi.name = "我方业务员签字";
                WoFangQianzi.serialNo = 1;

                ActionOperator qianzirenyuan = new ActionOperator();
                qianzirenyuan.operatorName = row["WFYEWUYUAN"].ToString();
                qianzirenyuan.operatorContact = row["WFYWDH"].ToString();
                WoFangQianzi.actionOperators = new List<ActionOperator>() { qianzirenyuan };


                //Action_2
                Action WoFangGaiZhang = new Action();
                WoFangGaiZhang.type = "CORPORATE";
                WoFangGaiZhang.name = "我方组织盖章";
                WoFangGaiZhang.serialNo = 2;

                ActionOperator gaizhangrenyuan = new ActionOperator();
                gaizhangrenyuan.operatorName = GaiZhangName;
                gaizhangrenyuan.operatorContact = GaiZhangContact;
                WoFangGaiZhang.actionOperators = new List<ActionOperator>() { gaizhangrenyuan };

                fqfActions.Add(WoFangQianzi);
                fqfActions.Add(WoFangGaiZhang);
                fqsignatory.actions = fqfActions;
                signatories.Add(fqsignatory);


                //签署方客户方信息添加
                Signatory khsignatory = new Signatory();
                khsignatory.tenantType = "COMPANY";
                khsignatory.tenantName = row["KHFMC"].ToString();
                khsignatory.serialNo = "2";
                khsignatory.remind = true;
                khsignatory.contact = row["RECEIVERCONTACT"].ToString();
                khsignatory.receiverName = row["RECEIVERNAME"].ToString();
                khsignatory.language = "zh_CN";

                List<Action> khfActions = new List<Action>();

                //Action_3
                Action DuiFangQianzi = new Action();
                DuiFangQianzi.type = "PERSONAL";
                DuiFangQianzi.name = "对方业务员签字";
                DuiFangQianzi.serialNo = 1;

                ActionOperator dfqianzirenyuan = new ActionOperator();
                dfqianzirenyuan.operatorName = row["RECEIVERNAME"].ToString();
                dfqianzirenyuan.operatorContact = row["RECEIVERCONTACT"].ToString();
                DuiFangQianzi.actionOperators = new List<ActionOperator>() { dfqianzirenyuan };


                //Action_4
                Action DuiFangGaiZhang = new Action();
                DuiFangGaiZhang.type = "CORPORATE";
                DuiFangGaiZhang.name = "对方组织盖章";
                DuiFangGaiZhang.serialNo = 2;
                ActionOperator dfgaizhangrenyuan = new ActionOperator();
                DuiFangGaiZhang.actionOperators = new List<ActionOperator>() { dfgaizhangrenyuan };

                khfActions.Add(DuiFangQianzi);
                khfActions.Add(DuiFangGaiZhang);
                khsignatory.actions = khfActions;


                signatories.Add(khsignatory);
                contract.signatories = signatories;
            }

            string serializecontract = JsonConvert.SerializeObject(contract);
            HBYHCWCommon.CommonMgr.WriteLogFile("创建JSON内容：" + serializecontract);
            string Contract_result =
                HBYHQYSCommon.Restful.Post(HBYHQYSCommon.Restful.NormalRestAddr.CREATE_BYCATEGORY_ADDR_TEST,
                    serializecontract);
            JObject contract_object = (JObject)JsonConvert.DeserializeObject(Contract_result);
            HBYHCWCommon.CommonMgr.WriteLogFile("已执行创建合同接口");
            if ((int)contract_object["code"] == 0)
            {
                string contract_id = contract_object["contractId"].ToString();
                long contract_sz = Convert.ToInt64(contract_id);
                string sql_2 =
                    $"UPDATE uf_es_record@OADB s SET s.QYSHTID = {contract_sz},s.QYSHTZTZ = 'draft',s.YYLCID = {categoryID} WHERE s.LCDJZJID = '{HTNM}'";
                int sqlStatement_result = db.ExecSqlStatement(sql_2);
                if (sqlStatement_result > 0)
                {
                    HBYHCWCommon.CommonMgr.WriteLogFile("修改数据成功，影响数据：" + sqlStatement_result);
                }
                else
                {
                    HBYHCWCommon.CommonMgr.WriteLogFile("修改数据失败，影响结果：" + sqlStatement_result);
                }
            }

            db.Commit();
            return (string)contract_object["message"];
        }

        /// <summary>
        /// (单方签署场景)未设置，之后会编写单方签署场景代码
        /// </summary>
        /// <param name="HTNM"></param>
        /// <returns></returns>
        public static string SetSingleContract(string HTNM)
        {
            return "NULL";
        }

        /// <summary>
        ///  设置电子签定时任务下载签署已完成的合同盖章后附件，将盖章后附件下载到ftp服务器后在HTFJ表插入一条新数据
        /// </summary>
        public static void Get_Document()
        {
            IGSPDatabase db = GSPContext.Current.Database;
            IDbDataParameter[] nulparams = null;
            DataSet nds = db.RunProcGetDataSet("HBYHINTERFACE_QYS_DOWNLOAD_DownLoadList", nulparams, 1);
            if (nds != null && nds.Tables.Count > 0 && nds.Tables[0].Rows.Count > 0)
            {
                HBYHCWCommon.CommonMgr.WriteLogFile("查询到要下载的合同结果：" + nds.Tables[0].Rows.Count);
                for (int k = 0; k < nds.Tables[0].Rows.Count; k++)
                {
                    DataRow row = nds.Tables[0].Rows[k];
                    string document_name =
                        HBYHQYSCommon.Restful.Get_document(row["DZHTID"].ToString(), row["LCDJZJID"].ToString());

                    HBYHCWCommon.CommonMgr.WriteLogFile(document_name);
                    HBYHCWCommon.CommonMgr.WriteLogFile("获取文件结果：" + document_name);

                    IDbDataParameter[] nparams = new IDbDataParameter[2];
                    nparams[0] = db.MakeInParam("DOCUMENTNAME", GSPDbDataType.VarChar, 36, document_name);
                    nparams[1] = db.MakeInParam("LCHTNM", GSPDbDataType.VarChar, 36, row["LCDJZJID"].ToString());
                    DataSet resultr = db.RunProcGetDataSet("HBYHINTERFACE_QYS_DOWNLOAD_AddQYSDocument", nparams, 1);
                    if (resultr != null && resultr.Tables.Count > 0 && resultr.Tables[0].Rows.Count > 0)
                    {
                        DataRow dataresRow = resultr.Tables[0].Rows[0];
                        HBYHCWCommon.CommonMgr.WriteLogFile("返回插入结果：" + dataresRow["RES"].ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 同步电子签公司认证情况，已认证的单位会修改ERP的集团往来单位定义自定义字段3的状态值为是
        /// </summary>
        public static void Sync_CompAuth()
        {
            IGSPDatabase db = GSPContext.Current.Database;
            string resultHTML = HBYHQYSCommon.Restful.Get_Organization_Auth();
            JObject deserializeObject = (JObject)JsonConvert.DeserializeObject(resultHTML);
            JToken token = deserializeObject["result"];

            //认证公司名称会放在一个list<string>里，用来存储已经认证成功的客户名称

            List<string> rzmd = new List<string>();

            foreach (var VARIABLE in token)
            {
                if ((string)VARIABLE["status"] == "AUTH_SUCCESS")
                {
                    rzmd.Add((string)VARIABLE["name"]);
                }
            }

            string serializeObject = JsonConvert.SerializeObject(rzmd);
            string result = serializeObject.TrimStart('[').TrimEnd(']').Replace('\"', '\'');

            string sql_3 =
                $"UPDATE LSWLDW L SET L.Customfield3 = '是' WHERE L.Lswldw_Dwmc IN ({result})";
            int sqlStatement_result = db.ExecSqlStatement(sql_3);
            if (sqlStatement_result >= 0)
            {
                HBYHCWCommon.CommonMgr.WriteLogFile("认证公司状态修改数据成功，影响数据：" + sqlStatement_result);
            }
            else
            {
                HBYHCWCommon.CommonMgr.WriteLogFile("认证公司状态修改数据失败，影响结果：" + sqlStatement_result);
            }

            db.Commit();
        }
    }
}