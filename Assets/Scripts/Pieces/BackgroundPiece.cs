using UnityEngine;
using UnityEngine.UI;

public class BackgroundPiece : MonoBehaviour
{
    public Point index;
    public Image image;

    public void Initialize( Sprite _pieceSource)
    {
        image.sprite = _pieceSource;
    }
}
