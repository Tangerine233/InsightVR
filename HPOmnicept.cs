using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;

// Omnincept API
using HP.Omnicept;
using HP.Omnicept.Messaging;
using HP.Omnicept.Messaging.Messages;
using HP.Omnicept.Unity;
using System.Runtime.InteropServices;
// using HP.Omnicept.Unity;

public class HPOmnicept : MonoBehaviour
{
    private String fileName;

    // omnicept client object
    private Glia m_gliaClient;
    private GliaValueCache m_gliaValCache;
    protected SubscriptionList m_subList;
    //protected Task m_connectTask;

    //
    public bool recordHR, recordEye, recordCam, recordIMU;
    public string startTime, dirName,dirCam, fileNameHR, fileNameEye,fileNameCam, fileNameIMU;


    public bool m_isConnected { get; private set; }
    public Action<EyeTracking> OnEyeTracking = tracking => { };

    private readonly List<uint> messageTypeList = new List<uint>
    {
        {0}, // none
        {MessageTypes.ABI_MESSAGE_HEART_RATE_FRAME }, // HR
        {MessageTypes.ABI_MESSAGE_EYE_TRACKING}, // Eye Tracking
        {MessageTypes.ABI_MESSAGE_CAMERA_IMAGE}, // Camera Image
        {MessageTypes.ABI_MESSAGE_IMU_FRAME}, //IMU
    };
    public Material cameraImageMat;
    private Texture2D cameraImageTex2D;



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

