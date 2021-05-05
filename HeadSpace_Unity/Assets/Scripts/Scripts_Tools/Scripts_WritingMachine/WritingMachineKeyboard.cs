using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WritingMachineKeyboard : MonoBehaviour
{
    public static Action<char> keyPress;
    public Transform keyboardStartPoint;
    public GameObject keyPrefab;
    public GameObject backSpacePrefab;
    public GameObject returnPrefab;
    public GameObject spacePrefab;
    public GameObject tabPrefab;

    private const float KEY_SPACING = 0.5f;
    private const float LINE_SPACING = 0.5f;
    private const float LINE_INDENT = 0.25f;

    private List<KeyMatch> _allKeys = new List<KeyMatch>();
    private int _keyCount;
    private char[] _qKeys = { 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p' };
    private char[] _aKeys = { 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l' };
    private char[] _zKeys = { 'z', 'x', 'c', 'v', 'b', 'n', 'm'};
    private int sortingOrder;

    private void Awake()
    {
        Spawn1Row();
        SpawnQRow();
        SpawnARow();
        SpawnZRow();
        _keyCount = _allKeys.Count;
    }

    private void Spawn1Row()
    {
        Vector2 spawnPos = keyboardStartPoint.position;

        for (char c = '1'; c <='9'; c++)
        {
            GameObject obj = Instantiate(keyPrefab, spawnPos, Quaternion.identity, transform);
            TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = c.ToString();
            Animator anim = obj.GetComponent<Animator>();
            _allKeys.Add(new KeyMatch(c, txt, anim));
            spawnPos.x += KEY_SPACING;

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            sortingOrder = sr.sortingOrder;
        }
        GameObject obj2 = Instantiate(keyPrefab, spawnPos, Quaternion.identity, transform);
        TextMeshProUGUI txt2 = obj2.GetComponentInChildren<TextMeshProUGUI>();
        txt2.text = '0'.ToString();
        Animator anim2 = obj2.GetComponent<Animator>();
        _allKeys.Add(new KeyMatch('0', txt2, anim2));

        spawnPos.x += KEY_SPACING;

        GameObject obj3 = Instantiate(backSpacePrefab, spawnPos, Quaternion.identity, transform);
        TextMeshProUGUI txt3 = null;
        Animator anim3 = obj3.GetComponent<Animator>();
        _allKeys.Add(new KeyMatch('@', txt3, anim3));
        sortingOrder++;
    }

    private void SpawnQRow()
    {
        Vector2 spawnPos = keyboardStartPoint.position;
        spawnPos.y -= LINE_SPACING;
        spawnPos.x += LINE_INDENT;

        for (int i = 0; i < _qKeys.Length; i++)
        {
            char c = char.ToUpper(_qKeys[i]);
            GameObject obj = Instantiate(keyPrefab, spawnPos, Quaternion.identity, transform);
            TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = c.ToString();
            Animator anim = obj.GetComponent<Animator>();
            _allKeys.Add(new KeyMatch(c, txt, anim));

            spawnPos.x += KEY_SPACING;

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            Canvas can = obj.GetComponentInChildren<Canvas>();
            sr.sortingOrder = sortingOrder;
            can.sortingOrder = sortingOrder;
        }
        sortingOrder++;
    }

    private void SpawnARow()
    {
        Vector2 spawnPos = keyboardStartPoint.position;
        spawnPos.y -= LINE_SPACING * 2;
        spawnPos.x += LINE_INDENT * 2;

        for (int i = 0; i < _aKeys.Length; i++)
        {
            char c = char.ToUpper(_aKeys[i]);
            GameObject obj = Instantiate(keyPrefab, spawnPos, Quaternion.identity, transform);
            TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = c.ToString();
            Animator anim = obj.GetComponent<Animator>();
            _allKeys.Add(new KeyMatch(c, txt, anim));

            spawnPos.x += KEY_SPACING;

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            Canvas can = obj.GetComponentInChildren<Canvas>();
            sr.sortingOrder = sortingOrder;
            can.sortingOrder = sortingOrder;
        }
        
        GameObject obj2 = Instantiate(returnPrefab, spawnPos, Quaternion.identity, transform);
        TextMeshProUGUI txt2 = null;
        Animator anim2 = obj2.GetComponent<Animator>();
        _allKeys.Add(new KeyMatch('#', txt2, anim2));

        SpriteRenderer sr2 = obj2.GetComponent<SpriteRenderer>();
        sr2.sortingOrder = sortingOrder;

        sortingOrder++;
    }

    private void SpawnZRow()
    {
        Vector2 spawnPos = keyboardStartPoint.position;
        spawnPos.y -= LINE_SPACING * 3;

        GameObject tabObj = Instantiate(tabPrefab, spawnPos, Quaternion.identity, transform);
        TextMeshProUGUI txt3 = null;
        Animator anim3 = tabObj.GetComponent<Animator>();
        _allKeys.Add(new KeyMatch('>', txt3, anim3));

        SpriteRenderer sr3 = tabObj.GetComponent<SpriteRenderer>();
        sr3.sortingOrder = sortingOrder;

        spawnPos.x += LINE_INDENT * 3;

        for (int i = 0; i < _zKeys.Length; i++)
        {
            char c = char.ToUpper(_zKeys[i]);
            GameObject obj = Instantiate(keyPrefab, spawnPos, Quaternion.identity, transform);
            TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = c.ToString();
            Animator anim = obj.GetComponent<Animator>();
            _allKeys.Add(new KeyMatch(c, txt, anim));

            spawnPos.x += KEY_SPACING;

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            Canvas can = obj.GetComponentInChildren<Canvas>();
            sr.sortingOrder = sortingOrder;
            can.sortingOrder = sortingOrder;
        }
        GameObject obj2 = Instantiate(spacePrefab, spawnPos, Quaternion.identity, transform);
        TextMeshProUGUI txt2 = null;
        Animator anim2 = obj2.GetComponent<Animator>();
        _allKeys.Add(new KeyMatch(' ', txt2, anim2));

        SpriteRenderer sr2 = obj2.GetComponent<SpriteRenderer>();
        sr2.sortingOrder = sortingOrder;

        sortingOrder++;
    }

    public void PressKey(char c)
    {
        c = char.ToUpper(c);
        for (int i = 0; i < _keyCount; i++)
        {
            if (_allKeys[i].key == c)
            {
                _allKeys[i].animator.SetTrigger("Press");
                if (keyPress != null)
                    keyPress(c);
                return;
            }
        }
    }

    public void PressKey(SpecialKey specialKey)
    {
        char c = '!';
        switch (specialKey)
        {
            case SpecialKey.Tab:
                c = '>';
                break;
            case SpecialKey.Backspace:
                c = '@';
                break;
            case SpecialKey.Space:
                c = ' ';
                break;
            case SpecialKey.Return:
                c = '#';
                break;
            default:
                break;
        }
        PressKey(c);
    }

    private struct KeyMatch
    {
        public char key;
        public TextMeshProUGUI textMesh;
        public Animator animator;

        public KeyMatch(char key, TextMeshProUGUI textMesh, Animator animator)
        {
            this.key = key;
            this.textMesh = textMesh;
            this.animator = animator;
        }
    }
}

public enum SpecialKey
{
    Tab,
    Backspace,
    Space,
    Return
}


