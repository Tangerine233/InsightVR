using UnityEngine;
using UnityEditor;

public class InsightVREditorWindow : EditorWindow
{

    InsightVR insightVR;

    private bool captureHR = true;
    private bool captureEyeData = true;
    private bool captureCameraImages = true;
    private bool captureIMUData = true;
    private string customDirectory = null;
    private bool enableCapture = false;
    private bool started = false;

    private InsightVR.Headset thisHeadset = InsightVR.Headset.HP_Omnicept_Reverb_G2;

    [MenuItem("Window/Insight VR")]
    public static void ShowWindow(){ EditorWindow.GetWindow<InsightVREditorWindow>("Insight VR"); }

    private void OnGUI()
    {

        GUILayout.Label("Enable InsightVR", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
        enableCapture = EditorGUILayout.Toggle("Enable Captures", enableCapture);
        EditorGUI.EndDisabledGroup();

        GUILayout.Label("Select Headset", EditorStyles.boldLabel);
        // Disable EnumPopup during play mode
        EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying || !enableCapture);
        thisHeadset = (InsightVR.Headset)EditorGUILayout.EnumPopup("Select Headset", thisHeadset);
        EditorGUI.EndDisabledGroup();

        // Disable Toggles during play mode
        EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying || !enableCapture);
        GUILayout.Label("Select parameters to capture", EditorStyles.boldLabel);
        captureHR = EditorGUILayout.Toggle("Heart Rate", captureHR);
        captureEyeData = EditorGUILayout.Toggle("Eye Tracking", captureEyeData);
        captureCameraImages = EditorGUILayout.Toggle("Face Camera Image", captureCameraImages);
        captureIMUData = EditorGUILayout.Toggle("IMU Datas", captureIMUData);
        EditorGUI.EndDisabledGroup();

        // Custom Directory
        GUILayout.Label("Custom Directory", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying || !enableCapture);
        customDirectory = EditorGUILayout.TextField(customDirectory);
        EditorGUI.EndDisabledGroup();
    }
    private void OnEnable()
    {
        enableCapture = false;
        started = false;
}

    private void OnDisable()
    {
        enableCapture = false;
        started = false;
    }

    public void Update() {

        if (EditorApplication.isPlaying && enableCapture)
        {
            if (!started)
            {
                started = true;
                StartCapture();
            }

            insightVR.Update();
        }
        else if (started)
        {
            insightVR.OnDestroy();
            started = false;
        }
    }
    
    private void StartCapture()
    {
        // create an instance of InsightVR
        insightVR = new InsightVR
        {
            // set the properties accordingly based on the GUI input
            captureHR = captureHR,
            captureEyeData = captureEyeData,
            captureCameraImages = captureCameraImages,
            captureIMUData = captureIMUData,
            thisHeadset = thisHeadset,
            customDirectory = customDirectory
        };
        insightVR.Start();
    }
}