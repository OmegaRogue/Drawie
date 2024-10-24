﻿using Drawie.Backend.Core.ColorsImpl;

namespace Drawie.Backend.Core.Shaders.Generation.Expressions;

public class Half4(string name) : ShaderExpressionVariable<Color>(name)
{
    private Expression? _overrideExpression;

    public override string ConstantValueString =>
        $"half4({ConstantValue.R}, {ConstantValue.G}, {ConstantValue.B}, {ConstantValue.A})";

    public Float1 R =>
        new Half4Float1Accessor(this, 'r')
        {
            ConstantValue = ConstantValue.R, OverrideExpression = _overrideExpression
        };

    public Float1 G =>
        new Half4Float1Accessor(this, 'g')
        {
            ConstantValue = ConstantValue.G, OverrideExpression = _overrideExpression
        };

    public Float1 B =>
        new Half4Float1Accessor(this, 'b')
        {
            ConstantValue = ConstantValue.B, OverrideExpression = _overrideExpression
        };

    public Float1 A =>
        new Half4Float1Accessor(this, 'a')
        {
            ConstantValue = ConstantValue.A, OverrideExpression = _overrideExpression
        };

    public static implicit operator Half4(Color value) => new("") { ConstantValue = value };
    public static explicit operator Color(Half4 value) => value.ConstantValue;

    public override Expression? OverrideExpression
    {
        get => _overrideExpression;
        set
        {
            _overrideExpression = value;
        }
    }

    public ShaderExpressionVariable GetValueAt(int index)
    {
        return index switch
        {
            0 => R,
            1 => G,
            2 => B,
            3 => A,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public static string ConstructorText(Expression r, Expression g, Expression b, Expression a) =>
        $"half4({r.ExpressionValue}, {g.ExpressionValue}, {b.ExpressionValue}, {a.ExpressionValue})";

    public static Expression Constructor(Expression r, Expression g, Expression b, Expression a) =>
        new Expression(ConstructorText(r, g, b, a));
}
