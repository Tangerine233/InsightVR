////////////////////////////////////////////////////////////////////////////////////////////////////////
//FileName: HPOmnicept.cs
//FileType: Visual C# Source file
//Author : Kainuo He
//Created On : 10/3/2023
//Last Modified On : 12/1/2023
//Description : Sub-Module for InsightVR framework on HP G2 Omincept Edition.Object construct with
//              bool[] of size 4 to enable files writing for captured Heart Rate, Eye Tracking,
//              Face Camera Image,and IMU
////////////////////////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using HP.Omnicept;
using HP.Omnicept.Messaging;
using HP.Omnicept.Messaging.Messages;

public class HPOmnicept
{
    // omnicept client object
    private Glia m_gliaClient;
    private GliaValueCache m_gliaValCache;
    protected SubscriptionList m_subList;
    public ITransportMessage msg;


    // declare variables
    private bool recordHR, recordEye, recordCam, recordIMU;
    private string parentDir, dirName,dirCam, fileNameHR, fileNameEye,fileNameCam, fileNameIMU, currTime;
    public bool m_isConnected { get; private set; }

    // message subscribtions list from Omnicept runtime
    private readonly List<uint> messageTypeList = new List<uint>
    {
        {0}, // none
        {MessageTypes.ABI_MESSAGE_HEART_RATE_FRAME }, // HR
        {MessageTypes.ABI_MESSAGE_EYE_TRACKING}, // Eye Tracking
        {MessageTypes.ABI_MESSAGE_CAMERA_IMAGE}, // Camera Image
        {MessageTypes.ABI_MESSAGE_IMU_FRAME}, //IMU
    };
    private Texture2D cameraImageTex2D;

    // items for each captures, item each row in the CSV files
    private string[] hrItems = new string[] { "Time", "HR" };
    private string[] eyeItems = new string[] { "Time", "leftGazeX", "leftGazeY" , "leftGazeY" , "leftGazeZ" , "leftGazeConfidence" , "leftPilposition" , "leftOpenness" , "leftOpennessConfidence", "leftPupilDilation", "leftPupilDilationConfidence",
                    "rightGazeX", "rightGazeY", "rightGazeZ", "rightGazeConfidence", "rightPilposition", "rightOpenness", "rightOpennessConfidence" , "rightPupilDilation", "rightPupilDilationConfidence", 
                    "combinGazeX", "combinGazeY", "combinGazeZ", "combineGazeConfidence"};
    private string[] faceItems = new string[] { "Time", "frameNumber", "FPS" };
    private string[] imuItems = new string[] { "Time", "IMU#", "Acc-X", "Acc-Y", "Acc-Z", "Gyro -X", "Gyro-Y", "Gyro-Z" };


    ///////////////////////////////////////////////////
    // Connect Functions
    ///////////////////////////////////////////////////
    public void StopGlia()
        {
            if (m_gliaValCache != null)
                m_gliaValCache?.Stop();
            if (m_gliaClient != null)
                m_gliaClient?.Dispose();
            m_gliaValCache = null;
            m_gliaClient = null;
            m_isConnected = false;
            Glia.cleanupNetMQConfig();
        }


    public bool StartGlia()
    {
        StopGlia();

        try
        {
            m_gliaClient = new Glia("InsightVROmnicept", new SessionLicense(String.Empty, String.Empty, LicensingModel.Core, false));
            m_gliaValCache = new GliaValueCache(m_gliaClient.Connection);

            m_subList = new SubscriptionList();
            foreach (var mType in messageTypeList)
            {
                m_subList.Subscriptions.Add(new Subscription(mType, String.Empty, String.Empty, String.Empty, String.Empty, new MessageVersionSemantic("1.0.0")));
            }

            m_gliaClient.setSubscriptions(m_subList);
            m_isConnected = true;

            Debug.Log("[InsightVR_Omnicept] Connected To Omnicept Runtime");
        }
        catch (Exception e)
        {
            m_isConnected = false;
            Debug.Log("[InsightVR_Omnicept] Failed to load Glia for reason :" + e);
        }
        return m_isConnected;
    }
    ///////////////////////////////////////////////////




    ///////////////////////////////////////////////////
    // Class Constructor
    ///////////////////////////////////////////////////
    //constructor overloading
    public HPOmnicept(bool[] captures, string rootDir)
    {
        constructor(captures, rootDir + "/");
    }
    public HPOmnicept(bool[] captures)
    {
        constructor(captures, "");
    }
    public HPOmnicept()
    {
        bool[] captures = new bool[] { true, true, true, true };
        constructor(captures, "");
    }
    public HPOmnicept(string rootDir)
    {
        bool[] captures = new bool[] { true, true, true, true };
        constructor(captures, rootDir + "/");
    }

