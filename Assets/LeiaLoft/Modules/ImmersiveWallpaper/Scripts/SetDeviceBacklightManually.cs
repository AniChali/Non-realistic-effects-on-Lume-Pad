using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeiaLoft;

namespace LeiaLoft.Examples.ImmersiveMode
{
	/// <summary>
	/// Class for setting a LitByLeia Device's backlight without engaging Unity SDK further
	/// </summary>
	public class SetDeviceBacklightManually : MonoBehaviour
	{
		private enum SupportedDeviceBacklightModes
		{
			MODE_IMMERSIVE = 5,
			MODE_2D = 2,
			MODE_3D = 3
		};

#pragma warning disable 649
		[SerializeField] private SupportedDeviceBacklightModes initialDeviceBacklightMode;
#pragma warning restore 649

		private ILeiaDevice mDevice;

		private ILeiaDevice device
		{
			get
			{
				if (mDevice == null)
				{
					if (Application.platform == RuntimePlatform.Android)
					{
						mDevice = new AndroidLeiaDevice("androidNoInterlacing");
					}
					else { mDevice = new OfflineEmulationLeiaDevice("androidNoInterlacing"); }
				}
				return mDevice;
			}
		}

		/// <summary>
        /// Allow users to set backlight mode using a Unity drop-down. Confirm that your target device supports this mode
        /// </summary>
		public int LightfieldInt
		{
			get
			{
				return device.GetBacklightMode();
			}
			set
			{
				switch(value)
                {
					case 0: device.SetBacklightMode((int) SupportedDeviceBacklightModes.MODE_IMMERSIVE); return;
					case 1: device.SetBacklightMode((int) SupportedDeviceBacklightModes.MODE_2D); return;
					case 2: device.SetBacklightMode((int) SupportedDeviceBacklightModes.MODE_3D); return;

					default:
						LogUtil.Log(LogLevel.Error, "Index {0} not possible to convert into a backlight mode", value); return;
                }
			}
		}

		// Use this for initialization
		void Start()
		{
			device.SetBacklightMode((int)initialDeviceBacklightMode);
		}
	}
}
