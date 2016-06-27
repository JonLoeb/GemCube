#ifndef _H_WRAPPER
#define _H_WRAPPER

#include <GemSDK/GemSDK.h>
#include "GemScanListener.h"
#include "GemListener.h"

#ifdef __cplusplus
extern "C"
{
#endif
    
    void init(OnReadyCallback onReady, OnInitializationErrorCallback onInitError, OnDeviceDiscoveredCallback onDeviceDisc);
    void connectGem(Gem* gem, OnErrorOccurredCallback onError, OnStateChangedCallback onStateChanged, OnCombinedDataCallback onCombinedData, OnTapDataCallback onTapData);
    void disconnectGem(Gem* gem);
    void startScan();
    void stopScan();
    BOOL isScanning();
    char* getGemAddress(Gem* gem);
    
#ifdef __cplusplus
}
#endif

#endif //_H_WRAPPER