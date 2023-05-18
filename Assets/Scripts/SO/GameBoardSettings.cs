using UnityEngine;

[CreateAssetMenu(fileName = "GameBoardSettings", menuName = "Settings/GameBoardSettings")]
public class GameBoardSettings : ScriptableObject
{
    public Sprite DefaultSprite;
    public Sprite BlokedSprite;

    public ArrayLayout BoardLayout;
}
