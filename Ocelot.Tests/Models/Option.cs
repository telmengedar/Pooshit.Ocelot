using System;

namespace Pooshit.Ocelot.Tests.Models;

public class Option {
    /// <summary>
    /// data for an option
    /// </summary>
    /// <summary>
    /// id of option
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// name of option
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// description for option
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// type of value
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// if set defines the minimum valid value of a value
    /// </summary>
    public string Min { get; set; }

    /// <summary>
    /// if set defines the maximum valid value of a value
    /// </summary>
    public string Max { get; set; }

    /// <summary>
    /// if true, specifying a value is required
    /// </summary>
    public bool Mandatory { get; set; }
}