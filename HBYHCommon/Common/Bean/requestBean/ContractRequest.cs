using System;
using System.Collections.Generic;

namespace Genersoft.GS.HBYHQYSCommon.Bean.requestBean
{
    public class ContractRequest
    {
        public string subject;
        public bool ordinal;
        public string sn;
        public string description;
        public string categoryId;
        public bool send;
        public List<string> documents;
        public string creatorName;
        public string tenantName;
        public string businessData;
        public List<Signatory> signatories;
        public bool autoCreateCounterSign;
        public bool extraSign;
        public bool mustSign;
        public int signAll;
        public int msgCode;
    }
}