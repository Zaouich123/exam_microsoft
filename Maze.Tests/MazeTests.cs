using MazeApp = Maze.App;
using Xunit;

namespace Maze.Tests;

public class MazeTests
{
    [Fact]
    public void Constructor_ParsesStartExitAndWalls()
    {
        var input = string.Join(Environment.NewLine, new[]
        {
            "D..#.",
            "##...",
            ".#.#.",
            "..#..",
            "####S"
        });

        var maze = new MazeApp.Maze(input);

        Assert.Equal((0, 0), maze.Start);
        Assert.Equal((4, 4), maze.Exit);
        Assert.True(maze.Grid[0][3]);
        Assert.True(maze.Grid[1][0]);
        Assert.False(maze.Grid[0][1]);
        Assert.False(maze.Grid[4][4]);
    }

    [Fact]
    public void Constructor_InitializesDistancesToZero()
    {
        var input = string.Join(Environment.NewLine, new[]
        {
            "D..",
            ".#.",
            "..S"
        });

        var maze = new MazeApp.Maze(input);

        Assert.Equal(maze.Grid.Length, maze.Distances.Length);
        for (var y = 0; y < maze.Distances.Length; y++)
        {
            Assert.Equal(maze.Grid[y].Length, maze.Distances[y].Length);
            for (var x = 0; x < maze.Distances[y].Length; x++)
            {
                Assert.Equal(0, maze.Distances[y][x]);
            }
        }
    }

    [Fact]
    public void GetNeighbours_ReturnsAllFour_WhenAvailable()
    {
        var maze = new MazeApp.Maze(string.Join(Environment.NewLine, new[]
        {
            "D...",
            "....",
            "....",
            "...S"
        }));

        var neighbours = maze.GetNeighbours(1, 1);

        Assert.Equal(4, neighbours.Count);
        Assert.Contains((1, 0), neighbours);
        Assert.Contains((1, 2), neighbours);
        Assert.Contains((0, 1), neighbours);
        Assert.Contains((2, 1), neighbours);
    }

    [Fact]
    public void GetNeighbours_ExcludesWalls()
    {
        var maze = new MazeApp.Maze(string.Join(Environment.NewLine, new[]
        {
            "S#.",
            "...",
            "..D"
        }));

        var neighbours = maze.GetNeighbours(1, 1);

        Assert.DoesNotContain((1, 0), neighbours);
        Assert.Contains((1, 2), neighbours);
        Assert.Contains((0, 1), neighbours);
        Assert.Contains((2, 1), neighbours);
    }

    [Fact]
    public void GetNeighbours_ExcludesStart()
    {
        var maze = new MazeApp.Maze(string.Join(Environment.NewLine, new[]
        {
            "...",
            ".D.",
            "..S"
        }));

        var neighbours = maze.GetNeighbours(1, 2);

        Assert.DoesNotContain((1, 1), neighbours);
        Assert.Contains((0, 2), neighbours);
        Assert.Contains((2, 2), neighbours);
    }

    [Fact]
    public void GetNeighbours_ExcludesTopOutside()
    {
        var maze = new MazeApp.Maze(string.Join(Environment.NewLine, new[]
        {
            "D...",
            "....",
            "....",
            "...S"
        }));

        var neighbours = maze.GetNeighbours(2, 0);

        Assert.Equal(3, neighbours.Count);
        Assert.DoesNotContain((2, -1), neighbours);
    }

    [Fact]
    public void GetNeighbours_ExcludesBottomOutside()
    {
        var maze = new MazeApp.Maze(string.Join(Environment.NewLine, new[]
        {
            "D...",
            "....",
            "....",
            "...S"
        }));

        var neighbours = maze.GetNeighbours(1, 3);

        Assert.Equal(3, neighbours.Count);
        Assert.DoesNotContain((1, 4), neighbours);
    }

