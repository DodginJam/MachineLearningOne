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

    [field: SerializeField]
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
        int currentRow = 0;
        int rowIndex = 0;

        int currentColumn = 0;
        int columnIndex = 0;

        bool searchComplete = false;
        bool winFound = false;

        User winnerOwner = User.None;

        // Checking the rows for a complete seqeunce of ownership of tiles.
        while (currentRow < Tiles.GetLength(0) && searchComplete == false)
        {
            Debug.Log($"Row: {currentRow}, Index: {rowIndex}");
            rowIndex++;

            if (rowIndex >= Tiles.GetLength(1))
            {
                currentRow++;
                rowIndex = 0;
            }
        }

        while (currentColumn < Tiles.GetLength(1) && searchComplete == false)
        {
            Debug.Log($"Column: {currentColumn}, Index: {columnIndex}");
            columnIndex++;

            if (columnIndex >= Tiles.GetLength(0))
            {
                currentColumn++;
                columnIndex = 0;
            }
        }

        Debug.Log("End of turn, checking win condition.");
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
