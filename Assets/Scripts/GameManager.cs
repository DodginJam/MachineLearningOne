using System;
using UnityEngine;
using System.Collections.Generic;
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

        for (int i = 0; i < Tiles.GetLongLength(0); i++)
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
            Debug.Log("No Space Available.");
            Draw();
            return;
        }

        // Switch the current player to the opponent.
        Debug.Log("Future play possible, switching current player.");
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
        bool winFound = false;

        User previousTileOwner = User.None;
        int playerCount = 0;
        int aICount = 0;

        // Checking columns.
        for (int i = 0; i < Tiles.GetLength(0); i++)
        {
            for (int j = 0; j < Tiles.GetLength(1); j++)
            {
                // Loop over the columns and check for player, AI or unowned tiles and track ownership in orders of 3 in a row to find a win.
                if (Tiles[i, j].TileOwner == User.Player)
                {
                    // If the player count in the row is zero and the previous tile is unowned, then add to count.
                    // Also allow count if the player count is greater then zero and the previous tile owner is also the player.
                    if ((playerCount == 0 && previousTileOwner == User.None) || (playerCount > 0 && previousTileOwner == User.Player))
                    {
                        playerCount++;
                        previousTileOwner = User.Player;
                    }

                }
                else if (Tiles[i, j].TileOwner == User.AI)
                {
                    // If the AI count in the row is zero and the previous tile is unowned, then add to count.
                    // Also allow count if the AI count is greater then zero and the previous tile owner is also the AI.
                    if ((aICount == 0 && previousTileOwner == User.None) || (aICount > 0 && previousTileOwner == User.AI))
                    {
                        aICount++;
                        previousTileOwner = User.AI;
                    }
                }
                else if (Tiles[i, j].TileOwner == User.None)
                {
                    // On an unowned tile, any row being made is broken therefore reset the player / AI counts.
                    playerCount = 0;
                    aICount = 0;
                    previousTileOwner = User.None;
                    continue;
                }

                // Break the inner loop if a win is found.
                if (playerCount == 3)
                {
                    winFound = true;
                    break;
                }
                else if (aICount == 3)
                {
                    winFound = true;
                    break;
                }

                // Break the outer loop if win is found.
                if (winFound)
                {
                    break;
                }
            }

            Debug.Log("Column complete");
            // Resetting the count in the Column line.
            playerCount = 0;
            aICount = 0;
            previousTileOwner = User.None;
        }

        // Checking rows.
        for (int i = 0; i < Tiles.GetLength(1); i++)
        {
            for (int j = 0; j < Tiles.GetLength(0); j++)
            {
                // Loop over the rows and check for player, AI or unowned tiles and track ownership in orders of 3 in a row to find a win.
                if (Tiles[j, i].TileOwner == User.Player)
                {
                    if ((playerCount == 0 && previousTileOwner == User.None) || (playerCount > 0 && previousTileOwner == User.Player))
                    {
                        playerCount++;
                        previousTileOwner = User.Player;
                    }

                }
                else if (Tiles[j, i].TileOwner == User.AI)
                {
                    if ((aICount == 0 && previousTileOwner == User.None) || (aICount > 0 && previousTileOwner == User.AI))
                    {
                        aICount++;
                        previousTileOwner = User.AI;
                    }
                }
                else if (Tiles[j, i].TileOwner == User.None)
                {
                    playerCount = 0;
                    aICount = 0;
                    previousTileOwner = User.None;
                }

                // Break the inner loop if a win is found.
                if (playerCount == 3)
                {
                    winFound = true;
                    break;
                }
                else if (aICount == 3)
                {
                    winFound = true;
                    break;
                }

                // Break the outer loop if win is found.
                if (winFound)
                {
                    break;
                }
            }

            Debug.Log("Row complete");
            // Resetting the count in the Row line.
            playerCount = 0;
            aICount = 0;
            previousTileOwner = User.None;
        }

        return winFound;
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
        Debug.Log("No win found, checking for spaces.");

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
}

public enum User
{
    None,
    Player,
    AI
}
