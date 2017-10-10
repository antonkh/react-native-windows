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

var NativeModules = require('NativeModules');
var RCTDeviceEventEmitter = require('RCTDeviceEventEmitter');

var RCTAccessibilityInfo = NativeModules.AccessibilityInfo;
var HIGH_CONTRAST_EVENT = 'highContrastDidChange';
var _subscriptions = new Map();

var AccessibilityInfo = {
  
  fetch: function(): Promise {
    return new Promise((resolve, reject) => {
      resolve(false);
    });
  },

  fetchIsHighContrast: function (): Promise {
    return RCTAccessibilityInfo.fetchIsHighContrast();
  },

  addEventListener: function (
    eventName: string,
    handler: Function
  ): void {
    if (eventName !== HIGH_CONTRAST_EVENT) {
      return {
        remove() { }
      };      
    }

    var listener = RCTDeviceEventEmitter.addListener(eventName, enabled => handler(enabled));

    _subscriptions.set(handler, listener);
  },

  removeEventListener: function(
    eventName: string,
    handler: Function
  ): void {
    var listener = _subscriptions.get(handler);
    listener && listener.remove();
    _subscriptions.delete(handler);  
  },

};

module.exports = AccessibilityInfo;
