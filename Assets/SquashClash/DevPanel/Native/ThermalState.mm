#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
// Read-only OS diagnostic, excluded from production by BuildScript.
extern "C" int SquashClashThermalState() {
    return static_cast<int>([[NSProcessInfo processInfo] thermalState]);
}
extern "C" int SquashClashApplicationState() {
    return static_cast<int>([UIApplication sharedApplication].applicationState);
}
extern "C" int SquashClashIdleTimerDisabled() {
    return [UIApplication sharedApplication].idleTimerDisabled ? 1 : 0;
}
