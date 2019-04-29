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
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;	//List
using System.Text;

/// Contains the interface with the Augmented Vision API for different platforms
public class xmgAugmentedVisionBridge
{
    // -------------------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    public struct xmgVideoCaptureOptions
    {
        /// 0 is 320x240; 1, is 640x480; 2 is 720p (-1 if no internal capture);
        public int m_resolutionMode;

        /// 0 is frontal; 1 is back
        public int m_frontal;
        
        /// 0 auto-focus now; 1 auto-focus continually; 2 locked; 3 focus to infinity; 4 focus macro;
        public int m_focusMode;
        
        /// (no effect on Android) 
        public int m_exposureMode;

        /// 0 auto-white balance; 1 day-light white balance
        public int m_whileBalanceMode;              
    }

    static public void PrepareNativeVideoCapture(
        ref xmgVideoCaptureOptions videoCaptureOptions,
        int resolutionMode,
        int frontal,
        int focusMode,
        int exposureMode,
        int whileBalanceMode)
    {
        videoCaptureOptions.m_resolutionMode = resolutionMode;
        videoCaptureOptions.m_frontal = frontal;
        videoCaptureOptions.m_focusMode = focusMode;
        videoCaptureOptions.m_exposureMode = exposureMode;
        videoCaptureOptions.m_whileBalanceMode = whileBalanceMode;
    }

    static public void PrepareNativeVideoCaptureDefault(
        ref xmgVideoCaptureOptions videoCaptureOptions,
        int resolutionMode,
        int frontal)
    {
        videoCaptureOptions.m_resolutionMode = resolutionMode;
        videoCaptureOptions.m_frontal = frontal;
        videoCaptureOptions.m_focusMode = 1;
        videoCaptureOptions.m_exposureMode = 1;
        videoCaptureOptions.m_whileBalanceMode = 1;
#if (UNITY_ANDROID)
        videoCaptureOptions.m_focusMode = 2;        // 1 is for continuous auto-focus (so manual focus is not needed)
        videoCaptureOptions.m_exposureMode = -1;    // -1 is default
        videoCaptureOptions.m_whileBalanceMode = -1; // -1 is default
#endif
    }

    // -------------------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    public struct xmgImage
    {
        public int m_width;                 // Image dimension
        public int m_height;                // Image dimension
        public IntPtr m_imageData;          // Image data
        public int m_iWStep;                // Image Width Step (set to 0 for automatic computation)        
        public int m_colorType;             // pixel format XMG_BW=0, XMG_RGB=1, XMG_BGR=2, XMG_YUV=3, XMG_RGBA=4, XMG_BGRA=5, XMG_ARGB=6        
        public int m_type;                  // internal parameter do not change        
        public bool m_flippedHorizontaly;   // True if image is horizontally flipped 
    }

    static public void PrepareImage(
        ref xmgImage dstimage,
        int width, int height,
        int colortype,
        IntPtr ptrdata)
    {
        dstimage.m_width = width;
        dstimage.m_height = height;
        dstimage.m_colorType = colortype;
        dstimage.m_type = 0;
#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
        dstimage.m_flippedHorizontaly = false;
#else
        dstimage.m_flippedHorizontaly = true;
#endif
        dstimage.m_iWStep = 0;
        dstimage.m_imageData = ptrdata;
    }

