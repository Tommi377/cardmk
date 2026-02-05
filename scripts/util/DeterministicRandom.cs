using System;
using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Deterministic random number generator using XorShift128+ algorithm.
/// Provides reproducible random sequences for replays and multiplayer synchronization.
/// </summary>
public sealed class DeterministicRandom
{
    private ulong _state0;
    private ulong _state1;

    /// <summary>
    /// Initializes with a seed using SplitMix64 to generate initial state.
    /// </summary>
    /// <param name="seed">Seed value for initialization.</param>
    public DeterministicRandom(ulong seed)
    {
        // SplitMix64 initialization to get good initial state
        _state0 = SplitMix64(ref seed);
        _state1 = SplitMix64(ref seed);

        // Ensure non-zero state
        if (_state0 == 0 && _state1 == 0)
        {
            _state0 = 1;
        }

        Log.Debug($"DeterministicRandom initialized with seed {seed}, state=({_state0:X16}, {_state1:X16})");
    }

    /// <summary>
    /// Initializes with explicit state (for restoration from save).
    /// </summary>
    public DeterministicRandom(ulong state0, ulong state1)
    {
        _state0 = state0;
        _state1 = state1;

        // Ensure non-zero state
        if (_state0 == 0 && _state1 == 0)
        {
            _state0 = 1;
        }

        Log.Debug($"DeterministicRandom restored from state ({_state0:X16}, {_state1:X16})");
    }

    /// <summary>
    /// Gets the current internal state for serialization.
    /// </summary>
    public (ulong State0, ulong State1) GetState() => (_state0, _state1);

    /// <summary>
    /// Generates the next random 64-bit unsigned integer.
    /// </summary>
    public ulong NextUInt64()
    {
        ulong s0 = _state0;
        ulong s1 = _state1;
        ulong result = s0 + s1;

        s1 ^= s0;
        _state0 = RotateLeft(s0, 24) ^ s1 ^ (s1 << 16);
        _state1 = RotateLeft(s1, 37);

        return result;
    }

    /// <summary>
    /// Returns a random integer in the range [0, maxExclusive).
    /// </summary>
    public int Next(int maxExclusive)
    {
        if (maxExclusive <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be positive");

        // Use rejection sampling to avoid modulo bias
        ulong randomValue = NextUInt64();
        ulong range = (ulong)maxExclusive;
        ulong maxValid = ulong.MaxValue - (ulong.MaxValue % range);

        while (randomValue >= maxValid)
        {
            randomValue = NextUInt64();
        }

        return (int)(randomValue % range);
    }

    /// <summary>
    /// Returns a random integer in the range [minInclusive, maxExclusive).
    /// </summary>
    public int Next(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
            throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be greater than minInclusive");

        int range = maxExclusive - minInclusive;
        return minInclusive + Next(range);
    }

    /// <summary>
    /// Shuffles a list in place using Fisher-Yates algorithm.
    /// </summary>
    public void Shuffle<T>(IList<T> list)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Picks a random element from a read-only list.
    /// </summary>
    public T PickRandom<T>(IReadOnlyList<T> list)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));
        if (list.Count == 0)
            throw new ArgumentException("Cannot pick from empty list", nameof(list));

        int index = Next(list.Count);
        return list[index];
    }

    // Helper: SplitMix64 for seed initialization
    private static ulong SplitMix64(ref ulong state)
    {
        ulong result = (state += 0x9E3779B97f4A7C15);
        result = (result ^ (result >> 30)) * 0xBF58476D1CE4E5B9;
        result = (result ^ (result >> 27)) * 0x94D049BB133111EB;
        return result ^ (result >> 31);
    }

    // Helper: Rotate left
    private static ulong RotateLeft(ulong value, int shift)
    {
        return (value << shift) | (value >> (64 - shift));
    }
}