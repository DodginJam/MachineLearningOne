using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Tile[,] Tiles
    { get; set; }

    [field: SerializeField, Min(3)]
    public int Width
    { get; private set; } = 3;

    [field: SerializeField, Min(3)]
    public int Length
    { get; private set; } = 3;

    [field: SerializeField]
    public float Spacing
    { get; private set; } = 1f;

    [field: SerializeField]
    public GameObject TilePrefab
    { get; private set; }

    public User CurrentPlayer
    { get; private set; }

    public event Action<bool> TilesInteractionaStatusEvent;

    private void Start()
    {
        // Create the tiles in a grid.
        Tiles = GenerateTiles(Width, Length, Spacing);

        // Default the tiles to not be interactable until game start.
        TilesInteractionaStatusEvent?.Invoke(false);

        // Randomly assign the current player between the user and the AI.
        SwitchPlayer(UnityEngine.Random.Range(0, 2) == 0 ? User.Player : User.AI);


        if (CurrentPlayer == User.Player)
        {
            PlayerTurnStart();
        }
        else if (CurrentPlayer == User.AI)
        {
            AITurn();
        }
    }

    public void PlayerTurnStart()
    {
        TilesInteractionaStatusEvent?.Invoke(true);
    }

    public void AITurn()
    {
        TilesInteractionaStatusEvent?.Invoke(false);

        // Loop through the valid tiles that the AI can make a play on.
        List<Tile> validTiles = new List<Tile>();

        for (int i = 0; i < Tiles.GetLength(0); i++)
        {
            for (int j = 0; j < Tiles.GetLength(1); j++)
            {
                Tile currentTile = Tiles[i, j];

                if (currentTile.TileOwner == User.None)
                {
                    validTiles.Add(currentTile);
                }
            }
        }

        int randomValidTileIndex = UnityEngine.Random.Range(0, validTiles.Count);

        // End the turn after the AI makes a move.
        validTiles[randomValidTileIndex].AIClickEvent();
    }

    private Tile[,] GenerateTiles(int width = 3, int length = 3, float spacing = 1)
    {
        Tile[,] tiles = new Tile[width, length];

        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                // Spawn the TilePrefab, positioning it along the grid spacing.
                GameObject newTile = Instantiate(TilePrefab, new Vector3(transform.position.x + (j * spacing), 0, transform.position.z + (i * spacing)), Quaternion.identity);

                // Grab the reference to the tile script component.
                if (newTile.TryGetComponent<Tile>(out Tile tile))
                {
                    tiles[j, i] = tile;
                    TilesInteractionaStatusEvent += tile.SetInteractableState;
                    tile.OnClick += EndTurn;
                }
                else
                {
                    Debug.LogError("The gameobject spawned as a new tile doesn't contain the tile script component.");
                }
            }
        }

        return tiles;
    }

    public void EndTurn()
    {
        // Check if the current player has the condition to win, if win, run win method and return, else continue.
        if (CheckWin())
        {
            Win(CurrentPlayer);
            return;
        }

        // Check if there are spaces for future moves - if not, run draw method and return, else continue
        if (NoSpacesAvailable())
        {
            Draw();
            return;
        }

        // Switch the current player to the opponent.
        SwitchPlayer();

        // Start the turn of the new current player.
        if (CurrentPlayer == User.Player)
        {
            PlayerTurnStart();
        }
        else if (CurrentPlayer == User.AI)
        {
            AITurn();
        }
    }

    public bool CheckWin()
    {
        return CheckLines(Tiles.GetLength(0), Tiles.GetLength(1), true) || CheckLines(Tiles.GetLength(0), Tiles.GetLength(1), false) || CheckDiagonal(Tiles.GetLength(0), Tiles.GetLength(1), true) || CheckDiagonal(Tiles.GetLength(0), Tiles.GetLength(1), false);
    }

    public void Win(User winner)
    {
        Debug.Log($"Winner is: {winner}");
        TilesInteractionaStatusEvent?.Invoke(false);
    }

    public void Draw()
    {
        Debug.Log("Draw Game due to no play space and no victory for either player.");
        TilesInteractionaStatusEvent?.Invoke(false);
    }

    public bool NoSpacesAvailable()
    {
        bool isNoSpaceAvailable = true;

        foreach (Tile tile in Tiles)
        {
            if (tile.TileOwner == User.None)
            {
                isNoSpaceAvailable = false;
                break;
            }
        }

        return isNoSpaceAvailable;
    }

    /// <summary>
    /// Switch the active current player to what is not currently active.
    /// </summary>
    public void SwitchPlayer()
    {
        CurrentPlayer = CurrentPlayer == User.Player ? User.AI : User.Player;
    }

    /// <summary>
    /// Switch the player to the provided player passed through.
    /// </summary>
    /// <param name="newPlayer"></param>
    public void SwitchPlayer(User newPlayer)
    {
        CurrentPlayer = newPlayer;
    }

    public bool CheckLines(int firstDimensionLength, int secondDimensionLength, bool isColumns)
    {
        User previousTileOwner = User.None;
        int winnerCount = 0;

        int i;
        int j;

        List<Tile> winningTiles = new List<Tile>();

        // Checking lines.
        for (i = 0; DimensionCheckOne(); i++)
        {
            for (j = 0; DimensionCheckTwo(); j++)
            {
                Tile currentTile = GetTileIndex(isColumns);

                // Loop over the line and check for player, AI or unowned tiles and track ownership in orders of 3 in a row to find a win.
                if (currentTile.TileOwner != User.None)
                {
                    if ((winnerCount == 0 && previousTileOwner == User.None) || winnerCount > 0 && previousTileOwner == currentTile.TileOwner)
                    {
                        winnerCount++;
                        previousTileOwner = currentTile.TileOwner;
                        winningTiles.Add(currentTile);
                    }
                    else
                    {
                        winnerCount = 0;
                        previousTileOwner = currentTile.TileOwner;
                        winningTiles.Clear();
                    }
                }
                else
                {
                    // On an unowned tile, any row being made is broken therefore reset the winner counts.
                    winnerCount = 0;
                    previousTileOwner = User.None;
                    winningTiles.Clear();
                    continue;
                }

                // If a win is found, return the method as true.
                if (winnerCount == 3)
                {
                    foreach(Tile tile in winningTiles)
                    {
                        tile.GetComponentInChildren<UnityEngine.UI.Image>().color = Color.green;
                    }

                    return true;
                }
            }

            // Resetting the count in the line if no win is found.
            Debug.Log("Line complete");
            winnerCount = 0;
            previousTileOwner = User.None;
            winningTiles.Clear();
        }

        return false;

        // In method methods.
        bool DimensionCheckOne()
        {
            return isColumns ? i < firstDimensionLength : i < secondDimensionLength;
        }

        bool DimensionCheckTwo()
        {
            return isColumns ? j < secondDimensionLength : j < firstDimensionLength;
        }

        Tile GetTileIndex(bool isColumns)
        {
            return isColumns ? Tiles[i, j] : Tiles[j, i];
        }
    }

    bool CheckDiagonal(int firstDimensionLength, int secondDimensionLength, bool startZeroZero)
    {
        return false;
    }
}

public enum User
{
    None,
    Player,
    AI
}
