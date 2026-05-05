namespace CrossAgent.Abstractions.Audit;

/// <summary>
/// Canonical audit event kinds emitted by the runtime, patterns, and tooling layer.
/// New values may be added at the end; existing values must not be reused or reordered.
/// </summary>
public enum AuditEventKind
{
    SessionStarted = 0,
    TaskReceived = 1,
    ModelSelected = 2,
    PatternSelected = 3,
    StepStarted = 4,
    StepCompleted = 5,
    ToolCallRequested = 6,
    ToolCallApproved = 7,
    ToolCallRejected = 8,
    ToolResultReceived = 9,
    ValidationPassed = 10,
    ValidationFailed = 11,
    SessionCompleted = 12,
    SessionFailed = 13,
    PolicyRejected = 14
}
