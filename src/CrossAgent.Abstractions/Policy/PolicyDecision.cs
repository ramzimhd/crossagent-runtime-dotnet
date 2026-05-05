namespace CrossAgent.Abstractions.Policy;

/// <summary>
/// Result of a policy evaluation. <see cref="Reason"/> is required when
/// <see cref="Allowed"/> is false so audit logs and error messages can explain rejection.
/// </summary>
public sealed record PolicyDecision
{
    public required bool Allowed { get; init; }

    public string? Reason { get; init; }

    public static PolicyDecision Allow() => new() { Allowed = true };

    public static PolicyDecision Deny(string reason) => new() { Allowed = false, Reason = reason };
}
