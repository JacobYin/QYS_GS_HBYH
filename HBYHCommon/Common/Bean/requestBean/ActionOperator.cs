namespace Genersoft.GS.HBYHQYSCommon.Bean.requestBean
{
    public class ActionOperator
    {
        private string operatorName;
        private string operatorContact;
        private string operatorNumber;

        public ActionOperator()
        {
        }

        public string OperatorName
        {
            get => operatorName;
            set => operatorName = value;
        }

        public string OperatorContact
        {
            get => operatorContact;
            set => operatorContact = value;
        }

        public string OperatorNumber
        {
            get => operatorNumber;
            set => operatorNumber = value;
        }
    }
}