namespace Genersoft.GS.HBYHQYSCommon.Bean.requestBean
{
    public class ContractActionRequest
    {
        private string contractId;
        private string bizId;
        private string reason;
        private string name;
        private string[] downloadItems;

        public ContractActionRequest()
        {
        }

        public string ContractId
        {
            get => contractId;
            set => contractId = value;
        }

        public string BizId
        {
            get => bizId;
            set => bizId = value;
        }

        public string Reason
        {
            get => reason;
            set => reason = value;
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public string[] DownloadItems
        {
            get => downloadItems;
            set => downloadItems = value;
        }
    }
}