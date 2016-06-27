#include "GemListener.h"

@implementation GemListener

- (instancetype)initWithGem:(Gem*)gem onErrorCallback:(OnErrorOccurredCallback)onErrorOccurred stateChangedCallback:(OnStateChangedCallback)onStateChanged onCombinedDataCallback:(OnCombinedDataCallback)onCombinedData onTapDataCallback:(OnTapDataCallback)onTapData
{
    if(!(self = [super init]))
    {
        // TODO handle error
    }
    
    self.gem = gem;
    _onErrorOccurredCallback = onErrorOccurred;
    _onStateChangedCallback = onStateChanged;
    _onCombinedDataCallback = onCombinedData;
    _onTapDataCallback = onTapData;
    
    return self;
}

- (void)onErrorOccurred:(NSError*)error
{
    _onErrorOccurredCallback(self.gem, (int)error.code);
}

- (void)onStateChanged:(GemState)state
{
    _onStateChangedCallback(self.gem, state);
}

- (void)onCombinedData:(GemCombinedData*)data
{
    float quaternion[4];
    float acceleration[3];
    
    for(NSUInteger i = 0; i < 4; i++)
        quaternion[i] = [[data.quaternion objectAtIndex:i] floatValue];
    
    for(NSUInteger i = 0; i < 3; i++)
        acceleration[i] = [[data.acceleration objectAtIndex:i] floatValue];
    
    _onCombinedDataCallback(self.gem, quaternion, acceleration);
}

- (void)onTapData:(unsigned int)direction
{
    _onTapDataCallback(self.gem, direction);
}

@end