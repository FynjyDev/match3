using UnityEngine;

[System.Serializable]
public class Point
{
    public int x;
    public int y;

    public Point(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public void Mult(int _multiplication)
    {
        x *= _multiplication;
        y *= _multiplication;
    }
    public void Add(Point _point)
    {
        x += _point.x;
        y += _point.y;
    }
    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }
    public bool Equals(Point _point)
    {
        return (x == _point.x && y == _point.y);
    }
    public static Point FromVector(Vector2 _vector)
    {
        return new Point((int)_vector.x, (int)_vector.y);
    }       
    public static Point FromVector(Vector3 _vector)
    {
        return new Point((int)_vector.x, (int)_vector.y);
    }
    public static Point Mult(Point _point, int _multiplication)
    {
        return new Point(_point.x * _multiplication, _point.y * _multiplication);
    }
    public static Point Add(Point _point_1, Point _point_2)
    {
        return new Point(_point_1.x + _point_2.x, _point_1.y + _point_2.y);
    }
    public static Point Clone(Point _point)
    {
        return new Point(_point.x, _point.y);
    }
    public static Point zero
    {
        get { return new Point(0, 0); }
    } 
    public static Point one
    {
        get { return new Point(1, 1); }
    }   
    public static Point up
    {
        get { return new Point(0, 1); }
    }  
    public static Point down
    {
        get { return new Point(0, -1); }
    }   
    public static Point right
    {
        get { return new Point(1, 0); }
    }   
    public static Point left
    {
        get { return new Point(-1, 0); }
    }
}
