#import <CoreBluetooth/CoreBluetooth.h>
#import "BluetoothAddress.h"
#import "GemDelegate.h"

/**
 * A class represnting a single Gem device.
 */
@interface Gem : NSObject

/**
 * The Gem's MAC address.
 */
@property (nonatomic, readonly) BluetoothAddress* bluetoothAddress; // TODO make this readonly and find a way to change it in GemManager

/**
 * Get the hardware (model) version string.
 */
@property (nonatomic, readonly) NSString* hardwareVersion;

/**
 * Get the current fireware version string.
 */
@property (nonatomic, readonly) NSString* firmwareVersion;

/**
 * Get the battery level percentage.
 */
@property (nonatomic, readonly) NSNumber* batteryLevel;

/**
 * Indicating wheter this Gem is connected or not.
 */
@property (nonatomic, readonly) BOOL isConnected;

/**
 * Set the object that supposed to receive this Gem's events (the delegate object must conform to the GemDelegate protocol).
 */
@property (weak, nonatomic) id<GemDelegate> delegate;


// TODO make the constructors private?
- (instancetype)initWithPeripheral:(CBPeripheral*)peripheral andWithBluetoothAddress:(BluetoothAddress*)bluetoothAddress;

// TODO is this really neccesary?
+ (instancetype)gemWithPeripheral:(CBPeripheral*)peripheral andWithBluetoothAddress:(BluetoothAddress*)bluetoothAddress;

/**
 * Get the name of the Gem.
 */
- (NSString*)getName;

@end