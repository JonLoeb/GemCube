#import <Foundation/Foundation.h>

@interface GemRawData : NSObject

@property (nonatomic, readonly) NSArray<NSNumber*>* acceleration;
@property (nonatomic, readonly) NSArray<NSNumber*>* gyroscope;

- (instancetype)initWithAcceleration:(NSArray<NSNumber*>*)acceleration andWithGyroscope:(NSArray<NSNumber*>*)gyroscope;

@end