    private void constructor(bool[] captures, string rootDir)
    {
        StartGlia();

        //create dir
        parentDir = rootDir;
        currTime = DateTime.Now.ToString("yyyyMMdd_HH_mm_ss");
        dirName = parentDir + currTime + "_HP_Omincept_captures";
        Directory.CreateDirectory(dirName);

        // get follow booleans from the main framework
        recordHR = captures[0];
        recordEye = captures[1];
        recordCam = captures[2];
        recordIMU = captures[3];

        // create capture files
        if (recordHR)
        {
            fileNameHR = dirName + "/HR_" + currTime + ".csv";

            // first line
            writeCSV(fileNameHR, hrItems);
        }
        if (recordEye)
        {
            fileNameEye = dirName + "/Eye_" + currTime + ".csv";

            // first line
            writeCSV(fileNameEye, eyeItems);
        }
        if (recordCam)
        {
            fileNameCam = dirName + "/Face_" + currTime + ".csv";

            // first line
            writeCSV(fileNameCam, faceItems);

            // create dir for pics
            dirCam = dirName + "/" + "FaceImages_" + currTime;
            Directory.CreateDirectory(dirCam);

            // create image temp
            cameraImageTex2D = new Texture2D(400, 400, TextureFormat.R8, false);
        }
        if (recordIMU)
        {
            fileNameIMU = dirName + "/IMU_" + currTime + ".csv";

            // first line
            writeCSV (fileNameIMU, imuItems);
        }
    }
    ///////////////////////////////////////////////////




    ///////////////////////////////////////////////////
    // Run time Update
    ///////////////////////////////////////////////////
    public void Update()
    {
        if (m_isConnected)
        {
            msg = RetrieveMessage();
            while (msg != null)
            {
                HandleMessage(msg);
                msg = RetrieveMessage();
            }
        }
    }


    ITransportMessage RetrieveMessage()
    {
        ITransportMessage msg = null;
        if (m_gliaValCache != null)
        {
            try
            {
                msg = m_gliaValCache.GetNext();
            }
            catch (HP.Omnicept.Errors.TransportError e)
            {
                Debug.Log("[InsightVR_OmniceptModule] Failed to start Glia! :" + e);
            }
        }
        return msg;
    }

    void HandleMessage(ITransportMessage msg)
    {
        switch (msg.Header.MessageType)
        {
            case MessageTypes.ABI_MESSAGE_HEART_RATE:
                if (!recordHR) break;
                HrMessage(msg);
                break;


            case MessageTypes.ABI_MESSAGE_EYE_TRACKING:
                if (!recordEye) break;
                EyeMessage(msg);
                break;


            case MessageTypes.ABI_MESSAGE_CAMERA_IMAGE:
                if (!recordCam) break;
                FaceCamMessage(msg);
                break;


            case MessageTypes.ABI_MESSAGE_IMU_FRAME:
                if (!recordIMU) break;
                ImuMessage(msg);
                break;
                
            
            default:
                break;
        }
    }
    ///////////////////////////////////////////////////


    ///////////////////////////////////////////////////
    /// write files functions to append
    ///////////////////////////////////////////////////
    void writeCSV(string fileName, string[] values)
    {
        // if file not exits, create file
        if (!File.Exists(fileName))
        {
            File.WriteAllText(fileName, "");
        }


        // write file
        string line = "";
        line += values[0];
        for (int i = 1; i < values.Length; i++)
        {
            line += "," + values[i];
        }
        line += "\n";

        File.AppendAllText(fileName, line);
    }

    void writePNG(string fileName)
    {
        byte[] faceImage = cameraImageTex2D.EncodeToPNG();
        File.WriteAllBytes(fileName, faceImage);
    }
    ///////////////////////////////////////////////////





