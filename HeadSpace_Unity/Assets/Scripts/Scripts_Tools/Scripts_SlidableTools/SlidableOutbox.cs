using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidableOutbox : SlidableTool
{
    [Header("Animation settings")]
    public float smoothTime;

    private DropZone_Outbox _dropZone;
    private Animator _animator;

    private Vector2 _lerpStartPos;
    private IEnumerator _openingRoutine;

    private void OnEnable()
    {
        WritingMachineController.commandReadyToTear += OnCommandReadyToTear;
    }

    private void OnDisable()
    {
        WritingMachineController.commandReadyToTear -= OnCommandReadyToTear;
    }

    protected override void Awake()
    {
        base.Awake();
        _dropZone = GetComponentInChildren<DropZone_Outbox>();
        _animator = GetComponent<Animator>();
    }

    //protected override void Update()
    //{
    //    base.Update();

    //    if (Input.GetKeyDown(KeyCode.O))
    //    {
    //        StartCoroutine(OpenOutbox());
    //    }
    //}

    protected override void FullyCloseTool()
    {
        base.FullyCloseTool();
        _dropZone.SendCommands();
    }

    private void OnCommandReadyToTear()
    {
        if (_openingRoutine != null)
            return;

        _openingRoutine = OpenOutbox();
        StartCoroutine(_openingRoutine);
    }

    public override void Select()
    {
        if (_openingRoutine != null)
        {
            StopCoroutine(_openingRoutine);
            _openingRoutine = null;
        }

        base.Select();
    }

    private IEnumerator OpenOutbox()
    {
        CheckOpenState();
        _lerpStartPos = transform.position;
        //float time = 0f;
        Vector2 velocity = new Vector2();

        while (Vector2.Distance(transform.position, openPos) > 0.01f)
        {
            //time += Time.deltaTime * openingSpeed;
            //Vector2 newPos = Vector2.Lerp(_lerpStartPos, openPos, time);
            //transform.position = newPos;

            Vector2 smooth = Vector2.SmoothDamp(transform.position, openPos, ref velocity, smoothTime);
            transform.position = smooth;

            yield return new WaitForEndOfFrame();
        }
        transform.position = openPos;
        _openingRoutine = null;
    }
}
