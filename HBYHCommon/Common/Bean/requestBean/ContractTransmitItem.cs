namespace Genersoft.GS.HBYHQYSCommon.Bean.requestBean
{
    public class ContractTransmitItem
    {
        private string receiverMobile;
        private string receiverName;

        public ContractTransmitItem()
        {
        }

        public string ReceiverMobile
        {
            get => receiverMobile;
            set => receiverMobile = value;
        }

        public string ReceiverName
        {
            get => receiverName;
            set => receiverName = value;
        }
    }
}