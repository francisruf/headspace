using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCoords : MonoBehaviour
{
    // Static Variables
    private static GridInfo _currentGridInfo;  // Stock les informations de la grille actuelle
    public static GridInfo CurrentGridInfo { get { return _currentGridInfo; } }

    // Subscription à l'action newGameGrid
    private void OnEnable()
    {
        //GridManager.newGameGrid += AssignGridInfo;
    }

    // Unsubscription
    private void OnDisable()
    {
        //GridManager.newGameGrid -= AssignGridInfo;
    }

    // Assigne les valeurs de _currentGridInfo (appel à chaque nouvelle grille)
    public void AssignGridInfo(GridInfo info)
    {
        _currentGridInfo = info;
    }

    // Convertir une position en WorldCoords vers GridCoords
    // Fonction STATIC = accessible sans référence d'objet
    public static Vector2 FromWorldToGrid(Vector2 worldCoords)
    {
        // Si la grille n'est pas instanciée, redonne les mêmes coordonnées que celles fournies.
        if (_currentGridInfo == null)
            return worldCoords;

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
        // Si la grille n'est pas instanciée, redonne les mêmes coordonnées que celles fournies.
        if (_currentGridInfo == null)
        {
            validCoords = false;
            return worldCoords;
        }

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
        // Si la grille n'est pas instanciée, redonne les mêmes coordonnées que celles fournies.
        if (_currentGridInfo == null)
        {
            return gridCoords;
        }

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
        // Si la grille n'est pas instanciée, redonne les mêmes coordonnées que celles fournies.
        if (_currentGridInfo == null)
        {
            validCoords = false;
            return gridCoords;
        }

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
        // Si la grille n'est pas instanciée, redonne les mêmes coordonnées que celles fournies.
        if (_currentGridInfo == null)
        {
            validCoords = false;
            return new TileCoordinates(0, 0);
        }

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

    // Convertir des coordonnées de grille en coordonnées de la TUILE qui contient ces coordonnées
    public static TileCoordinates FromWorldToTile(Vector2 worldCoords)
    {
        // Si la grille n'est pas instanciée, redonne les mêmes coordonnées que celles fournies.
        if (_currentGridInfo == null)
        {
            return new TileCoordinates(0, 0);
        }

        Vector2 gridCoords = FromWorldToGrid(worldCoords);

        int tileX = Mathf.FloorToInt(gridCoords.x);
        int tileY = Mathf.FloorToInt(gridCoords.y);

        tileX = Mathf.Clamp(tileX, 0, _currentGridInfo.gameGridTiles.GetLength(0) - 1);
        tileY = Mathf.Clamp(tileY, 0, _currentGridInfo.gameGridTiles.GetLength(1) - 1);

        return new TileCoordinates(tileX, tileY);
    }

    // Fonction qui assigne des coordonnées de grille et qui SNAP l'objet au bon endroit du même coup.
    public static Vector2 SnapObjectToGrid(GameObject obj)
    {
        bool validCoords = false;
        Vector2 worldCoords = obj.transform.position;
        Vector2 gridCoords = FromWorldToGrid(worldCoords, out validCoords);

        if (validCoords)
        {
            obj.transform.position = FromGridToWorld(gridCoords);
            return gridCoords;
        }
        else
        {
            return new Vector2(0f, 0f);
        }
    }

    public static Vector2 RoundCoords(Vector2 coords)
    {
        Vector2 roundedCoords = coords;
        roundedCoords.x = Mathf.Round(roundedCoords.x * 10.0f) * 0.1f;
        roundedCoords.y = Mathf.Round(roundedCoords.y * 10.0f) * 0.1f;
        return roundedCoords;
    }

    // Fonction qui retourne une position aléatoire à l'intérieur d'une tuile envoyée en appel
    public static Vector2 GetRandomCoordsInTile(GridTile targetTile)
    {
        if (_currentGridInfo == null)
            return Vector2.zero;

        float maxX = targetTile.transform.position.x + targetTile.tileDimensions.x;
        float minX = targetTile.transform.position.x;
        float minY = targetTile.transform.position.y;
        float maxY = targetTile.transform.position.y + targetTile.tileDimensions.y;

        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        Vector2 gridCoords = FromWorldToGrid(new Vector2(randomX, randomY));

        return gridCoords;
    }

    public static Vector2 RoundCoords(Vector2 coords, out bool validCoords)
    {
        validCoords = true;

        if (_currentGridInfo == null)
        {
            Debug.LogError("GridCoords error : No current grid could be found");
            validCoords = false;
            return Vector2.zero;
        }

        if (coords.x < -0.001f)
            validCoords = false;

        if (coords.x > _currentGridInfo.gameGridSize.x)
            validCoords = false;

        if (coords.y < -0.001f)
            validCoords = false;

        if (coords.y > _currentGridInfo.gameGridSize.y)
            validCoords = false;

        Vector2 roundedCoords = coords;
        roundedCoords.x = Mathf.Round(roundedCoords.x * 10.0f) * 0.1f;
        roundedCoords.y = Mathf.Round(roundedCoords.y * 10.0f) * 0.1f;
        return roundedCoords;
    }

    public static string GetTileName(TileCoordinates tileCoords)
    {
        char c = 'A';
        int count = 0;

        while (count < tileCoords.tileY)
        {
            c++;
            count++;
        }

        string y = c.ToString();
        string x = (tileCoords.tileX + 1).ToString();
        return y + x;
    }

    public static string GetTileLetter(float tileYCoord)
    {
        char c = 'A';
        int count = 0;

        while (count < tileYCoord)
        {
            c++;
            count++;
        }

        return c.ToString();
    }

    public static Vector2 FromTileNameToWorld(string tileName)
    {
        TileCoordinates tilePosition = FromTileNameToTilePosition(tileName);
        return FromTilePositionToWorld(tilePosition);
    }

    public static TileCoordinates FromTileNameToTilePosition(string tileName)
    {
        int lenght = tileName.Length;
        if (lenght < 2)
        {
            Debug.Log("INVALID TILE NAME");
            return new TileCoordinates(0, 0);
        }

        int tileX = 0;
        if (lenght == 2)
        {
            tileX = int.Parse(tileName[1].ToString()) - 1;
        }
        else if (lenght > 2)
        {
            tileX = int.Parse(tileName.Substring(1, lenght-1)) - 1;
        }

        char yChar = tileName[0];
        int tileY = -((int)'A' - (int)yChar);
        TileCoordinates tilePosition = new TileCoordinates(tileX, tileY);

        return tilePosition;
    }

    public static Vector2 FromTilePositionToWorld(TileCoordinates tilePosition)
    {
        if (_currentGridInfo == null)
        {
            Debug.Log("GridCoords error : No current grid could be found");
            return Vector2.zero;
        }

        if (tilePosition.tileX >= _currentGridInfo.gameGridSize.x || tilePosition.tileX < 0)
        {
            Debug.Log("GridCoords error : TileCoords outside of range");
            return Vector2.zero;
        }

        if (tilePosition.tileY >= _currentGridInfo.gameGridSize.y || tilePosition.tileY < 0)
        {
            Debug.Log("GridCoords error : TileCoords outside of range");
            return Vector2.zero;
        }

        GridTile targetTile = _currentGridInfo.gameGridTiles[tilePosition.tileX, tilePosition.tileY];
        float tileSize = _currentGridInfo.gameGridWorldBounds.size.x / _currentGridInfo.gameGridSize.x;
        Vector2 worldPos = targetTile.transform.position;
        worldPos.x += tileSize / 2f;
        worldPos.y -= tileSize / 2f;

        return worldPos;
    }

    public static TileCoordinates FromWorldToTilePosition(Vector2 worldCoords)
    {

        float tileSize = _currentGridInfo.gameGridWorldBounds.size.x / _currentGridInfo.gameGridSize.x;
        Vector2 gridZero = _currentGridInfo.gameGridWorldBounds.min;
        gridZero.y = _currentGridInfo.gameGridWorldBounds.max.y;

        Vector2 distFromZero = worldCoords - gridZero;
        Vector2 decimalCoords = distFromZero / tileSize;
        decimalCoords.y = -decimalCoords.y;

        int tileX = Mathf.FloorToInt(decimalCoords.x);
        int tileY = Mathf.FloorToInt(decimalCoords.y);

        return new TileCoordinates(tileX, tileY);
    }

    public static bool IsInTile(Vector2 worldPos, out TileCoordinates tileCoords)
    {
        tileCoords = new TileCoordinates(0, 0);

        if (_currentGridInfo == null)
        {
            return false;
        }

        bool inTile = _currentGridInfo.gameGridWorldBounds.Contains(worldPos);
        if (inTile)
        {
            tileCoords = FromWorldToTilePosition(worldPos);
        }

        return inTile;
    }
}
