using UnityEngine;
using System.Collections;

namespace GemSDK.Unity
{
    internal class GemAndroidManager : IGemManager
    {
        private AndroidJavaObject service;
        private AndroidJavaObject activity;

        internal GemAndroidManager()
        {
            service = new AndroidJavaClass("com.gemsense.gemsdk.GemManager").CallStatic<AndroidJavaObject>("getDefault");
            activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        }

        public IGem GetGem(string address)
        {
            return new AndroidGem(address, activity);
        }

        public void Connect()
        {
            service.Call<bool>("bindService", activity);
        }

        public void Disconnect()
        {
            service.Call("unbindService", activity);
        }

        public IGem GetGem(int pos)
        {
            string[] gemsKnown;

            gemsKnown = GemSDKUtilityApp.GetWhiteList();

            if (gemsKnown != null && gemsKnown.Length > pos)
            {
                //Get the Gem instance for the curtain MAC address
                return GemManager.Instance.GetGem(gemsKnown[pos]);
            }
            else
            {
                return null;
            }
        }
    }
}