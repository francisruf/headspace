/* Classe qui sert principalement à resize la worldMap (la texture d'arrière-plan) en fonction de la grille 
 * qui est générée.
 * 
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicWorldMap : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private GridInfo _currentGridInfo;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        GridManager.newGameGrid += InitializeWorldMap;
    }

    private void OnDisable()
    {
        GridManager.newGameGrid -= InitializeWorldMap;
    }

    private void InitializeWorldMap(GridInfo gridInfo)
    {
        _currentGridInfo = gridInfo;
        _spriteRenderer.size = _currentGridInfo.gameGridWorldBounds.size;
    }
}
