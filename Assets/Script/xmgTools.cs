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
using System.IO;
using System.Text;

public enum xmgOrientationMode
{
    LandscapeLeft = 0,
    Portrait = 1,
    LandscapeRight = 2,
    PortraitUpsideDown = 3,
};

public enum xmgSegmentationMode
{
    ModeNone = 0,
    ModeHairSegmentation = 1,
    ModeBodySegmentation = 2,
    ModeBodySegmentationRobust = 3, // On ios only
};

/**
 * Common tool functions
 */
public class xmgTools : MonoBehaviour
{
    static public xmgOrientationMode GetRenderOrientation(bool isFrontalCamera = true)
    {
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL)
        return xmgOrientationMode.LandscapeLeft;
#elif (UNITY_ANDROID)
		if (Screen.orientation == ScreenOrientation.LandscapeRight) return xmgOrientationMode.LandscapeLeft;
		else if (Screen.orientation == ScreenOrientation.Portrait) return xmgOrientationMode.PortraitUpsideDown;
		else if (Screen.orientation == ScreenOrientation.LandscapeLeft) return xmgOrientationMode.LandscapeRight;
		else return xmgOrientationMode.Portrait;        
#elif (UNITY_IOS)
		if (isFrontalCamera)
		{
			if (Screen.orientation == ScreenOrientation.LandscapeRight) return xmgOrientationMode.LandscapeRight;
			else if (Screen.orientation == ScreenOrientation.Portrait) return xmgOrientationMode.Portrait;
			else if (Screen.orientation == ScreenOrientation.LandscapeLeft) return xmgOrientationMode.LandscapeLeft;
			else return xmgOrientationMode.PortraitUpsideDown;
		}
		else
		{
		if (Screen.orientation == ScreenOrientation.LandscapeRight) return xmgOrientationMode.LandscapeLeft;
		else if (Screen.orientation == ScreenOrientation.Portrait) return xmgOrientationMode.PortraitUpsideDown;
		else if (Screen.orientation == ScreenOrientation.LandscapeLeft) return xmgOrientationMode.LandscapeRight;
		else return xmgOrientationMode.Portrait;
		}
#endif
    }

    /// <summary>
    ///  Get an orientation mode accoring to render orientation as set in the Player Settings
    ///  Note: on some devices/OS versions (eg. Google Pixels), Portrait PortraitUpsideDown will fall back into Portrait
    /// </summary>
    static public xmgOrientationMode GetVideoOrientation(bool useNativeCamera, bool isFrontalCamera = true)
    {
        //return xmgOrientationMode.Portrait;
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL)
        return xmgOrientationMode.LandscapeLeft;
#elif (UNITY_ANDROID)        
		if (Screen.orientation == ScreenOrientation.LandscapeRight) return xmgOrientationMode.LandscapeRight;
		else if (Screen.orientation == ScreenOrientation.Portrait) return xmgOrientationMode.Portrait;
		else if (Screen.orientation == ScreenOrientation.LandscapeLeft) return xmgOrientationMode.LandscapeLeft;
		else return xmgOrientationMode.PortraitUpsideDown;
        
#elif (UNITY_IOS)
		if (isFrontalCamera)
		{
			if (Screen.orientation == ScreenOrientation.LandscapeRight) return xmgOrientationMode.LandscapeRight;
			else if (Screen.orientation == ScreenOrientation.Portrait) return xmgOrientationMode.PortraitUpsideDown;
			else if (Screen.orientation == ScreenOrientation.LandscapeLeft) return xmgOrientationMode.LandscapeLeft;
			else return xmgOrientationMode.Portrait;
		}
		else
		{
			if (Screen.orientation == ScreenOrientation.LandscapeRight) return xmgOrientationMode.LandscapeRight;
			else if (Screen.orientation == ScreenOrientation.Portrait) return xmgOrientationMode.Portrait;
			else if (Screen.orientation == ScreenOrientation.LandscapeLeft) return xmgOrientationMode.LandscapeLeft;
			else return xmgOrientationMode.PortraitUpsideDown;
		}