    static public void PrepareGrayImage(
        ref xmgImage dstimage,
        int width, int height,
        IntPtr ptrdata)
    {
        dstimage.m_width = width;
        dstimage.m_height = height;
        dstimage.m_colorType = 0;
        dstimage.m_type = 0;
        dstimage.m_flippedHorizontaly = true;
        dstimage.m_iWStep = 0;
        dstimage.m_imageData = ptrdata;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct xmgMarkerInfo
    {
        public int markerID;
        public Vector3 position;
        public Vector3 euler;
        public Quaternion rotation;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct xmgVideoCaptureInfo
    {
        public int resolution_mode;         // 0 is 320x240; 1, is 640x480; 2 is 720p
        public int frontal;                 // 0 is frontal; 1 is back
        public int focus_mode;              // 0 auto-focus now; 1 auto-focus continually; 2 locked; 3; focus to point
        public int exposure_mode;           // 0 auto-focus now; 1 auto-focus continually; 2 locked; 3; focus to point
        public int while_balance_mode;      // 0 auto-focus now; 1 auto-focus continually; 2 locked; 3; focus to point
    }
    
    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------
    // Fonctions for Marker Detection
    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------


#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
	public static extern int xzimgMarkerInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
    public static extern int xzimgMarkerInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgMarkerInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#endif

    // --------------------------------------------------------------------------------------------------------------------------

#if (UNITY_EDITOR || UNITY_STANDALONE|| UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgMarkerRelease();
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
    public static extern void xzimgMarkerRelease();
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern void xzimgMarkerRelease();
#endif
    
    // --------------------------------------------------------------------------------------------------------------------------

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgMarkerSetActiveIndices(ref int arrIndices, ref float arrMarkerWidth, int nbrOfIndices);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern void xzimgMarkerSetActiveIndices(ref int arrIndices, ref float arrMarkerWidth, int nbrOfIndices);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern void xzimgMarkerSetActiveIndices(ref int arrIndices, ref float arrMarkerWidth, int nbrOfIndices);
#endif

    // --------------------------------------------------------------------------------------------------------------------------

#if (UNITY_EDITOR || UNITY_STANDALONE|| UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
	public static extern int xzimgMarkerDetect([In][Out] ref xmgImage imageIn, int markerType, int filterStrenght);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
    public static extern int xzimgMarkerDetect([In][Out] ref xmgImage imageIn, int markerType, int filterStrenght);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgMarkerDetect([In][Out] ref xmgImage imageIn, int markerType, int filterStrengh);
#endif

    // --------------------------------------------------------------------------------------------------------------------------

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgMarkerGetNumber();
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern int xzimgMarkerGetNumber();
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgMarkerGetNumber();
#endif

    // --------------------------------------------------------------------------------------------------------------------------

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgMarkerGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern void xzimgMarkerGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern void xzimgMarkerGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#endif

    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------
    // Fonctions for Natural Image Detection
    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgNaturalImageInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	//public static extern int xzimgNaturalImageInitialize([In][Out] ref xmgVideoCaptureInfo videoOptions, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
    public static extern int xzimgNaturalImageInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgNaturalImageRelease();
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
    public static extern void xzimgNaturalImageRelease();
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern void xzimgNaturalImageRelease();
#endif


#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgNaturalImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgNaturalImageFinalizeLearning();
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageFinalizeLearning();
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageFinalizeLearning();
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
	public static extern int xzimgNaturalImageTrack([In][Out] ref xmgImage imageIn, int filterStrenght);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	//public static extern int xzimgNaturalImageTrack(int filterStrenght);
    public static extern int xzimgNaturalImageTrack([In][Out] ref xmgImage imageIn, int filterStrenght);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageTrack([In][Out] ref xmgImage imageIn, int filterStrenght);
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgNaturalImageBuildClassifier([In] ref byte image, int length, int maxWidthOrHeight);
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgFillNaturalImageClassifier([In][Out] ref byte classifier);
#endif


#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgNaturalImageGetNumber();
#elif UNITY_IPHONE    
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageGetNumber();
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageGetNumber();
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgNaturalImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#elif UNITY_IPHONE    
	[DllImport ("__Internal")] 
	public static extern void xzimgNaturalImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
//#elif (UNITY_ANDROID)
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#endif

    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------
    // Fonctions for Framed-Image Detection
    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------
#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgFramedImageInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	//public static extern int xzimgFramedImageInitialize([In][Out] ref xmgVideoCaptureInfo videoOptions, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
    public static extern int xzimgFramedImageInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#endif


#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgFramedImageRelease();
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
    public static extern void xzimgFramedImageRelease();
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern void xzimgFramedImageRelease();
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
	public static extern int xzimgFramedImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
	public static extern int xzimgFramedImageDetect([In][Out] ref xmgImage imageIn, int iFilterStrenght, int bRecursiveTracking, int bRecursiveIdentification, int identificationMode);
#elif UNITY_IPHONE    
	[DllImport ("__Internal")] 
	//public static extern int xzimgFramedImageDetect(int iFilterStrenght, int bRecursiveTracking, int bRecursiveIdentification, int identificationMode);
    public static extern int xzimgFramedImageDetect([In][Out] ref xmgImage imageIn, int iFilterStrenght, int bRecursiveTracking, int bRecursiveIdentification, int identificationMode);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageDetect([In][Out] ref xmgImage imageIn, int iFilterStrenght, int bRecursiveTracking, int bRecursiveIdentification, int identificationMode);
#endif



#if (UNITY_EDITOR || UNITY_STANDALONE ||UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgFramedImageGetNumber();
#elif UNITY_IPHONE    
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageGetNumber();
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageGetNumber();
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgFramedImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern void xzimgFramedImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern void xzimgFramedImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#endif

    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------
    /// General functions
    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------
    

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgFramedImageBuildClassifier([In] ref byte image, int length, int maxWidthOrHeight);
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgFillFramedImageClassifier([In][Out] ref byte classifier);
#endif

    /// Video capture is a bit specific on Android because of messy Android API
#if (UNITY_ANDROID && !UNITY_EDITOR)
    private static AndroidJavaObject m_videoActivity = null;
    private static AndroidJavaObject m_activityContext = null;
    public static void xzimgCamera_create([In][Out] ref xmgVideoCaptureOptions videoCaptureParams)
    {
        if (m_activityContext == null)
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            m_activityContext = jc.GetStatic<AndroidJavaObject>("currentActivity");
        }

        if (m_videoActivity == null)
        {
            AndroidJavaClass xzimg_video_plugin = new AndroidJavaClass("com.xzimg.videocapture.VideoCaptureAPI");
            if (xzimg_video_plugin != null)
            {
                m_videoActivity = xzimg_video_plugin.CallStatic<AndroidJavaObject>("instance");
            }
        }
        if (m_videoActivity != null)
            m_videoActivity.Call("xzimgCamera_create", 
                videoCaptureParams.m_resolutionMode, 
                videoCaptureParams.m_frontal,
                videoCaptureParams.m_focusMode,
                videoCaptureParams.m_whileBalanceMode);
    }
    

    public static void xzimgCamera_delete()
    {
        if (m_videoActivity != null)
            m_videoActivity.Call("xzimgCamera_delete");
    }

    public static void xzimgCamera_focus()
    {
        if (m_videoActivity != null)
            m_videoActivity.Call("xzimgCamera_focus");
    }

    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgCamera_getCaptureWidth();
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgCamera_getCaptureHeight();
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgCamera_getImage(System.IntPtr rgba_frame);

#endif

    /// Video capture on IOS
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern int xzimgCamera_create([In][Out] ref xmgVideoCaptureOptions videoCaptureParams);
    [DllImport("__Internal")]
    public static extern int xzimgCamera_delete();
    [DllImport("__Internal")]
    public static extern int xzimgCamera_getCaptureWidth();
    [DllImport("__Internal")]
    public static extern int xzimgCamera_getCaptureHeight();
    [DllImport("__Internal")]
    public static extern int xzimgCamera_getImage(System.IntPtr rgba_frame);
#endif
}


