/**
 * Copyright (c) 2015-present, Facebook, Inc.
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the
 * LICENSE file in the root directory of this source tree. An additional grant
 * of patent rights can be found in the PATENTS file in the same directory.
 *
 * @providesModule AccessibilityInfo
 * @flow
 */
'use strict';

const NativeModules = require('NativeModules');
const RN = require('react-native');

const RCTAccessibilityInfo = NativeModules.AccessibilityInfo;
const HIGH_CONTRAST_EVENT = 'highContrastDidChange';

type ChangeEventName = $Enum<{
  change: string,
  [HIGH_CONTRAST_EVENT]: string,
}>;

var warning = require('fbjs/lib/warning');

var AccessibilityInfo = {

  fetch: function(): Promise<*> {
    return new Promise((resolve, reject) => {
      reject('AccessibilityInfo is not supported on this platform.');
    });
  },

  fetchIsHighContrast: function (): Promise {
    return RCTAccessibilityInfo.fetchIsHighContrast();
  },

  addEventListener: function (
    eventName: string,
    handler: Function
  ): Object {
    if (eventName === HIGH_CONTRAST_EVENT) {
      return RN.NativeAppEventEmitter.addListener(eventName, handler);
    }

    warning(false, 'AccessibilityInfo does not support this event on this platform.');

    return {
      remove: () => { }
    };
  },

  removeEventListener: function(
    eventName: string,
    handler: Function
  ): void {
    if (eventName === HIGH_CONTRAST_EVENT) {
      RN.NativeAppEventEmitter.removeListener(eventName, handler);
    } else {
      warning(false, 'AccessibilityInfo does not support this event on this platform.');
    }
  },

};

module.exports = AccessibilityInfo;
