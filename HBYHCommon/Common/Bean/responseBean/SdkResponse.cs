namespace Genersoft.GS.HBYHQYSCommon.Bean.responseBean
{
    public class SdkResponse<T>
    {
        private int code;

        private string message;

        private T result;

        public SdkResponse(int code, string message, T result)
        {
            this.code = code;
            this.message = message;
            this.result = result;
        }

        public int Code
        {
            get { return code; }
            set { code = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public T Result
        {
            get { return result; }
            set { result = value; }
        }
    }
}