using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using Genersoft.Platform.XFormController.FormController.Basic;
using Genersoft.Platform.XForm.SPI;
using Genersoft.Platform.XFormEngine.MLFC.XFML.DOM;
using Genersoft.Platform.AppFramework.Service;
using Genersoft.Platform.Resource.Metadata.Component.Attributes;
using Genersoft.Platform.Core.Error;
using Genersoft.Platform.Resource.Metadata.Common;
using Genersoft.Platform.Core.Common;
using Genersoft.Platform.Controls.WinForms;

using Genersoft.GS.HBYHCWClient;



namespace Genersoft.GS.HBYHCWController
{
    /// <summary>
    /// 费用报销单管理界面扩展类
    /// </summary>
    public class FYBXDMgr : ListMgrController
    {
        #region 构造函数
        public FYBXDMgr(IFormActionContext context)
            : base(context)
        {
            this.CompContext.Context = context;
            //获取GSPState
            this.gspState = this.Context.ElementHandler.ADPUIState.Copy();
            //表格
            this.repeat1 = this.CompContext.GetElementByID<RepeatElement>(XFMLConstants.ELEMENT_REPEAT, "XDataGrid1");
            //帮助
            //部门帮助
            this.XDept = this.CompContext.GetElementByID<DataDictLookUpElement>(XFMLConstants.ELEMENT_DATA_DICT_LOOKUP, "XDept");
            //人员帮助
            this.XEmp = this.CompContext.GetElementByID<DataDictLookUpElement>(XFMLConstants.ELEMENT_DATA_DICT_LOOKUP, "XEmp");
            //文本
            //单号
            this.XBillCode = this.CompContext.GetElementByID<TextBoxElement>(XFMLConstants.ELEMENT_TEXTBOX, "XBillCode");
            //日期
            //this.XBeginDate = this.CompContext.GetElementByID<CalendarElement>(XFMLConstants.ELEMENT_CALENDAR, "XBeginDate");
            //this.XEndDate = this.CompContext.GetElementByID<CalendarElement>(XFMLConstants.ELEMENT_CALENDAR, "XEndDate");
            //this.XHTBH = this.CompContext.GetElementByID<TextBoxElement>(XFMLConstants.ELEMENT_TEXTBOX, "XHTBH");
            //下拉框
            //this.XHAVEHT = this.CompContext.GetElementByID<ComboBoxElement>(XFMLConstants.ELEMENT_COMBOBOX, "XHAVEHT");
            //this.XJHWCBZ = this.CompContext.GetElementByID<ComboBoxElement>(XFMLConstants.ELEMENT_COMBOBOX, "XJHWCBZ");
            //this.XPeriod = this.CompContext.GetElementByID<ComboBoxElement>(XFMLConstants.ELEMENT_COMBOBOX, "XPeriod");
        }
        #endregion

        #region 表单控件
        /// <summary>
        /// 获取Repeat控件
        /// </summary>
        private RepeatElement repeat1 = null;
        /// <summary>
        /// 部门
        /// </summary>
        private DataDictLookUpElement XDept = null;
        /// <summary>
        /// 人员
        /// </summary>
        private DataDictLookUpElement XEmp = null;
        /// <summary>
        /// 单号
        /// </summary>
        private TextBoxElement XBillCode = null;
        #endregion 表单控件

        #region 表单变量
        private GSPState gspState = null;
        /// <summary>
        /// 表单绑定DataSet
        /// </summary>
        private DataSet ds
        {
            get
            {
                if (this.CompContext.DefaultInstanceData.DataSet == null)
                    return null;
                else
                    return this.CompContext.DefaultInstanceData.DataSet;
            }
        }
        #endregion 表单变量

