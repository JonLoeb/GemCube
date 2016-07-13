using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace GemSDK.Unity.Apple
{
	public class AppleGem : IGem
	{
		public GemState State { get; private set; }
		public Quaternion Rotation { get { return reference * LastQuaternion; } }
		public Vector3 Acceleration { get { return LastAcceleration; } }
		public PedometerData Pedometer { get; private set; }
		public GemSystemInfo SystemInfo { get; private set; }

		internal IntPtr GemPointer { get; set; }
		internal string Address { get; private set; }

		public static List<AppleGem> Gems = new List<AppleGem>();

		private Quaternion reference = Quaternion.identity;

		private readonly object lastAccelerationMutex = new object();
		private Vector3 _lastAcceleration = Vector3.zero;
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

		private readonly object lastQuaternionMutex = new object();
		private Quaternion _lastQuaternion = Quaternion.identity;
		private Quaternion LastQuaternion
		{
			get
			{
				Quaternion result;

				lock (lastQuaternionMutex)
				{
					result = _lastQuaternion;
				}

				return result;
			}

			set
			{
				lock (lastQuaternionMutex)
				{
					_lastQuaternion = value;
				}
			}
		}

		private bool initalized = false;
		private volatile bool tapOccured = false;

		internal AppleGem(string address)
		{
			GemPointer = IntPtr.Zero;
			Address = address;

			lock (Gems)
			{
				Gems.Add(this);
			}
		}

		internal void Connect()
		{
			NativeWrapper.connectGem(GemPointer, OnErrorOccured, OnStateChanged, OnCombinedData, OnTapData);
			initalized = true;
		}

		public void Release()
		{
			if (!initalized)
				return;

			NativeWrapper.disconnectGem(GemPointer);
			initalized = false;
		}

		public void Calibrate()
		{
			CalibrateOrigin();
		}

		public void CalibrateOrigin()
		{
			reference = Quaternion.Inverse(LastQuaternion);
		}

		public void CalibrateAzimuth()
		{
			reference = Quaternion.AngleAxis( -GetAzimuth(LastQuaternion), Vector3.up);
		}

		public void SetPedometerActive(bool isActive)
		{
			throw new NotSupportedException("Not supported on Apple devices");
		}

		public void SetTapActive(bool isActive)
		{
			// Tap is enabled by default in iOS and OSX
		}

		public void ResetPedometer()
		{
			throw new NotSupportedException("Not supported on Apple devices");
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

		[MonoPInvokeCallback(typeof(NativeWrapper.OnErrorOccurredCallback))]
		private static void OnErrorOccured(IntPtr gem, int error)
		{
			AppleGem _gem = FindGemByPointer(gem);

			if (_gem == null)
				throw new Exception("OnErrorOccured called on non-existing/disconnected gem");

			if (error == 6)
			{
				Debug.LogErrorFormat("{0}: timed out, trying to reconnect...", _gem.Address);
				_gem.Connect();
			}
			else
			{
				Debug.LogErrorFormat("{0}: error: {1}", _gem.Address, error);
			}
		}

		[MonoPInvokeCallback(typeof(NativeWrapper.OnStateChangedCallback))]
		private static void OnStateChanged(IntPtr gem, NativeWrapper.GemState state)
		{
			AppleGem _gem = FindGemByPointer(gem);

			Debug.LogFormat("{0}, {1}", _gem.Address, state);

			if (_gem == null)
				throw new Exception("OnStateChanged called on non-existing/disconnected gem");
			
			switch (state)
			{
			case NativeWrapper.GemState.Connected:
				_gem.State = GemState.Connected;
				break;
			case NativeWrapper.GemState.Connecting:
				_gem.State = GemState.Connecting;
				break;
			case NativeWrapper.GemState.Disconnected:
				_gem.State = GemState.Disconnected;
				break;
			case NativeWrapper.GemState.Disconnecting:
				_gem.State = GemState.Disconnecting;
				break;
			}
		}

		[MonoPInvokeCallback(typeof(NativeWrapper.OnCombinedDataCallback))]
		private static void OnCombinedData(IntPtr gem, IntPtr quaternion, IntPtr acceleration)
		{
			float[] q = new float[4];
			Marshal.Copy(quaternion, q, 0, 4);

			float[] acc = new float[3];
			Marshal.Copy(acceleration, acc, 0, 3);

			AppleGem _gem = FindGemByPointer(gem);

			if (_gem == null)
				throw new Exception("OnCombinedData called on non-existing/disconnected gem");

			_gem.LastQuaternion = new Quaternion(q[1], q[2], q[3], q[0]);
			_gem.LastAcceleration = new Vector3(acc[0], acc[1], acc[2]);

			//LastAcceleration = new Vector3(acc[0], acc[1], acc[2]);
		}

		[MonoPInvokeCallback(typeof(NativeWrapper.OnTapDataCallback))]
		private static void OnTapData(IntPtr gem, uint direction)
		{
			AppleGem _gem = FindGemByPointer(gem);

			if (_gem == null)
				throw new Exception("OnTapData called on non-existing/disconnected gem");

			_gem.tapOccured = true;
		}

		public static AppleGem FindGemByPointer(IntPtr pointer)
		{
			// TODO optimize

			lock (Gems)
			{
				foreach (AppleGem gem in Gems)
				{
					if (gem.GemPointer == pointer)
						return gem;
				}
			}

			return null;
		}

		public static AppleGem FindGemByAddress(string address)
		{
			// TODO optimize

			lock (Gems)
			{
				foreach (AppleGem gem in Gems)
				{
					if (gem.Address == address)
						return gem;
				}
			}

			return null;
		}
	}
}

