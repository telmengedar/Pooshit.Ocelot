using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Models;

/// <summary>
/// Lightweight entity used exclusively by <c>PostgresPreparedStatementTests</c>.
/// Isolated table name avoids collisions with other parallel test fixtures.
/// </summary>
[Table("pstmt_test")]
public class PreparedTestEntity {
    [DefaultValue(0)]
    public int Id { get; set; }

    public string Label { get; set; }

    [DefaultValue(0)]
    public double Value { get; set; }
}
