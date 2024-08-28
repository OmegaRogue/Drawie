﻿namespace Drawie.Core.Shaders.Generation.Expressions;

public class Expression
{
    public virtual string ExpressionValue { get; }

    public Expression()
    {
        
    }
    
    public Expression(string expressionValue)
    {
        ExpressionValue = expressionValue;
    }
}
