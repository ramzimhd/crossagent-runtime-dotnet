using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossAgents.Core;

/// <summary>
/// A simple ordered graph of named steps used by patterns to record their phases
/// in the audit log. The graph itself does not run anything - it captures
/// declared phases and their order so patterns can emit consistent step events.
/// </summary>
public sealed class ExecutionGraph
{
    private readonly List<string> _steps = new();

    /// <summary>Append a step at the end of the graph.</summary>
    public ExecutionGraph AddStep(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Step name must be non-empty.", nameof(name));
        }

        if (_steps.Contains(name, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"Step '{name}' already exists in the graph.");
        }

        _steps.Add(name);
        return this;
    }

    public IReadOnlyList<string> Steps => _steps;

    public bool Contains(string name) => _steps.Contains(name, StringComparer.Ordinal);
}
