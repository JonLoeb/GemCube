#ifndef _H_GEMLISTENER
#define _H_GEMLISTENER

#include <GemSDK/GemSDK.h>

typedef void(*OnErrorOccurredCallback)(Gem*, int);
typedef void(*OnStateChangedCallback)(Gem*, GemState);
typedef void(*OnCombinedDataCallback)(Gem*, float*, float*);
typedef void(*OnTapDataCallback)(Gem*, unsigned int);

@interface GemListener : NSObject <GemDelegate>
{
    OnErrorOccurredCallback _onErrorOccurredCallback;
    OnStateChangedCallback _onStateChangedCallback;
    OnCombinedDataCallback _onCombinedDataCallback;
    OnTapDataCallback _onTapDataCallback;
}

@property (weak, nonatomic) Gem* gem;

- (instancetype)initWithGem:(Gem*)gem onErrorCallback:(OnErrorOccurredCallback)onErrorOccurred stateChangedCallback:(OnStateChangedCallback)onStateChanged onCombinedDataCallback:(OnCombinedDataCallback)onCombinedData onTapDataCallback:(OnTapDataCallback)onTapData;

@end

#endif //_H_GEMLISTENER