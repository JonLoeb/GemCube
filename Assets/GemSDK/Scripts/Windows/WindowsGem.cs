using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace GemSDK.Unity.Windows
{
    public class WindowsGem : IGem
    {
        private readonly object lastMutex = new object();

        private Quaternion _last;
        private Vector3 _lastAcceleration;
        private readonly object lastAccelerationMutex = new object();
        private Quaternion reference;
        private byte[] address;
        private string addressStr;
        private bool tapOccured = false;

        private bool tapActive = false;
        private bool pedometerActive = false;

        private NativeWrapper.gemOnCombinedDataReceived onCombinedDataRecieved;
        private NativeWrapper.gemOnPedometerReceived onPedometerDataRecieved;
        private NativeWrapper.gemOnTapDataReceived onTapDataRecieved;
        private NativeWrapper.gemOnStateChanged onStateChanged;

        private Quaternion Last
        {
            get
            {
                Quaternion result;

                lock (lastMutex)
                {
                    result = _last;
                }

                return result;
            }

            set
            {
                lock (lastMutex)
                {
                    _last = value;
                }
            }
        }

        private Vector3 LastAcceleration
        {
            get
            {
                Vector3 result;

                lock (lastAccelerationMutex)
                {
                    result = _lastAcceleration;
                }

                return result;
            }

            set
            {
                lock (lastAccelerationMutex)
                {
                    _lastAcceleration = value;
                }
            }
        }

        public WindowsGem(string address)
        {
            _last = Quaternion.identity;
            reference = Quaternion.identity;
            _lastAcceleration = Vector3.zero;
            State = GemState.Disconnected;
            Pedometer = new PedometerData();

            string[] str = address.Split(':');
            var addrBytes = new byte[str.Length];

            for (int i = 0; i < addrBytes.Length; i++)
            {
                addrBytes[i] = Byte.Parse(str[i], System.Globalization.NumberStyles.HexNumber);

            }

            this.address = addrBytes;
            this.addressStr = address;

            State = GemState.Disconnected;

            onCombinedDataRecieved = new NativeWrapper.gemOnCombinedDataReceived(CombinedDataReceived);
            NativeWrapper.gemSetOnCombinedData(addrBytes, onCombinedDataRecieved);

            onPedometerDataRecieved = new NativeWrapper.gemOnPedometerReceived(PedometerDataReceived);
            NativeWrapper.gemSetOnPedometerData(addrBytes, onPedometerDataRecieved);

            onTapDataRecieved = new NativeWrapper.gemOnTapDataReceived(TapDataReceived);
            NativeWrapper.gemSetOnTapData(addrBytes, onTapDataRecieved);

            onStateChanged = new NativeWrapper.gemOnStateChanged(StateChanged);
            NativeWrapper.gemSetOnStateChanged(addrBytes, onStateChanged);
        }

        public GemState State { get; private set; }

        public Quaternion Rotation { get { return reference * Last; } }

        public Vector3 Acceleration { get { return LastAcceleration; } }

        public void Calibrate()
        {
            CalibrateOrigin();
        }

        public void CalibrateOrigin()
        {
            reference = Quaternion.Inverse(Last);
        }

        public void CalibrateAzimuth()
        {
            reference = Quaternion.AngleAxis( -GetAzimuth(Last), Vector3.up);
        }

        public void Connect()
        {
            GemStatusCode result = NativeWrapper.gemConnect(address);

            Debug.Log("GemSDK: Connection to \"" + addressStr + "\" -> " + result.ToString());
        }

        public void Release()
        {
            Debug.Log("GemSDK: Disconnection from \"" + addressStr + "\" -> " + NativeWrapper.gemDisconnect(address).ToString());
        }

        private void Reconnect()
        {
            if (State == GemState.Disconnected)
            {
                Release();
                Connect();
            }
        }

        private void StateChanged(NativeWrapper.GemState state)
        {
            Debug.Log("GemSDK: State changed from \"" + addressStr + "\" -> " + state.ToString());

            if (state == NativeWrapper.GemState.Connected)
            {
                State = GemState.Connected;

                if (tapActive)
                    SetTapActive(true);

                if (pedometerActive)
                    SetPedometerActive(true);
            }
            else if (state == NativeWrapper.GemState.Connecting)
            {
                State = GemState.Connecting;
            }
            else if (state == NativeWrapper.GemState.Disconnected)
            {
                GemState lastState = State;
                State = GemState.Disconnected;
                
                if (lastState == GemState.Connecting)
                {
                    Reconnect();
                }
            }
            else if (state == NativeWrapper.GemState.Disconnecting)
            {
                State = GemState.Disconnecting;
            }
        }

        #region not implemented yet

        public void ResetPedometer()
        {
            //throw new NotImplementedException();   
        }

        #endregion

        public GemSystemInfo SystemInfo { 
            get 
            { 
                GemInfo info;
                if (NativeWrapper.gemGetInfo(address, out info) == GemStatusCode.Success)
                {
                    GemSystemInfo res = new GemSystemInfo();
                    res.Address = addressStr;
                    res.FirmwareRevision = info.firmwareVer;
                    res.HardwareRevision = info.hardwareVer;
                    res.Name = info.name;

                    return res;
                }
                else
                {
                    return null;
                }
            } 
        }

        public PedometerData Pedometer { get; private set; }

        public void SetPedometerActive(bool isActive)
        {
            if (State != GemState.Connected)
            {
                //Save state to activate after connection
                pedometerActive = true;
                return;
            }

            if (isActive) {
                NativeWrapper.gemEnablePedometer(address);
            }
            else {
                NativeWrapper.gemDisablePedometer(address);
            }
        }

        public void SetTapActive(bool isActive) 
        {
            if (State != GemState.Connected)
            {
                //Save state to activate after connection
                tapActive = true;
                return;
            }

            if(isActive) {
                NativeWrapper.gemEnableTap(address);
            }
            else {
                NativeWrapper.gemDisableTap(address);
            }
        }
             
        public bool CheckTapOccured()
        {
            if(tapOccured == true) {
                tapOccured = false;
                return true;
            }
            else {
                return false;
            }
        }

        private void CombinedDataReceived(IntPtr quaternion, IntPtr acceleration)
        {
            float[] q = new float[4];
            Marshal.Copy(quaternion, q, 0, 4);

            float[] acc = new float[3];
            Marshal.Copy(acceleration, acc, 0, 3);

            Last = new Quaternion(q[1], q[2], q[3], q[0]);
            LastAcceleration = new Vector3(acc[0], acc[1], acc[2]);
        }

        private void PedometerDataReceived(uint steps, float walktime)
        {
            PedometerData pedometer = new PedometerData();
            pedometer.Steps = (int)steps;
            pedometer.WalkTime = walktime;

            Pedometer = pedometer;
        }

        private void TapDataReceived(uint direction)
        {
            tapOccured = true;
        }

        private float GetAzimuth(Quaternion quat)
        {
            Vector3 forward = quat * new Vector3(0f, 0f, 1f);
            Vector3 right = quat * new Vector3(1f, 0f, 0f);

            float forwProj = Mathf.Sqrt(forward.x * forward.x + forward.z * forward.z);
            float rightProj = Mathf.Sqrt(right.x * right.x + right.z * right.z);

            float angle;

            if (forwProj >= rightProj)
            {
                angle = Mathf.Acos(forward.z / forwProj);

            }
            else
            {
                angle = Mathf.Acos(right.x / rightProj);
            }

            if (forward.x < 0)
                angle = 2f * (float)Mathf.PI - angle;

            angle *= 180f / Mathf.PI;

            return angle;
        }
    }
}