#import <Foundation/NSObject.h>
#import <Foundation/NSNotification.h>

extern "C" void ClearIOSCookieByValue(const char *cookie)
{
            NSString *cValue = [NSString stringWithUTF8String:cookie];
            NSLog(@"ClearIOSCookieByValue Value - %@", cValue);
            NSHTTPCookieStorage *cookieStorage = [NSHTTPCookieStorage sharedHTTPCookieStorage];
            for(NSHTTPCookie *each in cookieStorage.cookies)
            {
                NSString *value = [NSString stringWithFormat:@"%@=%@", each.name, each.value];
                   NSLog(@"ClearIOSCookieByValue CookieStorageItem - %@", value);
                if (cValue == value)
                    [cookieStorage deleteCookie:each];
            }
}

extern "C" void ClearAllIOSCookies()
{
            NSHTTPCookieStorage *cookieStorage = [NSHTTPCookieStorage sharedHTTPCookieStorage];
            for(NSHTTPCookie *each in cookieStorage.cookies)
                [cookieStorage deleteCookie:each];
            NSLog(@"ClearAllIOSCookies");
}