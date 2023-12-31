using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InsightVR : MonoBehaviour
{
    [SerializeField]
    public bool captureHR = true;
    [SerializeField]
    public bool captureEyeData = true;
    [SerializeField]
    public bool captureCameraImages = true;
    [SerializeField]
    public bool captureIMUData = true;
    [SerializeField] 
    public Headset thisHeadset = new Headset();

    //[SerializeField]
    //public bool[] captures;

    [SerializeField] 
    public string customDirectory = null;

    //public Material cameraImageMat;
    //private Texture2D cameraImageTex2D;

    HPOmnicept hPOmniceptObject;
    
    //add the different headsets as object classes and add method implementation
    //MetaQuest metaQuest; 
    //HTCVive htcVive;

    public enum Headset {
        HP_Omnicept_Reverb_G2,
        Meta_Quest_Pro,
        HTC_Vive
        //can add more headsets down the line
    }


    //start method
    //initializes everything needed to track data
    //files, what sensors to capture, and which device is being used
    public void Start()
    {
        //cameraImageTex2D = new Texture2D(400, 400, TextureFormat.R8, false);
        //if (cameraImageMat != null)
        //{
        //    cameraImageMat.mainTexture = cameraImageTex2D;
        //}

        bool[] captures = new bool[4];
        captures[0] = captureHR;
        captures[1] = captureEyeData;
        captures[2] = captureCameraImages;
        captures[3] = captureIMUData;

        switch(thisHeadset) {
            case Headset.HP_Omnicept_Reverb_G2:
                if(customDirectory != null) {
                    hPOmniceptObject = new HPOmnicept(captures, customDirectory);
                }
                else {
                    hPOmniceptObject = new HPOmnicept(captures);
                }
                break;
            case Headset.Meta_Quest_Pro:
                if(customDirectory != null) {
                    //metaQuest = new MetaQuest(captures, customDirectory);
                }
                else {
                    //metaQuest = new MetaQuest(captures);
                }
                break;
            case Headset.HTC_Vive:
            if(customDirectory != null) {
                    //htcVive = new htcVive(captures, customDirectory);
                }
                else {
                    //htcVive = new htcVive(captures);
                }
                break;
            default:
                break;
        }

        Debug.Log($"[InsightVR][{thisHeadset}] Capture Started");
    }

    //calls update method in each class to write to csv file
    public void Update() {
        switch(thisHeadset) {
            case Headset.HP_Omnicept_Reverb_G2:
                hPOmniceptObject.Update();
                break;
            case Headset.Meta_Quest_Pro:
                //metaQuest.Update();
                break;
            case Headset.HTC_Vive:
                //htcVive.Update();
                break;
            default:
                break;
        }
    }

    public void OnDestroy()
    {
        //Destroy(cameraImageTex2D);
        switch (thisHeadset)
        {
            case Headset.HP_Omnicept_Reverb_G2:
                hPOmniceptObject.OnDestroy();
                break;
            case Headset.Meta_Quest_Pro:
                //metaQuest.Update();
                break;
            case Headset.HTC_Vive:
                //htcVive.Update();
                break;
            default:
                break;
        }

        Debug.Log($"[InsightVR][{thisHeadset}] Capture Ends");
    }

    public void DisconnectHandler(string msg)
    {
        Debug.Log("Disconnected: " + msg);
    }

    public void ConnectionFailureHandler(HP.Omnicept.Errors.ClientHandshakeError error)
    {
        Debug.Log("Failed to connect: " + error);
    }
}