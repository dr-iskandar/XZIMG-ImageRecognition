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
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

/// Base Class for Augmented Vision Tracker
public class xmgAugmentedVisionBase : MonoBehaviour
{
    public xmgVideoCaptureParameters m_videoParameters;
    public xmgVisionParameters m_visionParameters;
    
    protected WebCamTexture m_webcamTexture = null;
    protected xmgVideoCapturePlane m_capturePlane = null;
    protected Color[] m_imageData;
    protected xmgAugmentedVisionBridge.xmgImage m_image;
    protected xmgAugmentedVisionBridge.xmgVideoCaptureOptions m_xmgVideoParams;

    // -------------------------------------------------------------------------------

    public void CheckParameters()
    {
        m_videoParameters.CheckVideoCaptureParameters();
    }

    public  void Awake()
    {
#if UNITY_ANDROID
        // -- Camera permission for Android
        GameObject dialog = null;
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            dialog = new GameObject();
        }
#endif
    }

    // -------------------------------------------------------------------------------

    public virtual void Start() {
        CheckParameters();
        if (!m_videoParameters.useNativeCapture)
        {
            // -- Unity webcam capture
            if (m_capturePlane == null)
            {
                m_capturePlane = (xmgVideoCapturePlane)gameObject.AddComponent(typeof(xmgVideoCapturePlane));
                m_webcamTexture = m_capturePlane.OpenVideoCapture(ref m_videoParameters);
                m_capturePlane.CreateVideoCapturePlane(
                    m_videoParameters.VideoPlaneScale,
                    m_videoParameters);
            }
            if (m_webcamTexture == null)
            {
                Debug.Log("Error - No camera detected!");
                return;
            }
        }
        else
        {
            // -- Native camera capture using xzimgCamera
            xmgAugmentedVisionBridge.PrepareNativeVideoCaptureDefault(
                ref m_xmgVideoParams,
                m_videoParameters.videoCaptureMode,
                m_videoParameters.UseFrontal ? 1 : 0);

            m_capturePlane = (xmgVideoCapturePlane)gameObject.AddComponent(typeof(xmgVideoCapturePlane));
            m_capturePlane.CreateVideoCapturePlane(
                m_videoParameters.VideoPlaneScale,
                m_videoParameters);

#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
            xmgAugmentedVisionBridge.xzimgCamera_create(ref m_xmgVideoParams);
#endif
        }
    }

    public virtual void Update() {
#if (!UNITY_EDITOR && UNITY_ANDROID)
        // -- double tap to start camera focus event
        if (xmgTools.IsDoubleTap())
            xmgAugmentedVisionBridge.xzimgCamera_focus();
#endif
    }

    /// Get a video frame
    public bool UpdateCamera()
    {
        if (!m_videoParameters.useNativeCapture)
        {
            if (m_capturePlane == null || !m_capturePlane.GetData())
                return false;
        }
        else
        {
#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
            int res = xmgAugmentedVisionBridge.xzimgCamera_getImage(
                m_capturePlane.m_PixelsHandle.AddrOfPinnedObject());
#endif
        }
        return true;
    }

    // -------------------------------------------------------------------------------

    public virtual void OnDisable()
    {
        if (m_capturePlane != null)
        {
            m_capturePlane.ReleaseVideoCapturePlane();
            m_capturePlane = null;
        }
#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
        if (m_videoParameters.useNativeCapture)
            xmgAugmentedVisionBridge.xzimgCamera_delete();
#endif
    }

    // -------------------------------------------------------------------------------

    void OnApplicationPaused(bool pauseStatus)
    {
        // Do something here if you need
    }

    // -------------------------------------------------------------------------------

    void OnApplicationFocus(bool status)
    {
        // track when losing/recovering focus
        bool handle_focus_loss = false;
        if (handle_focus_loss)
        {
#if (UNITY_STANDALONE || UNITY_EDITOR)
            if (m_webcamTexture != null && status == false)
                m_webcamTexture.Stop();     // you can pause as well
            else if (m_webcamTexture != null && status == true)
                m_webcamTexture.Play();
#endif
        }
    }

    // -------------------------------------------------------------------------------

    void OnGUI()
    {
        {
            GUILayout.Label(xmgDebug.m_debugMessage);            
            if (Screen.orientation == ScreenOrientation.Portrait) GUILayout.Label("Portrait");
            if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) GUILayout.Label("PortraitUpsideDown");
            if (Screen.orientation == ScreenOrientation.LandscapeLeft) GUILayout.Label("LandscapeLeft");
            if (Screen.orientation == ScreenOrientation.LandscapeRight) GUILayout.Label("LandscapeRight");
        }
    }

    // -------------------------------------------------------------------------------

    public void UpdateDebugDisplay(int iDetected)
    {
        if (iDetected > 0)
        {
            xmgDebug.m_debugMessage = "Marker Detected";
        }
        else if (iDetected == -11)
            xmgDebug.m_debugMessage = "Protection Alert - Wait or restart";
        else
            xmgDebug.m_debugMessage = "Marker not Detected";
    }



    // -------------------------------------------------------------------------------

    public void PrepareCamera()
    {
        float arVideo = m_capturePlane.m_captureWidth / m_capturePlane.m_captureHeight;
        float arScreen = m_videoParameters.GetScreenAspectRatio();
        float fovy_degree = (float)m_videoParameters.CameraVerticalFOV;

        // Compute correct focal length according to video capture crops and different available modes
        if (m_videoParameters.videoPlaneFittingMode == xmgVideoPlaneFittingMode.FitScreenHorizontally &&
                (xmgTools.GetRenderOrientation() == xmgOrientationMode.LandscapeLeft || 
                xmgTools.GetRenderOrientation() == xmgOrientationMode.LandscapeRight))
        {
            float fovx = (float)xmgTools.ConvertFov(
                m_videoParameters.CameraVerticalFOV,
                m_videoParameters.GetVideoAspectRatio());
            Camera.main.fieldOfView = (float)xmgTools.ConvertFov(
                fovx, 1.0f / m_videoParameters.GetScreenAspectRatio());
        }
        if (m_videoParameters.videoPlaneFittingMode == xmgVideoPlaneFittingMode.FitScreenVertically &&
                (xmgTools.GetRenderOrientation() == xmgOrientationMode.LandscapeLeft || 
                xmgTools.GetRenderOrientation() == xmgOrientationMode.LandscapeRight))
        {
            //float scaleY = (float)xmgVideoCapturePlane.GetScaleY(m_videoParameters);
            Camera.main.fieldOfView = m_videoParameters.CameraVerticalFOV;
        }

        if (m_videoParameters.videoPlaneFittingMode == xmgVideoPlaneFittingMode.FitScreenHorizontally &&
                (xmgTools.GetRenderOrientation() == xmgOrientationMode.Portrait || 
                xmgTools.GetRenderOrientation() == xmgOrientationMode.PortraitUpsideDown))
        {
            Camera.main.fieldOfView = (float)xmgTools.ConvertFov(
                m_videoParameters.CameraVerticalFOV, 
                m_videoParameters.GetVideoAspectRatio());
        }

        if (m_videoParameters.videoPlaneFittingMode == xmgVideoPlaneFittingMode.FitScreenVertically &&
                (xmgTools.GetRenderOrientation() == xmgOrientationMode.Portrait || 
                xmgTools.GetRenderOrientation() == xmgOrientationMode.PortraitUpsideDown))
        {
            Camera.main.fieldOfView = (float)xmgTools.ConvertFov(
                fovy_degree,
                arVideo,
                arScreen);
        }

        Camera.main.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        Camera.main.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
    }
    // -------------------------------------------------------------------------------

    //   /// Create a planar mesh and texture coordinates adapted for landscapeRight mode 
    //   public Mesh createPlanarMesh()
    //{
    //	Vector3[] Vertices = new Vector3[] { new Vector3(-1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, -1, 0), new Vector3(-1, -1, 0) };
    //	//Vector2[] UV = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
    //       Vector2[] UV = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
    //       int[] Triangles = new int[] { 0, 1, 2, 0, 2, 3 };
    //	Mesh mesh = new Mesh();
    //	mesh.vertices = Vertices;
    //	mesh.triangles = Triangles;
    //	mesh.uv = UV;
    //	return mesh;
    //   }

    // -------------------------------------------------------------------------------

    //    public void UpdateBackgroundPlaneOrientation(bool frontalCamera)
    //	{
    //		transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
    //		if (Screen.orientation == ScreenOrientation.Portrait) 
    //			gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
    //		else if (Screen.orientation == ScreenOrientation.LandscapeLeft)
    //			gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
    //		else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
    //			gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
    //#if UNITY_IOS
    //		if (frontalCamera)
    //		{

    //			transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
    //			if (Screen.orientation == ScreenOrientation.Portrait) 
    //				gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
    //			else if (Screen.orientation == ScreenOrientation.LandscapeRight)
    //				gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
    //			else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
    //				gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
    //		}
    //#endif
    //		if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
    //			Camera.main.fieldOfView = (float)m_videoParameters.GetPortraitMainCameraFovV();

    //	}

    // -------------------------------------------------------------------------------

    //  public void PrepareBackgroundPlane(bool frontalCamera)
    //  {
    //// Reset camera rotation and position
    //Camera.main.transform.position = new Vector3(0, 0, 0);
    //Camera.main.transform.rotation = Quaternion.Euler(0, 0, 0);

    //// Create a mesh to apply video texture
    //Mesh mesh = createPlanarMesh();
    //gameObject.AddComponent<MeshFilter>().mesh = mesh;

    //      // Rotate the mesh according to current screen orientation
    //      transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
    //      if (Screen.orientation == ScreenOrientation.Portrait) 
    //	gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
    //else if (Screen.orientation == ScreenOrientation.LandscapeLeft)
    //	gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
    //else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
    //	gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);

    //      // Prepare ratios and camera fov
    //      Camera.main.fieldOfView = (float)m_videoParameters.GetMainCameraFovV();
    //      if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
    //          Camera.main.fieldOfView = (float)m_videoParameters.GetPortraitMainCameraFovV();

    //      // Modify Game Object's position & orientation
    //      double VideoPlaneDistance = 750;
    //      gameObject.transform.position = new Vector3(0, 0, (float)VideoPlaneDistance);
    //      double[] scale = m_videoParameters.GetVideoPlaneScale(VideoPlaneDistance);

    //      if (m_videoParameters.MirrorVideo)
    //          transform.localScale = new Vector3((float)scale[0], (float)scale[1], (float)1);
    //      else
    //          transform.localScale = new Vector3((float)-scale[0], (float)scale[1], (float)1);
    //      transform.localScale *= m_videoParameters.VideoPlaneScale;

    //      // __ Assign video texture to the renderer
    //      if (!GetComponent<Renderer>())
    //          gameObject.AddComponent<MeshRenderer>();

    //      gameObject.GetComponent<Renderer>().material = new Material( Shader.Find("Custom/VideoShader"));

    //  }
}

//public class xmgTools
//{
//    static public float ConvertToRadian(float degreeAngle )
//    {
//        return (degreeAngle * ((float)Math.PI / 180.0f));
//    }
//    static public double ConvertToRadian(double degreeAngle)
//    {
//        return (degreeAngle * (Math.PI / 180.0f));
//    }
//    static public float ConvertToDegree(float degreeAngle)
//    {
//        return (degreeAngle * (180.0f / (float)Math.PI));
//    }
//    static public double ConvertToDegree(double degreeAngle)
//    {
//        return (degreeAngle * (180.0f / Math.PI));
//    }
//    static public double ConvertHorizontalFovToVerticalFov(double radianAngle, double aspectRatio)
//    {
//        return ( Math.Atan(1.0 / aspectRatio * Math.Tan(radianAngle/2.0)) * 2.0);
//    }

//    static public double ConvertVerticalFovToHorizontalFov(double radianAngle, double aspectRatio)
//    {
//        return (Math.Atan(aspectRatio * Math.Tan(radianAngle / 2.0)) * 2.0);
//    }
//}