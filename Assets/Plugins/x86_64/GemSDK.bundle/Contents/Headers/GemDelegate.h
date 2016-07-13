#import "GemCombinedData.h"
#import "GemRawData.h"

/**
 * Represents the state which a Gem can be in.
 */
typedef NS_ENUM(NSInteger, GemState)
{
    GemStateConnected,
    GemStateConnecting,
    GemStateDisconnected,
    GemStateDisconnecting
};

/**
 * An interface for Gem-related events.
 */
@protocol GemDelegate <NSObject>

@required

/**
 * Called when an error occured, all the details are in NSError.
 * @param error Contains details on the error.
 */
- (void)onErrorOccurred:(NSError*)error;

/**
 * Called everytime the Gem's state changed.
 * @param state The new state.
 */
- (void)onStateChanged:(GemState)state;

@optional

/**
 * Called when the Gem detects a tap.
 * @param direction The direction from which the tap is received.
 */
- (void)onTapData:(unsigned int)direction;

/**
 * Called everytime the Gem has new perdometer related data, usually when it detects a step.
 * @param steps The amount of steps counted since the perdometer was activated.
 * @param walkTime The amount of time in seconds the user has walked continuously.
 */
- (void)onPedometerData:(unsigned int)steps walkTime:(float)walkTime;

/**
 * Called when a new CombinedData is available from the Gem.
 * @param data The new data.
 */
- (void)onCombinedData:(GemCombinedData*)data;

/**
 * Called when a new RawData is available from the Gem.
 * @param data The new data.
 */
- (void)onRawData:(GemRawData*)data;

@end