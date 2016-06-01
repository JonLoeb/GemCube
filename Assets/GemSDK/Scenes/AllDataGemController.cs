using UnityEngine;
using UnityEngine.UI;
using GemSDK.Unity;
using System.Collections;
using System;
using GemSDK.QuaternionUtils;

public class AllDataGemController : MonoBehaviour
{
    public Text StateText;
    public Text RotationText;
    public Text AccelerationText;
    public Text PedometerText;
    public Text LastTapText;
    public Text AzimuthElevationText;
    public Text TiltElevationText;
    public Text EulerAnglesText;

    private float? lastTapTime = null;

    private IGem gem;
    
	// Use this for initialization
	void Start()
    {
        GemManager.Instance.Connect();

        //To get gem by number instead of address, on Android the Gem should be paired to Gem SDK Utility app
        gem = GemManager.Instance.GetGem(0);
        //gem = GemManager.Instance.GetGem("FF:FF:FF:FF:FF:FF");

        //Enable extra features
        if (gem != null)
        {
            gem.SetPedometerActive(true);
            gem.SetTapActive(true);
        }
    }

    void FixedUpdate()
    {
        if (gem != null)
        {
            if (Input.GetMouseButton(0))
            {
                gem.CalibrateAzimuth();

                //Use instead of CalibrateAzimuth() to calibrate also tilt and elevation
                //gem.ColibrateOrigin(); 
            }

            if(gem.CheckTapOccured()) 
            {
                lastTapTime = Time.unscaledTime;
            }

            //Show all the data
            Quaternion rotation = gem.Rotation;

            transform.rotation = gem.Rotation;
            StateText.text = gem.State.ToString();
            RotationText.text = gem.Rotation.ToString();
            AccelerationText.text = gem.Acceleration.ToString();

            PedometerData pedometer = gem.Pedometer;
            PedometerText.text = String.Format("{0} steps, {1:0.#}s walking", pedometer.Steps, pedometer.WalkTime); 

            AzimuthElevationText.text = AzimuthElevationConverter.Convert(rotation).ToString();
            TiltElevationText.text = TiltElevationConverter.Convert(rotation).ToString();
            EulerAnglesText.text = rotation.eulerAngles.ToString();

            if (lastTapTime != null)
            {
                LastTapText.text = string.Format("{0:0.0}s ago", Time.unscaledTime - lastTapTime);
            }
            else
            {
                LastTapText.text = "never";
            }
        }
    } 

    void OnApplicationQuit()
    {
        GemManager.Instance.Disconnect();
    }

    //For Android to unbind Gem Service when the app is not in focus
    void OnApplicationPause(bool paused)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (paused)
                GemManager.Instance.Disconnect();
            else
                GemManager.Instance.Connect();
        }
    }
}
