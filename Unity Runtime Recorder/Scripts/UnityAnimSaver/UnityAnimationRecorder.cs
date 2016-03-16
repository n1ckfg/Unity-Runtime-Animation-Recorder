﻿using UnityEngine;
using UnityEditor;
using System.Collections;

public class UnityAnimationRecorder : MonoBehaviour {

	// save file path
	public string savePath;
	public string fileName;

	public KeyCode startRecordKey;
	public KeyCode stopRecordKey;

	// options
	public bool showLogGUI = false;
	string logMessage = "";

	public bool recordLimitedFrames = false;
	public int recordFrames = 1000;
	int frameIndex = 0;

	public bool changeTimeScale = false;
	public float timeScaleOnStart = 0.0f;
	public float timeScaleOnRecord = 1.0f;

	Transform[] recordObjs;
	UnityObjectAnimation[] objRecorders;

	bool isStart = false;
	float nowTime = 0.0f;

	// Use this for initialization
	void Start () {
		recordObjs = gameObject.GetComponentsInChildren<Transform> ();
		objRecorders = new UnityObjectAnimation[recordObjs.Length];

		for (int i = 0; i < recordObjs.Length; i++) {
			string path = AnimationRecorderHelper.GetTransformPathName (transform, recordObjs [i]);
			objRecorders [i] = new UnityObjectAnimation ( path, recordObjs [i]);
		}

		if (changeTimeScale)
			Time.timeScale = timeScaleOnStart;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetKeyDown (startRecordKey)) {
			CustomDebug ("Start Recorder");
			isStart = true;
			Time.timeScale = timeScaleOnRecord;
		}

		if (Input.GetKeyDown (stopRecordKey)) {
			CustomDebug ("End Record, generating .anim file");
			isStart = false;

			ExportAnimationClip ();
		}

		if (isStart) {
			nowTime += Time.deltaTime;

			for (int i = 0; i < objRecorders.Length; i++) {
				objRecorders [i].AddFrame (nowTime);
			}
		}

	}

	void FixedUpdate () {

		if (isStart) {

			if (frameIndex < recordFrames) {
				for (int i = 0; i < objRecorders.Length; i++) {
					objRecorders [i].AddFrame (nowTime);
				}

				++frameIndex;
			} else {
				isStart = false;
				ExportAnimationClip ();
				CustomDebug ("Recording Finish, generating .anim file");
			}
		}
	}

	void OnGUI () {
		if (showLogGUI)
			GUILayout.Label (logMessage);
	}

	void ExportAnimationClip () {

		string exportFilePath = savePath + fileName;

		AnimationClip clip = new AnimationClip ();
		clip.name = fileName;

		for (int i = 0; i < objRecorders.Length; i++) {
			UnityCurveContainer[] curves = objRecorders [i].curves;

			for (int x = 0; x < curves.Length; x++) {
				clip.SetCurve (objRecorders [i].pathName, typeof(Transform), curves [x].propertyName, curves [x].animCurve);
			}
		}

		clip.EnsureQuaternionContinuity ();
		AssetDatabase.CreateAsset ( clip, exportFilePath );

		CustomDebug (".anim file generated to " + exportFilePath);
	}

	void CustomDebug ( string message ) {
		if (showLogGUI)
			logMessage = message;
		else
			Debug.Log (message);
	}
}