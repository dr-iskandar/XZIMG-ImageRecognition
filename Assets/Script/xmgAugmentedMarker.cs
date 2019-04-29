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

public class xmgAugmentedMarker : xmgAugmentedVisionBase
{
    public override void Start()
    {
        base.Start();

        float fovx_radian = m_videoParameters.CameraVerticalFOV * 3.1415f / 180.0f;
        int success = xmgAugmentedVisionBridge.xzimgMarkerInitialize(
            m_capturePlane.m_captureWidth,
            m_capturePlane.m_captureHeight,
            m_videoParameters.GetProcessingWidth(m_capturePlane.m_captureWidth),
            m_videoParameters.GetProcessingHeight(m_capturePlane.m_captureHeight),
            fovx_radian);
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
        LoadMarkersImages();
    }

    // -------------------------------------------------------------------------------

    public override void OnDisable()
    {
        base.OnDisable();
        xmgAugmentedVisionBridge.xzimgMarkerRelease();
	}

    // -------------------------------------------------------------------------------

    public override void Update()
    {
        // -- Video Capture and the rest
        if (!UpdateCamera()) return;

        // -- Detection
        xmgAugmentedVisionBridge.xzimgMarkerDetect(
            ref m_image, m_visionParameters.MarkerType, m_visionParameters.FilterStrength);

        // -- Rendering
        DisableObjects();
        int iNbrOfDetection = xmgAugmentedVisionBridge.xzimgMarkerGetNumber();
        UpdateDebugDisplay(iNbrOfDetection);
        if (iNbrOfDetection > 0)
        {
            for (int i = 0; i < iNbrOfDetection; i++)
            {
                xmgAugmentedVisionBridge.xmgMarkerInfo markerInfo = new xmgAugmentedVisionBridge.xmgMarkerInfo();
                xmgAugmentedVisionBridge.xzimgMarkerGetInfoForUnity(i, ref markerInfo);
                int indexPivot = GetPivotIndex(markerInfo.markerID);
                if (indexPivot >= 0)
                {
                    EnableObject(indexPivot);
                    UpdateObjectPosition(ref markerInfo);
                }
            }
        }
        m_capturePlane.ApplyTexture();
    }

    // -------------------------------------------------------------------------------

    // Load Markers
    void LoadMarkersImages()
    {
        if (m_visionParameters.ObjectPivotLinks.Count > 0)
        {
            int[] arrIndices = new int[m_visionParameters.ObjectPivotLinks.Count];
            float[] arrObjectWidth = new float[m_visionParameters.ObjectPivotLinks.Count];
            for (int i = 0; i < m_visionParameters.ObjectPivotLinks.Count; i++)
            {
                arrIndices[i] = m_visionParameters.ObjectPivotLinks[i].MarkerIndex;
                if (m_visionParameters.ObjectPivotLinks[i].ObjectRealWidth <= 0)
                    m_visionParameters.ObjectPivotLinks[i].ObjectRealWidth = 1;
                arrObjectWidth[i] = m_visionParameters.ObjectPivotLinks[i].ObjectRealWidth;
            }
            xmgAugmentedVisionBridge.xzimgMarkerSetActiveIndices(
                ref arrIndices[0], ref arrObjectWidth[0], arrIndices.Length);
        }
    }

    // -------------------------------------------------------------------------------

    private int GetPivotIndex(int MarkerIndex)
	{
		
		for (int i = 0; i < m_visionParameters.ObjectPivotLinks.Count; i++)
			if (m_visionParameters.ObjectPivotLinks[i].MarkerIndex == MarkerIndex) return i;
		return MarkerIndex;
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
		if (indexPivot < m_visionParameters.ObjectPivotLinks.Count && m_visionParameters.ObjectPivotLinks[indexPivot].ScenePivot)
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
		int pivotIndex = GetPivotIndex(markerData.markerID);
		if (pivotIndex < m_visionParameters.ObjectPivotLinks.Count &&
            m_visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot)
		{
			Vector3 position = markerData.position;
            position.x *= m_videoParameters.VideoPlaneScale;
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
            m_visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localPosition = quatRot*position;
            m_visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localRotation = quatRot * quat;
            m_visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localScale = new Vector3(
                m_videoParameters.VideoPlaneScale, 
                m_videoParameters.VideoPlaneScale, 
                m_videoParameters.VideoPlaneScale);
        }
	}
}