            Debug.Log("Connected To Omnicept Runtime");
        }
        catch (Exception e)
        {
            m_isConnected = false;
            Debug.Log("[InsightVR_Omnicept] Failed to load Glia for reason :" + e);
        }
        return m_isConnected;
    }

    public void Start()
    {
        StartGlia();

        //create dir
        startTime = DateTime.Now.ToString("yyyyMMdd_HH_mm_ss_ms");
        dirName = startTime + "_HP_Omincept_captures";
        dirCam = dirName + "/FaceImages";
        Directory.CreateDirectory(dirName);

        // TO-DO: get follow booleans from the main framework
        var (recordHR, recordEye, recordCam, recordIMU) = (true, true, true, true);

        // create capture files
        if (recordHR)
        {
            fileNameHR = dirName+"/HR_" + startTime + ".csv";
            // first line
            File.WriteAllText(fileNameHR, "Time,HR/n");
        }
        if (recordEye)
        {
            fileNameEye = dirName + "/Eye_" + startTime + ".csv";

            // first line
            string[] eyeVar = { "leftGazeX", "leftGazeY"
                    , "leftGazeY"
                    , "leftGazeZ"
                    , "leftGazeConfidence"
                    , "leftPilposition"
                    , "leftOpenness"
                    , "leftOpennessConfidence"
                    , "leftPupilDilation"
                    , "leftPupilDilationConfidence"
                    , "rightGazeX"
                    , "rightGazeY"
                    , "rightGazeZ"
                    , "rightGazeConfidence"
                    , "rightPilposition"
                    , "rightOpenness"
                    , "rightOpennessConfidence"
                    , "rightPupilDilation"
                    , "rightPupilDilationConfidence"
                    , "combinGazeX"
                    , "combinGazeY"
                    , "combinGazeZ"
                    + "combineGazeConfidence"};

            File.WriteAllText(fileNameEye, "Time,");
            for (int i = 0; i < eyeVar.Length; i++)
            {
                File.AppendAllText(fileNameEye, eyeVar[0] + ",");
            }
            File.AppendAllText(fileNameEye, "/n");

        }
        if (recordCam)
        {
            fileNameCam = dirName + "/Face_" + startTime + ".csv";

            // first line
            File.WriteAllText(fileNameCam, "Time,frameNumber,fps/n");

            // create dir for pics
            Directory.CreateDirectory(dirCam);

            // create image temp
            cameraImageTex2D = new Texture2D(400, 400, TextureFormat.R8, false);
        }
        if (recordIMU)
        {
            fileNameIMU = dirName + "/IMU_" + startTime + ".csv";

            // first line
            File.WriteAllText(fileNameIMU, "Time,/n");
        }

    }


    void Update()
    {
        if (m_isConnected)
        {
            ITransportMessage msg = RetrieveMessage();
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

    void timeStampHandler(Timestamp ts)
    {

    }

    void HandleMessage(ITransportMessage msg)
    {
        switch (msg.Header.MessageType)
        {
            case MessageTypes.ABI_MESSAGE_HEART_RATE:
                if (!recordHR) break;
                var heartRate = m_gliaClient.Connection.Build<HeartRate>(msg);

                // populate string para
                var rate = heartRate.Rate.ToString();


                // timeStamps
                var timeStampSystem = heartRate.Timestamp.SystemTimeMicroSeconds;
                var timeStampOmnicept = heartRate.Timestamp.OmniceptTimeMicroSeconds;
                var timeStampHardware = heartRate.Timestamp.HardwareTimeMicroSeconds;

                // test log
                Debug.Log("HR: "+rate);

                // write file
                File.AppendAllText(fileNameHR, DateTime.Now.ToString() + "," + rate + "\n");
                break;


            case MessageTypes.ABI_MESSAGE_EYE_TRACKING:
                if (!recordEye) break;
                var eyeTracking = m_gliaClient.Connection.Build<EyeTracking>(msg);

                // Debug.Log(eyeTracking);

                // populate string para
                // left eye
                var leftGazeX = eyeTracking.LeftEye.Gaze.X.ToString();
                var leftGazeY = eyeTracking.LeftEye.Gaze.Y.ToString();
                var leftGazeZ = eyeTracking.LeftEye.Gaze.Z.ToString();
                var leftGazeConfidence = eyeTracking.LeftEye.Gaze.Confidence.ToString();
                var leftPilposition = "";
                try { leftPilposition = eyeTracking.LeftEye.PupilPosition.ToString(); } catch {}
                var leftOpenness = eyeTracking.LeftEye.Openness.ToString();
                var leftOpennessConfidence = eyeTracking.LeftEye.OpennessConfidence.ToString();
                var leftPupilDilation = eyeTracking.LeftEye.PupilDilation.ToString();
                var leftPupilDilationConfidence = eyeTracking.LeftEye.PupilDilationConfidence.ToString();

                //right eye
                var rightGazeX = eyeTracking.RightEye.Gaze.X.ToString();
                var rightGazeY = eyeTracking.RightEye.Gaze.Y.ToString();
                var rightGazeZ = eyeTracking.RightEye.Gaze.Z.ToString();
                var rightGazeConfidence = eyeTracking.RightEye.Gaze.Confidence.ToString();
                var rightPilposition = "";
                try { rightPilposition = eyeTracking.RightEye.PupilPosition.ToString(); } catch {}
                var rightOpenness = eyeTracking.RightEye.Openness.ToString();
                var rightOpennessConfidence = eyeTracking.RightEye.OpennessConfidence.ToString();
                var rightPupilDilation = eyeTracking.RightEye.PupilDilation.ToString();
                var rightPupilDilationConfidence = eyeTracking.RightEye.PupilDilationConfidence.ToString();

                // combine
                var combinGazeX = eyeTracking.CombinedGaze.X.ToString();
                var combinGazeY = eyeTracking.CombinedGaze.Y.ToString();
                var combinGazeZ = eyeTracking.CombinedGaze.Z.ToString();
                var combineGazeConfidence = eyeTracking.CombinedGaze.Confidence.ToString();

                // debug log
                // Debug.Log("LeftEyeGaze: X-"+leftGazeX+" Y-"+leftGazeY+" Z-"+leftGazeZ);


                // write file
                File.AppendAllText(fileNameHR, DateTime.Now.ToString() + "," 
                    + leftGazeX + ","  
                    + leftGazeY + ","
                    + leftGazeZ + ","
                    + leftGazeConfidence + ","
                    + leftPilposition + ","
                    + leftOpenness + ","
                    + leftOpennessConfidence + ","
                    + leftPupilDilation + ","
                    + leftPupilDilationConfidence + ","
                    + rightGazeX + ","
                    + rightGazeY + ","
                    + rightGazeZ + ","
                    + rightGazeConfidence + ","
                    + rightPilposition + ","
                    + rightOpenness + ","
                    + rightOpennessConfidence + ","
                    + rightPupilDilation + ","
                    + rightPupilDilationConfidence + ","
                    + combinGazeX + ","
                    + combinGazeY + ","
                    + combinGazeZ + ","
                    + combineGazeConfidence + "\n");
                break;


            case MessageTypes.ABI_MESSAGE_CAMERA_IMAGE:
                if (!recordCam) break;
                var cameraImage = m_gliaClient.Connection.Build<CameraImage>(msg);

                // Load data into the texture and upload it to the GPU.
                cameraImageTex2D.LoadRawTextureData(cameraImage.ImageData);
                cameraImageTex2D.Apply();

                var frameNumber = cameraImage.FrameNumber;
                var fPS = cameraImage.FramesPerSecond;
                Debug.Log("FaceImage: frameNumber-" + frameNumber + " fPS-" + fPS);


                // write csv file
                File.AppendAllText(fileNameCam, DateTime.Now.ToString() + "," + frameNumber + "," + fPS);

                // write png file
                byte[] faceImage = cameraImageTex2D.EncodeToPNG();

                File.WriteAllBytes("/FaceImages/Image_"+ frameNumber.ToString() +"_"+DateTime.Now.ToString("_yyyyMMdd_HH_mm_ss")+".png", faceImage);

                break;


            case MessageTypes.ABI_MESSAGE_IMU_FRAME:
                if (!recordIMU) break;

                var imuFrame = m_gliaClient.Connection.Build<IMUFrame>(msg);

                Debug.Log("imuFrame: "+imuFrame.ToString());

                var acc = imuFrame.Data[0].ToString();
                var gyro = imuFrame.Data[1].ToString();


                // test log
                Debug.Log("IMU: acc-"+acc+" gyro-"+gyro);


                break;
            
            default:
                break;
        }

    }

}
/*
            case MessageTypes.ABI_MESSAGE_HEART_RATE_FRAME:
                break;

   
            //---------------
            case MessageTypes.ABI_MESSAGE_HEART_RATE_VARIABILITY:
                if (OnHeartRateVariability != null)
                {
                    var heartRateVariability = m_gliaClient.Connection.Build<HeartRateVariability>(msg);
                    OnHeartRateVariability.Invoke(heartRateVariability);
                }
                break;

            //---------------
            case MessageTypes.ABI_MESSAGE_PPG:
                break;
            case MessageTypes.ABI_MESSAGE_PPG_FRAME:
                if (OnPPGEvent != null)
                {
                    var ppgFrame = m_gliaClient.Connection.Build<PPGFrame>(msg);
                    OnPPGEvent.Invoke(ppgFrame);
                }
                break;

            //---------------
            case MessageTypes.ABI_MESSAGE_EYE_TRACKING:
                if (OnEyeTracking != null)
                {
                    var eyeTracking = m_gliaClient.Connection.Build<EyeTracking>(msg);
                    OnEyeTracking.Invoke(eyeTracking);
                }
                break;
            case MessageTypes.ABI_MESSAGE_EYE_TRACKING_FRAME:
                break;

            //---------------
            case MessageTypes.ABI_MESSAGE_VSYNC:
                if (OnVSync != null)
                {
                    var vsync = m_gliaClient.Connection.Build<VSync>(msg);
                    OnVSync.Invoke(vsync);
                }
                break;

            //---------------
            case MessageTypes.ABI_MESSAGE_SCENE_COLOR:
                break;
            case MessageTypes.ABI_MESSAGE_SCENE_COLOR_FRAME:
                break;

            //---------------
            case MessageTypes.ABI_MESSAGE_COGNITIVE_LOAD:
                if (OnCognitiveLoad != null)
                {
                    var cload = m_gliaClient.Connection.Build<CognitiveLoad>(msg);
                    OnCognitiveLoad.Invoke(cload);
                }
                break;
            case MessageTypes.ABI_MESSAGE_COGNITIVE_LOAD_INPUT_FEATURE:
                break;

            //---------------
            case MessageTypes.ABI_MESSAGE_BYTE_MESSAGE:
                break;

            //---------------
            case MessageTypes.ABI_MESSAGE_CAMERA_IMAGE:
                if (OnCameraImage != null)
                {
                    var cameraImage = m_gliaClient.Connection.Build<CameraImage>(msg);
                    OnCameraImage.Invoke(cameraImage);
                }
                break;
            case MessageTypes.ABI_MESSAGE_DATA_VAULT_RESULT:
                if (OnDataVaultResult != null)
                {
                    var dataVaultResult = m_gliaClient.Connection.Build<DataVaultResult>(msg);
                    OnDataVaultResult.Invoke(dataVaultResult);
                }
                break;

            //---------------
            case MessageTypes.ABI_MESSAGE_IMU:
                break;
            case MessageTypes.ABI_MESSAGE_IMU_FRAME:
                if (OnIMUEvent != null)
                {
                    var imuFrame = m_gliaClient.Connection.Build<IMUFrame>(msg);
                    OnIMUEvent.Invoke(imuFrame);
                }
                break;
            case MessageTypes.ABI_MESSAGE_SUBSCRIPTION_RESULT_LIST:
                if (OnSubscriptionResultListEvent != null)
                {
                    var SRLmsg = m_gliaClient.Connection.Build<SubscriptionResultList>(msg);
                    OnSubscriptionResultListEvent.Invoke(SRLmsg);
                }
                break;
                */
