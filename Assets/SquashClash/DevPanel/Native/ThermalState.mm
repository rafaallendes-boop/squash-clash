#import <Foundation/Foundation.h>
// Read-only OS diagnostic, excluded from production by BuildScript.
extern "C" int SquashClashThermalState() {
    return static_cast<int>([[NSProcessInfo processInfo] thermalState]);
}
