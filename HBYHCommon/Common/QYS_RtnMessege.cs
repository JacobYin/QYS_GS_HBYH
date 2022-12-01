using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genersoft.GS.HBYHQYSCommon
{
    public class QYS_RtnMessege
    {
        /// <summary>
        /// 状态[200:成功，500失败]
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 返回数据内容
        /// </summary>
        public string data { get; set; }
    }
}