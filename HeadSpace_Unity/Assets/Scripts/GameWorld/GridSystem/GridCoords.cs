using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCoords : MonoBehaviour
{
    // Static Variables
    private static GridInfo _currentGridInfo;  // Stock les informations de la grille actuelle

    public Vector2 TestCoords;
    public Vector2 TestWorldCoords;

    // Subscription à l'action newGameGrid
    private void OnEnable()
    {
        GridManager.newGameGrid += AssignGridInfo;
    }

    // Unsubscription
    private void OnDisable()
    {
        GridManager.newGameGrid -= AssignGridInfo;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            bool valid;
            Debug.Log("Grid coords : " + TestCoords);
            Debug.Log("World coords : " + FromGridToWorld(TestCoords, out valid));

            Debug.DrawLine(FromGridToWorld(TestCoords), FromGridToWorld(TestCoords) + Vector2.up, Color.red, 5f);

            if (!valid)
            {
                Debug.Log("COORDINATES ARE OUTSIDE GRID");
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            bool valid;
            Debug.Log("World coords : " + TestWorldCoords);
            Debug.Log("Grid coords : " + FromWorldToGrid(TestWorldCoords, out valid));

            Debug.DrawLine(TestWorldCoords, TestWorldCoords + Vector2.up, Color.cyan, 5f);

            if (!valid)
            {
                Debug.Log("COORDINATES ARE OUTSIDE GRID");
            }
        }
    }

    // Assigne les valeurs de _currentGridInfo (appel à chaque nouvelle grille)
    private void AssignGridInfo(GridInfo info)
    {
        _currentGridInfo = info;
    }

    // Convertir une position en WorldCoords vers GridCoords
    // Fonction STATIC = accessible sans référence d'objet
    public static Vector2 FromWorldToGrid(Vector2 worldCoords)
    {
        float tileSize = _currentGridInfo.gameGridWorldBounds.size.x / _currentGridInfo.gameGridSize.x;
        Vector2 gridZero = _currentGridInfo.gameGridWorldBounds.min;
        Vector2 distFromZero = worldCoords - gridZero;
        Vector2 gridCoords = distFromZero / tileSize;

        return RoundCoords(gridCoords);
    }

    // OVERLOAD : Permet de valider si les coordonnées fournies se trouvent dans la grille ou non
    // ** Un overload est une fonction du même nom, mais qui prend des paramètres différents et peut avoir sa logique unique
    public static Vector2 FromWorldToGrid(Vector2 worldCoords, out bool validCoords)
    {
        Vector2 gridCoords = FromWorldToGrid(worldCoords);
        validCoords = true;

        if (gridCoords.x < -0.001f)
            validCoords = false;

        if (gridCoords.x > _currentGridInfo.gameGridSize.x)
            validCoords = false;

        if (gridCoords.y < -0.001f)
            validCoords = false;

        if (gridCoords.y > _currentGridInfo.gameGridSize.y)
            validCoords = false;

        return gridCoords;
    }

    // Convertir une position en GridCoords vers des WorldCoords (unités unity)
    // Fonction STATIC = accessible sans référence d'objet
    public static Vector2 FromGridToWorld(Vector2 gridCoords)
    {
        Vector2 roundedCoords = RoundCoords(gridCoords);

        float tileSize = _currentGridInfo.gameGridWorldBounds.size.x / _currentGridInfo.gameGridSize.x;
        Vector2 gridZero = _currentGridInfo.gameGridWorldBounds.min;
        Vector2 worldCoords = gridZero + (tileSize * roundedCoords);

        return worldCoords;
    }

    // OVERLOAD : Permet de VALIDER si les coordonnées fournies se trouvent dans la grille ou non
    // ** Un overload est une fonction du même nom, mais qui prend des paramètres différents et peut avoir sa logique unique
    public static Vector2 FromGridToWorld(Vector2 gridCoords, out bool validCoords)
    {
        Vector2 worldCoords = FromGridToWorld(gridCoords);
        validCoords = true;

        if (gridCoords.x < -0.001f)
            validCoords = false;

        if (gridCoords.x > _currentGridInfo.gameGridSize.x)
            validCoords = false;

        if (gridCoords.y < -0.001f)
            validCoords = false;

        if (gridCoords.y > _currentGridInfo.gameGridSize.y)
            validCoords = false;

        return worldCoords;
    }

    // Convertir des coordonnées de grille en coordonnées de la TUILE qui contient ces coordonnées
    public static TileCoordinates FromGridToTile(Vector2 gridCoords)
    {
        int tileX = Mathf.FloorToInt(gridCoords.x);
        int tileY = Mathf.FloorToInt(gridCoords.y);

        return new TileCoordinates(tileX, tileY);
    }

    // Convertir des coordonnées de grille en coordonnées de la TUILE qui contient ces coordonnées
    public static TileCoordinates FromGridToTile(Vector2 gridCoords, out bool validCoords)
    {
        validCoords = true;

        if (gridCoords.x < -0.001f)
            validCoords = false;

        if (gridCoords.x > _currentGridInfo.gameGridSize.x)
            validCoords = false;

        if (gridCoords.y < -0.001f)
            validCoords = false;

        if (gridCoords.y > _currentGridInfo.gameGridSize.y)
            validCoords = false;

        return FromGridToTile(gridCoords);
    }

    public static Vector2 RoundCoords(Vector2 coords)
    {
        Vector2 roundedCoords = coords;
        roundedCoords.x = Mathf.Round(roundedCoords.x * 10.0f) * 0.1f;
        roundedCoords.y = Mathf.Round(roundedCoords.y * 10.0f) * 0.1f;
        return roundedCoords;
    }
}
