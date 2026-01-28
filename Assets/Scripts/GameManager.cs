using System;
using UnityEngine;
using UnityEngine.UI;

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
                    tiles[i, j] = tile;
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
        Debug.Log("End of turn, checking win condition.");

        // Check if there are spaces for future moves - if not, run draw method and return, else continue
        Debug.Log("No win found, checking for spaces.");

        // Switch the current player to the opponent.
        Debug.Log("Future play possible, switching current player.");

        SwitchPlayer();
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

    }
}

public enum User
{
    None,
    Player,
    AI
}
