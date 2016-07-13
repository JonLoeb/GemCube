#define BLUETOOTH_ADDRESS_SIZE 6

/**
 * A class represting Bluetooth Low-Energy device MAC Address.
 */
@interface BluetoothAddress : NSObject // TODO consider rename it to GemBluetoothAddress

- (instancetype)initWithAddress:(const unsigned char*)address;
+ (instancetype)bluetoothAddressWithString:(NSString*)address;
- (NSString*)description;
- (BOOL)isEqual:(BluetoothAddress*)bluetoothAddress;

@end