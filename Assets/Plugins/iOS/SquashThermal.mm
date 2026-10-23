#import <Foundation/Foundation.h>
// Runtime diagnostics for both dev and prod; independent of the DevPanel spike bridge.
extern "C" int SquashThermalState() {
    return static_cast<int>([[NSProcessInfo processInfo] thermalState]);
}
