#ifndef _H_GEMLISTENER
#define _H_GEMLISTENER

#include <GemSDK/GemSDK.h>

typedef void(*OnErrorOccurredCallback)(Gem*, int);
typedef void(*OnStateChangedCallback)(Gem*, GemState);
typedef void(*OnCombinedDataCallback)(Gem*, float*, float*);

@interface GemListener : NSObject <GemDelegate>
{
    OnErrorOccurredCallback _onErrorOccurredCallback;
    OnStateChangedCallback _onStateChangedCallback;
    OnCombinedDataCallback _onCombinedDataCallback;
}

@property (weak, nonatomic) Gem* gem;

- (instancetype)initWithGem:(Gem*)gem onErrorCallback:(OnErrorOccurredCallback)onErrorOccurred stateChangedCallback:(OnStateChangedCallback)onStateChanged combinedDataCallback:(OnCombinedDataCallback)onCombinedDataCallback;

@end

#endif //_H_GEMLISTENER