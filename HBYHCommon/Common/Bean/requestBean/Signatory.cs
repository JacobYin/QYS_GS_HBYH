using System.Collections.Generic;

namespace Genersoft.GS.HBYHQYSCommon.Bean.requestBean
{
    public class Signatory
    {
        public string tenantType;
        public string tenantName;
        public string serialNo;
        public bool remind;
        public string receiverName;
        public string contact;
        public string language;
        public List<Action> actions;
    }
}