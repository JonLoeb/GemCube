using System;
using UnityEngine;

namespace GemSDK.Unity
{
    public interface IGem
    {
        /// <summary>
        /// Connection state
        /// </summary>
        GemState State { get; }

        /// <summary>
        /// Rotation
        /// </summary>
        Quaternion Rotation { get; }

        /// <summary>
        /// Acceleration
        /// </summary>
        Vector3 Acceleration { get; }
        
        /// <summary>
        /// Pedometer
        /// </summary>
        PedometerData Pedometer { get; }

        /// <summary>
        /// Detailed information about the Gem: model, address, firmware version, hardware version 
        ///</summary>
        ///<remarks>Not available on Windows</remarks> 
        GemSystemInfo SystemInfo { get; }

        /// <summary>
        /// Disconnect the Gem
        /// </summary>
        void Release();

        /// <summary>
        /// [Deprecated] Use CalibrateOrigin() instead of it
        /// </summary>
        void Calibrate();

        /// <summary>
        /// Calibrates rotation (uses current rotation as an origin)
        /// </summary>
        void CalibrateOrigin();

        /// <summary>
        /// Calibrates only azimuth (compensates azimuth origin ambiguity)
        /// </summary>
        void CalibrateAzimuth();

        ///// <summary>
        ///// Attempt to connect again to the Gem
        ///// </summary>
        //void Reconnect();

        /// <summary>
        /// Enable/Disable pedometer data streaming
        /// </summary>
        /// <param name="isActive"><code>true</code> to enbale or <code>false</code> to disable</param>
        void SetPedometerActive(bool isActive);

        /// <summary>
        /// Enable/Disable tap event handling
        /// </summary>
        /// <param name="isActive"><code>true</code> to enbale or <code>false</code> to disable</param>
        /// <remarks>On Android is always active by default</remarks> 
        void SetTapActive(bool isActive);

        /// <summary>
        /// Set current pedometer values to 0. Be careful: with firmware version below 1.2.0 will cause restart of the Gem 
        /// </summary>
        /// <remarks>Not available on Windows</remarks> 
        void ResetPedometer();

        /// <summary>
        /// Check if at least one tap was catched between calls
        /// </summary>
        /// <returns>True if tap has been catched</returns>
        bool CheckTapOccured();
    }
}
