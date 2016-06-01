#include "GemScanListener.h"

@implementation GemScanListener

- (instancetype)initWithReadyCallback:(OnReadyCallback)onReady andWithInitializationErrorCallback:(OnInitializationErrorCallback)onInitializationError andWithDeviceDiscoveredCallback:(OnDeviceDiscoveredCallback)onDeviceDiscovered
{
    if(!(self = [super init]))
    {
        // TODO handle error
    }
    
    _onReadyCallback = onReady;
    _onInitializationErrorCallback = onInitializationError;
    _onDeviceDiscoveredCallback = onDeviceDiscovered;
    
    return self;
}

- (void)onReady
{
    _onReadyCallback();
}

- (void)onInitializationError:(InitializationError)error
{
    _onInitializationErrorCallback(error);
}

- (void)onDeviceDiscovered:(Gem*)gem rssi:(NSNumber*)rssi
{
    _onDeviceDiscoveredCallback(gem, [rssi intValue]);
}

@end