#import <CoreBluetooth/CoreBluetooth.h>
#import "Gem.h"
#import "GemScanDelegate.h"

/**
 * A class for managing all Gem related operations (scan, connect, etc...).
 * The user of this class must set a delegate by assigning an object conforming to the protocol GemScanDelegate to GemManager::delegate before calling any other method.
 */
@interface GemManager : NSObject

/**
 * Set the object that receives events from this manager (the delegate object must conform to the GemScanDelegate protocol).
 */
@property (nonatomic, weak) id<GemScanDelegate> delegate;

- (instancetype)initWithDelegate:(id<GemScanDelegate>)delegate;

/**
 * Connects a Gem.
 * After calling this function, the Gem delegate's GemDelegate::onStateChanged: will be called with the state GemState::GemStateConnecting.
 * If the operation was successful, the Gem delegate's GemDelegate::onStateChanged: will be called with the parameter state set to GemStateConnected.
 * If the operation has failed, the Gem delegate's GemDelegate::onErrorOcurred: will be called with the error parameter containing details about why the connection has failed.
 * @see disconnectGem:
 */
- (void)connectGem:(Gem*)gem;

/**
 * Disconnects a Gem, before calling this function you must set a delegate for the Gem.
 * After calling this function, the Gem delegate's onStateChanged: will be called with the state GemStateDisconnecting.
 * When the Gem disconnected successfully, the Gem delegate's GemDelegate::onStateChanged: will be called with the parameter state set to GemStateDisconnected.
 * If the operation has failed, the Gem delegate's GemDelegate::onErrorOcurred: will be called with the error parameter containing details.
 * @see connectGem:
 */
- (void)disconnectGem:(Gem*)gem;

/**
 * Starts scanning for Gems, when a new Gem has been discovered GemScanDelegate::onGemDiscovered:rssi: will be called.
 * To conserve energy and performance for the iOS device, stop the scan by calling stopScan: as soon as you find the Gem you want to connect to.
 * @see stopScan:
 */
- (void)startScan;

/**
 * Stop scanning for Gems.
 * @see startScan:
 */
- (void)stopScan;

/**
 * @returns A boolean value indicating whether the scan is active or not.
 */
- (BOOL)isScanning;

/**
 * Enables the Gem's on-board pedometer, when data is available GemDelegate::onPedometer:walkTime: will be called.
 * @see disablePedometer:
 */
- (void)enablePedometer:(Gem*)gem;

/**
 * Disables the Gem's on-board pedometer.
 * @see enablePedometer:
 */
- (void)disablePedometer:(Gem*)gem;

/**
 * Make the Gem send RawData instead of CombinedData.
 * @see disableRawData:
 */
- (void)enableRawData:(Gem*)gem;

/**
 * Disables RawData and enables CombinedData back on.
 * @see enableRawData:
 */
- (void)disableRawData:(Gem*)gem;

@end