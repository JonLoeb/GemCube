/**
 * Initialization failure reasons.
 */
typedef NS_ENUM(NSInteger, InitializationError)
{
    InitializationErrorBluetoothUnknown = 0,    /**< Bluetooth is in an unknown state, new call to either this function or to onReady will be invoked soon with more data. */
    InitializationErrorBluetoothResetting,      /**< The connection to the system's bluetooth service is momentarily lost, a new call will be invoked when new information is available.  */
    InitializationErrorBluetoothUnsupported,    /**< This device does not support Bluetooth Low Energy. */
    InitializationErrorBluetoothUnauthorized,   /**< This application is not authorized to use Bluetooth Low Energy */
    InitializationErrorBluetoothOff             /**< Bluetooth is currently turned-off in this device. */
};

/*
 * An interface for GemManager's operations.
 */
@protocol GemScanDelegate

@required

/**
 * Called when GemManager is ready to start receiving commands.
 */
- (void)onReady;

/**
 * Called when GemManager initialization failed, usually failed to detect bluetooth or something similar.
 * @param error Contains details on the error.
 */
- (void)onInitializationError:(InitializationError)error;

/**
 * Called everytime a new Gem is discovered.
 * @param gem The gem discovered.
 * @param rssi An RSSI value representing the signal intensity of the discovered Gem. Usually useful for rough estimate of the Gem's distance from the iOS device, the lower the RSSI value is the closest the Gem is to the device.
 */
- (void)onDeviceDiscovered:(Gem*)gem rssi:(NSNumber*)rssi;

@end