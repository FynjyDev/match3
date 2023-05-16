using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public ArrayLayout BoardLayout;

    [Header("UI Elements")]
    public Sprite[] Pieces;
    public RectTransform GameBoardRect;
    public RectTransform KilledBoardRect;

    [Header("Prefabs")]
    public GameObject nodePiece;
    public GameObject killedPiece;

    private int width = 9;
    private int height = 14;
    private int[] fills;
    private Node[,] board;

    private List<NodePiece> update;
    private List<FlippedPieces> flipped;
    private List<NodePiece> dead;
    private List<KilledPiece> killed;

    private System.Random random;

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        List<NodePiece> _finishedUpdating = new List<NodePiece>();

        for (int i = 0; i < update.Count; i++)
        {
            NodePiece _piece = update[i];
            if (!_piece.UpdatePiece()) _finishedUpdating.Add(_piece);
        }

        for (int i = 0; i < _finishedUpdating.Count; i++)
        {
            NodePiece _piece = _finishedUpdating[i];
            FlippedPieces _flip = GetFlipped(_piece);
            NodePiece _flippedPiece = null;

            int _x = _piece.index.x;
            fills[_x] = Mathf.Clamp(fills[_x] - 1, 0, width);

            List<Point> _connected = IsConnected(_piece.index, true);
            bool _wasFlipped = (_flip != null);

            if (_wasFlipped) //If we flipped to make this update
            {
                _flippedPiece = _flip.GetOtherPiece(_piece);
                AddPoints(ref _connected, IsConnected(_flippedPiece.index, true));
            }

            if (_connected.Count == 0) //If we didn't make a match
            {
                if (_wasFlipped) //If we flipped
                    FlipPieces(_piece.index, _flippedPiece.index, false); //Flip back
            }
            else //If we made a match
            {
                for (int j = 1; j < _connected.Count; j++)
                {
                    Point _pnt = _connected[j];

                    KillPiece(_pnt);

                    Node _node = GetNodeAtPoint(_pnt);
                    NodePiece _nodePiece = _node.GetPiece();

                    if (_nodePiece != null)
                    {
                        _nodePiece.gameObject.SetActive(false);
                        dead.Add(_nodePiece);
                    }
                    _node.SetPiece(null);
                }

                //InstantiatePiece((int)_piece.pos.x, (int)_piece.pos.y);
                ApplyGravityToBoard();
            }

            flipped.Remove(_flip); //Remove the flip after update
            update.Remove(_piece);
        }
    }

    public void ApplyGravityToBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = (height - 1); y >= 0; y--) //Start at the bottom and grab the next
            {
                Point _point = new Point(x, y);
                Node _node = GetNodeAtPoint(_point);
                int _value = GetValueAtPoint(_point);
                if (_value != 0) continue; //If not a hole, move to the next
                for (int ny = (y - 1); ny >= -1; ny--)
                {
                    Point _nextPoint = new Point(x, ny);
                    int _nextValue = GetValueAtPoint(_nextPoint);
                    if (_nextValue == 0)
                        continue;
                    if (_nextValue != -1)
                    {
                        Node _gotten = GetNodeAtPoint(_nextPoint);
                        NodePiece _piece = _gotten.GetPiece();

                        //Set the hole
                        _node.SetPiece(_piece);
                        update.Add(_piece);

                        //Make a new hole
                        _gotten.SetPiece(null);
                    }
                    else//Use dead ones or create new pieces to fill holes (hit a -1) only if we choose to
                    {
                        int _newValue = FillPiece();
                        NodePiece _piece;
                        Point _fallPoint = new Point(x, (-1 - fills[x]));
                        if (dead.Count > 0)
                        {
                            NodePiece _revived = dead[0];
                            _revived.gameObject.SetActive(true);
                            _piece = _revived;

                            dead.RemoveAt(0);
                        }
                        else
                        {
                            GameObject _obj = Instantiate(nodePiece, GameBoardRect);
                            NodePiece _newPiece = _obj.GetComponent<NodePiece>();
                            _piece = _newPiece;
                        }

                        _piece.Initialize(_newValue, _point, Pieces[_newValue - 1]);
                        _piece.rect.anchoredPosition = GetPositionFromPoint(_fallPoint);

                        Node _holeNode = GetNodeAtPoint(_point);
                        _holeNode.SetPiece(_piece);
                        ResetPiece(_piece);
                        fills[x]++;
                    }
                    break;
                }
            }
        }
    }

    private FlippedPieces GetFlipped(NodePiece p)
    {
        FlippedPieces _flip = null;
        for (int i = 0; i < flipped.Count; i++)
        {
            if (flipped[i].GetOtherPiece(p) != null)
            {
                _flip = flipped[i];
                break;
            }
        }
        return _flip;
    }

    private void StartGame()
    {
        fills = new int[width];
        string _seed = GetRandomSeed();
        random = new System.Random(_seed.GetHashCode());
        update = new List<NodePiece>();
        flipped = new List<FlippedPieces>();
        dead = new List<NodePiece>();
        killed = new List<KilledPiece>();

        InitializeBoard();
        VerifyBoard();
        InstantiateBoard();
    }

    private void InitializeBoard()
    {
        board = new Node[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                board[x, y] = new Node((BoardLayout.rows[y].row[x]) ? -1 : FillPiece(), new Point(x, y));
            }
        }
    }

    private void VerifyBoard()
    {
        List<int> _remove;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point _point = new Point(x, y);
                int _value = GetValueAtPoint(_point);
                if (_value <= 0) continue;

                _remove = new List<int>();
                while (IsConnected(_point, true).Count > 0)
                {
                    _value = GetValueAtPoint(_point);
                    if (!_remove.Contains(_value))
                        _remove.Add(_value);
                    SetValueAtPoint(_point, NewValue(ref _remove));
                }
            }
        }
    }

    private void InstantiateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                InstantiatePiece(x, y);
            }
        }
    }

    private void InstantiatePiece(int x, int y)
    {
        Node _node = GetNodeAtPoint(new Point(x, y));

        int _value = _node.value;

        if (_value > 0)
        {
            GameObject _pieceGO = Instantiate(nodePiece, GameBoardRect);
            NodePiece _piece = _pieceGO.GetComponent<NodePiece>();
            RectTransform _rect = _pieceGO.GetComponent<RectTransform>();

            _rect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y));
            _piece.Initialize(_value, new Point(x, y), Pieces[_value - 1]);
            _node.SetPiece(_piece);
        }
    }

    public void ResetPiece(NodePiece _piece)
    {
        _piece.ResetPosition();
        update.Add(_piece);
    }

    public void FlipPieces(Point _point_1, Point _point_2, bool _isMain)
    {
        if (GetValueAtPoint(_point_1) < 0) return;

        Node _node_1 = GetNodeAtPoint(_point_1);
        NodePiece _piece_1 = _node_1.GetPiece();

        if (GetValueAtPoint(_point_2) > 0)
        {
            Node _node_2 = GetNodeAtPoint(_point_2);
            NodePiece _piece_2 = _node_2.GetPiece();
            _node_1.SetPiece(_piece_2);
            _node_2.SetPiece(_piece_1);

            if (_isMain)
                flipped.Add(new FlippedPieces(_piece_1, _piece_2));

            update.Add(_piece_1);
            update.Add(_piece_2);
        }
        else
            ResetPiece(_piece_1);
    }

    private void KillPiece(Point _point)
    {
        List<KilledPiece> _available = new List<KilledPiece>();
        for (int i = 0; i < killed.Count; i++)
            if (!killed[i].Falling) _available.Add(killed[i]);

        KilledPiece _killedPiece = null;
        if (_available.Count > 0)
            _killedPiece = _available[0];
        else
        {
            GameObject _kill = Instantiate(killedPiece, KilledBoardRect);
            KilledPiece _killPiece = _kill.GetComponent<KilledPiece>();
            _killedPiece = _killPiece;
            killed.Add(_killPiece);
        }

        int value = GetValueAtPoint(_point) - 1;
        if (_killedPiece != null && value >= 0 && value < Pieces.Length)
            _killedPiece.Initialize(Pieces[value], GetPositionFromPoint(_point));
    }

    private List<Point> IsConnected(Point _point, bool _isMain)
    {
        List<Point> _connectedPoints = new List<Point>();
        int _value = GetValueAtPoint(_point);
        Point[] _checkDirections =
        {
            Point.up,
            Point.right,
            Point.down,
            Point.left
        };

        foreach (Point _pointDir in _checkDirections)
        {
            List<Point> _line = new List<Point>();

            int _same = 0;
            for (int i = 1; i < 3; i++)
            {
                Point check = Point.Add(_point, Point.Mult(_pointDir, i));
                if (GetValueAtPoint(check) == _value)
                {
                    _line.Add(check);
                    _same++;
                }
            }

            if (_same > 1)
                AddPoints(ref _connectedPoints, _line);
        }

        for (int i = 0; i < 2; i++)
        {
            List<Point> _line = new List<Point>();

            int _same = 0;
            Point[] _check = { Point.Add(_point, _checkDirections[i]), Point.Add(_point, _checkDirections[i + 2]) };
            foreach (Point _next in _check)
            {
                if (GetValueAtPoint(_next) == _value)
                {
                    _line.Add(_next);
                    _same++;
                }
            }

            if (_same > 1)
                AddPoints(ref _connectedPoints, _line);
        }

        for (int i = 0; i < 4; i++)
        {
            List<Point> _square = new List<Point>();

            int _same = 0;
            int _next = i + 1;
            if (_next >= 4)
                _next -= 4;

            Point[] _check = { Point.Add(_point, _checkDirections[i]), Point.Add(_point, _checkDirections[_next]), Point.Add(_point, Point.Add(_checkDirections[i], _checkDirections[_next])) };
            foreach (Point _pnt in _check)
            {
                if (GetValueAtPoint(_pnt) == _value)
                {
                    _square.Add(_pnt);
                    _same++;
                }
            }

            if (_same > 2)
                AddPoints(ref _connectedPoints, _square);
        }

        if (_isMain)
        {
            for (int i = 0; i < _connectedPoints.Count; i++)
                AddPoints(ref _connectedPoints, IsConnected(_connectedPoints[i], false));
        }

        return _connectedPoints;
    }

    private void AddPoints(ref List<Point> _points, List<Point> _add)
    {
        foreach (Point _point in _add)
        {
            bool _doAdd = true;
            for (int i = 0; i < _points.Count; i++)
            {
                if (_points[i].Equals(_point))
                {
                    _doAdd = false;
                    break;
                }
            }

            if (_doAdd) _points.Add(_point);
        }
    }

    private int FillPiece()
    {
        int _value = 1;
        _value = (random.Next(0, 100) / (100 / Pieces.Length)) + 1;
        return _value;
    }

    private int GetValueAtPoint(Point _point)
    {
        if (_point.x < 0 || _point.x >= width || _point.y < 0 || _point.y >= height) return -1;
        return board[_point.x, _point.y].value;
    }

    private void SetValueAtPoint(Point _point, int _value)
    {
        board[_point.x, _point.y].value = _value;
    }

    private Node GetNodeAtPoint(Point _point)
    {
        return board[_point.x, _point.y];
    }

    private int NewValue(ref List<int> _remove)
    {
        List<int> _available = new List<int>();
        for (int i = 0; i < Pieces.Length; i++)
            _available.Add(i + 1);
        foreach (int i in _remove)
            _available.Remove(i);

        if (_available.Count <= 0) return 0;
        return _available[random.Next(0, _available.Count)];
    }

    private string GetRandomSeed()
    {
        string _seed = "";
        string _acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdeghijklmnopqrstuvwxyz1234567890!@#$%^&*()";
        for (int i = 0; i < 20; i++)
            _seed += _acceptableChars[Random.Range(0, _acceptableChars.Length)];
        return _seed;
    }

    public Vector2 GetPositionFromPoint(Point _point)
    {
        return new Vector2(32 + (64 * _point.x), -32 - (64 * _point.y));
    }
}

[System.Serializable]
public class Node
{
    public int value;
    public Point index;
    NodePiece piece;

    public Node(int _value, Point _pointIndex)
    {
        value = _value;
        index = _pointIndex;
    }

    public void SetPiece(NodePiece _point)
    {
        piece = _point;
        value = (piece == null) ? 0 : piece.value;
        if (piece == null) return;
        piece.SetIndex(index);
    }

    public NodePiece GetPiece()
    {
        return piece;
    }
}

[System.Serializable]
public class FlippedPieces
{
    public NodePiece piece_1;
    public NodePiece piece_2;

    public FlippedPieces(NodePiece _piece_1, NodePiece _piece_2)
    {
        piece_1 = _piece_1; piece_2 = _piece_2;
    }

    public NodePiece GetOtherPiece(NodePiece _piece)
    {
        if (_piece == piece_1)
            return piece_2;
        else if (_piece == piece_2)
            return piece_1;
        else
            return null;
    }
}