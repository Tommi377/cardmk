using Xunit;

namespace RealMK.Tests;

[Trait("Category", "Unit")]
public class HexCoordTests
{
    [Fact]
    public void Constructor_CreatesCoordWithCorrectValues()
    {
        HexCoord coord = new(3, -2);

        Assert.Equal(3, coord.Q);
        Assert.Equal(-2, coord.R);
    }

    [Fact]
    public void CubeCoordinates_AreCorrectlyDerived()
    {
        HexCoord coord = new(3, -2);

        Assert.Equal(3, coord.X);   // X = Q
        Assert.Equal(-1, coord.Y);  // Y = -Q - R = -3 - (-2) = -1
        Assert.Equal(-2, coord.Z);  // Z = R
        Assert.Equal(-1, coord.S);  // S = Y
    }

    [Fact]
    public void CubeCoordinateSum_IsAlwaysZero()
    {
        HexCoord coord1 = new(0, 0);
        HexCoord coord2 = new(3, -2);
        HexCoord coord3 = new(-5, 7);

        Assert.Equal(0, coord1.X + coord1.Y + coord1.Z);
        Assert.Equal(0, coord2.X + coord2.Y + coord2.Z);
        Assert.Equal(0, coord3.X + coord3.Y + coord3.Z);
    }

    [Fact]
    public void Distance_FromOriginToAdjacentHex_IsOne()
    {
        HexCoord origin = new(0, 0);
        HexCoord adjacent = new(1, 0);

        int distance = origin.DistanceTo(adjacent);

        Assert.Equal(1, distance);
    }

    [Fact]
    public void Distance_FromOriginTo_2_Minus1_IsTwo()
    {
        HexCoord origin = new(0, 0);
        HexCoord target = new(2, -1);

        int distance = origin.DistanceTo(target);

        Assert.Equal(2, distance);
    }

    [Fact]
    public void Distance_IsSymmetric()
    {
        HexCoord a = new(3, -2);
        HexCoord b = new(-1, 4);

        Assert.Equal(a.DistanceTo(b), b.DistanceTo(a));
    }

    [Fact]
    public void Distance_ToSelf_IsZero()
    {
        HexCoord coord = new(5, -3);

        Assert.Equal(0, coord.DistanceTo(coord));
    }

    [Fact]
    public void Directions_ContainsSixElements()
    {
        Assert.Equal(6, HexCoord.Directions.Count);
    }

    [Fact]
    public void Directions_AreAllDistinctAndAdjacentToOrigin()
    {
        HexCoord origin = new(0, 0);

        foreach (HexCoord dir in HexCoord.Directions)
        {
            Assert.Equal(1, origin.DistanceTo(dir));
        }

        // All directions are unique
        Assert.Equal(6, HexCoord.Directions.Distinct().Count());
    }

    [Fact]
    public void Neighbor_ReturnsCorrectAdjacentHex()
    {
        HexCoord coord = new(2, 3);

        HexCoord neighborE = coord.Neighbor(0);
        HexCoord neighborSE = coord.Neighbor(1);
        HexCoord neighborSW = coord.Neighbor(2);
        HexCoord neighborW = coord.Neighbor(3);
        HexCoord neighborNW = coord.Neighbor(4);
        HexCoord neighborNE = coord.Neighbor(5);

        Assert.Equal(new HexCoord(3, 3), neighborE);
        Assert.Equal(new HexCoord(2, 4), neighborSE);
        Assert.Equal(new HexCoord(1, 4), neighborSW);
        Assert.Equal(new HexCoord(1, 3), neighborW);
        Assert.Equal(new HexCoord(2, 2), neighborNW);
        Assert.Equal(new HexCoord(3, 2), neighborNE);
    }

    [Fact]
    public void Neighbor_ThrowsOnInvalidDirection()
    {
        HexCoord coord = new(0, 0);

        Assert.Throws<ArgumentOutOfRangeException>(() => coord.Neighbor(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => coord.Neighbor(6));
    }

    [Fact]
    public void AllNeighbors_ReturnsSixUniqueHexes()
    {
        HexCoord coord = new(0, 0);

        List<HexCoord> neighbors = coord.AllNeighbors().ToList();

        Assert.Equal(6, neighbors.Count);
        Assert.Equal(6, neighbors.Distinct().Count());

        foreach (HexCoord neighbor in neighbors)
        {
            Assert.Equal(1, coord.DistanceTo(neighbor));
        }
    }

    [Fact]
    public void Addition_CombinesCoordinates()
    {
        HexCoord a = new(3, -2);
        HexCoord b = new(-1, 4);

        HexCoord result = a + b;

        Assert.Equal(new HexCoord(2, 2), result);
    }

    [Fact]
    public void Subtraction_DifferencesCoordinates()
    {
        HexCoord a = new(3, -2);
        HexCoord b = new(-1, 4);

        HexCoord result = a - b;

        Assert.Equal(new HexCoord(4, -6), result);
    }

    [Fact]
    public void AddingAndSubtractingDirection_IsReversible()
    {
        HexCoord origin = new(5, -3);

        foreach (HexCoord dir in HexCoord.Directions)
        {
            HexCoord moved = origin + dir;
            HexCoord returned = moved - dir;

            Assert.Equal(origin, returned);
        }
    }

    [Fact]
    public void ScalarMultiplication_ScalesCoordinates()
    {
        HexCoord coord = new(3, -2);

        HexCoord doubled = coord * 2;
        HexCoord tripled = coord * 3;

        Assert.Equal(new HexCoord(6, -4), doubled);
        Assert.Equal(new HexCoord(9, -6), tripled);
    }

    [Fact]
    public void Equality_ComparesValues()
    {
        HexCoord a = new(3, -2);
        HexCoord b = new(3, -2);
        HexCoord c = new(2, -2);

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
        Assert.True(a == b);
        Assert.True(a != c);
    }

    [Fact]
    public void GetHashCode_IsConsistentForEqualValues()
    {
        HexCoord a = new(3, -2);
        HexCoord b = new(3, -2);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentForDifferentValues()
    {
        HexCoord a = new(3, -2);
        HexCoord b = new(3, -1);
        HexCoord c = new(2, -2);

        // Not guaranteed but highly likely
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a.GetHashCode(), c.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        HexCoord coord = new(3, -2);

        string result = coord.ToString();

        Assert.Equal("(3, -2)", result);
    }
}
