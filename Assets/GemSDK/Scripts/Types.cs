using UnityEngine;
using System.Collections;
using System;

namespace GemSDK.Unity
{
    public class GemSystemInfo
    {
        /// <summary>
        /// Gem bluetooth name
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// Gem bluetooth address
        /// </summary>
        public String Address { get; set; }
        public String FirmwareRevision { get; set; }
        public String HardwareRevision { get; set; }

    }

    public class PedometerData
    {
        /// <summary>
        /// Steps count
        /// </summary>
        public int Steps { get; set; }
        /// <summary>
        /// Walk time in seconds
        /// </summary>
        public float WalkTime { get; set; }
    }

    public enum GemState
    {
        Connecting = 2,
        Connected = 3,
        Disconnecting = 1,
        Disconnected = 0
    }

    internal enum GemError
    {
        ConnectingTimeout = 201,
        WrongDevice = 202,
        SensorsUnavailable = 203,
    }

#if (UNITY_ANDROID)
    internal enum GemRole
    {
        Primary = 0,
        Secondary = 1
    }
#elif (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
    internal enum GemRole : int
    {
        None = 0x00,
        Primary = 0x01,
        Secondary = 0x02
    }
#endif
}