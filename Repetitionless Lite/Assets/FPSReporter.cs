using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSReporter : MonoBehaviour
{
    private const int TOTAL_REQUIRED_AVERAGES = 6;

    [SerializeField] private FPSCounter _fpsCounter;
    [SerializeField] private TextMeshProUGUI _averagesCounterText; 
    [Space(20)]

    [SerializeField] private string _currentMaterialString;
    [SerializeField] private GameObject _finalScreen;
    [SerializeField] private TextMeshProUGUI _finalScreenText;
    [SerializeField] private Button _finalScreenButton;

    List<float> _fpsAverages = new List<float>();

    private void Start()
    {
        if (_averagesCounterText != null)
            _averagesCounterText.text = $"Progress: 0/{TOTAL_REQUIRED_AVERAGES}";

        if (_finalScreen != null)
            _finalScreen.SetActive(false);

        if (_fpsCounter == null)
            return;

        _fpsCounter.OnAverageReported += AverageReported;
    }

    private void AverageReported(float average)
    {
        _fpsAverages.Add(average);

        if (_averagesCounterText != null)
            _averagesCounterText.text = $"Progress: {_fpsAverages.Count}/{TOTAL_REQUIRED_AVERAGES}";

        if (_fpsAverages.Count < TOTAL_REQUIRED_AVERAGES)
            return;

        _fpsCounter.OnAverageReported -= AverageReported;
        _fpsCounter.Stop();

        string finalText = $"{_currentMaterialString}\n5s Average FPS: ";

        float totalAverage = 0;
        for (int i = 0; i < _fpsAverages.Count; i++) {
            totalAverage += _fpsAverages[i];
            
            if (i != 0) finalText += ", ";
            finalText += Math.Round(_fpsAverages[i], 2);
        }
        totalAverage /= _fpsAverages.Count;

        finalText += $"\nTotal Average FPS: {totalAverage}";
        
        _finalScreenText.text = finalText;
        _finalScreenButton.onClick.AddListener(() => {
            GUIUtility.systemCopyBuffer = finalText; 
        });
        _finalScreen.SetActive(true);
        
    }
}
