using UnityEngine;
using System.Collections;

namespace GemSDK.Unity
{
    internal class AndroidGem : AndroidJavaProxy, IGem
    {
        private bool bound;
        private bool isPedometerActive = false;
        private AndroidJavaObject activity;
        private AndroidJavaObject gemWrapper;

        public volatile GemState _state = GemState.Disconnected;

        public GemState State
        {
            get
            {
                return _state;
            }
        }

        public GemSystemInfo SystemInfo { get; private set; }

        private void Reconnect()
        {
            gemWrapper.Call("reconnect");
        }

        internal AndroidGem(string address, AndroidJavaObject activity)
            : base("com.gemsense.gemsdk.unity.UnityCallback")
        {
            address = address.ToUpper();

            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                gemWrapper = new AndroidJavaObject("com.gemsense.gemsdk.unity.GemWrapper", address, this);
            }));
        }

        public void Release()
        {
            if (gemWrapper != null)
            {
                gemWrapper.Call("release");
            }
        }

        public void SetPedometerActive(bool isActive)
        {
            isPedometerActive = isActive;

            if (State == GemState.Connected)
            {
                gemWrapper.Call("setPedometerActive", isActive);
            }
        }

        public void SetTapActive(bool isActive)
        {
            //Always active by default. Nothing to do
        }

        /// <summary>
        /// Works only with Gems vesion >= 1.2.0
        /// </summary>
        public void ResetPedometer()
        {
            if (State == GemState.Connected)
            {
                gemWrapper.Call("resetPedometer");
            }
        }

        public Quaternion Rotation
        {
            get
            {
                if (gemWrapper != null)
                {
                    float[] q = gemWrapper.Call<float[]>("getLastQuaternion");
                    return new Quaternion(q[1], q[2], q[3], q[0]);
                }
                else return Quaternion.identity;
            }
        }

        public Vector3 Acceleration
        {
            get
            {
                if (gemWrapper != null)
                {
                    float[] a = gemWrapper.Call<float[]>("getLastAcceleration");
                    return new Vector3(a[0], a[1], a[2]);
                }
                else return Vector3.zero;
            }
        }

        public PedometerData Pedometer
        {
            get
            {
                if (gemWrapper != null)
                {
                    AndroidJavaObject androidPedometer = gemWrapper.Call<AndroidJavaObject>("getLastPedometerData");

                    PedometerData pedometerData = new PedometerData()
                    {
                        Steps = androidPedometer.Get<int>("steps"),
                        WalkTime = androidPedometer.Get<float>("walkTime")
                    };

                    return pedometerData;
                }
                else return new PedometerData();
            }
        }

        /// <summary>
        /// Check if at least one tap was catched between calls
        /// </summary>
        /// <returns>True if tap has been catched</returns>
        public bool CheckTapOccured()
        {
            if (gemWrapper == null)
            {
                return false;
            }

            return gemWrapper.Call<bool>("checkTapOccured");
        }

        public void Calibrate()
        {
            CalibrateOrigin();
        }

        public void CalibrateOrigin()
        {
            if (gemWrapper != null)
            {
                gemWrapper.Call("calibrateOrigin");
            }
        }

        public void CalibrateAzimuth()
        {
            if (gemWrapper != null)
            {
                gemWrapper.Call("calibrateAzimuth");
            }
        }

        #region UnityCallback
        public void onStateChanged(int state)
        {
            this._state = (GemState)state;

            if (_state == GemState.Connected)
            {
                var sysInfoAndroid = gemWrapper.Call<AndroidJavaObject>("getGemSystemInfo");

                SystemInfo = new GemSystemInfo()
                {
                    Name = sysInfoAndroid.Get<string>("name"),
                    Address = sysInfoAndroid.Get<string>("address"),
                    FirmwareRevision = sysInfoAndroid.Get<string>("firmwareRevision"),
                    HardwareRevision = sysInfoAndroid.Get<string>("hardwareRevision")
                };

                SetPedometerActive(isPedometerActive);
            }
            Debug.Log("new connection status is " + ((GemState)state).ToString().ToUpper());
        }

        public void onErrorOccurred(int errorCode)
        {
            Debug.Log("error " + ((GemError)errorCode).ToString().ToUpper());
            //Gem wasn't found during scan timeout
            if ((GemError)errorCode == GemError.ConnectingTimeout)
            {
                Reconnect();
            }
        }

        #endregion
    }
}
