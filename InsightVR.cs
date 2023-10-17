using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HP.Omnicept;
using HP.Omnicept.Messaging;
using HP.Omnicept.Messaging.Messages;
using System.IO;

public class InsightVR : MonoBehaviour
{
    [SerializeField]
    private bool showHeartRateMessages = true;
    [SerializeField]
    private bool showPPGMessages = true;
    [SerializeField]
    private bool showEyeTrackingMessages = true;
    [SerializeField]
    private bool showVsyncMessages = true;
    [SerializeField]
    private bool showCameraImageMessages = true;
    [SerializeField]
    private bool showCameraImageTexture = true;
    [SerializeField]
    private bool showIMUMessages = true;
    [SerializeField]
    private bool showSubscriptionResultListMessages = true;

    public Material cameraImageMat;
    private Texture2D cameraImageTex2D;

    public int startTime;

    public int timeStamp;


    public void Start()
    {
        cameraImageTex2D = new Texture2D(400, 400, TextureFormat.R8, false);
        if (cameraImageMat != null)
        {
            cameraImageMat.mainTexture = cameraImageTex2D;
        }
        startTime = DateTime.Now.Millisecond;
    }

    public void OnDestroy()
    {
        Destroy(cameraImageTex2D);
    }

    public void writeHR(uint rate)
    {
        //call function in HPOmnicept class to return current heart rate, then input into CSV alongside time stamp
        //function call, get value of heartRate.rate
        
        //gets current time for time stamp, need to subtract start time for runtime value
        timeStamp = DateTime.UtcNow.Millisecond;

        //add time stamp and heart rate to csv
        var line = String.Format("Time", "Heart Rate");
        var line2 = String.Format(heartRate, timeStamp);
        //File.AppendAllText("TestFile.csv", rate.ToString() + "\n");
    }


    public void HeartRateHandler(HeartRate hr)
    {
        if (showHeartRateMessages && hr != null)
        {
            Debug.Log(hr);
            writeHR(hr.Rate);
            //File.AppendAllText("TestFile.csv", hr.GetHashCode().ToString());


        }
    }



    public void PPGFrameHandler(PPGFrame ppg)
    {
        if (showPPGMessages && ppg != null)
        {
            Debug.Log(ppg);
        }
    }


    public void EyeTrackingHandler(EyeTracking eyeTracking)
    {
        //call function in HPOmnicept class to return current eye tracking value, then input into CSV alongside time stamp
        //function call, get value of eye tracking
        
        //gets current time for time stamp, need to subtract start time for runtime value
        timeStamp = DateTime.UtcNow.Millisecond;

        //add time stamp and heart rate to csv
        var line = String.Format("Time", "Heart Rate");
        var line2 = String.Format(heartRate, timeStamp);
        File.AppendAllText("TestFile.csv", rate.ToString() + "\n");
    }

    public void VSyncHandler(VSync vsync)
    {
        if (showVsyncMessages && vsync != null)
        {
            Debug.Log(vsync);
        }
    }

    public void CameraImageHandler(CameraImage cameraImage)
    {
        if (cameraImage != null)
        {
            if (showCameraImageMessages)
            {
                Debug.Log(cameraImage);
            }
            if (showCameraImageTexture && cameraImageMat != null && cameraImage.SensorInfo.Location == "Mouth")
            {
                // Load data into the texture and upload it to the GPU.
                cameraImageTex2D.LoadRawTextureData(cameraImage.ImageData);
                cameraImageTex2D.Apply();
            }
        }
    }

    public void IMUFrameHandler(IMUFrame imu)
    {
        if (showIMUMessages && imu != null)
        {
            Debug.Log(imu);
        }
    }

    public void DisconnectHandler(string msg)
    {
        Debug.Log("Disconnected: " + msg);
    }

    public void ConnectionFailureHandler(HP.Omnicept.Errors.ClientHandshakeError error)
    {
        Debug.Log("Failed to connect: " + error);
    }

    public void SubscriptionResultListHandler(SubscriptionResultList SRLmsg)
    {
        if (showSubscriptionResultListMessages && SRLmsg != null)
        {
            Debug.Log(SRLmsg);
        }
    }
}