using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Linq;

public class Node
{
    public List<Node> Children { get; set; }
    public Vector2 leftTop { get; set; }
    public Vector2 rightTop { get; set; }
    public Vector2 leftBottom { get; set; }
    public Vector2 rightBottom { get; set; }

    public Node(Vector2 _leftTop, Vector2 _rightTop, Vector2 _leftBottom, Vector2 _rightBottom)
    {
        Children = new List<Node>();

        leftTop = _leftTop;
        rightTop = _rightTop;
        leftBottom = _leftBottom;
        rightBottom = _rightBottom;
    }
}

public class Program
{
    public static float blockMinX = 100;
    public static float blockMinY = 100;
    public static float irregular = 80;
    public static float outsideLevel = -1f;

    public static float leftX = 0;
    public static float rightX = 500;
    public static float bottomY = 0;
    public static float topY = 800;

    public static void Main()
    {
        Random random = new Random();

        Node root = new Node(new Vector2(leftX, topY), new Vector2(rightX, topY), new Vector2(leftX, bottomY), new Vector2(rightX, bottomY));
        BSP(root, ref random);

        PrintLastChildren(root);
    }

    public static void BSP(Node _node, ref Random _random)
    {
        float width = Math.Abs(_node.rightBottom.X - _node.leftBottom.X); //공간의 바닥에 블록이 놓일거기 때문에
        float height = Math.Min(Math.Abs(_node.rightTop.Y - _node.rightBottom.Y), Math.Abs(_node.leftTop.Y - _node.leftBottom.Y)); //공간의 어디에 놓여도 블록이 들어가야 하기 때문에 min Y축 기준

        bool widthPartition = Math.Abs(width) / 2 >= blockMinX;
        bool heightPartition = Math.Abs(height) / 2 >= blockMinY;

        float partitionStandard = NextFloat(0f, height + width, ref _random);

        if (partitionStandard < width && widthPartition) //width 기준 분할
        {
            WidthPatitioning(ref _node, ref _random);
        }
        else if(heightPartition) //height 기준 분할
        {
            HeightPatitioning(ref _node, ref _random);
        }
        else
        {
            BlockPlacement(_node);
        }

        foreach (var child in _node.Children)
        {
            BSP(child, ref _random);
        }
    }

    public static void PrintLastChildren(Node node)
    {
        if (node.Children.Count == 0)
        {
            Random random = new Random();            
            Console.WriteLine($"{node.leftTop.X} {node.leftTop.Y} {node.rightTop.X} {node.rightTop.Y} " +
                                          $"{node.rightBottom.X} {node.rightBottom.Y} {node.leftBottom.X} {node.leftBottom.Y} {random.Next(0, 255)} {random.Next(0, 255)} {random.Next(0, 255)}");
        }
        else
        {
            foreach (var child in node.Children)
            {
                PrintLastChildren(child);
            }
        }
    }

    public static float NextFloat(float _min, float _max, ref Random _random)
    {
        double val = (_random.NextDouble() * (_max - _min) + _min);
        return (float)val;
    }

    public static Vector2 CalculateVertex(Vector2 _v1, Vector2 _v2, Vector2 _d)
    {
        if (_d.X == outsideLevel)
        {
            float ratio = (_d.Y - _v1.Y) / (_v2.Y - _v1.Y);
            float x = _v1.X + (_v2.X - _v1.X) * ratio;
            return new Vector2(x, _d.Y);
        }
        else
        {
            float ratio = (_d.X - _v1.X) / (_v2.X - _v1.X);
            float y = _v1.Y + (_v2.Y - _v1.Y) * ratio;
            return new Vector2(_d.X, y);
        }
    }

    public static void WidthPatitioning(ref Node _node, ref Random _random)
    {
        //float splitTopX = NextFloat(_node.leftTop.X, Math.Min(_node.rightTop.X + irregular, rightX), ref _random);
        //float splitBottomX = NextFloat(splitTopX, Math.Min(splitTopX + irregular, rightX), ref _random);
        float splitTopX, splitBottomX;

        if (_random.NextDouble() < 0.5)
        {
            splitTopX = NextFloat(_node.leftTop.X + blockMinX, Math.Min(_node.rightTop.X + irregular, rightX) - blockMinX, ref _random);
            splitBottomX = NextFloat(splitTopX, Math.Min(splitTopX + irregular, rightX), ref _random);
        }
        else
        {
            splitBottomX = NextFloat(_node.leftBottom.X + blockMinX, Math.Min(_node.rightBottom.X + irregular, rightX) - blockMinX, ref _random);
            splitTopX = NextFloat(splitBottomX, Math.Min(splitBottomX + irregular, rightX), ref _random);
        }

        Vector2 topX = CalculateVertex(_node.leftTop, _node.rightTop, new Vector2(splitTopX, outsideLevel));
        Vector2 bottomX = CalculateVertex(_node.leftBottom, _node.rightBottom, new Vector2(splitBottomX, outsideLevel));

        _node.Children.Add(new Node(_node.leftTop, topX, _node.leftBottom, bottomX));
        _node.Children.Add(new Node(topX, _node.rightTop, bottomX, _node.rightBottom));
    }

    public static void HeightPatitioning(ref Node _node, ref Random _random)
    {
        //float splitLeftY = NextFloat(_node.leftTop.Y, Math.Min(_node.leftBottom.Y + irregular, topY), ref _random);
        //float splitRightY = NextFloat(splitLeftY, Math.Min(splitLeftY + irregular, topY), ref _random);
        float splitLeftY, splitRightY;
        if (_random.Next(0, 2) == 1)
        {
            splitLeftY = NextFloat(_node.leftBottom.Y + blockMinY, Math.Min(_node.leftTop.Y + irregular, topY) - blockMinY, ref _random);
            splitRightY = NextFloat(splitLeftY, Math.Min(splitLeftY + irregular, topY), ref _random);
        }
        else        
        {
            splitRightY = NextFloat(_node.rightBottom.Y + blockMinY, Math.Min(_node.rightTop.Y + irregular, topY) - blockMinY, ref _random);
            splitLeftY = NextFloat(splitRightY, Math.Min(splitRightY + irregular, topY), ref _random);
        }            

        splitLeftY = NextFloat(_node.leftBottom.Y + blockMinY, Math.Min(_node.leftTop.Y + irregular, topY) - blockMinY, ref _random);
        splitRightY = NextFloat(splitLeftY, Math.Min(splitLeftY + irregular, topY), ref _random);

        Vector2 leftY = CalculateVertex(_node.leftTop, _node.leftBottom, new Vector2(outsideLevel, splitLeftY));
        Vector2 rightY = CalculateVertex(_node.rightTop, _node.rightBottom, new Vector2(outsideLevel, splitRightY));

        _node.Children.Add(new Node(leftY, rightY, _node.leftBottom, _node.rightBottom));
        _node.Children.Add(new Node(_node.leftTop, _node.rightTop, leftY, rightY));
    }
}

