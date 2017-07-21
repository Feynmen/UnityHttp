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

        public void Init(string serverRootUrl, int requestTimeout = 50)
        {
            _serverUrl = serverRootUrl;
        }

        public void Send<TRequest, TResponse>(RequestDataBase data, Action<TResponse> callback) where TRequest : RequestBase<TRequest>, new() where TResponse : SimpleResponse, new()
        {
            Debug.Log("Send - " + typeof(TRequest));
            Debug.Log(data);
            var request = RequestBase<TRequest>.Create(_serverUrl, data);
            StartCoroutine(SendRequestWithTimer(request, () =>
            {
                request.OnSendCompleted();
                callback.Invoke(ParseResponse<TResponse, TRequest>(request));
            }, () =>
            {
                callback.Invoke(new TResponse { error = "Timeout", errorCode = 408 });
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
