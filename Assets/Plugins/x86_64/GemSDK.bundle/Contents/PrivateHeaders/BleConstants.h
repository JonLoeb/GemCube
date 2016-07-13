#import <CoreBluetooth/CoreBluetooth.h>

@interface BleConstants : NSObject

+ (CBUUID*)GemServiceDataUUID;
+ (CBUUID*)GemCharacteristicDataCombinedUUID;
+ (CBUUID*)GemCharacteristicDataQuaternionUUID;
+ (CBUUID*)GemCharacteristicDataSensorsUUID;
+ (CBUUID*)GemCharacteristicDataPedometerUUID;
+ (CBUUID*)GemCharacteristicDataTapUUID;

// Battery Service

+ (CBUUID*)GattServiceBatteryUUID;
+ (CBUUID*)GattCharacteristicBatteryLevelUID;

// Info Service

+ (CBUUID*)GattServiceInfoUUID;
+ (CBUUID*)GattCharacteristicNameUID;
+ (CBUUID*)GattCharacteristicFirmwareUID;
+ (CBUUID*)GattCharacteristicHardwareUID;

@end