    ///////////////////////////////////////////////////
    /// Messages pipeline
    ///////////////////////////////////////////////////
    string updateCurrTime(long microSec)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(microSec / 1000).DateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss:fff");
    }


    string[] HrMessage(ITransportMessage msg)
    {
        string[] hr = new string[2];
        var heartRate = m_gliaClient.Connection.Build<HeartRate>(msg);

        currTime = updateCurrTime(heartRate.Timestamp.SystemTimeMicroSeconds);

        // populate string para
        hr[0] = currTime;
        hr[1] = heartRate.Rate.ToString();

        // write file
        writeCSV(fileNameHR, hr);

        return hr;
    }

    string[] EyeMessage(ITransportMessage msg)
    {
        var eyeTracking = m_gliaClient.Connection.Build<EyeTracking>(msg);
        currTime = updateCurrTime(eyeTracking.Timestamp.SystemTimeMicroSeconds);

        // left eye
        string leftGazeX = eyeTracking.LeftEye.Gaze.X.ToString();
        string leftGazeY = eyeTracking.LeftEye.Gaze.Y.ToString();
        string leftGazeZ = eyeTracking.LeftEye.Gaze.Z.ToString();
        string leftGazeConfidence = eyeTracking.LeftEye.Gaze.Confidence.ToString();
        string leftPilposition = "";
        try { leftPilposition = eyeTracking.LeftEye.PupilPosition.ToString(); } catch { }
        string leftOpenness = eyeTracking.LeftEye.Openness.ToString();
        string leftOpennessConfidence = eyeTracking.LeftEye.OpennessConfidence.ToString();
        string leftPupilDilation = eyeTracking.LeftEye.PupilDilation.ToString();
        string leftPupilDilationConfidence = eyeTracking.LeftEye.PupilDilationConfidence.ToString();

        //right eye
        string rightGazeX = eyeTracking.RightEye.Gaze.X.ToString();
        string rightGazeY = eyeTracking.RightEye.Gaze.Y.ToString();
        string rightGazeZ = eyeTracking.RightEye.Gaze.Z.ToString();
        string rightGazeConfidence = eyeTracking.RightEye.Gaze.Confidence.ToString();
        string rightPilposition = "";
        try { rightPilposition = eyeTracking.RightEye.PupilPosition.ToString(); } catch { }
        string rightOpenness = eyeTracking.RightEye.Openness.ToString();
        string rightOpennessConfidence = eyeTracking.RightEye.OpennessConfidence.ToString();
        string rightPupilDilation = eyeTracking.RightEye.PupilDilation.ToString();
        string rightPupilDilationConfidence = eyeTracking.RightEye.PupilDilationConfidence.ToString();

        // combine
        string combinGazeX = eyeTracking.CombinedGaze.X.ToString();
        string combinGazeY = eyeTracking.CombinedGaze.Y.ToString();
        string combinGazeZ = eyeTracking.CombinedGaze.Z.ToString();
        string combineGazeConfidence = eyeTracking.CombinedGaze.Confidence.ToString();


        string[] eye = new string[] {currTime,leftGazeX,leftGazeY,leftGazeZ,leftGazeConfidence,leftPilposition,leftOpenness,leftOpennessConfidence,leftPupilDilation,leftPupilDilationConfidence
            ,rightGazeX,rightGazeY,rightGazeZ,rightGazeConfidence,rightPilposition,rightOpenness,rightOpennessConfidence,rightPupilDilation,rightPupilDilationConfidence
            ,combinGazeX,combinGazeY,combinGazeZ,combineGazeConfidence};


        // write file
        writeCSV(fileNameEye, eye);

        return eye;
    }

    string[] FaceCamMessage(ITransportMessage msg)
    {
        var cameraImage = m_gliaClient.Connection.Build<CameraImage>(msg);
        currTime = updateCurrTime(cameraImage.Timestamp.SystemTimeMicroSeconds);

        // Load data into the texture and upload it to the GPU.
        cameraImageTex2D.LoadRawTextureData(cameraImage.ImageData);
        cameraImageTex2D.Apply();

        string frameNumber = cameraImage.FrameNumber.ToString();
        string fPS = cameraImage.FramesPerSecond.ToString();


        string[] face = new string[] { currTime, frameNumber, fPS };


        // write csv file
        writeCSV(fileNameCam, face);

        // write png file
        writePNG(dirCam + "/Frame_" + frameNumber + "_At_" + currTime + ".png");

        return face;
    }

    string[,] ImuMessage(ITransportMessage msg)
    {
        var imuFrame = m_gliaClient.Connection.Build<IMUFrame>(msg);
        currTime = updateCurrTime(imuFrame.Timestamp.SystemTimeMicroSeconds);

        int items = 8;
        string[,] imu = new string[imuFrame.Data.Count,items];
        for (int i = 0; i < imuFrame.Data.Count; i++)
        {
            imu[i, 0] = currTime;
            imu[i, 1] = i.ToString();
            imu[i, 2] = imuFrame.Data[i].Acc.X.ToString();
            imu[i, 3] = imuFrame.Data[i].Acc.Y.ToString();
            imu[i, 4] = imuFrame.Data[i].Acc.Z.ToString();
            imu[i, 5] = imuFrame.Data[i].Gyro.X.ToString();
            imu[i, 6] = imuFrame.Data[i].Gyro.Y.ToString();
            imu[i, 7] = imuFrame.Data[i].Gyro.Z.ToString();
        }

        // write file
        string[] temp = new string[items];
        for (int i = 0; i < imuFrame.Data.Count; i++)
        {
            for (int j = 0; j < items; j++)
            {
                temp[j] = imu[i,j];
            }
            writeCSV(fileNameIMU, temp);
        }

        return imu;
    }
    ///////////////////////////////////////////////////
}