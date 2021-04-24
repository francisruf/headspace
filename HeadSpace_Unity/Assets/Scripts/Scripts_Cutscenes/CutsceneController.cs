using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CutsceneController : MonoBehaviour
{
    public static Action<string, SceneLoadType> cutsceneOver;

    [Header("Cutscene contents")]
    public List<CutsceneFrame> allCutsceneFrames;
    private int frameCount;

    [Header("Next scene info")]
    public string nextSceneName;
    public SceneLoadType sceneType;

    private SpriteRenderer _spriteRenderer;
    private TextMeshProUGUI _cutSceneText;

    private int _frameIndex;
    private int _textIndex;
    private bool _ended;

    private int _charactersPerSec;
    private IEnumerator _currentTextRoutine;
    private IEnumerator _currentFrameRoutine;

    private void OnEnable()
    {
        LevelManager.loadingDone += StartCutscene;
    }

    private void OnDisable()
    {
        LevelManager.loadingDone -= StartCutscene;
    }

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _cutSceneText = GetComponentInChildren<TextMeshProUGUI>();
        frameCount = allCutsceneFrames.Count;
        _cutSceneText.text = "";

        _spriteRenderer.sprite = allCutsceneFrames[_frameIndex].frame;
        _textIndex = 0;
    }

    private void Start()
    {
        if (TextHelper.instance != null)
            _charactersPerSec = TextHelper.instance.charactersPerSec;
        else
            _charactersPerSec = 30;
    }

    private void StartCutscene()
    {
        _currentFrameRoutine = DisplayNextFrame();
        StartCoroutine(_currentFrameRoutine);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnPlayerInput();

        if (Input.GetKeyDown(KeyCode.Space))
            OnPlayerInput();

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
            OnPlayerInput();

        if (Input.GetKeyDown(KeyCode.Return))
            OnPlayerInput();
    }

    private void OnPlayerInput()
    {
        if (_ended)
            return;

        if (frameCount <= 0)
            return;

        if (_currentFrameRoutine != null)
            return;

        if (_currentTextRoutine != null)
        {
            StopCoroutine(_currentTextRoutine);
            _currentTextRoutine = null;
            ForceFullText();
        }

        else if (_textIndex < allCutsceneFrames[_frameIndex].allLines.Count)
        {
            DisplayNextLine();
        }

        else if (_frameIndex + 1 < frameCount)
        {
            _frameIndex++;
            _currentFrameRoutine = DisplayNextFrame();
            StartCoroutine(_currentFrameRoutine);
        }

        else
        {
            EndCutscene();
        }
    }

    private void DisplayNextLine()
    {
        if (_currentTextRoutine != null)
        {
            StopCoroutine(_currentTextRoutine);
            _currentTextRoutine = null;
            ForceFullText();
        }

        ClearText();
        _cutSceneText.text = allCutsceneFrames[_frameIndex].allLines[_textIndex];

        _currentTextRoutine = TypeAnimationRoutine(_cutSceneText);
        StartCoroutine(_currentTextRoutine);

        _textIndex++;
    }

    private void ForceFullText()
    {
        _cutSceneText.enabled = true;
        _cutSceneText.maxVisibleCharacters = int.MaxValue;
        _cutSceneText.ForceMeshUpdate();
    }

    private void ClearText()
    {
        _cutSceneText.text = "";
    }


    private void EndCutscene()
    {
        _ended = true;

        if (cutsceneOver != null)
            cutsceneOver(nextSceneName, sceneType);
    }

    private IEnumerator TypeAnimationRoutine(TextMeshProUGUI textMesh)
    {
        textMesh.enabled = false;
        yield return new WaitForSeconds(0.25f);

        TMP_TextInfo info = textMesh.textInfo;
        int count = 0;
        textMesh.enabled = true;
        textMesh.ForceMeshUpdate();
        int maxVisible = info.characterCount;
        textMesh.maxVisibleCharacters = 0;

        while (count <= maxVisible)
        {
            count++;
            textMesh.maxVisibleCharacters = count;
            yield return new WaitForSeconds(1f / _charactersPerSec);
        }
        _currentTextRoutine = null;
    }

    private IEnumerator DisplayNextFrame()
    {
        yield return new WaitForSeconds(0.05f);
        _cutSceneText.text = "";
        _spriteRenderer.sprite = allCutsceneFrames[_frameIndex].frame;

        _textIndex = 0;

        if (allCutsceneFrames[_frameIndex].allLines.Count > 0)
            DisplayNextLine();
        else
            ClearText();

        _currentFrameRoutine = null;
    }
}

[System.Serializable]
public struct CutsceneFrame
{
    public Sprite frame;
    public List<string> allLines;
}
