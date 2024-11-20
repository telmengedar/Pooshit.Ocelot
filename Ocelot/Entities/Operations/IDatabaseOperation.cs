﻿using Pooshit.Ocelot.Entities.Operations.Prepared;

namespace Pooshit.Ocelot.Entities.Operations;

/// <summary>
/// loads values from a statements
/// </summary>
public interface IDatabaseOperation {
        
    /// <summary>
    /// generates command text and data necessary to execute operation
    /// </summary>
    /// <param name="preparator">preparator to write generated data to</param>
    void Prepare(IOperationPreparator preparator);
}