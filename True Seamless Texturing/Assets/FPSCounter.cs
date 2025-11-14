using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _text;

    private IEnumerator _mainCoroutine;
    private IEnumerator _5sCoroutine;

    private List<float> _last5sFps = new List<float>();
    
    public Action<float> OnAverageReported;

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;

        _mainCoroutine = CountFPS();
        _5sCoroutine = Get5sFps();
        StartCoroutine(_mainCoroutine);
        StartCoroutine(_5sCoroutine);
    }

    public void Stop()
    {
        StopCoroutine(_mainCoroutine);
        StopCoroutine(_5sCoroutine);
    }

    private IEnumerator CountFPS()
    {
        while (true) {
            float fps = 1f / Time.unscaledDeltaTime;
            _text.text = $"FPS: {Math.Round(fps, 2)}";

            _last5sFps.Add(fps);

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator Get5sFps()
    {
        while (true) {
            yield return new WaitForSeconds(5f);

            float average = 0;
            foreach (float fps in _last5sFps)
                average += fps;
            average /= _last5sFps.Count;

            _last5sFps.Clear();
            
            OnAverageReported?.Invoke(average);

            Debug.Log("Average: " + average);
        }
    }
}
