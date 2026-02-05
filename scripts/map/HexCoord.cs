using System;
using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Axial hex coordinate system (q, r).
/// Cube coordinates derived: x=q, y=-q-r, z=r
/// </summary>
public readonly record struct HexCoord(int Q, int R) : IEquatable<HexCoord>
{
    /// <summary>
    /// Cube coordinate X (same as Q in axial).
    /// </summary>
    public int X => Q;

    /// <summary>
    /// Cube coordinate Y (derived from Q and R).
    /// </summary>
    public int Y => -Q - R;

    /// <summary>
    /// Cube coordinate Z (same as R in axial).
    /// </summary>
    public int Z => R;

    /// <summary>
    /// Cube coordinate S (alternate naming, same as Y).
    /// </summary>
    public int S => -Q - R;

    // Six hex directions (axial coordinates)
    public static readonly HexCoord DirectionE = new(1, 0);
    public static readonly HexCoord DirectionNE = new(1, -1);
    public static readonly HexCoord DirectionNW = new(0, -1);
    public static readonly HexCoord DirectionW = new(-1, 0);
    public static readonly HexCoord DirectionSW = new(-1, 1);
    public static readonly HexCoord DirectionSE = new(0, 1);

    /// <summary>
    /// All six directions in clockwise order starting from East.
    /// </summary>
    public static readonly IReadOnlyList<HexCoord> Directions = [
        DirectionE, DirectionSE, DirectionSW, DirectionW, DirectionNW, DirectionNE
    ];

    /// <summary>
    /// Gets the neighbor in the specified direction.
    /// </summary>
    /// <param name="direction">Direction index (0-5).</param>
    public HexCoord Neighbor(int direction)
    {
        if (direction is < 0 or >= 6)
            throw new ArgumentOutOfRangeException(nameof(direction), "Direction must be 0-5");

        return this + Directions[direction];
    }

    /// <summary>
    /// Enumerates all six neighbors.
    /// </summary>
    public IEnumerable<HexCoord> AllNeighbors()
    {
        foreach (HexCoord dir in Directions)
        {
            yield return this + dir;
        }
    }

    /// <summary>
    /// Calculates Manhattan distance to another hex coordinate using cube coordinates.
    /// </summary>
    public int DistanceTo(HexCoord other)
    {
        int dx = Math.Abs(X - other.X);
        int dy = Math.Abs(Y - other.Y);
        int dz = Math.Abs(Z - other.Z);
        return (dx + dy + dz) / 2;
    }

    // Operator overloads
    public static HexCoord operator +(HexCoord a, HexCoord b) => new(a.Q + b.Q, a.R + b.R);
    public static HexCoord operator -(HexCoord a, HexCoord b) => new(a.Q - b.Q, a.R - b.R);
    public static HexCoord operator *(HexCoord coord, int scalar) => new(coord.Q * scalar, coord.R * scalar);

    public override int GetHashCode() => HashCode.Combine(Q, R);

    public override string ToString() => $"({Q}, {R})";
}