#endif
    }

    // -------------------------------------------------------------------------------------------------------------------

    static public xmgOrientationMode GetDeviceCurrentOrientation(int captureDeviceOrientation, bool isFrontalCamera = false)
    {
        xmgOrientationMode orientation = xmgOrientationMode.LandscapeLeft;// Default portrait
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL)
        orientation = (xmgOrientationMode)captureDeviceOrientation;
#elif (UNITY_ANDROID)
        orientation = xmgOrientationMode.Portrait; // Default
        DeviceOrientation deviceOrientation = Input.deviceOrientation;
        if (deviceOrientation == DeviceOrientation.LandscapeRight) orientation = xmgOrientationMode.LandscapeRight;
        if (deviceOrientation == DeviceOrientation.LandscapeLeft) orientation = xmgOrientationMode.LandscapeLeft;
        if (deviceOrientation == DeviceOrientation.PortraitUpsideDown) orientation = xmgOrientationMode.PortraitUpsideDown;
		if (!isFrontalCamera && deviceOrientation == DeviceOrientation.Portrait) orientation = xmgOrientationMode.PortraitUpsideDown;
		if (!isFrontalCamera && deviceOrientation == DeviceOrientation.PortraitUpsideDown) orientation = xmgOrientationMode.Portrait;
#elif (UNITY_IOS)
		orientation = xmgOrientationMode.PortraitUpsideDown; // Default
		DeviceOrientation deviceOrientation = Input.deviceOrientation;
		if (deviceOrientation == DeviceOrientation.LandscapeRight) orientation = xmgOrientationMode.LandscapeLeft;
		if (deviceOrientation == DeviceOrientation.LandscapeLeft) orientation = xmgOrientationMode.LandscapeRight;
		if (deviceOrientation == DeviceOrientation.PortraitUpsideDown) orientation = xmgOrientationMode.Portrait;
		if (!isFrontalCamera && deviceOrientation == DeviceOrientation.LandscapeRight) orientation = xmgOrientationMode.LandscapeRight;
		if (!isFrontalCamera && deviceOrientation == DeviceOrientation.LandscapeLeft) orientation = xmgOrientationMode.LandscapeLeft;

#endif
        return orientation;
    }

    // -------------------------------------------------------------------------------

    static public float ConvertToRadian(float degreeAngle)
    {
        return (degreeAngle * ((float)Math.PI / 180.0f));
    }

    static public double ConvertToRadian(double degreeAngle)
    {
        return (degreeAngle * (Math.PI / 180.0f));
    }

    static public float ConvertToDegree(float degreeAngle)
    {
        return (degreeAngle * (180.0f / (float)Math.PI));
    }

    static public double ConvertToDegree(double degreeAngle)
    {
        return (degreeAngle * (180.0f / Math.PI));
    }

    static public double ConvertHorizontalFovToVerticalFov(double radianAngle, double aspectRatio)
    {
        return (Math.Atan(1.0 / aspectRatio * Math.Tan(radianAngle / 2.0)) * 2.0);
    }

    static public double ConvertVerticalFovToHorizontalFov(double radianAngle, double aspectRatio)
    {
        return (Math.Atan(aspectRatio * Math.Tan(radianAngle / 2.0)) * 2.0);
    }

    static public double ConvertFov(double degreeAngle, double aspectRatio)
    {
        return ConvertToDegree(Math.Atan(aspectRatio * Math.Tan(ConvertToRadian(degreeAngle) / 2.0)) * 2.0);
    }

    static public float ConvertFov(float fox_video_degree, float video_ar, float screen_ar)
    {
        double aspectRatio = (double)video_ar / screen_ar;
        return (float)ConvertToDegree(Math.Atan(aspectRatio * Math.Tan(ConvertToRadian((double)fox_video_degree) / 2.0)) * 2.0);

    }

    // -------------------------------------------------------------------------------

    static public Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2, TextureFormat.BGRA32, false);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

    // -------------------------------------------------------------------------------

    public static bool IsDoubleTap()
    {
        bool result = false;
        float MaxTimeWait = 1;
        float VariancePosition = 10;

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            float DeltaTime = Input.GetTouch(0).deltaTime;
            float DeltaPositionLenght = Input.GetTouch(0).deltaPosition.magnitude;

            if (DeltaTime > 0 && DeltaTime < MaxTimeWait && DeltaPositionLenght < VariancePosition)
                result = true;
        }
        return result;
    }
}