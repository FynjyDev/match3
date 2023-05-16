using UnityEngine;

public class MovePieces : MonoBehaviour
{
    public static MovePieces instance;
    public GameBoard GameBoard;

    private NodePiece moving;
    private Point newIndex;
    private Vector2 mouseStart;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (moving == null) return;

        Vector2 _direction = ((Vector2)Input.mousePosition - mouseStart);
        Vector2 _newDir = _direction.normalized;
        Vector2 _targetDir = new Vector2(Mathf.Abs(_direction.x), Mathf.Abs(_direction.y));

        newIndex = Point.Clone(moving.index);
        Point _add = Point.zero;

        if (_direction.magnitude > 32)
        {
            if (_targetDir.x > _targetDir.y)
                _add = (new Point((_newDir.x > 0) ? 1 : -1, 0));
            else if (_targetDir.y > _targetDir.x)
                _add = (new Point(0, (_newDir.y > 0) ? -1 : 1));
        }
        newIndex.Add(_add);

        Vector2 _pos = GameBoard.GetPositionFromPoint(moving.index);

        if (!newIndex.Equals(moving.index))
            _pos += Point.Mult(new Point(_add.x, -_add.y), 16).ToVector();

        moving.MovePositionTo(_pos);
    }

    public void MovePiece(NodePiece _piece)
    {
        if (moving != null) return;
        moving = _piece;
        mouseStart = Input.mousePosition;
    }

    public void DropPiece()
    {
        if (moving == null) return;

        if (!newIndex.Equals(moving.index))
            GameBoard.FlipPieces(moving.index, newIndex, true);
        else
            GameBoard.ResetPiece(moving);
        moving = null;
    }
}