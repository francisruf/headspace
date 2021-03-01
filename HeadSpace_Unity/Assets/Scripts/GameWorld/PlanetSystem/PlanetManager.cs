﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetManager : MonoBehaviour
{
    // Variables publiques
    public int planetCount;
    public GameObject planetPrefab;

    // Informations de la grille de jeu actuelle
    private GridInfo _currentGridInfo;

    // Liste de toutes les planètes générées
    private List<Planet> _allPlanets = new List<Planet>();

    // Subscription à différentes actions
    private void OnEnable()
    {
        GridManager.gridDataDestroyed += OnGridDataDestroyed;
        GridManager.newGameGrid += OnNewGameGrid;
        GridManager.firstAnomalyTile += OnFirstAnomalyTile;
    }

    // Unsubscription
    private void OnDisable()
    {
        GridManager.gridDataDestroyed -= OnGridDataDestroyed;
        GridManager.newGameGrid -= OnNewGameGrid;
        GridManager.firstAnomalyTile -= OnFirstAnomalyTile;
    }

    // Fonction appelée lorsqu'une nouvelle grille est générée qui stock les informations de cette grille
    private void OnNewGameGrid(GridInfo gridInfo)
    {
        _currentGridInfo = gridInfo;
    }

    // Fonction appelée lorsque l'anomalie a été assignée, afin d'assigner par la suite les planètes
    private void OnFirstAnomalyTile()
    {
        SpawnPlanets();
    }

    // Fonction appelée lorsqu'un planète spawn, qui sert à garder à jour la liste de planètes
    private void OnNewPlanetSpawned(Planet planet)
    {
        _allPlanets.Add(planet);
    }

    // Fonction appelée lorsque la grille actuelle est détruite qui reset les variables et références
    private void OnGridDataDestroyed()
    {
        _allPlanets.Clear();
        _currentGridInfo = null;
    }

    private void SpawnPlanets()
    {
        if (_currentGridInfo == null)
            return;

        List<GridTile> allowedSpawnTiles = GetAllowedSpawnTiles();

        for (int i = 0; i < planetCount; i++)
        {
            Debug.Log("WTF");
            int randomIndex = Random.Range(0, allowedSpawnTiles.Count);
            Vector2 spawnPos = GridCoords.GetRandomCoordsInTile(allowedSpawnTiles[randomIndex]);
            Planet planet = Instantiate(planetPrefab).GetComponent<Planet>();
            planet.PlaceGridObject(spawnPos);
            _allPlanets.Add(planet);
        }
    }

    private List<GridTile> GetAllowedSpawnTiles()
    {
        List<GridTile> candidateTiles = _currentGridInfo.GetEmptyTiles();
        List<GridTile> anomalyTiles = _currentGridInfo.GetAnomalyTiles();
        List<GridTile> neighbours1 = new List<GridTile>();
        List<GridTile> neighbours2 = new List<GridTile>();
        List<GridTile> allNeighbours = new List<GridTile>();

        foreach (var tile in anomalyTiles)
        {
            foreach (var emptyNeighbour in tile.EmptyNeighbours)
            {
                neighbours1.Add(emptyNeighbour);
                allNeighbours.Add(emptyNeighbour);
            }
        }

        foreach (var tile in neighbours1)
        {
            foreach (var emptyNeighbour in tile.EmptyNeighbours)
            {
                neighbours2.Add(emptyNeighbour);
                allNeighbours.Add(emptyNeighbour);
            }
        }

        foreach (var tile in allNeighbours)
        {
            if (candidateTiles.Contains(tile))
            {
                candidateTiles.Remove(tile);
            }
        }

        return candidateTiles;
    }
}