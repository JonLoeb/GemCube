using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GemSDK.Unity
{
    public class GemAndroidScanner : AndroidJavaProxy
    {
        private AndroidJavaObject activity;
        private AndroidJavaObject scanner;
        private object _lock = new object();
        private List<GemInfo> devices;

        public class GemInfo 
        {
            public string Address;
            public int RSSI;

            public GemInfo(string address, int rssi) 
            {
                Address = address;
                RSSI = rssi;
            }
        }

        /// <summary>
        /// List of information of the gems that have been found 
        /// </summary>
        public List<GemInfo> Devices {
            get 
            {
                lock(_lock) 
                {
                    return devices;
                }
            }
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="address">Address of the gem to connect</param>
        public GemAndroidScanner()
            : base("com.gemsense.gemsdk.GemScanListener")
        {
            devices = new List<GemInfo>();

            activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                scanner = new AndroidJavaObject("com.gemsense.gemsdk.GemScanner", this);   
            }));
        }
        
        /// <summary>
        /// Start scanning process for curtatian time
        /// </summary>
        /// <param name="seconds"></param>
        public void StartScan(int seconds) {
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                scanner.Call("scan", seconds);
            }));
        }

        /// <summary>
        /// Stop scanning process
        /// </summary>
        public void StopScan() {
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                scanner.Call("stop");
            }));
        }

        //Calback implementation   
        public void onScanFinish() {

        }

        public void onDeviceDiscovered(AndroidJavaObject device, int rssi) {
            string address = device.Call<string>("getAddress");

            lock(_lock) 
            {
                bool found = false;
                
                foreach(var info in devices) 
                {
                    if(address == info.Address) 
                    {
                        info.RSSI = rssi;

                        found = true;
                        break;
                    }
                }


                if(!found) {
                    devices.Add(new GemInfo(address, rssi));
                }
            }
        }
    }
}
