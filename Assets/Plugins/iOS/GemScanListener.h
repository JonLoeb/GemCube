#ifndef _H_GEMSCANLISTENER
#define _H_GEMSCANLISTENER

#include <GemSDK/GemSDK.h>

typedef void (*OnReadyCallback)();
typedef void (*OnInitializationErrorCallback)(InitializationError error);
typedef void (*OnDeviceDiscoveredCallback)(Gem* gem, int rssi);

@interface GemScanListener : NSObject <GemScanDelegate>
{
    OnReadyCallback _onReadyCallback;
    OnInitializationErrorCallback _onInitializationErrorCallback;
    OnDeviceDiscoveredCallback _onDeviceDiscoveredCallback;
}

- (instancetype)initWithReadyCallback:(OnReadyCallback)onReady andWithInitializationErrorCallback:(OnInitializationErrorCallback)onInitializationError andWithDeviceDiscoveredCallback:(OnDeviceDiscoveredCallback)onDeviceDiscovered;

@end

#endif //_H_GEMSCANLISTENER