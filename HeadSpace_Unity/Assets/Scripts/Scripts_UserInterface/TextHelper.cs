using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextHelper : MonoBehaviour
{
    // Singleton
    public static TextHelper instance;

    [Header("Settings")]
    public int charactersPerSec;

    private void Awake()
    {
        // Déclaration du singleton
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        charactersPerSec = Mathf.Clamp(charactersPerSec, 1, int.MaxValue);
    }

    public void TypeAnimation(TextMeshProUGUI textMesh)
    {
        StartCoroutine(TypeAnimationRoutine(textMesh));
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
            yield return new WaitForSeconds(1f / charactersPerSec);
        }
    }
}
