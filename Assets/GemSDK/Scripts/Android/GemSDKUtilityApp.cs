using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GemSDK.Unity {
    /// <summary>
    /// Allows to get information from "GemSDK Tools" app if it's installed
    /// </summary>
    public class GemSDKUtilityApp
    {
        /// <summary>
        /// Gets gems adresses that were saved to the white list in "Gem SDK Utility" app.
        /// If app is not installed its Google Play Market page will be opened
        /// </summary>
        /// <returns>Gems addresses</returns>
        public static string[] GetWhiteList()
        {
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            var androidClass = new AndroidJavaClass("com.gemsense.gemsdk.GemSDKUtilityApp");

            return androidClass.CallStatic<string[]>("getWhiteList", activity);
        }
    }
}
