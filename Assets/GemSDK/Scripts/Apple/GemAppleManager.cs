using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using AOT;

namespace GemSDK.Unity.Apple
{
	public class GemAppleManager : IGemManager
	{
		private static GemAppleManager instance = null; // need to keep one instance local so the static callbacks will be able to access it
		private volatile bool ready; // volatile because it will be accessed from callbacks and Unity main thread

		private readonly object isScanningSyncObj = new object(); 
		private volatile bool isScanning = false;

		public GemAppleManager()
		{
			if (instance == null)
				instance = this;
			else
				throw new Exception("Cannot create more than on instance of GemAppleManager");

			NativeWrapper.init(OnReady, OnInitializationError, OnDeviceDiscovered);
		}

		public void Connect()
		{
			// TODO
		}

		public void Disconnect()
		{
			instance.StopScan();

			lock (AppleGem.Gems)
			{
				foreach (AppleGem gem in AppleGem.Gems)
				{
					if (gem.State != GemState.Disconnected)
						gem.Release();
				}
			}
		}

		public IGem GetGem(String address)
		{
			AppleGem gem = AppleGem.FindGemByAddress(address);

			if (gem == null)
			{
				gem = new AppleGem(address);

				if(ready)
					StartScan();
			}

			return gem;
		}

		public IGem GetGem(int pos)
		{
			throw new NotSupportedException("Currently this function is not supported on iOS");
		}

		private void StartScan()
		{
			lock (isScanningSyncObj)
			{
				if (!isScanning)
				{
					NativeWrapper.startScan();
					isScanning = true;
				}
			}

			Debug.Log("Scan started");
		}

		private void StopScan()
		{
			lock (isScanningSyncObj)
			{
				if (isScanning)
				{
					NativeWrapper.stopScan();
					isScanning = false;
				}
			}

			Debug.Log("Scan stopped");
		}

		[MonoPInvokeCallback(typeof(NativeWrapper.OnReadyCallback))]
		static void OnReady()
		{
			Debug.Log("GemSDK: Ready");
			instance.ready = true;

			bool pending = false;

			lock (AppleGem.Gems)
			{
				foreach (AppleGem gem in AppleGem.Gems)
				{
					if (gem.GemPointer == IntPtr.Zero)
					{
						pending = true;
						break;
					}	
				}
			}

			if (pending)
				instance.StartScan();
		}

		[MonoPInvokeCallback(typeof(NativeWrapper.OnInitializationErrorCallback))]
		static void OnInitializationError(NativeWrapper.InitializationError error)
		{
			Debug.LogError("GemSDK: init error: " + error.ToString());
		}

		[MonoPInvokeCallback(typeof(NativeWrapper.OnDeviceDiscoveredCallback))]
		static void OnDeviceDiscovered(IntPtr gem, int rssi)
		{
			string address = NativeWrapper.getGemAddress(gem);
			Debug.Log("OnDeviceDiscovered: " + address + ", rssi: " + rssi);
			AppleGem _gem = AppleGem.FindGemByAddress(address);

			if (_gem != null)
			{
				if (_gem.GemPointer == IntPtr.Zero)
				{
					_gem.GemPointer = gem;
					_gem.Connect();
				}
				else if (_gem.GemPointer != gem)
				{
					Debug.LogWarningFormat("Two pointers ({0},{1}) for the same Gem address {2}", _gem.GemPointer, gem, address);
				}

				// check if other gems are pending, if found then don't stop the scan
				foreach(AppleGem iosGem in AppleGem.Gems)
				{
					if (iosGem.GemPointer == IntPtr.Zero)
						return;
				}

				instance.StopScan();
			}
		}
	}
}