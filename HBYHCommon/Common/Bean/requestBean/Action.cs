using System;
using System.Collections.Generic;

namespace Genersoft.GS.HBYHQYSCommon.Bean.requestBean
{
    public class Action
    {
        public string type;
        public string name;
        public string actionNo;
        public int serialNo;
        public string sealId;
        public string sealIds;
        public string sealNames;
        public string sealCategoryName;
        public bool autoSign;
        public List<ActionOperator> actionOperators;
    }
}