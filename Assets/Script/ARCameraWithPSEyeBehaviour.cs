using UnityEngine;
using System;
using System.Collections;
using jp.nyatla.nyartoolkit.cs.markersystem;
using jp.nyatla.nyartoolkit.cs.core;
using NyARUnityUtils;
using System.IO;

public class ARCameraWithPSEyeBehaviour : MonoBehaviour
{
	// NyARToolKit
	private NyARUnityMarkerSystem markerSystemLeft_;
	private NyARUnityMarkerSystem markerSystemRight_;
	private NyARUnityPSEye psEyeLeft_;
	private NyARUnityPSEye psEyeRight_;
	private int markerId_;
	public string MarkerName = "MarkerHiro"; 
	public GameObject MarkerObjectLeft  = null;
	public GameObject MarkerObjectRight = null;
	
	// PS Eye
	public int FrameRate = 60;
	
	// Camera Object
	public GameObject BackgroundLeft  = null;
	public GameObject BackgroundRight = null;
	public Camera CameraLeft  = null;
	public Camera CameraRight = null;
	
	// Background
	public int LeftLayer  = 30;
	public int RightLayer = 31;
	
	void Awake()
	{
		// Check PS Eye number
		int psEyeCount = PSEyeTexture.CLEyeGetCameraCount();
		switch (psEyeCount) {
			case 0:
				Debug.LogError("No PS Eye found");
				return;
			case 1:
				Debug.LogError("Only one PS Eye found");
				return;
		}
		
		// Make PS Eye texture
		var leftPsEyeTexture = new PSEyeTexture(0, FrameRate, true);
		psEyeLeft_ = new NyARUnityPSEye(leftPsEyeTexture);
		var rightPsEyeTexture = new PSEyeTexture(1, FrameRate);
		psEyeRight_ = new NyARUnityPSEye(rightPsEyeTexture);
		
		// Marker system
		var configLeft     = new NyARMarkerSystemConfig(leftPsEyeTexture.Width, leftPsEyeTexture.Height);
		markerSystemLeft_  = new NyARUnityMarkerSystem(configLeft);
		var configRight    = new NyARMarkerSystemConfig(rightPsEyeTexture.Width, rightPsEyeTexture.Height);
		markerSystemRight_ = new NyARUnityMarkerSystem(configRight);
		
		// Load marker from texture
		var markerTexture = (Texture2D)(Resources.Load(MarkerName, typeof(Texture2D)));
		markerId_ = markerSystemLeft_.addARMarker(markerTexture, 16, 25, 80);
		markerId_ = markerSystemRight_.addARMarker(markerTexture, 16, 25, 80);
		
		// Marker layer
		MarkerObjectLeft.layer  = LeftLayer;
		MarkerObjectLeft.transform.FindChild("Cube").gameObject.layer = LeftLayer;
		MarkerObjectRight.layer = RightLayer;
		MarkerObjectRight.transform.FindChild("Cube").gameObject.layer = RightLayer;

		// Set camera background 
		// - Left
		BackgroundLeft.renderer.material.mainTexture = leftPsEyeTexture.Texture;
		BackgroundLeft.layer = LeftLayer;
		CameraLeft.cullingMask &= ~(1 << RightLayer);
		markerSystemLeft_.setARBackgroundTransform(BackgroundLeft.transform);
		markerSystemLeft_.setARCameraProjection(CameraLeft);
		// - Right
		BackgroundRight = GameObject.Find("BackgroundRight");
		BackgroundRight.renderer.material.mainTexture = rightPsEyeTexture.Texture;
		BackgroundRight.layer = RightLayer;
		CameraRight.cullingMask &= ~(1 << LeftLayer);
		markerSystemRight_.setARBackgroundTransform(BackgroundRight.transform);
		markerSystemRight_.setARCameraProjection(CameraRight);
		
		// Rotate based on PS Eyes' physical direction
		GameObject.Find("ARWorld").transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, -90.0f));
	}
	
	void Start()
	{
		psEyeLeft_.start();
		psEyeRight_.start();
	}
	
	void Update()
	{
		psEyeLeft_.update();
		psEyeRight_.update();
		markerSystemLeft_.update(psEyeLeft_);
		markerSystemRight_.update(psEyeRight_);
		
		// Left camera marker
		if (markerSystemLeft_.isExistMarker(markerId_)) {
			onFindLeftMaker();
		} else {
			onLostLeftMarker();
		}
		
		// Right camera marker
		if (markerSystemRight_.isExistMarker(markerId_)) {
			onFindRightMaker();
		} else {
			onLostRightMarker();
		}
	}
	
	void onFindLeftMaker()
	{
		markerSystemLeft_.setMarkerTransform(markerId_, MarkerObjectLeft.transform);
	}
	
	void onFindRightMaker()
	{
		markerSystemRight_.setMarkerTransform(markerId_, MarkerObjectRight.transform);
	}
	
	void onLostLeftMarker()
	{
		MarkerObjectLeft.transform.localPosition = new Vector3(0, 0, -100);
	}
	
	void onLostRightMarker()
	{
		MarkerObjectRight.transform.localPosition = new Vector3(0, 0, -100);
	}
}
