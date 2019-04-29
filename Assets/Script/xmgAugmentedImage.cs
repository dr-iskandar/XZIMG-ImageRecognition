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


/// Image Augmentation Engine
public class xmgAugmentedImage : xmgAugmentedVisionBase
{
    public override void Start()
    {
        base.Start();

        float fovy_radian = m_videoParameters.CameraVerticalFOV * 3.1415f / 180.0f;
        int success = xmgAugmentedVisionBridge.xzimgNaturalImageInitialize(
            m_capturePlane.m_captureWidth,
            m_capturePlane.m_captureHeight,
            m_videoParameters.GetProcessingWidth(m_capturePlane.m_captureWidth),
            m_videoParameters.GetProcessingHeight(m_capturePlane.m_captureHeight),
            fovy_radian);
        if (success == 1)
            print("xzimgNaturalImageInitialize - success");

        // -- Prepare image
#if (!UNITY_EDITOR && UNITY_IOS)
        int colorMode = 5; // XMG_BGRA
#else
        int colorMode = 4; // XMG_RGBA
#endif
        xmgAugmentedVisionBridge.PrepareImage(
            ref m_image,
            m_capturePlane.m_captureWidth,
            m_capturePlane.m_captureHeight,
            colorMode,
            m_capturePlane.m_PixelsHandle.AddrOfPinnedObject());

        PrepareCamera();
        LoadImages();
    }

    // -------------------------------------------------------------------------------

    public override void OnDisable()
    {
        base.OnDisable();
        xmgAugmentedVisionBridge.xzimgNaturalImageRelease();
    }

    // -------------------------------------------------------------------------------

    public override void Update()
    {
        base.Update();

        // -- Video Capture and the rest
        if (!UpdateCamera()) return;

        // -- Detection
        xmgAugmentedVisionBridge.xzimgNaturalImageTrack(
            ref m_image, m_visionParameters.FilterStrength);

        // -- Rendering
        DisableObjects();
        int iNbrOfDetection = xmgAugmentedVisionBridge.xzimgNaturalImageGetNumber();
        UpdateDebugDisplay(iNbrOfDetection);
        if (iNbrOfDetection > 0)
        {
            for (int i = 0; i < iNbrOfDetection; i++)
            {
                xmgAugmentedVisionBridge.xmgMarkerInfo markerInfo = new xmgAugmentedVisionBridge.xmgMarkerInfo();
                xmgAugmentedVisionBridge.xzimgNaturalImageGetInfoForUnity(i, ref markerInfo);
                EnableObject(markerInfo.markerID);
                UpdateObjectPosition(ref markerInfo);
            }
        }
        m_capturePlane.ApplyTexture();

    }
    
    // -------------------------------------------------------------------------------

    // Load Resources from the /Assets/Resources directory
    void LoadImages()
	{
		for (int i = 0; i < m_visionParameters.ObjectPivotLinks.Count; i++)
		{
			if (m_visionParameters.ObjectPivotLinks[i].Classifier)
			{
                if (m_visionParameters.ObjectPivotLinks[i].ObjectRealWidth <= 0)
                    m_visionParameters.ObjectPivotLinks[i].ObjectRealWidth = 1;

				TextAsset asset = m_visionParameters.ObjectPivotLinks[i].Classifier as TextAsset;
				byte[] arrBytes = new byte[asset.bytes.Length];
				Buffer.BlockCopy(asset.bytes, 0, arrBytes, 0, asset.bytes.Length);
				GCHandle bytesHandle = GCHandle.Alloc(arrBytes, GCHandleType.Pinned);
				int success = xmgAugmentedVisionBridge.xzimgNaturalImageAddTarget(
                    bytesHandle.AddrOfPinnedObject(),
                    arrBytes.Length,
                    m_visionParameters.ObjectPivotLinks[i].ObjectRealWidth);
				if (success == 1) 
					print("xzimgMarkerlessLoadClassifier - Success");
				else 
					print("failed to load " + asset.name);

				bytesHandle.Free();

			}
		}
	}

    // -------------------------------------------------------------------------------

    private void DisableObjects()
	{
		if (m_visionParameters.ObjectPivotLinks.Count > 0)
		{
			for (int i = 0; i < m_visionParameters.ObjectPivotLinks.Count; i++)
			{
				if (m_visionParameters.ObjectPivotLinks[i].ScenePivot)
				{
					Renderer[] renderers = m_visionParameters.ObjectPivotLinks[i].ScenePivot.GetComponentsInChildren<Renderer>();
					foreach (Renderer r in renderers) r.enabled = false;
				}
			}
		}
	}

    // -------------------------------------------------------------------------------

    private void EnableObject(int indexPivot)
	{
		if (indexPivot < m_visionParameters.ObjectPivotLinks.Count &&
            m_visionParameters.ObjectPivotLinks[indexPivot].ScenePivot)
		{
            m_visionParameters.ObjectPivotLinks[indexPivot].ScenePivot.SetActive(true);
			Renderer[] renderers = m_visionParameters.ObjectPivotLinks[indexPivot].ScenePivot.GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renderers) r.enabled = true;
		}
	}

    // -------------------------------------------------------------------------------

    void UpdateObjectPosition(ref xmgAugmentedVisionBridge.xmgMarkerInfo markerData)
    {
        Quaternion quatRot = Quaternion.Euler(0, 0, 0);
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
		if (Screen.orientation == ScreenOrientation.Portrait) 
            quatRot = Quaternion.Euler(0, 0, -90);
        else if (Screen.orientation == ScreenOrientation.LandscapeRight)
            quatRot = Quaternion.Euler(0, 0, 180);
        else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            quatRot = Quaternion.Euler(0, 0, 90);
#endif
        int pivotIndex = markerData.markerID;
        if (pivotIndex < m_visionParameters.ObjectPivotLinks.Count &&
            m_visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot)
        {
            Vector3 position = markerData.position;
            position.x *= m_videoParameters.VideoPlaneScale;
            position.y = -position.y;
            position.y *= m_videoParameters.VideoPlaneScale;
            Quaternion quat = Quaternion.Euler(markerData.euler);
#if UNITY_IOS

			if (m_videoParameters.UseFrontal)
				{
					position.x = -position.x;
					position.y = -position.y;
					quat.x = -quat.x;
					quat.y = -quat.y;
					
				}
#endif
			if (m_videoParameters.MirrorVideo)
            {
                quat.y = -quat.y;
                quat.z = -quat.z;
                position.x = -position.x;
            }
            m_visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localPosition = quatRot * position;
            m_visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localRotation = quatRot * quat;
            m_visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localScale = new Vector3(
                m_videoParameters.VideoPlaneScale, 
                m_videoParameters.VideoPlaneScale, 
                m_videoParameters.VideoPlaneScale);
        }
    }
}
