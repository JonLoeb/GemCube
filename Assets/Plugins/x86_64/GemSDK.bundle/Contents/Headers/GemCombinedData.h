#import <Foundation/Foundation.h>

/**
 * Container for CombinedData
 */
@interface GemCombinedData : NSObject

/**
 * Acceleration 3D vector containing 3 floats in the following order: x, y, z.
 */
@property (nonatomic, readonly) NSArray<NSNumber*>* acceleration;

/**
 * Quaternion 4D vector containing 4 floats in the following order: x, y, z, w.
 */
@property (nonatomic, readonly) NSArray<NSNumber*>* quaternion;

- (instancetype)initWithAcceleration:(NSArray<NSNumber*>*)acceleration andWithQuaternion:(NSArray<NSNumber*>*)quaternion;

@end