using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPadButtonController : MonoBehaviour
{
    private Animator _animator;
    public int buttonValue;

    private IEnumerator _currentRoutine;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void ToggleAvailable(bool available)
    {
        _animator.SetBool("IsAvailable", available);
    }

    public void Press()
    {
        if (_currentRoutine == null)
        {
            _currentRoutine = PressButton();
            StartCoroutine(_currentRoutine);
        }
    }

    private IEnumerator PressButton()
    {
        int frameCount = 0;
        _animator.SetTrigger("Press");

        while (frameCount < 4)
        {
            yield return new WaitForEndOfFrame();
            frameCount++;
        }
        
        _currentRoutine = null;
    }

    public int GetValue()
    {
        return buttonValue;
    }
}
