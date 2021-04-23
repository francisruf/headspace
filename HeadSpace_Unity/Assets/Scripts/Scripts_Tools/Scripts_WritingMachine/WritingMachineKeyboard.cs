using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WritingMachineKeyboard : MonoBehaviour
{
    public Transform keyboardStartPoint;
    public GameObject keyPrefab;
    public GameObject backSpacePrefab;
    public GameObject returnPrefab;
    public GameObject spacePrefab;

    private const float KEY_SPACING = 0.5f;
    private const float LINE_SPACING = 0.5f;
    private const float LINE_INDENT = 0.25f;

    private List<KeyMatch> _allKeys = new List<KeyMatch>();
    private char[] _qKeys = { 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p' };
    private char[] _aKeys = { 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l' };
    private char[] _zKeys = { 'z', 'x', 'c', 'v', 'b', 'n', 'm'};

    private void Awake()
    {
        Spawn1Row();
        SpawnQRow();
        SpawnARow();
        SpawnZRow();
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
        }
        GameObject obj2 = Instantiate(keyPrefab, spawnPos, Quaternion.identity, transform);
        TextMeshProUGUI txt2 = obj2.GetComponentInChildren<TextMeshProUGUI>();
        txt2.text = '0'.ToString();
        Animator anim2 = obj2.GetComponent<Animator>();
        _allKeys.Add(new KeyMatch('0', txt2, anim2));

        spawnPos.x += KEY_SPACING;

        GameObject obj3 = Instantiate(backSpacePrefab, spawnPos, Quaternion.identity, transform);
        TextMeshProUGUI txt3 = null;
        Animator anim3 = obj2.GetComponent<Animator>();
        _allKeys.Add(new KeyMatch('-', txt3, anim3));
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
        }
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
        }
        GameObject obj2 = Instantiate(returnPrefab, spawnPos, Quaternion.identity, transform);
        TextMeshProUGUI txt2 = null;
        Animator anim2 = obj2.GetComponent<Animator>();
        _allKeys.Add(new KeyMatch('+', txt2, anim2));
    }

    private void SpawnZRow()
    {
        Vector2 spawnPos = keyboardStartPoint.position;
        spawnPos.y -= LINE_SPACING * 3;
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
        }
        GameObject obj2 = Instantiate(spacePrefab, spawnPos, Quaternion.identity, transform);
        TextMeshProUGUI txt2 = null;
        Animator anim2 = obj2.GetComponent<Animator>();
        _allKeys.Add(new KeyMatch(' ', txt2, anim2));
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


