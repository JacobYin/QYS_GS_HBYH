using System;
using System.Collections.Generic;

namespace Genersoft.GS.HBYHQYSCommon.Bean.requestBean
{
    public class Action
    {
        private string type;
        private string name;
        private string actionNo;
        private int serialNo;
        private long sealId;
        private string sealIds;
        private string sealNames;
        private string sealCategoryName;
        private long sealOwner;
        private bool autoSign;
        private List<ActionOperator> actionOperators;

        public Action()
        {
        }

        public string Type
        {
            get => type;
            set => type = value;
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public string ActionNo
        {
            get => actionNo;
            set => actionNo = value;
        }

        public int SerialNo
        {
            get => serialNo;
            set => serialNo = value;
        }

        public long SealId
        {
            get => sealId;
            set => sealId = value;
        }

        public string SealIds
        {
            get => sealIds;
            set => sealIds = value;
        }

        public string SealNames
        {
            get => sealNames;
            set => sealNames = value;
        }

        public string SealCategoryName
        {
            get => sealCategoryName;
            set => sealCategoryName = value;
        }

        public long SealOwner
        {
            get => sealOwner;
            set => sealOwner = value;
        }

        public bool AutoSign
        {
            get => autoSign;
            set => autoSign = value;
        }

        public List<ActionOperator> ActionOperators
        {
            get => actionOperators;
            set => actionOperators = value;
        }
    }
}