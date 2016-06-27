using UnityEngine;
using System;
using System.Collections;
using GemSDK.Unity.Apple;
using GemSDK.Unity.Windows;

namespace GemSDK.Unity
{
    /// <summary>
    /// Represents connection to remote Gem Service regardless of platform
    /// </summary>
    public class GemManager
    {
        private static IGemManager instance; 
     
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static IGemManager Instance
        {
            get
            {
                if (instance == null)
                {
                    #if (UNITY_ANDROID && !UNITY_EDITOR)
                        instance = new GemAndroidManager();
					#elif (UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
						instance = new GemAppleManager();
                    #elif (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
                        instance = new GemWindowsManager();
                    #else
                        Debug.Log("GemSDK: This platform is not supported");
                    #endif
                }     
                
                return instance;
            }
        }
    }
}
