using UnityEngine;
using UnityEngine.Networking;

namespace UnityHTTP.Request
{
    public abstract class RequestBase<T> where T : RequestBase<T>, new()
    {
        public static T Create(string serverUrl, RequestDataBase requestData = null)
        {
            var request = new T();
            request.FullUrl = serverUrl + request.RequestUrl;
            request.SetRequestData(requestData);
            return request;
        }

        public string FullUrl { private set; get; }

        protected UnityWebRequest _unityWebRequest;
        public UnityWebRequest UnityWebRequest
        {
            get
            {
                if (_unityWebRequest == null)
                {
                    InitRequest();
                }
                return _unityWebRequest;
            }
        }
        public AsyncOperation Send()
        {
            return UnityWebRequest.Send();
        }

        public void Abort()
        {
            if (_unityWebRequest != null)
            {
                _unityWebRequest.Abort();
            }
        }

        public abstract string RequestUrl { get; }

        public abstract void OnSendCompleted();
        protected abstract void InitRequest();
        protected abstract void SetRequestData(RequestDataBase requestData);
    }
}
