#import <TargetConditionals.h>

#if TARGET_OS_IPHONE
    #import <UIKit/UIKit.h>
#else
    #import <Cocoa/Cocoa.h>
#endif

//! Project version number for GemSDK.
FOUNDATION_EXPORT double GemSDKVersionNumber;

//! Project version string for GemSDK.
FOUNDATION_EXPORT const unsigned char GemSDKVersionString[];

#import <GemSDK/GemManager.h>
#import <GemSDK/Gem.h>
#import <GemSDK/BluetoothAddress.h>
#import <GemSDK/GemDelegate.h>
#import <GemSDK/GemScanDelegate.h>