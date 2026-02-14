using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

    [field: SerializeField, Min(3)]
    public int WinningLineAmount
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

    [field: SerializeField, Min(1)]
    public int DepthLimit_AI
    { get; private set; } = 4;

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

        /*
        // Loop through the valid tiles that the AI can make a play on.
                List<Tile> validTiles = new List<Tile>();

                for (int i = 0; i < Tiles.GetLength(0); i++)
                {
                    for (int j = 0; j < Tiles.GetLength(1); j++)
                    {
                        Tile currentUser = Tiles[i, j];

                        if (currentUser.TileOwner == User.None)
                        {
                            validTiles.Add(currentUser);
                        }
                    }
                }

                int randomValidTileIndex = UnityEngine.Random.Range(0, validTiles.Count);


                // End the turn after the AI makes a move.
                validTiles[randomValidTileIndex].AIClickEvent();
        */

        // Get the best move that the AI agent can take to try and win the game.
        Debug.Log("AI thinking...");
        Vector2Int bestMove = GetBestMove();

        if (bestMove.x >= 0 && bestMove.y >= 0)
        {
            Tiles[bestMove.x, bestMove.y].AIClickEvent();
        }
        else
        {
            Debug.LogError("The local variable titled best move has an invalid coordinate.");
        }
    }

    Vector2Int GetBestMove()
    {
        // Generate a copy of the board as an 2D array of the user owners.
        User[,] boardState = GetBoardState();

        int bestScore = int.MinValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        // Loop through all the unowned tiles and test the scoring produced on it.
        for (int i = 0; i < boardState.GetLength(0); i++)
        {
            for (int j = 0; j < boardState.GetLength(1); j++)
            {
                // Check unowned tiles only.
                if (boardState[i, j] == User.None)
                {
                    // Tempoarily own the tile so that future moves can be simulated based on this board state.
                    boardState[i, j] = User.AI;
                    int score = Minimax(boardState, false, 0);

                    boardState[i, j] = User.None;

                    // If the score is the new best, assign it as the new best score and store the indices.
                    if (score > bestScore)  
                    {
                        bestScore = score;
                        bestMove = new Vector2Int (i, j);
                    }
                }
            }
        }

        return bestMove;
    }

    /// <summary>
    /// Taking copy of the board, simulate the score of playing in each space on the board to a given depth of moves using recursion.
    /// </summary>
    /// <param name="boardState"></param>
    /// <param name="isMaximizing"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    int Minimax(User[,] boardState, bool isMaximizing, int depth)
    {
        // Get a score of the state of the board for how well the simulated position is for the AI player.
        int result = EvaluateBoard(boardState);

        // If the score is found to not result in a winner, then allow the checks on further tiles.
        if (result != 0)
        {
            return result;
        }

        // If the board is full, end the tile checks.
        if (IsBoardFull(boardState))
        {
            return 0;
        }

        // Limit check for how deep in depth of simulated boards to check for sake of performance.
        if (depth >= DepthLimit_AI)
        {
            return 0;
        }

        // For checking the cost of the AI move from the AI perspective as to if it is a winning move.
        if (isMaximizing)
        {
            int bestScore = int.MinValue;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Length; y++)
                {
                    if (boardState[x, y] == User.None)
                    {
                        // Using recursion to simulate future moves deeper into a given boardstate.
                        boardState[x, y] = User.AI;

                        int score = Minimax(boardState, false, depth + 1);

                        boardState[x, y] = User.None;

                        bestScore = Mathf.Max(score, bestScore);
                    }
                }
            }

            return bestScore;
        }
        else
        // For checking the cost of the players move from the AI perspective as to if it is a losing move.
        {
            int bestScore = int.MaxValue;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Length; y++)
                {
                    if (boardState[x, y] == User.None)
                    {
                        // Using recursion to simulate future moves deeper into a given boardstate.
                        boardState[x, y] = User.Player;

                        int score = Minimax(boardState, true, depth + 1);

                        boardState[x, y] = User.None;

                        bestScore = Mathf.Min(score, bestScore);
                    }
                }
            }

            return bestScore;
        }
    }

    // Return a score based on the wehther the board represents a winning or a losing play.
    int EvaluateBoard(User[,] boardState)
    {
        User winner = GetWinner(boardState);

        if (winner == User.AI)
        {
            return 10;
        }
        else if (winner == User.Player)
        {
            return -10;
        }

        return 0;
    }

    User GetWinner(User[,] boardState)
    {
        User winner = User.None;

        if (winner == User.None)
        {
            CheckLines(true, boardState, out winner);
        }

        if (winner == User.None)
        {
            CheckLines(false, boardState, out winner);
        }

        if (winner == User.None)
        {
            CheckDiagonal(0, 0, true, boardState, out winner);
        }

        if (winner == User.None)
        {
            CheckDiagonal(0, 0, false, boardState, out winner);
        }

        return winner;
    }

    bool IsBoardFull(User[,] board)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Length; y++)
            {
                if (board[x, y] == User.None)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Return a copy of the board state as flagged by the ownership of the tiles.
    /// </summary>
    /// <returns></returns>
    User[,] GetBoardState()
    {
        User[,] boardState = new User[Tiles.GetLength(0), Tiles.GetLength(1)];

        for (int i = 0; i < boardState.GetLength(0); i++)
        {
            for (int j = 0; j < boardState.GetLength(1); j++)
            {
                boardState[i, j] = Tiles[i, j].TileOwner;
            }
        }

        return boardState;
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
                    tile.UpdateTextDisplay($"{i}, {j}");
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
        return CheckLines(Tiles.GetLength(0), Tiles.GetLength(1), true) || CheckLines(Tiles.GetLength(0), Tiles.GetLength(1), false) || CheckDiagonal(0, 0, false) || CheckDiagonal(0, 0, true);
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

    bool CheckLines(bool isColumns, User[,] boardState, out User winner)
    {
        User previousTileOwner = User.None;
        int winnerCount = 0;
        List<User> winningTiles = new List<User>();

        int i;
        int j;

        // Checking lines.
        for (i = 0; DimensionCheckOne(); i++)
        {
            for (j = 0; DimensionCheckTwo(); j++)
            {
                User currentUser = GetTileIndex(isColumns);

                // currentUser.GetComponentInChildren<UnityEngine.UI.Image>().color = i == 0 ? Color.magenta : j == 0 ? Color.gray : Color.white;

                // Loop over the line and check for player, AI or unowned tiles and track ownership in orders of 3 in a row to find a win.
                if (currentUser != User.None)
                {
                    // If the winning counter is zero, start the count. Also increment counter if the counter is greater then zero and the current tile is same owner as last tile.
                    if (winnerCount == 0 || winnerCount > 0 && previousTileOwner == currentUser)
                    {
                        winnerCount++;
                        winningTiles.Add(currentUser);
                    }
                    // If the previous tile was owned by a different owner the the current, reset the winner to one for the new owners streak.
                    else
                    {
                        winningTiles.Clear();
                        winnerCount = 1;
                        winningTiles.Add(currentUser);
                    }

                    previousTileOwner = currentUser;
                }
                else
                {
                    // On an unowned tile, any winning tile collection being made is broken therefore reset the winner counts.
                    winnerCount = 0;
                    previousTileOwner = User.None;
                    winningTiles.Clear();
                    continue;
                }

                // If a win is found, return the method as true.
                if (winnerCount == WinningLineAmount)
                {
                    winner = currentUser;
                    return true;
                }
            }

            // Resetting the winning counter if no win is found within the current line before moving on to the next.
            winnerCount = 0;
            previousTileOwner = User.None;
            winningTiles.Clear();
        }

        //  False is returned when no winner is found during the nested for loops.
        winner = User.None;
        return false;

        // In method methods.
        bool DimensionCheckOne()
        {
            return isColumns ? i < boardState.GetLength(0) : i < boardState.GetLength(1);
        }

        bool DimensionCheckTwo()
        {
            return isColumns ? j < boardState.GetLength(1) : j < boardState.GetLength(0);
        }

        User GetTileIndex(bool isColumns)
        {
            return isColumns ? boardState[i, j] : boardState[j, i];
        }
    }

    public bool CheckLines(int firstDimensionLength, int secondDimensionLength, bool isColumns)
    {
        User previousTileOwner = User.None;
        int winnerCount = 0;
        List<Tile> winningTiles = new List<Tile>();

        int i;
        int j;

        // Checking lines.
        for (i = 0; DimensionCheckOne(); i++)
        {
            for (j = 0; DimensionCheckTwo(); j++)
            {
                Tile currentTile = GetTileIndex(isColumns);

                // currentUser.GetComponentInChildren<UnityEngine.UI.Image>().color = i == 0 ? Color.magenta : j == 0 ? Color.gray : Color.white;

                // Loop over the line and check for player, AI or unowned tiles and track ownership in orders of 3 in a row to find a win.
                if (currentTile.TileOwner != User.None)
                {
                    // If the winning counter is zero, start the count. Also increment counter if the counter is greater then zero and the current tile is same owner as last tile.
                    if (winnerCount == 0 || winnerCount > 0 && previousTileOwner == currentTile.TileOwner)
                    {
                        winnerCount++;
                        winningTiles.Add(currentTile);
                    }
                    // If the previous tile was owned by a different owner the the current, reset the winner to one for the new owners streak.
                    else
                    {
                        winningTiles.Clear();
                        winnerCount = 1;
                        winningTiles.Add(currentTile);
                    }

                    previousTileOwner = currentTile.TileOwner;
                }
                else
                {
                    // On an unowned tile, any winning tile collection being made is broken therefore reset the winner counts.
                    winnerCount = 0;
                    previousTileOwner = User.None;
                    winningTiles.Clear();
                    continue;
                }

                // If a win is found, return the method as true.
                if (winnerCount == WinningLineAmount)
                {
                    foreach (Tile tile in winningTiles)
                    {
                        tile.GetComponentInChildren<UnityEngine.UI.Image>().color = Color.green;
                    }

                    return true;
                }
            }

            // Resetting the winning counter if no win is found within the current line before moving on to the next.
            winnerCount = 0;
            previousTileOwner = User.None;
            winningTiles.Clear();
        }

        //  False is returned when no winner is found during the nested for loops.
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

    bool CheckDiagonal(int startPointIndexOne, int startPointIndexTwo, bool fliped, User[,] boardState, out User winner)
    {
        List<User> winningTiles = new List<User>();

        for (int sum = 0; sum < Width + Length - 1; sum++)
        {
            User previousTileOwner = User.None;
            int winnerCount = 0;
            winningTiles.Clear();

            for (int x = 0; x < Width; x++)
            {
                int y = sum - x;

                if (y >= 0 && y < Length)
                {
                    int newX = x;

                    if (fliped)
                    {
                        newX = (Width - 1) - x;
                    }

                    User currentUser = boardState[newX, y];

                    // currentUser.GetComponentInChildren<UnityEngine.UI.Image>().color = i == 0 ? Color.magenta : j == 0 ? Color.gray : Color.white;

                    // Loop over the line and check for player, AI or unowned tiles and track ownership in orders of 3 in a row to find a win.
                    if (currentUser != User.None)
                    {
                        // If the winning counter is zero, start the count. Also increment counter if the counter is greater then zero and the current tile is same owner as last tile.
                        if (winnerCount == 0 || winnerCount > 0 && previousTileOwner == currentUser)
                        {
                            winnerCount++;
                            winningTiles.Add(currentUser);
                        }
                        // If the previous tile was owned by a different owner the the current, reset the winner to one for the new owners streak.
                        else
                        {
                            winningTiles.Clear();
                            winnerCount = 1;
                            winningTiles.Add(currentUser);
                        }

                        previousTileOwner = currentUser;
                    }
                    else
                    {
                        // On an unowned tile, any winning tile collection being made is broken therefore reset the winner counts.
                        winnerCount = 0;
                        previousTileOwner = User.None;
                        winningTiles.Clear();
                        continue;
                    }

                    // If a win is found, return the method as true.
                    if (winnerCount == WinningLineAmount)
                    {
                        winner = currentUser;
                        return true;
                    }

                }
            }
        }

        winner = User.None;
        return false;
    }

    bool CheckDiagonal(int startPointIndexOne, int startPointIndexTwo, bool fliped)
    {
        List<Tile> winningTiles = new List<Tile>();

        for (int sum = 0; sum < Width + Length - 1; sum++)
        {
            User previousTileOwner = User.None;
            int winnerCount = 0;
            winningTiles.Clear();

            for (int x = 0; x < Width; x++)
            {
                int y = sum - x;

                if (y >= 0 && y < Length)
                {
                    int newX = x;

                    if (fliped)
                    {
                        newX = (Width - 1) - x;
                    }

                    Tile currentTile = Tiles[newX, y];

                    // currentUser.GetComponentInChildren<UnityEngine.UI.Image>().color = i == 0 ? Color.magenta : j == 0 ? Color.gray : Color.white;

                    // Loop over the line and check for player, AI or unowned tiles and track ownership in orders of 3 in a row to find a win.
                    if (currentTile.TileOwner != User.None)
                    {
                        // If the winning counter is zero, start the count. Also increment counter if the counter is greater then zero and the current tile is same owner as last tile.
                        if (winnerCount == 0 || winnerCount > 0 && previousTileOwner == currentTile.TileOwner)
                        {
                            winnerCount++;
                            winningTiles.Add(currentTile);
                        }
                        // If the previous tile was owned by a different owner the the current, reset the winner to one for the new owners streak.
                        else
                        {
                            winningTiles.Clear();
                            winnerCount = 1;
                            winningTiles.Add(currentTile);
                        }

                        previousTileOwner = currentTile.TileOwner;
                    }
                    else
                    {
                        // On an unowned tile, any winning tile collection being made is broken therefore reset the winner counts.
                        winnerCount = 0;
                        previousTileOwner = User.None;
                        winningTiles.Clear();
                        continue;
                    }

                    // If a win is found, return the method as true.
                    if (winnerCount == WinningLineAmount)
                    {
                        foreach (Tile tile in winningTiles)
                        {
                            tile.GetComponentInChildren<UnityEngine.UI.Image>().color = Color.green;
                        }

                        return true;
                    }

                }
            }
        }

        return false;
    }
}

public enum User
{
    None,
    Player,
    AI
}
