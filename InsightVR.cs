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
    
    [SerializeField] 
    Headset thisHeadset = new Headset();

    public Material cameraImageMat;
    private Texture2D cameraImageTex2D;

    public int startTime, timeStamp, currentTime;

    string fileNameHR, fileNameEye, fileNameIMU, fileNameFace;

    enum Headset {
        HP_Omnicept_Reverb_G2,
        Meta_Quest_Pro,
        HTC_Vive
        //can add more headsets down the line
    }


    public void Start()
    {
        cameraImageTex2D = new Texture2D(400, 400, TextureFormat.R8, false);
        if (cameraImageMat != null)
        {
            cameraImageMat.mainTexture = cameraImageTex2D;
        }
        startTime = DateTime.Now.Millisecond;
        fileNameHR = "HR_" + DateTime.Now.ToString("_yyyy_MMdd_HH_mm_ss") + ".csv";
        fileNameEye = "Eye_" + DateTime.Now.ToString("_yyyy_MMdd_HH_mm_ss") + ".csv";
        fileNameIMU = "IMU_" + DateTime.Now.ToString("_yyyy_MMdd_HH_mm_ss") + ".csv";
        fileNameFace = "Face_" + DateTime.Now.ToString("_yyyy_MMdd_HH_mm_ss") + ".csv";

        var header = String.Format("Time", "Heart Rate");
        File.WriteAllText(fileNameHR, header);

        header = String.Format("Time", "Combined Gaze", "Pupil Position Left", "Pupil Position Right", "Pupil Dilation", "Openness");
        File.WriteAllText(fileNameEye, header);
        
        header = String.Format("Time", "IMU Data");
        File.WriteAllText(fileNameIMU, header);
        
        header = String.Format("Time", "Face Tracking Data");
        File.WriteAllText(fileNameFace, header);

        thisHeadset = Headset.HP_Omnicept_Reverb_G2;
    }

    public void OnDestroy()
    {
        Destroy(cameraImageTex2D);
    }

    public void HeartRateHandler(HeartRate hr)
    {
        if (showHeartRateMessages && hr != null)
        {
            Debug.Log(hr);
        }
    }

    public void writeHR(String rate, String[] timeStamp)
    {
        var data = $"{"Hello"},{"Hi"}";
        File.AppendAllText(fileNameHR, data + "\n");
        
        /*
        //gets current time for time stamp, need to subtract start time for runtime value
        currentTime = DateTime.UtcNow.Millisecond;
        timeStamp = startTime - currentTime;

        //add time stamp and heart rate to csv
        var data = String.Format(timeStamp, rate);
        File.AppendAllText(fileNameHR, data + "\n");
        */
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
        if (showEyeTrackingMessages && eyeTracking != null)
        {
            Debug.Log(eyeTracking);
        }   
    }

    public void writeEyeData(String[] leftEye, String[] rightEye, String[] combinedEyes, String[] timestamp) {
        /*
        //gets current time for time stamp, need to subtract start time for runtime value
        currentTime = DateTime.UtcNow.Millisecond;
        timeStamp = startTime - currentTime;

        Vector3 combinedCurrent;
        Vector2 leftPupPos, rightPupPos;
        float leftPupDil, rightPupDil, leftOpen, rightOpen;

        //"Combined Gaze", "Pupil Position Left", "Pupil Position Right", "Pupil Dilation", "Openness"
        if(eyeTracking.combinedGazeConfidence >= .4f) {
            combinedCurrent = new Vector3(eyeTracking.combinedGaze.x, eyeTracking.combinedGaze.y, eyeTracking.combinedGaze.z);
        } else {
            combinedCurrent = null;
        }

        if(eyeTracking.leftPupilDilationConfidence >= .4f) {
            leftPupDil = eyeTracking.leftPupilDilation;
        } else {
            leftPupDil = null;
        }

        if(eyeTracking.rightPupilDilationConfidence >= .4f) {
            rightPupDil = eyeTracking.rightPupilDilation;
        } else {
            rightPupDil = null;
        }

        if(eyeTracking.leftPupilPositionConfidence >= .4f) {
            leftPupPos = new Vector2(eyeTracking.leftPupilPosition.x, eyeTracking.leftPupilPosition.y);
        } else {
            leftPupPos = null;
        }

        if(eyeTracking.rightPupilPositionConfidence >= .4f) {
            rightPupPos = new Vector2(eyeTracking.rightPupilPosition.x, eyeTracking.rightPupilPosition.y);
        } else {
            rightPupPos = null;
        }

        if(eyeTracking.leftEyeOpennessConfidence >= .4f) {
            leftOpen = eyeTracking.leftEyeOpenness;
        } else {
            leftOpen = null;
        }

        if(eyeTracking.rightEyeOpennessConfidence >= .4f) {
            rightOpen = eyeTracking.rightEyeOpenness;
        } else {
            rightOpen = null;
        }

        //add time stamp and eye data to csv
        var data = String.Format(timeStamp, combinedCurrent.ToString(), leftPupDil, rightPupDil, leftPupPos, rightPupPos, leftOpen, rightOpen);
        File.AppendAllText(fileNameEye, data + "\n");
        */
        var data = $"{"Hello"},{"Test"}";
        File.AppendAllText(fileNameEye, data + "\n");
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
        //currentTime = DateTime.UtcNow.Millisecond;
        //timeStamp = startTime - currentTime;

        //var data = String.Format(timeStamp, cameraImage);
        //File.AppendAllText(fileNameFace, data + "\n");
    }

    public void IMUFrameHandler(IMUFrame imu)
    {
        if (showIMUMessages && imu != null)
        {
            Debug.Log(imu);
        }
        //currentTime = DateTime.UtcNow.Millisecond;
        //timeStamp = startTime - currentTime;

        //var data = String.Format(timeStamp, imu);
        //File.AppendAllText(fileNameIMU, data + "\n");
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