using ReactNative.Bridge;
using ReactNative.Modules.Core;
using System.Collections.Generic;
using Windows.UI.ViewManagement;

namespace ReactNative.Modules.Accessibilityinfo
{
    class AccessibilityInfoModule : ReactContextNativeModuleBase
    {
        private readonly AccessibilitySettings _accessibility = new AccessibilitySettings();
        private readonly UISettings _settings = new UISettings();

        private string GetHexString(UIElementType type)
        {
            var color = _settings.UIElementColor(type);
            return "rgba(" + color.R + "," + color.G + "," + color.B + "," + color.A + ")";
        }

        private IDictionary<string, string> GetHighContrastColors()
        {
            return new Dictionary<string, string>
            {
                {  "windowText", GetHexString(UIElementType.WindowText) },
                {  "hotlight", GetHexString(UIElementType.Hotlight) },
                {  "grayText", GetHexString(UIElementType.GrayText) },
                {  "highlightText", GetHexString(UIElementType.HighlightText) },
                {  "highlight", GetHexString(UIElementType.Highlight) },
                {  "buttonText", GetHexString(UIElementType.ButtonText) },
                {  "buttonFace", GetHexString(UIElementType.ButtonFace) },
                {  "window", GetHexString(UIElementType.Window) },
            };
        }

        public AccessibilityInfoModule(ReactContext reactContext)
            : base(reactContext)
        {
            _accessibility.HighContrastChanged += (sender, args) =>
            {
                Context.GetJavaScriptModule<RCTDeviceEventEmitter>()
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