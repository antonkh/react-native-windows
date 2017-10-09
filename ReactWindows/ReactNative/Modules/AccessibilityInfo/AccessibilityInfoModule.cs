using ReactNative.Bridge;
using ReactNative.Modules.Core;
using Windows.UI.ViewManagement;

namespace ReactNative.Modules.Accessibilityinfo
{
    class AccessibilityInfoModule : ReactContextNativeModuleBase
    {
        private readonly AccessibilitySettings _accessibility = new AccessibilitySettings();

        public AccessibilityInfoModule(ReactContext reactContext)
            : base(reactContext)
        {
            _accessibility.HighContrastChanged += (sender, args) =>
            {
                Context.GetJavaScriptModule<RCTNativeAppEventEmitter>()
                    .emit("highContrastDidChange", sender.HighContrast);
            };
        }

        public override string Name => "AccessibilityInfo";

        [ReactMethod]
        public void fetchIsHighContrast(IPromise promise)
        {
            promise.Resolve(_accessibility.HighContrast);
        }
    }
}