        /// <summary>
        /// 表单加载后
        /// </summary>
        [ControllerMethod]
        public void BXDMgr_AfterDisplay()
        {
            string userID = this.CompContext.ElementHandler.ADPUIState.UserID;//登录用户ID
            string bizDate = this.CompContext.ElementHandler.ADPUIState.BizDate;//登录日期
            //可以添加设置控件默认值的代码
            //this.XSalesOrg.VisualComponent.Editor.Properties.DictEntry.ID = "057bb366-9943-455b-9";
            //this.XSalesOrg.VisualComponent.Editor.Text = "金玉米销售组织";

            //帮助后
            this.XDept.DictEntryPicked += (sender, e) =>
            {
                //清空
                this.XEmp.VisualComponent.EditValue = "";
                this.XEmp.VisualComponent.Editor.Properties.DictEntry.ID = "";
                this.XEmp.VisualComponent.Editor.Properties.DictEntry.Code = "";
                this.XEmp.VisualComponent.Editor.Properties.DictEntry.Name = "";
            };
            //帮助前
            this.XEmp.VisualComponent.DictEntryPicking += (sender, e) =>
            {
                //可以校验部门是否填写
                if (string.IsNullOrEmpty(this.XDept.VisualComponent.Editor.Properties.DictEntry.ID))
                {
                    throw new GSPException("请先选择部门", ErrorLevel.Warning);
                    //UMessageBox.Warning("请先选择部门");
                    //return;
                }
                //如果填写了，则根据部门过滤
                List<Express> filter = new List<Express>();
                filter.Add(new Express("string", "DEFORGID", " = ", XDept.VisualComponent.Editor.Properties.DictEntry.ID, " "));

                e.DictInfo.FilterExpression = ExpressParser.Serialize(filter);
            };
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        [ControllerMethod]
        public void BXD_RefreshData()
        {
            if (string.IsNullOrEmpty(XDept.VisualComponent.Editor.Properties.DictEntry.ID))
            {
                this.CompContext.ShowInfo("请选择部门");
                return;
            }

            string dept = XDept.VisualComponent.Editor.Properties.DictEntry.ID;
            string emp = XEmp.VisualComponent.Editor.Properties.DictEntry.ID;
            string billCode = XBillCode.VisualComponent.EditText.ToString();

            GSPFilter filter = new GSPFilter();
            List<Express> expressList = new List<Express>();

            try
            {

                if (!string.IsNullOrEmpty(emp))
                {
                    expressList.Add(new Express
                    {
                        Field = "BXRYNM",
                        Compare = " = ",
                        DataType = "String",
                        Value = emp.Trim(),
                        Relation = " And "
                    });
                }
                if (!string.IsNullOrEmpty(billCode))
                {
                    expressList.Add(new Express
                    {
                        Field = "DJBH",
                        Compare = " like ",
                        DataType = "String",
                        Value = string.Format("%{0}%", billCode.ToString().Trim()),
                        Relation = " And "
                    });
                }

                if (!string.IsNullOrEmpty(dept))
                {
                    expressList.Add(new Express
                    {
                        Field = "SSBMNM",
                        Compare = " = ",
                        DataType = "String",
                        Value = dept.Trim(),
                        Relation = ""
                    });
                }
                Pagination pagination = new Pagination();
                //pagination.PageIndex = repeat1.VisualComponent.PageNavigator.PageCurrent;
                //pagination.PageSize = repeat1.VisualComponent.PageNavigator.PageSize;
                DataURI uri = new DataURI(ExpressParser.Serialize(expressList), "", pagination);
                this.LoadDataByFilter(uri);
            }
            catch (Exception ex)
            {
                this.CompContext.ShowInMessageBox(ex.Message);
            }
        }

        /// <summary>
        /// 推送选中数据到第三方系统
        /// </summary>
        [ControllerMethod]
        public void PushData()
        {
            int index = this.repeat1.SelectedIndex;//选中行的索引
            if (index < 0)
                throw new GSPException("请选择数据行", ErrorLevel.Info);
            DataSet set = this.CompContext.DefaultInstanceData.DataSet;
            //选中行
            DataRow selectMainRow = this.repeat1.SelectedDataRow;
            string billid = selectMainRow["ID"].ToString();//单据主键
            string billcode = selectMainRow["BillCode"].ToString();//单据编号
            string billstate = selectMainRow["BillState"].ToString();//单据状态
            if (billstate != "2")
            {
                throw new GSPException("单据" + billcode + "不是审批通过状态，不允许推送", ErrorLevel.Info);
                 
            }
            
            string reStr = FYBXDClient.Current.PushData(billid);
            if (!string.IsNullOrEmpty(reStr))
            {
                throw new GSPException("单据" + billcode + "推送发送异常:" + reStr, ErrorLevel.Info);
            }
            else
            {
                UMessageBox.Information("操作成功！");
            }

        }
    }
}
