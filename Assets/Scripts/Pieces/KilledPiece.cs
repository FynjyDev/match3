using UnityEngine;
using UnityEngine.UI;

public class KilledPiece : MonoBehaviour
{
    public bool Falling;

    private float speed = 16f;
    private float gravity = 32f;

    private Vector2 moveDir;

    private RectTransform rect;
    private Image img;

    public void Initialize(Sprite _pieceSource, Vector2 _start)
    {
        Falling = true;

        moveDir = Vector2.up;
        moveDir.x = Random.Range(-1.0f, 1.0f);
        moveDir *= speed / 2;

        img = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        img.sprite = _pieceSource;
        rect.anchoredPosition = _start;
    }

    void Update()
    {
        if (!Falling) return;
        moveDir.y -= Time.deltaTime * gravity;
        moveDir.x = Mathf.Lerp(moveDir.x, 0, Time.deltaTime);
        rect.anchoredPosition += moveDir * Time.deltaTime * speed;
        if (rect.position.x < -64f || rect.position.x > Screen.width + 64f || rect.position.y < -64f || rect.position.y > Screen.height + 64f)
            Falling = false;
    }
}