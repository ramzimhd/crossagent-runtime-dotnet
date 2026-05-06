namespace CrossAgents.Core;

/// <summary>
/// Stable identifiers for the patterns that ship with this repository. Custom
/// patterns may use any string id; these constants exist so applications can
/// refer to built-in patterns without taking a dependency on CrossAgents.Patterns.
/// </summary>
public static class KnownPatternIds
{
    public const string NoTool = "no-tool";
    public const string PlanExecuteValidate = "plan-execute-validate";
    public const string JsonPlan = "json-plan";
    public const string BoundedReAct = "bounded-react";
}
