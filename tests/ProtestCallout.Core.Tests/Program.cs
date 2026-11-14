using ProtestCallout.Core.Crowd;
using ProtestCallout.Core.Session;

var passed = 0;

Run("75% gate rolls exactly once", () =>
{
    var random = new FixedRandom(74);
    var gate = new SessionCalloutGate(75, random);
    Assert(gate.ShouldSchedule, "74 should pass a 75% threshold.");
    Assert(gate.ShouldSchedule, "The cached result should remain true.");
    Assert(random.Calls == 1, "The random source should be called exactly once.");
});

Run("75% gate rejects the threshold boundary", () =>
{
    var random = new FixedRandom(75);
    var gate = new SessionCalloutGate(75, random);
    Assert(!gate.ShouldSchedule, "75 should not pass a 75% threshold.");
    Assert(!gate.ShouldSchedule, "The cached result should remain false.");
    Assert(random.Calls == 1, "The random source should be called exactly once.");
});

Run("session reset permits one new roll", () =>
{
    var random = new FixedRandom(20);
    var gate = new SessionCalloutGate(75, random);
    _ = gate.ShouldSchedule;
    gate.ResetForNewSession();
    _ = gate.ShouldSchedule;
    Assert(random.Calls == 2, "Reset should permit exactly one additional roll.");
});

Run("crowd plan contains 100 civilians and 11 officers", () =>
{
    var plan = CrowdPlan.Create(100, 11);
    int civilians = plan.Counts
        .Where(pair => pair.Key != ParticipantRole.RiotOfficer)
        .Sum(pair => pair.Value);
    Assert(civilians == 100, "Civilian role counts must sum to 100.");
    Assert(plan.Counts[ParticipantRole.RiotOfficer] == 11, "Officer count must be 11.");
});

Run("crowd plan rejects fewer than 100 civilians", () =>
{
    bool threw = false;
    try { _ = CrowdPlan.Create(99, 11); }
    catch (ArgumentOutOfRangeException) { threw = true; }
    Assert(threw, "A 99-person civilian crowd must be rejected.");
});

Console.WriteLine($"Passed {passed} core tests.");

void Run(string name, Action test)
{
    test();
    passed++;
    Console.WriteLine($"PASS: {name}");
}

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

sealed class FixedRandom : Random
{
    private readonly int _value;
    public int Calls { get; private set; }

    public FixedRandom(int value) => _value = value;

    public override int Next(int minValue, int maxValue)
    {
        Calls++;
        return Math.Max(minValue, Math.Min(_value, maxValue - 1));
    }
}
