#import <UIKit/UIKit.h>
// App haptics for First Shot S3. Firm = Heavy, Discreet = Light; generators are created once and reused.
// Unity's player thread is the iOS main thread; calls still hop to main if they arrive elsewhere.
static UIImpactFeedbackGenerator* sFirm = nil;
static UIImpactFeedbackGenerator* sDiscreet = nil;
static void SquashHapticsRun(void (^block)(void)) {
    if ([NSThread isMainThread]) block(); else dispatch_async(dispatch_get_main_queue(), block);
}
static void SquashHapticsEnsure() {
    if (sFirm == nil) sFirm = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
    if (sDiscreet == nil) sDiscreet = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
}
extern "C" void SquashHapticsPrepare() {
    SquashHapticsRun(^{ SquashHapticsEnsure(); [sFirm prepare]; [sDiscreet prepare]; });
}
// style: 0 = Firm, 1 = Discreet.
extern "C" void SquashHapticsImpact(int style) {
    SquashHapticsRun(^{ SquashHapticsEnsure(); if (style == 0) [sFirm impactOccurred]; else [sDiscreet impactOccurred]; });
}
