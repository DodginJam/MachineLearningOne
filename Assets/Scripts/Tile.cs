using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [field: SerializeField]
    public Button Button
    {  get; private set; }

    [field: SerializeField]
    public TextMeshProUGUI TextDisplay
    { get; private set; }

    [field: SerializeField]
    public User TileOwner
    { get; private set; } = User.None;

    public bool IsInteractable
    { get; private set; }

    public event Action OnClick;

    private void Awake()
    {
        if (TextDisplay != null)
        {
            TextDisplay.text = TilesText[User.None];
        }

        Button.onClick.AddListener(PlayerClickEvent);
    }

    public void SetInteractableState(bool newStatus)
    {
        IsInteractable = newStatus;
    }

    public void SetTileOwner(User currentPlayer)
    {
        TileOwner = currentPlayer;
        TextDisplay.text = TilesText[currentPlayer];
    }

    void PlayerClickEvent()
    {
        if (IsInteractable)
        {
            SetTileOwner(User.Player);

            OnClick?.Invoke();
        }
    }

    void AIClickEvent()
    {
        SetTileOwner(User.AI);

        OnClick?.Invoke();
    }

    static Dictionary<User, string> TilesText = new Dictionary<User, string>();

    static Tile()
    {
        TilesText.Add(User.None, string.Empty);
        TilesText.Add(User.Player, "O");
        TilesText.Add(User.AI, "X");
    }
}
