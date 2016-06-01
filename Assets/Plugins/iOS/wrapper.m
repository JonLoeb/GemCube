#include "wrapper.h"

GemScanListener* listener;
GemManager* gemManager;
NSMutableDictionary* gems;

void init(OnReadyCallback onReady, OnInitializationErrorCallback onInitError, OnDeviceDiscoveredCallback onDeviceDisc)
{
    listener = [[GemScanListener alloc] initWithReadyCallback:onReady andWithInitializationErrorCallback:onInitError andWithDeviceDiscoveredCallback:onDeviceDisc];
    gemManager = [[GemManager alloc] initWithDelegate: listener];
    gems = [[NSMutableDictionary alloc] init];
}

void connectGem(Gem* gem, OnErrorOccurredCallback onError, OnStateChangedCallback onStateChanged, OnCombinedDataCallback onCombinedData)
{
    GemListener* gemListener = [[GemListener alloc] initWithGem:gem onErrorCallback:onError stateChangedCallback:onStateChanged combinedDataCallback:onCombinedData];
    [gems setObject:gemListener forKey:[gem.bluetoothAddress description]]; // We must keep a strong reference to gemListener so it won't get deallocated
    gem.delegate = gemListener; // Keeps a weak reference
    [gemManager connectGem: gem];
}

void disconnectGem(Gem* gem)
{
    [gemManager disconnectGem: gem];
}

void startScan()
{
    [gemManager startScan];
}

void stopScan()
{
    [gemManager stopScan];
}

BOOL isScanning()
{
    return [gemManager isScanning];
}

char* getGemAddress(Gem* gem)
{
    const char* utf8String = [gem.bluetoothAddress.description UTF8String];
    unsigned int size = strlen(utf8String) + 1;
    char* copy = (char*)malloc(size);
    memcpy(copy, utf8String, size);
    
    return copy;
}
