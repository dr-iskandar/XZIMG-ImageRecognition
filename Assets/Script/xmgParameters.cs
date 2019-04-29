/**
 *
 * Copyright (c) 2019 XZIMG Limited , All Rights Reserved
 * No part of this software and related documentation may be used, copied,
 * modified, distributed and transmitted, in any form or by any means,
 * without the prior written permission of XZIMG Limited
 *
 * contact@xzimg.com, www.xzimg.com
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class xmgObjectPivotLink
{
	[Tooltip("Drag and drop a pivot (a GameObject) from the scene")]
	public GameObject ScenePivot;
	
	[Tooltip("Drag and drop a Classifier (.bytes format) from the ./Resources folder\n. Classifiers can be created using the right mouse button menu (Create->XZIMG Classifier)")]
	public TextAsset Classifier;

	[Tooltip("Specify the index of the marker you want to detect\n This parameters is only available when using black and white markers)")]
	public int MarkerIndex;

	[Tooltip("Specify the real width size of your object (centimers, inches, ...)\ncorresponding height is calculated automatically.")]
	public float ObjectRealWidth = 1.0f;
}

public enum xmgVideoPlaneFittingMode
{
    FitScreenHorizontally,
    FitScreenVertically,
};

[System.Serializable]
public class xmgVideoCaptureParameters
{
    [Tooltip("Use Native Capture or Unity WebCameraTexture class - Should be activated for mobiles")]
    public bool useNativeCapture = true;

    [Tooltip("Video device index \n -1 for automatic research")]
    public int videoCaptureIndex = -1;

    [Tooltip("Video capture mode \n 1 is VGA \n 2 is 720p \n 3 is 1080p")]
    public int videoCaptureMode = 2;

    [Tooltip("Use frontal camera (for mobiles only)")]
    public bool UseFrontal = false;

    [Tooltip("Mirror the video")]
    public bool MirrorVideo = false;

    [Tooltip("Choose if the video plane should fit  horizontally or vertically the screen (only relevent in case screen aspect ratio is different from video capture aspect ratio)")]
    public xmgVideoPlaneFittingMode videoPlaneFittingMode = xmgVideoPlaneFittingMode.FitScreenHorizontally;

    [Tooltip("To scale up/down the rendering plane")]
    public float VideoPlaneScale = 1.0f;

    [Tooltip("Camera vertical FOV \nThis value will change the main camera vertical FOV")]
    public float CameraVerticalFOV = 50f;

    // image is flipped upside down (depending on pixel formats and devices)
    private bool m_isVideoVerticallyFlipped = true;

    public void CheckVideoCaptureParameters()
    {
#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL)
        if (useNativeCapture)
            Debug.Log("xmgVideoCaptureParameters (useNativeCapture) - Video Capture cannot be set to native for PC/MAC platforms => forcing to FALSE");
        if (UseFrontal)
            Debug.Log("xmgVideoCaptureParameters (UseFrontal) - Frontal mode option is not available for PC/MAC platforms - Use camera index edit box instead => forcing to FALSE");
        useNativeCapture = false;
        UseFrontal = false;
#endif

#if (!UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS))
        useNativeCapture = true;
        if (UseFrontal && !MirrorVideo)
        {
            MirrorVideo = true;
            Debug.Log("xmgVideoCaptureParameters (MirrorVideo) - Mirror mode is forced on mobiles when using frontal camera => forcing to TRUE");       
        }
        if (!UseFrontal && MirrorVideo)
        {
            MirrorVideo = false;
            Debug.Log("xmgVideoCaptureParameters (MirrorVideo) - Mirror mode is deactivate on mobiles when using back camera => forcing to FALSE");       
        }
#endif

#if (UNITY_EDITOR_OSX || (UNITY_WEBGL && !UNITY_EDITOR))
        m_isVideoVerticallyFlipped = false;
#endif
        // -- Manage video capture size
        if (videoCaptureMode == 0)
            videoCaptureMode = 1;

#if (!UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS))
        if (videoCaptureMode == 3)
            videoCaptureMode = 2;       // Full HD would be too long to process on older phones
#endif

#if (!UNITY_EDITOR && UNITY_WEBGL)
       // Only mode supported by WebGL
       videoCaptureMode = 1;      
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE) && UNITY_EDITOR_OSX
        if (videoCaptureMode < 2)
            // Video Capture on MACOS through Unity is tricky 
            // (you might want to change following value when using a non-standard Camera
            videoCaptureMode = 2;       
#endif
    }

    public bool GetVerticalMirror()
    {
        return m_isVideoVerticallyFlipped;
    }

    public static int getVideoCaptureMode(int width, int height)
    {
        if (width == 320 && height == 240) return 0;
        if (width == 640 && height == 480) return 1;
        if (width == 1280 && height == 720) return 2;
        if (width == 1920 && height == 1080) return 4;
        return -1;
    }

    public int GetVideoCaptureWidth()
    {
        if (videoCaptureMode == 0) return 320;
        if (videoCaptureMode == 2) return 1280;
        if (videoCaptureMode == 3) return 1920;
        return 640;
    }
    public int GetVideoCaptureHeight()
    {
        if (videoCaptureMode == 0) return 240;
        if (videoCaptureMode == 2) return 720;
        if (videoCaptureMode == 3) return 1080;
        return 480;
    }
    public int GetProcessingWidth()
    {
        if (videoCaptureMode == 0) return 320;
        if (videoCaptureMode == 2) return 640;
        if (videoCaptureMode == 3) return 480;
        return 640;
    }
    public int GetProcessingHeight()
    {
        if (videoCaptureMode == 0) return 240;
        if (videoCaptureMode == 2) return 360;
        if (videoCaptureMode == 3) return 270;
        return 480;
    }

    public int GetProcessingWidth(int videoCaptureWidth)
    {
        if (videoCaptureWidth > 640)
            return videoCaptureWidth / 4;
        else if (videoCaptureWidth > 320)
            return videoCaptureWidth / 2;
        return videoCaptureWidth;
    }
    public int GetProcessingHeight(int videoCaptureHeight)
    {
        if (videoCaptureHeight > 640)
            return videoCaptureHeight / 4;
        else if (videoCaptureHeight > 320)
            return videoCaptureHeight / 2;
        return videoCaptureHeight;
    }


public float GetVideoAspectRatio()
    {
        return (float)GetVideoCaptureWidth() / (float)GetVideoCaptureHeight();
    }

    public float GetScreenAspectRatio()
    {
        float screen_AR = (float)Screen.width / (float)Screen.height;
        return screen_AR;

    }
    public double GetMainCameraFovV()
    {
        float video_AR = (float)GetVideoAspectRatio();
        float screen_AR = GetScreenAspectRatio();
        double trackingCamera_fovh_radian = xmgTools.ConvertToRadian((double)CameraVerticalFOV);
        double trackingCamera_fovv_radian;
        if (videoPlaneFittingMode == xmgVideoPlaneFittingMode.FitScreenHorizontally)
            trackingCamera_fovv_radian = xmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovh_radian, (double)screen_AR);
        else
            trackingCamera_fovv_radian = xmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovh_radian, (double)video_AR);
        return xmgTools.ConvertToDegree(trackingCamera_fovv_radian);
    }

    // Usefull for portrait and reverse protraits modes
    public double GetPortraitMainCameraFovV()
    {
        float video_AR = (float)GetVideoAspectRatio();
        float screen_AR = GetScreenAspectRatio();

        double trackingCamera_fovh_radian = xmgTools.ConvertToRadian((double)CameraVerticalFOV);
        double trackingCamera_fovv_radian;
        if (videoPlaneFittingMode == xmgVideoPlaneFittingMode.FitScreenHorizontally)
            trackingCamera_fovv_radian = trackingCamera_fovh_radian;
        else
        {
            trackingCamera_fovv_radian = xmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovh_radian, (double)video_AR);
            trackingCamera_fovv_radian = xmgTools.ConvertVerticalFovToHorizontalFov(trackingCamera_fovv_radian, (double)screen_AR);
        }

        return xmgTools.ConvertToDegree(trackingCamera_fovv_radian);
    }


    public double[] GetVideoPlaneScale(double videoPlaneDistance)
    {
        double[] ret = new double[2];

        float video_AR = (float)GetVideoAspectRatio();
        float screen_AR = GetScreenAspectRatio();
        double scale_u, scale_v;

        if (videoPlaneFittingMode == xmgVideoPlaneFittingMode.FitScreenHorizontally)
        {
            double mainCamera_fovv_radian = xmgTools.ConvertToRadian((double)GetMainCameraFovV());
            double mainCamera_fovh_radian = xmgTools.ConvertVerticalFovToHorizontalFov(mainCamera_fovv_radian, (double)screen_AR);
            scale_u = (videoPlaneDistance * Math.Tan(mainCamera_fovh_radian / 2.0));
            scale_v = (videoPlaneDistance * Math.Tan(mainCamera_fovh_radian / 2.0) * 1.0 / video_AR);
        }
        else
        {
            double mainCamera_fovv_radian = xmgTools.ConvertToRadian((double)GetMainCameraFovV());
            scale_u = (videoPlaneDistance * Math.Tan(mainCamera_fovv_radian / 2.0) * video_AR);
            scale_v = (videoPlaneDistance * Math.Tan(mainCamera_fovv_radian / 2.0));
        }
        ret[0] = scale_u;
        ret[1] = scale_v;
        return ret;
    }
    //[Tooltip("Video capture index (choose your camera) for Desktop only")]
    //public int videoCaptureIndex = -1;

    //[Tooltip("Video capture mode \n 1 is VGA \n 2 is 720p \n 3 is 1080p")]
    //public int videoCaptureMode = 1;

    //[Tooltip("Choosing precision and speed")]
    //public bool HighPrecision = false;

    //[Tooltip("Use frontal camera (for mobiles only)")]
    //public bool UseFrontal = false;

    //[Tooltip("Mirror the video")]
    //public bool MirrorVideo = false;

    //[Tooltip("Choose if the video plane should fit  horizontally or vertically the screen (only relevent in case screen aspect ratio is different from video capture aspect ratio)")]
    //public xmgVideoPlaneFittingMode videoPlaneFittingMode = xmgVideoPlaneFittingMode.FitHorizontally;

    //[Tooltip("To scale up/down the rendering plane")]
    //public float VideoPlaneScale = 1.01f;

    //[Tooltip("Camera horizontal FOV \nThis value will change the main camera vertical FOV")]
    //public float CameraFOVX = 60f;

    //[Tooltip("Display debug information")]
    //public bool ScreenDebug = true;


    //public int GetVideoCaptureWidth()
    //{
    //    if (videoCaptureMode == 0) return 320;
    //    if (videoCaptureMode == 2) return 1280;
    //    if (videoCaptureMode == 3) return 1920;
    //    return 640;
    //}
    //public int GetVideoCaptureHeight()
    //{
    //    if (videoCaptureMode == 0) return 240;
    //    if (videoCaptureMode == 2) return 720;
    //    if (videoCaptureMode == 3) return 1080;
    //    return 480;
    //}
    //public int GetProcessingWidth()
    //{
    //    if (HighPrecision)
    //    {
    //        if (videoCaptureMode == 3)
    //            return 960;
    //        return GetVideoCaptureWidth();
    //    }

    //    if (videoCaptureMode == 0) return 320;
    //    if (videoCaptureMode == 2) return 640;
    //    if (videoCaptureMode == 3) return 480;
    //    return 320;
    //}
    //public int GetProcessingHeight()
    //{
    //    if (HighPrecision)
    //    {
    //        if (videoCaptureMode == 3)
    //            return 540;
    //        return GetVideoCaptureHeight();
    //    }
    //    if (videoCaptureMode == 0) return 240;
    //    if (videoCaptureMode == 2) return 360;
    //    if (videoCaptureMode == 3) return 270;
    //    return 240;
    //}
    //public int GetProcessingWidth(int videoCaptureWidth)
    //{
    //    if (videoCaptureWidth > 640)
    //        return videoCaptureWidth / 4;
    //    else if (videoCaptureWidth > 320)
    //        return videoCaptureWidth / 2;
    //    return videoCaptureWidth;
    //}
    //public int GetProcessingHeight(int videoCaptureHeight)
    //{
    //    if (videoCaptureHeight > 640)
    //        return videoCaptureHeight / 4;
    //    else if (videoCaptureHeight > 320)
    //        return videoCaptureHeight / 2;
    //    return videoCaptureHeight;
    //}


    //public double GetVideoAspectRatio()
    //{
    //    return (double)GetVideoCaptureWidth() / (double)GetVideoCaptureHeight();
    //}

    //public float GetScreenAspectRatio()
    //{
    //    float screen_AR = (float)Screen.width / (float)Screen.height;
    //    if (Screen.width < Screen.height)
    //        screen_AR = 1.0f / screen_AR;
    //    return screen_AR;

    //}
    //public double GetMainCameraFovV()
    //{
    //    float video_AR = (float)GetVideoAspectRatio();
    //    float screen_AR = GetScreenAspectRatio();
    //    double trackingCamera_fovh_radian = xmgTools.ConvertToRadian((double)CameraFOVX);
    //    double trackingCamera_fovv_radian;
    //    if (videoPlaneFittingMode == xmgVideoPlaneFittingMode.FitHorizontally)
    //        trackingCamera_fovv_radian = xmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovh_radian, (double)screen_AR);
    //    else
    //        trackingCamera_fovv_radian = xmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovh_radian, (double)video_AR);
    //    return xmgTools.ConvertToDegree(trackingCamera_fovv_radian);
    //}

    //// Usefull for portrait and reverse protraits modes
    //public double GetPortraitMainCameraFovV()
    //{
    //    float video_AR = (float)GetVideoAspectRatio();
    //    float screen_AR = GetScreenAspectRatio();

    //    double trackingCamera_fovh_radian = xmgTools.ConvertToRadian((double)CameraFOVX);
    //    double trackingCamera_fovv_radian;
    //    if (videoPlaneFittingMode == xmgVideoPlaneFittingMode.FitHorizontally)
    //        trackingCamera_fovv_radian = trackingCamera_fovh_radian;
    //    else
    //    {
    //        trackingCamera_fovv_radian = xmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovh_radian, (double)video_AR);
    //        trackingCamera_fovv_radian = xmgTools.ConvertVerticalFovToHorizontalFov(trackingCamera_fovv_radian, (double)screen_AR);
    //    }

    //    return xmgTools.ConvertToDegree(trackingCamera_fovv_radian);
    //}


    //public double[] GetVideoPlaneScale(double videoPlaneDistance)
    //{
    //    double[] ret = new double[2];

    //    float video_AR = (float)GetVideoAspectRatio();
    //    float screen_AR = GetScreenAspectRatio();
    //    double scale_u, scale_v;

    //    if (videoPlaneFittingMode == xmgVideoPlaneFittingMode.FitHorizontally)
    //    {
    //        double mainCamera_fovv_radian = xmgTools.ConvertToRadian((double)GetMainCameraFovV());
    //        double mainCamera_fovh_radian = xmgTools.ConvertVerticalFovToHorizontalFov(mainCamera_fovv_radian, (double)screen_AR);
    //        scale_u = (videoPlaneDistance * Math.Tan(mainCamera_fovh_radian / 2.0));
    //        scale_v = (videoPlaneDistance * Math.Tan(mainCamera_fovh_radian / 2.0) * 1.0 / video_AR);
    //    }
    //    else
    //    {
    //        double mainCamera_fovv_radian = xmgTools.ConvertToRadian((double)GetMainCameraFovV());
    //        scale_u = (videoPlaneDistance * Math.Tan(mainCamera_fovv_radian / 2.0) * video_AR);
    //        scale_v = (videoPlaneDistance * Math.Tan(mainCamera_fovv_radian / 2.0));
    //    }
    //    ret[0] = scale_u;
    //    ret[1] = scale_v;
    //    return ret;
    //}
}

[System.Serializable]
public class xmgVisionParameters
{
	[Tooltip("Number of internal squares inside a marker in a row \nchoose between 2, 3, 4 or 5\n This parameter is only available when using black and white markers")]
	public int MarkerType = 5;
	         
	[Tooltip("Fill this list with the scene pivot for which you want the pose to be modified")]
	public List<xmgObjectPivotLink> ObjectPivotLinks;
    
    [Tooltip("In case you need a filter to smooth the pose")]
    public bool RecursiveFilter = false;

    [Tooltip("Strength of the filter\n from 1 to 5")]
    public int FilterStrength = 1;

#if UNITY_EDITOR
    public class ClassifierEngine
    {
        string _infilename = "";
        string _outfilename = "";
        static public bool _isBuilding = false;
        private int _mode = 0;

        public ClassifierEngine(string infilename, string outfilename, int mode)
        {
            _infilename = infilename;
            _outfilename = outfilename;
            _mode = mode;
        }
        public void ThreadRun()
        {
            _isBuilding = true;
            Debug.Log("Building Classifier, please wait ...");
            byte[] image = System.IO.File.ReadAllBytes(_infilename);
            int classifierSize;
            if (_mode == 0)
            {
                classifierSize = xmgAugmentedVisionBridge.xzimgNaturalImageBuildClassifier(ref image[0], image.Length, 400);
                if (classifierSize > 0)
                {
                    byte[] classifier = new byte[classifierSize];
                    xmgAugmentedVisionBridge.xzimgFillNaturalImageClassifier(ref classifier[0]);

                    System.IO.File.WriteAllBytes(_outfilename, classifier);
                    Debug.Log("Building Classifier, please wait ... Classifier has been created!");
                }
                else
                    Debug.Log("impossible to create the Classifier for that image");
            }
            else if (_mode == 1)
            {
                classifierSize = xmgAugmentedVisionBridge.xzimgFramedImageBuildClassifier(ref image[0], image.Length, 400);
                if (classifierSize > 0)
                {
                    byte[] classifier = new byte[classifierSize];
                    xmgAugmentedVisionBridge.xzimgFillFramedImageClassifier(ref classifier[0]);

                    System.IO.File.WriteAllBytes(_outfilename, classifier);
                    Debug.Log("Building Classifier, please wait ... Classifier has been created!");
                }
                else
                    Debug.Log("impossible to create the Classifier for that image");

            }
            _isBuilding = false;
        }
    }

    [MenuItem("Assets/Create/XZIMG Natural Image Classifier")]
    private static void NaturalImageClassifier()
    {
        if (ClassifierEngine._isBuilding)
        {
            Debug.Log("Another classifier is being built, please retry in a few seconds");
            return;
        }
        string filename = Application.dataPath;
        string filenameBytes = filename + "/Resources/" + Selection.activeObject.name + "-naturalImage" + ".bytes";
        filename = filename + "/Resources/" + Selection.activeObject.name + ".jpg";
        if (System.IO.File.Exists(filename))
        {
            ClassifierEngine classifierEngine = new ClassifierEngine(filename, filenameBytes, 0);
            Thread producer = new Thread(new ThreadStart(classifierEngine.ThreadRun));
            try
            {
                producer.Start();
            }
            catch (ThreadStateException e)
            {
                Debug.Log(e);  // Display text of exception
            }
        }
    }


    [MenuItem("Assets/Create/XZIMG Framed Image Classifier")]
    private static void FramedImageClassifier()
    {
        string filename = Application.dataPath;
        string filenameBytes = filename + "/Resources/" + Selection.activeObject.name + "-framedImage" + ".bytes";
        filename = filename + "/Resources/" + Selection.activeObject.name + ".jpg";
        if (System.IO.File.Exists(filename))
        {
            ClassifierEngine classifierEngine = new ClassifierEngine(filename, filenameBytes, 1);
            Thread producer = new Thread(new ThreadStart(classifierEngine.ThreadRun));
            try
            {
                producer.Start();
            }
            catch (ThreadStateException e)
            {
                Debug.Log(e);  // Display text of exception
            }
        }
    }

#endif
}

//public class xmgTest : MonoBehaviour
//{
//    public GameObject test_;
//}

class xmgDebug
{
    public static string m_debugMessage = "";
}

//[CustomEditor(typeof(xmgParameters))]
////[CanEditMultipleObjects]
//public class xmgParametersEditor : Editor
//{
//    //REF http://docs.unity3d.com/ScriptReference/Editor.html
//    SerializedProperty scenePivotProp;

//    void OnEnable()
//    {
//        // Setup the SerializedProperties.
//        scenePivotProp = serializedObject.FindProperty("test");
//    }

//    public override void OnInspectorGUI()
//    {
//        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
//        serializedObject.Update();

//        EditorGUILayout.PropertyField(scenePivotProp, new GUIContent("LOL"));

//        // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
//        serializedObject.ApplyModifiedProperties();
//    }
//}
