using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CutsceneController : MonoBehaviour
{
    [Header("Cutscene contents")]
    public List<CutsceneFrame> allCutsceneFrames;
    private int frameCount;

    private SpriteRenderer _spriteRenderer;
    private TextMeshProUGUI _cutSceneText;

    private int _frameIndex;
    private int _textIndex;
    private bool _ended;

    private int _charactersPerSec;
    private IEnumerator _currentTextRoutine;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _cutSceneText = GetComponentInChildren<TextMeshProUGUI>();
        frameCount = allCutsceneFrames.Count;
    }

    private void Start()
    {
        if (TextHelper.instance != null)
            _charactersPerSec = TextHelper.instance.charactersPerSec;
        else
            _charactersPerSec = 30;

        StartCutscene();
    }

    private void StartCutscene()
    {
        DisplayNextFrame();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnPlayerInput();
    }

    private void OnPlayerInput()
    {
        if (_ended)
            return;

        if (frameCount <= 0)
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
            DisplayNextFrame();
        }
        else
        {
            EndCutscene();
        }
    }

    private void DisplayNextLine()
    {
        ClearText();
        _cutSceneText.text = allCutsceneFrames[_frameIndex].allLines[_textIndex];

        _currentTextRoutine = TypeAnimationRoutine(_cutSceneText);
        StartCoroutine(_currentTextRoutine);

        _textIndex++;
    }

    private void DisplayNextFrame()
    {
        _spriteRenderer.sprite = allCutsceneFrames[_frameIndex].frame;
        _textIndex = 0;

        if (allCutsceneFrames[_frameIndex].allLines.Count > 0)
            DisplayNextLine();
        else
            ClearText();
    }

    private void ForceFullText()
    {
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
        Debug.Log("END");
    }

    private IEnumerator TypeAnimationRoutine(TextMeshProUGUI textMesh)
    {
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
}

[System.Serializable]
public struct CutsceneFrame
{
    public Sprite frame;
    public List<string> allLines;
}
