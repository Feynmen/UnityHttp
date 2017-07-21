using UnityEngine;
using UnityEngine.Networking;

namespace UnityHTTP
{
    public sealed class Cookies
    {
        private static Cookies _instance;
        public static Cookies Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Cookies();
                }
                return _instance;
            }
        }

#if UNITY_IPHONE
        //iOSCoockieStorageHelper.mm
        [DllImport("__Internal")]
        private static extern void ClearIOSCookieByValue(string cookie);
        //iOSCoockieStorageHelper.mm
        [DllImport("__Internal")]
        private static extern void ClearAllIOSCookies();
#endif

        private const string COOKIE = "Cookie";
        private const string SET_COOKIE = "SET-COOKIE";
        private string _cookieFullString;

        public bool IsCookieExist { get { return PlayerPrefs.HasKey(COOKIE); } }

        public void CheckRequestForSetCookie(UnityWebRequest request, bool isSaveCookie = true)
        {
            var cookie = request.GetResponseHeader(SET_COOKIE);
            if (!string.IsNullOrEmpty(cookie))
            {
                ClearCookie();
                if (isSaveCookie)
                {
                    PlayerPrefs.SetString(COOKIE, cookie);
                }
                _cookieFullString = cookie;
            }
        }

        public bool TrySetCookieInRequest(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(_cookieFullString) || TryGetSavedCookie(out _cookieFullString))
            {
                request.SetRequestHeader(COOKIE, _cookieFullString);
                return true;
            }
            return false;
        }

        public void ClearCookie()
        {
            if (IsCookieExist)
            {
                PlayerPrefs.DeleteKey(COOKIE);
            }
            if (!string.IsNullOrEmpty(_cookieFullString))
            {
                ClearCookieByValue(_cookieFullString);
                _cookieFullString = null;
            }
            else //Warning! Maybe on iOS version lower than 10 could remove all device cookie
            {
                ClearAllCookies();
            }
        }

        private bool TryGetSavedCookie(out string cookie)
        {
            cookie = string.Empty;
            if (IsCookieExist)
            {
                cookie = PlayerPrefs.GetString(COOKIE);
                return true;
            }
            return false;
        }

        private void ClearAllCookies()
        {
#if UNITY_IPHONE
            ClearAllIOSCookies();
#endif
        }

        private void ClearCookieByValue(string cookie)
        {
#if UNITY_IPHONE
            if (cookie.IndexOf(";", StringComparison.Ordinal) > 0)
            {
                ClearIOSCookieByValue(cookie);
            }
            else
            {
                ClearAllIOSCookies();
            }
#endif
        }
    }
}
