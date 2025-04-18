using Drawie.Numerics;

namespace Drawie.Backend.Core.Shaders.Generation.Expressions;

public class Float4(string name) : ShaderExpressionVariable<Vec4D>(name), IMultiValueVariable
{
    private Expression? _overrideExpression;
    private Expression? _xOverrideExpression;
    private Expression? _yOverrideExpression;
    private Expression? _zOverrideExpression;
    private Expression? _wOverrideExpression;
    
    public override string ConstantValueString
    {
        get
        {
            string x = ConstantValue.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string y = ConstantValue.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string z = ConstantValue.Z.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string w = ConstantValue.W.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return $"float4({x}, {y}, {z}, {w})";
        }
    }

    public override Expression? OverrideExpression
    {
        get => _overrideExpression;
        set
        {
            _overrideExpression = value;
        }
    }

    public Float1 X
    {
        get
        {
            return new Float1($"{VariableName}.x")
            {
                ConstantValue = ConstantValue.X, OverrideExpression = _xOverrideExpression
            };
        }
    }

    public Float1 Y
    {
        get
        {
            return new Float1($"{VariableName}.y")
            {
                ConstantValue = ConstantValue.Y, OverrideExpression = _yOverrideExpression
            };
        }
    }
    public Float1 Z
    {
        get
        {
            return new Float1($"{VariableName}.z")
                   {
                       ConstantValue = ConstantValue.Z, OverrideExpression = _zOverrideExpression
                   };
        }
    }
    public Float1 W
    {
        get
        {
            return new Float1($"{VariableName}.w")
                   {
                       ConstantValue = ConstantValue.W, OverrideExpression = _wOverrideExpression
                   };
        }
    }

    public static implicit operator Float4(Vec4D value) => new Float4("") { ConstantValue = value };
    
    public static explicit operator Vec4D(Float4 value) => value.ConstantValue;

    public ShaderExpressionVariable GetValueAt(int index)
    {
        return index switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            3 => W,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public void OverrideExpressionAt(int index, Expression? expression)
    {
        switch (index)
        {
            case 0:
                _xOverrideExpression = expression;
                break;
            case 1:
                _yOverrideExpression = expression;
                break;
            case 2:
                _zOverrideExpression = expression;
                break;
            case 3:
                _wOverrideExpression = expression;
                break;
            default:
                throw new IndexOutOfRangeException();
        }
    }

    public int GetValuesCount()
    {
        return 4;
    }
}