    [Fact]
    public void GetNeighbours_ExcludesLeftOutside()
    {
        var maze = new MazeApp.Maze(string.Join(Environment.NewLine, new[]
        {
            "D...",
            "....",
            "....",
            "...S"
        }));

        var neighbours = maze.GetNeighbours(0, 2);

        Assert.Equal(3, neighbours.Count);
        Assert.DoesNotContain((-1, 2), neighbours);
    }

    [Fact]
    public void GetNeighbours_ExcludesRightOutside()
    {
        var maze = new MazeApp.Maze(string.Join(Environment.NewLine, new[]
        {
            "D...",
            "....",
            "....",
            "...S"
        }));

        var neighbours = maze.GetNeighbours(3, 1);

        Assert.Equal(3, neighbours.Count);
        Assert.DoesNotContain((4, 1), neighbours);
    }

    [Fact]
    public void Constructor_SeedsQueueWithStart()
    {
        var maze = new MazeApp.Maze("D.S");

        Assert.Single(maze.ToVisit);
        Assert.Equal((0, 0, 0), maze.ToVisit.Peek());
    }

    [Fact]
    public void Fill_ReturnsFalse_WhenExitNotReached()
    {
        var maze = new MazeApp.Maze("D.S");

        var result = maze.Fill();

        Assert.False(result);
    }

    [Fact]
    public void Fill_ReturnsTrue_WhenExitReached()
    {
        var maze = new MazeApp.Maze("D.S");

        var found = false;
        for (var i = 0; i < 3; i++)
        {
            if (maze.Fill())
            {
                found = true;
                break;
            }
        }

        Assert.True(found);
    }

    [Fact]
    public void Fill_IgnoresAlreadyVisitedCells()
    {
        var maze = new MazeApp.Maze(string.Join(Environment.NewLine, new[]
        {
            "D..",
            "...",
            "..S"
        }));

        maze.Fill();

        var before = maze.ToVisit.Count;
        var next = maze.ToVisit.Peek();
        maze.Distances[next.y][next.x] = 99;

        maze.Fill();

        Assert.Equal(before - 1, maze.ToVisit.Count);
    }

    [Fact]
    public void Fill_EnqueuesNeighboursWithIncrementedDistance()
    {
        var maze = new MazeApp.Maze(string.Join(Environment.NewLine, new[]
        {
            "D..",
            "...",
            "..S"
        }));

        maze.Fill();

        var items = maze.ToVisit.ToArray();

        Assert.Equal(2, items.Length);
        Assert.Contains((1, 0, 1), items);
        Assert.Contains((0, 1, 1), items);
    }

    [Fact]
    public void GetDistance_ComputesShortestDistance_ForMaze1()
    {
        var maze = new MazeApp.Maze("D.S");

        var distance = maze.GetDistance();

        Assert.Equal(2, distance);
    }

    [Fact]
    public void GetDistance_ComputesShortestDistance_ForMaze2()
    {
        var maze = new MazeApp.Maze(string.Join(Environment.NewLine, new[]
        {
            "D#.",
            ".#S",
            "..."
        }));

        var distance = maze.GetDistance();

        Assert.Equal(5, distance);
    }

    [Fact]
    public void GetShortestPath_ReturnsPath_ForMaze1()
    {
        var maze = new MazeApp.Maze("D.S");

        var path = maze.GetShortestPath();

        Assert.Equal(new List<(int, int)>
        {
            (2, 0),
            (1, 0),
            (0, 0)
        }, path);
    }

    [Fact]
    public void GetShortestPath_ReturnsPath_ForMaze2()
    {
        var maze = new MazeApp.Maze(string.Join(Environment.NewLine, new[]
        {
            "D#.",
            ".#S",
            "..."
        }));

        var path = maze.GetShortestPath();

        Assert.Equal(new List<(int, int)>
        {
            (2, 1),
            (2, 2),
            (1, 2),
            (0, 2),
            (0, 1),
            (0, 0)
        }, path);
    }
}
