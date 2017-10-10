using ReactNative.Bridge;
using ReactNative.Modules.Core;
using System.Collections.Generic;
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

        public override IReadOnlyDictionary<string, object> Constants
        {
            get
            {
                return new Dictionary<string, object>
                {
                    // TODO: It would be better to make fetchIsHighContrast synchronous,
                    // but this is not supported by the framework at the moment.
                    { "initialHighContrast", _accessibility.HighContrast },
                };
            }
        }

        [ReactMethod]
        public void fetchIsHighContrast(IPromise promise)
        {
            promise.Resolve(_accessibility.HighContrast);
        }
    }
}