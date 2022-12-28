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
        public string creatorContact;
        public List<Signatory> signatories;
        public bool extraSign;
        public bool mustSign;
        public bool autoCreateCounterSign;
        public int msgCode;
    }
}