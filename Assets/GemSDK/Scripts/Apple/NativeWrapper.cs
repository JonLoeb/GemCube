using System;
using System.Runtime.InteropServices;

namespace GemSDK.Unity.Apple
{
	internal static class NativeWrapper
	{
		#if UNITY_IOS
		private const string DLLName = "__Internal";
		#else
		private const string DLLName = "GemSDK";
		#endif
		// GemScanDelegate protocol

		public enum InitializationError : int
		{
			BluetoothUnknown = 0,
			BluetoothResetting = 1,
			BluetoothUnsupported = 2,
			BluetoothUnauthorized = 3,
			BluetoothOff = 4
		}

		public delegate void OnReadyCallback();

		public delegate void OnInitializationErrorCallback(InitializationError error);

		public delegate void OnDeviceDiscoveredCallback(IntPtr gem, int rssi);

		// GemDelegate protocol

		public enum GemState : int
		{
			Connected = 0,
			Connecting = 1,
			Disconnected = 2,
			Disconnecting = 3
		};

		public delegate void OnErrorOccurredCallback(IntPtr gem, int error);

		public delegate void OnStateChangedCallback(IntPtr gem, GemState state);

		public delegate void OnCombinedDataCallback(IntPtr gem, IntPtr quaternion, IntPtr acceleration);

		public delegate void OnTapDataCallback(IntPtr gem, uint direction);

		[DllImport(DLLName)]
		public static extern void init(OnReadyCallback onReady, OnInitializationErrorCallback onInitError, OnDeviceDiscoveredCallback onDeviceDisc);

		[DllImport(DLLName)]
		public static extern void connectGem(IntPtr gem, OnErrorOccurredCallback onError, OnStateChangedCallback onStateChanged, OnCombinedDataCallback onCombinedData, OnTapDataCallback onTapData);

		[DllImport(DLLName)]
		public static extern void disconnectGem(IntPtr gem);

		[DllImport(DLLName)]
		public static extern void startScan();

		[DllImport(DLLName)]
		public static extern void stopScan();

		[DllImport(DLLName)]
		public static extern bool isScanning();

		[DllImport(DLLName)]
		public static extern string getGemAddress(IntPtr gem);

	}
}

