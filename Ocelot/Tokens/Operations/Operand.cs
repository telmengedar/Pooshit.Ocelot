namespace Pooshit.Ocelot.Tokens.Operations {
    
    /// <summary>
    /// operand to be used in <see cref="OperationToken"/>s
    /// </summary>
    public enum Operand {
        
        /// <summary>
        /// multiplication
        /// </summary>
        Multiply,
        
        /// <summary>
        /// division
        /// </summary>
        Divide,

        /// <summary>
        /// addition
        /// </summary>
        Add,
        
        /// <summary>
        /// subtraction
        /// </summary>
        Subtract,

        /// <summary>
        /// negation
        /// </summary>
        Negate,
        
        /// <summary>
        /// logical not
        /// </summary>
        Not,

        /// <summary>
        /// like pattern
        /// </summary>
        Like,
        
        /// <summary>
        /// not like pattern
        /// </summary>
        NotLike,
        
        /// <summary>
        /// not equal
        /// </summary>
        NotEqual,
        
        /// <summary>
        /// equals
        /// </summary>
        Equal,
        
        /// <summary>
        /// less than
        /// </summary>
        Less,
        
        /// <summary>
        /// less than or equal
        /// </summary>
        LessOrEqual,
        
        /// <summary>
        /// greater than
        /// </summary>
        Greater,
        
        /// <summary>
        /// greater than or equal
        /// </summary>
        GreaterOrEqual,
        
        /// <summary>
        /// binary and
        /// </summary>
        And,
        
        /// <summary>
        /// binary or
        /// </summary>
        Or,

        /// <summary>
        /// binary exclusive or
        /// </summary>
        ExclusiveOr,
        
        /// <summary>
        /// logical and
        /// </summary>
        AndAlso,
        
        /// <summary>
        /// logical or
        /// </summary>
        OrElse,
        
        /// <summary>
        /// bitwise shift left
        /// </summary>
        ShiftLeft,
        
        /// <summary>
        /// bitwise shift right
        /// </summary>
        ShiftRight
    }
}