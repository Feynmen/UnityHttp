# UnityHttp
This is a small wrapper of UnityWebRequest to facilitate the work with the HTTP protocol. As well as the central control unit HttpCommunication.This module processes Cookies on both Android and iOS. The file iOSCoockieStorageHelper.mm, is plugin for iOS, thanks to the correct work on the cookies.
## How to start
1) Import into your Unity project UnityHTTP.dll and iOSCoockieStorageHelper.mm in Assets/Plugins folder OR import source codes
2) Create your request script which will be extended by RequestBase. For example:
```C#
// http://localhost/version
public class GetAPIVersionRequest : RequestBase<GetAPIVersionRequest>
{
    public override string RequestUrl
    {
        get { return "version"; }
    }

    protected override void InitRequest()
    {
        _unityWebRequest = UnityWebRequest.Get(FullUrl);
    }
    
    public override void OnSendCompleted() { }

    protected override void SetRequestData(RequestDataBase requestData) { }
}
```
3) If you need to send some data, create script which will be extended by RequestDataBase. For example ResetPasswordRequest.Data:
```C#
// http://localhost/forgotPass
 public class ResetPasswordRequest : RequestBase<ResetPasswordRequest>
    {
        private const string EMAIL_FIELD = "email";

        public override string RequestUrl
        {
            get { return "forgotPass"; }
        }

        private readonly WWWForm _requestData;

        public ResetPasswordRequest()
        {
            _requestData = new WWWForm();
        }
        
        protected override void SetRequestData(RequestDataBase requestData)
        {
            var data = requestData as Data;
            Debug.Assert(data != null, "data != null");
            _requestData.AddField(EMAIL_FIELD, data.email);
        }

        protected override void InitRequest()
        {
            _unityWebRequest = UnityWebRequest.Post(FullUrl, _requestData);
        }

        public override void OnSendCompleted() { }
        
        public class Data : RequestDataBase
        {
            public string email;
        }
    }
```
4) In your project you need to initialize Unity HTTP module (**only once**)
```C#
public class Test : MonoBehaviour 
{
    // Use this for initialization
    void Start ()
    {
        HttpCommunication.Instance.Init("http://localhost/");
        //Your code
    }
}
```
5) To send a request you should call Send method and pass parameters to it

```C#
public class Test : MonoBehaviour 
{
    // Use this for initialization
    void Start ()
    {
        HttpCommunication.Instance.Init("http://localhost/");
        HttpCommunication.Instance.Send<ResetPasswordRequest, SimpleResponse>(new ResetPasswordRequest.Data{email = "feynmen92@gmail.com"}, response =>
        {
            Debug.Log(response.ToString());
        });
        ///Your code
    }
}
```

6) Script SimpleResponse contains the default fields of response, but if you want to get some data, you should create a script that will be extended by SimpleResponse

```C#
    [Serializable]
    public class GetAPIVersionResponse : SimpleResponse
    {
        public int version;
    }
```
```C#
...
HttpCommunication.Instance.Send<GetAPIVersionRequest, GetAPIVersionResponse>(null, response =>
        {
            if (!response.IsError)
            {
                Debug.Log(response.version);
            }
        });
...
```
