﻿using Drawie.Core.ColorsImpl;

namespace Drawie.Core.Shaders.Generation.Expressions;

public class Half4(string name) : ShaderExpressionVariable<Color>(name)
{
    private Expression? _overrideExpression;
    public override string ConstantValueString => $"half4({ConstantValue.R}, {ConstantValue.G}, {ConstantValue.B}, {ConstantValue.A})";
    
    public Float1 R => new Float1(string.IsNullOrEmpty(VariableName) ? string.Empty : $"{VariableName}.r") { ConstantValue = ConstantValue.R, OverrideExpression = _overrideExpression};
    public Float1 G => new Float1(string.IsNullOrEmpty(VariableName) ? string.Empty : $"{VariableName}.g") { ConstantValue = ConstantValue.G, OverrideExpression = _overrideExpression};
    public Float1 B => new Float1(string.IsNullOrEmpty(VariableName) ? string.Empty : $"{VariableName}.b") { ConstantValue = ConstantValue.B, OverrideExpression = _overrideExpression};
    public Float1 A => new Float1(string.IsNullOrEmpty(VariableName) ? string.Empty : $"{VariableName}.a") { ConstantValue = ConstantValue.A, OverrideExpression = _overrideExpression};
    
    public static implicit operator Half4(Color value) => new Half4("") { ConstantValue = value };
    public static explicit operator Color(Half4 value) => value.ConstantValue;

    public override Expression? OverrideExpression
    {
        get => _overrideExpression;
        set
        {
            _overrideExpression = value;
        }
    }
}
