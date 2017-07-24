using System;
using System.Collections;
using UnityEngine;
using UnityHTTP.Request;
using UnityHTTP.Response;

namespace UnityHTTP
{
    public sealed class HttpCommunication : MonoBehaviour
    {
        private static HttpCommunication _instance;
        public static HttpCommunication Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("HttpCommunication").AddComponent<HttpCommunication>();
                }
                return _instance;
            }
        }

        private string _serverUrl;
        private int _requestTimeout;

        private HttpCommunication() { }

        /// <summary>
        /// Init HttpCommunication with server root URL and request timeout
        /// </summary>
        /// <param name="serverRootUrl">Server root URl. For example: "https://github.com/"</param>
        /// <param name="requestTimeout">Request timeout</param>
        public void Init(string serverRootUrl, int requestTimeout)
        {
            _serverUrl = serverRootUrl;
            _requestTimeout = requestTimeout;
        }

        /// <summary>
        /// Init HttpCommunication with server root URL
        /// </summary>
        /// <param name="serverRootUrl">Server root URl. For example: "https://github.com/"</param>
        public void Init(string serverRootUrl)
        {
            _serverUrl = serverRootUrl;
            _requestTimeout = 50;
        }

        /// <summary>
        /// Init HttpCommunication. You mast insert full server URl in your requests
        /// </summary>
        public void Init()
        {
            _serverUrl = string.Empty;
            _requestTimeout = 50;
        }
        /// <summary>
        /// Send a request to a server that has been extended from the RequestBase class
        /// </summary>
        /// <typeparam name="TRequest">Your request type that has been extended from the RequestBase class</typeparam>
        /// <typeparam name="TResponse">SimpleResponse type or RequestResponse type that has been extended from the SimpleResponse class</typeparam>
        /// <param name="requestData">Data container type that has been extended from the RequestDataBase class</param>
        /// <param name="resultCallback">Callback which signals the completion of communication with the server</param>
        public void Send<TRequest, TResponse>(RequestDataBase requestData, Action<TResponse> resultCallback) where TRequest : RequestBase<TRequest>, new() where TResponse : SimpleResponse, new()
        {
            Debug.Log("Send - " + typeof(TRequest));
            Debug.Log(requestData);
            var request = RequestBase<TRequest>.Create(_serverUrl, requestData);
            StartCoroutine(SendRequestWithTimer(request, compliteCallback: () =>
            {
                request.OnSendCompleted();
                if (resultCallback != null)
                {
                    resultCallback.Invoke(ParseResponse<TResponse, TRequest>(request));
                }
            }, timeoutCallback: () =>
            {
                if (resultCallback != null)
                {
                    resultCallback.Invoke(new TResponse {error = "Timeout", errorCode = 408});
                }
            }));
        }

        private IEnumerator SendRequestWithTimer<T>(RequestBase<T> request, Action compliteCallback, Action timeoutCallback) 
            where T : RequestBase<T>, new()
        {
            Cookies.Instance.TrySetCookieInRequest(request.UnityWebRequest);
            var asyncOperation = request.Send();
            float timer = _requestTimeout;
            while (!asyncOperation.isDone)
            {
                if (timer <= 0)
                {
                    request.Abort();
                    timeoutCallback.Invoke();
                    yield break;
                }
                timer -= Time.fixedDeltaTime;
                yield return null;
            }
            compliteCallback.Invoke();
        }

        private static TResponse ParseResponse<TResponse, TRequest>(RequestBase<TRequest> request) 
            where TResponse : SimpleResponse, new() where TRequest : RequestBase<TRequest>, new()
        {
            var json = "{}";
            if (request.UnityWebRequest.downloadHandler != null)
            {
                json = request.UnityWebRequest.downloadHandler.text;
            }
            if (request.UnityWebRequest.responseCode < 300 && request.UnityWebRequest.responseCode >= 200)
            {
                try
                {
                    if (json[0] == '[')
                    {
                        json = "{\"data\":" + json + "}";
                    }
                    return JsonUtility.FromJson<TResponse>(json);
                }
                catch (Exception e)
                {
                    return new TResponse()
                    {
                        error = "Exception! " + e.Message + "\nJSON: " + json
                    };
                }
            }
            if (request.UnityWebRequest.responseCode == 400)
            {
                var respounse = JsonUtility.FromJson<TResponse>(json);
                respounse.errorCode = request.UnityWebRequest.responseCode;
                return respounse;
            }
            return new TResponse()
            {
                errorCode = request.UnityWebRequest.responseCode,
                error = request.UnityWebRequest.error
            };
        }
    }
}
