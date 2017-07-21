using System;

namespace UnityHTTP.Response
{
    [Serializable]
    public class SimpleResponse
    {
        public bool IsError { get { return !string.IsNullOrEmpty(error) || errorCode != 0; } }
        public string error;
        public long errorCode;
        public string success;

        public override string ToString()
        {
            return base.ToString() + "IsError=" + IsError + " erorr=" + error + " errorCode=" + errorCode + " success=" + success;
        }
    }
}
