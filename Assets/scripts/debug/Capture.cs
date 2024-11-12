#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;

public class Capture : MonoBehaviour
{
    RecorderController m_recorderController;

    [SerializeField]
    private ScreenShotData[] screenShotDatas;

    private void Setting(string name, int width, int height)
    {
        string currentTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();

        m_recorderController = new RecorderController(controllerSettings);

        var mediaOutputFolder = Path.Combine(Application.dataPath, "../", "screenshot");


        //image
        var imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();

        imageRecorder.name = name;
        imageRecorder.Enabled = true;
        imageRecorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
        imageRecorder.CaptureAlpha = false;

        imageRecorder.OutputFile = Path.Combine(mediaOutputFolder, name + "_" + width + "_" + height + "_") + currentTime;

        Debug.Log(imageRecorder.OutputFile);

        imageRecorder.imageInputSettings = new GameViewInputSettings
        {
            OutputWidth = width,
            OutputHeight = height,
        };

        //setup Recording
        controllerSettings.AddRecorderSettings(imageRecorder);
        controllerSettings.SetRecordModeToSingleFrame(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(capture());
        }
    }

    IEnumerator capture()
    {
        foreach (ScreenShotData data in screenShotDatas)
        {
            Setting(data.name, data.width, data.height);
            m_recorderController.PrepareRecording();
            m_recorderController.StartRecording();
            yield return new WaitForSeconds(0.1f);
        }
    }
}

#endif
