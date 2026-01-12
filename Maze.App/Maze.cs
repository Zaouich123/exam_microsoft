namespace Maze.App;

public class Maze
{
    public int[][] Distances { get; init; }
    public bool[][] Grid { get; init; }
    public Queue<(int x, int y, int distance)> ToVisit { get; init; }
    public (int x, int y) Start { get; init; }
    public (int x, int y) Exit { get; init; }

    private readonly int _width;
    private readonly int _height;

    public Maze(string maze)
    {
        var lines = maze.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        _height = lines.Length;
        _width = lines.Length == 0 ? 0 : lines.Max(line => line.Length);

        Grid = new bool[_height][];
        Distances = new int[_height][];

        var start = (-1, -1);
        var exit = (-1, -1);

        for (var y = 0; y < _height; y++)
        {
            Grid[y] = new bool[_width];
            Distances[y] = new int[_width];

            var line = lines[y];
            for (var x = 0; x < _width; x++)
            {
                var cell = x < line.Length ? line[x] : '#';
                switch (cell)
                {
                    case 'D':
                        start = (x, y);
                        Grid[y][x] = false;
                        break;
                    case 'S':
                        exit = (x, y);
                        Grid[y][x] = false;
                        break;
                    case '#':
                        Grid[y][x] = true;
                        break;
                    case '.':
                        Grid[y][x] = false;
                        break;
                    default:
                        Grid[y][x] = true;
                        break;
                }
            }
        }

        Start = start;
        Exit = exit;
        ToVisit = new Queue<(int x, int y, int distance)>();
        if (Start.x >= 0 && Start.y >= 0)
        {
            ToVisit.Enqueue((Start.x, Start.y, 0));
        }
    }

    public int GetDistance()
    {
        while (ToVisit.Count > 0)
        {
            if (Fill())
            {
                return Distances[Exit.y][Exit.x];
            }
        }

        return -1;
    }

    public IList<(int, int)> GetNeighbours(int x, int y)
    {
        var neighbours = new List<(int, int)>();
        var candidates = new (int x, int y)[]
        {
            (x, y - 1),
            (x, y + 1),
            (x - 1, y),
            (x + 1, y)
        };

        foreach (var candidate in candidates)
        {
            if (!IsInside(candidate.x, candidate.y))
            {
                continue;
            }

            if (Grid[candidate.y][candidate.x])
            {
                continue;
            }

            if (candidate == Start)
            {
                continue;
            }

            neighbours.Add(candidate);
        }

        return neighbours;
    }

    public bool Fill()
    {
        if (ToVisit.Count == 0)
        {
            return false;
        }

        var current = ToVisit.Dequeue();
        var (x, y, distance) = current;

        if ((x, y) == Exit)
        {
            Distances[y][x] = distance;
            return true;
        }

        if ((x, y) != Start && Distances[y][x] != 0)
        {
            return false;
        }

        Distances[y][x] = distance;

        foreach (var neighbour in GetNeighbours(x, y))
        {
            ToVisit.Enqueue((neighbour.Item1, neighbour.Item2, distance + 1));
        }

        return false;
    }

    public IList<(int, int)> GetShortestPath()
    {
        var distance = Distances[Exit.y][Exit.x];
        if (distance == 0 && Exit != Start)
        {
            distance = GetDistance();
            if (distance <= 0)
            {
                return new List<(int, int)>();
            }
        }

        var path = new List<(int, int)> { Exit };
        var current = Exit;

        while (current != Start)
        {
            var currentDistance = Distances[current.y][current.x];
            var next = FindPreviousStep(current.x, current.y, currentDistance);

            if (next == current)
            {
                break;
            }

            path.Add(next);
            current = next;
        }

        return path;
    }

    private (int x, int y) FindPreviousStep(int x, int y, int distance)
    {
        var candidates = new (int x, int y)[]
        {
            (x, y - 1),
            (x, y + 1),
            (x - 1, y),
            (x + 1, y)
        };

        foreach (var candidate in candidates)
        {
            if (!IsInside(candidate.x, candidate.y))
            {
                continue;
            }

            if (Grid[candidate.y][candidate.x])
            {
                continue;
            }

            if (Distances[candidate.y][candidate.x] == distance - 1)
            {
                return candidate;
            }
        }

        return (x, y);
    }

    private bool IsInside(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height;
}
