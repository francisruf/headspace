using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* CLASSE QUI N'HÉRITE PAS DE MONOBEHAVIOUR : 
 * - Simplement une classe utilitaire qui sert à passer de l'information entre les scripts.
 * - Cette classe contient toutes les informations de la grille actuelle
 * --- Grosseur, Tuiles, Limites (bounds) et Cadrans
 */

public class GridInfo
{
    // Variables
    public GridTile[,] gameGridTiles;
    public Vector2 gameGridSize;
    public int tileCount;
    public Bounds gameGridWorldBounds;
    public GridQuadrants gameGridQuadrants;
    public int positiveQuadrantIndex;

    // CONSTRUCTEUR : Fonction qui sert à déclarer un nouvel objet de type GridInfo (avec new GridInfo(...))
    // - Et d'assigner les valeurs des variables, à l'aide des paramètres en appel (dans les parenthèses)
    public GridInfo(GridTile[,] gameGridTiles, Vector2 gameGridSize, Bounds gameGridWorldBounds)
    {
        this.gameGridTiles = gameGridTiles;
        this.gameGridSize = gameGridSize;
        this.tileCount = (int)gameGridSize.x * (int)gameGridSize.y;
        this.gameGridWorldBounds = gameGridWorldBounds;
        this.gameGridQuadrants = new GridQuadrants(gameGridWorldBounds);
        Debug.Log("New grid with " + tileCount + " tiles");
    }

    public List<GridTile> GetEmptyTiles()
    {
        List<GridTile> emptyTiles = new List<GridTile>();
        foreach (var tile in gameGridTiles)
        {
            if (tile.tileType == 0)
            {
                emptyTiles.Add(tile);
            }
        }
        Debug.Log("Empty tile count : " + emptyTiles.Count);

        return emptyTiles;
    }

    public List<GridTile> GetAnomalyTiles()
    {
        List<GridTile> anomalyTiles = new List<GridTile>();
        foreach (var tile in gameGridTiles)
        {
            if (tile.tileType > 0)
            {
                anomalyTiles.Add(tile);
            }
        }
        Debug.Log("Anomaly tile count : " + anomalyTiles.Count);

        return anomalyTiles;
    }
}

/* STRUCT : 
 * - Un struct est une STRUCTURE de données semblable à une classe.
 * - Ce struct permet de mémoriser plusieurs variables dans un même contenant.
 * -- Dans ce cas, un ensemble prédéfini de BOUNDS qui représentent les quadrants de la carte.
 */
public class GridQuadrants
{
    // Déclaration des Bounds à assigner lors de la création d'un struct GridQuadrants
    public Bounds TopLeft;
    public Bounds TopCenter;
    public Bounds TopRight;
    public Bounds BottomLeft;
    public Bounds BottomCenter;
    public Bounds BottomRight;

    // Array qui contient tous les cadrans
    private Bounds[] _allQuadrants;

    // CONSTRUCTEUR de gridquadrants (pour instantier avec new())
    public GridQuadrants(Bounds gridBounds)
    {
        // Les quadrants correspondent à la carte divisée en 6 (3 segments X, 2 segments Y)
        float xThird = gridBounds.size.x / 3f;
        float yHalf = gridBounds.size.y / 2f;

        // Aller chercher le point (0.0) de la grille et assiger le centre (offset) et dimensions (quadrantsize) des bounds
        Vector3 originPos = gridBounds.min;
        Vector3 offset = originPos + new Vector3(xThird / 2f, yHalf / 2f, 0f);
        Vector3 quadrantSize = new Vector3(xThird, yHalf, gridBounds.size.z);

        // Assigner les dimensions et le bon centre pour chaque quandrant
        TopLeft = new Bounds(offset + new Vector3(0f, yHalf, 0f), quadrantSize);
        TopCenter = new Bounds(offset + new Vector3(xThird, yHalf, 0f), quadrantSize);
        TopRight = new Bounds(offset + new Vector3(xThird * 2f, yHalf, 0f), quadrantSize);
        BottomLeft = new Bounds(offset, quadrantSize);
        BottomCenter = new Bounds(offset + new Vector3(xThird, 0f, 0f), quadrantSize);
        BottomRight = new Bounds(offset + new Vector3(xThird * 2f, 0f, 0f), quadrantSize);

        // Ajout des cadrans à la liste
        _allQuadrants = new Bounds[] { TopLeft, TopCenter, TopRight, BottomLeft, BottomCenter, BottomRight };
    }

    public List<Bounds> GetOtherBounds(int positiveQuadrantIndex)
    {
        List<Bounds> otherBounds = new List<Bounds>();

        for (int i = 0; i < _allQuadrants.Length; i++)
        {
            if (i != positiveQuadrantIndex)
            {
                otherBounds.Add(_allQuadrants[i]);
            }
        }
        return otherBounds;
    }

    public QuadrantMatch GetRandomQuadrantMatch(out int quadrantIndex)
    {
        int randomPositiveIndex = 0;
        int count = _allQuadrants.Length;
        int superRandomPositiveDice = Random.Range(0, 10);
        int superRandomNegativeDice = Random.Range(0, 10);

        Bounds positiveQuadrant;
        Bounds negativeQuadrant;

        if (superRandomPositiveDice < 5)
            randomPositiveIndex = Random.Range(0, count / 2);
        else
            randomPositiveIndex = Random.Range((count / 2) + 1, count);

        positiveQuadrant = _allQuadrants[randomPositiveIndex];
        quadrantIndex = randomPositiveIndex;

        switch (randomPositiveIndex)
        {
            // SI le cadran est TopLeft OU BottomLeft
            case 0:
            case 3:
                if (superRandomNegativeDice < 5)
                    negativeQuadrant = TopRight;
                else
                    negativeQuadrant = BottomRight;
                break;

            // SI le cadran est TopRight OU BottomRight
            case 2:
            case 5:
                if (superRandomNegativeDice < 5)
                    negativeQuadrant = TopLeft;
                else
                    negativeQuadrant = BottomLeft;
                break;

            // SI le cadran est TopCenter
            case 1:
                if (superRandomNegativeDice < 5)
                    negativeQuadrant = BottomLeft;
                else
                    negativeQuadrant = BottomRight;
                break;

            // SI le cadran est BottomCenter
            case 4:
                if (superRandomNegativeDice < 5)
                    negativeQuadrant = TopLeft;
                else
                    negativeQuadrant = TopRight;
                break;

            // Default case (ne sera pas utilisé)
            default:
                Debug.Log("DEFAULT");
                negativeQuadrant = BottomLeft;
                break;
        }
        //Debug.Log("Positive : " + positiveQuadrant);
        //Debug.Log("Negative : " + negativeQuadrant);
        return new QuadrantMatch(positiveQuadrant, negativeQuadrant);
    }

    public struct QuadrantMatch
    {
        public Bounds positiveQuadrant;
        public Bounds negativeQuadrant;

        public QuadrantMatch(Bounds positiveQuadrant, Bounds negativeQuadrant)
        {
            this.positiveQuadrant = positiveQuadrant;
            this.negativeQuadrant = negativeQuadrant;
        }
    }
}

