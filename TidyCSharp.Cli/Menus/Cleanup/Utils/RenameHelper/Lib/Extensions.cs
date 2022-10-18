using Microsoft.CodeAnalysis;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper.Lib;

public static class Extensions
{
    public static string GetFullNamespace(this ISymbol symbol)
    {
        if ((symbol.ContainingNamespace == null) ||
            (string.IsNullOrEmpty(symbol.ContainingNamespace.Name)))
        {
            return null;
        }

        // get the rest of the full namespace string
        var restOfResult = symbol.ContainingNamespace.GetFullNamespace();

        var result = symbol.ContainingNamespace.Name;

        if (restOfResult != null)
            // if restOfResult is not null, append it after a period
            result = restOfResult + '.' + result;

        return result;
    }

    public static string GetFullTypeString(this INamedTypeSymbol type)
    {
        var result = type.Name;

        if (type.TypeArguments.Count() > 0)
        {
            result += "<";

            var isFirstIteration = true;

            foreach (INamedTypeSymbol typeArg in type.TypeArguments)
            {
                if (isFirstIteration) isFirstIteration = false;
                else result += ", ";

                result += typeArg.GetFullTypeString();
            }

            result += ">";
        }

        return result;
    }

    public static string ConvertAccessabilityToString(this Accessibility accessability)
    {
        switch (accessability)
        {
            case Accessibility.Internal:
                return "internal";
            case Accessibility.Private:
                return "private";
            case Accessibility.Protected:
                return "protected";
            case Accessibility.Public:
                return "public";
            case Accessibility.ProtectedAndInternal:
                return "protected internal";
            default:
                return "private";
        }
    }

    public static string GetMethodSignature(this IMethodSymbol methodSymbol)
    {
        var result = methodSymbol.DeclaredAccessibility.ConvertAccessabilityToString();

        if (methodSymbol.IsAsync) result += " async";

        if (methodSymbol.IsAbstract)
            result += " abstract";

        if (methodSymbol.IsVirtual) result += " virtual";

        if (methodSymbol.IsStatic) result += " static";

        if (methodSymbol.IsOverride)
        {
            result += " override";
        }

        if (methodSymbol.ReturnsVoid)
        {
            result += " void";
        }
        else
        {
            result += " " + (methodSymbol.ReturnType as INamedTypeSymbol).GetFullTypeString();
        }

        result += " " + methodSymbol.Name + "(";

        var isFirstParameter = true;

        foreach (IParameterSymbol parameter in methodSymbol.Parameters)
        {
            if (isFirstParameter) isFirstParameter = false;
            else result += ", ";

            if (parameter.RefKind == RefKind.Out)
            {
                result += "out ";
            }
            else if (parameter.RefKind == RefKind.Ref)
            {
                result += "ref ";
            }

            var parameterTypeString =
                (parameter.Type as INamedTypeSymbol).GetFullTypeString();

            result += parameterTypeString;

            result += " " + parameter.Name;

            if (parameter.HasExplicitDefaultValue)
            {
                result += " = " + parameter.ExplicitDefaultValue.ToString();
            }
        }

        result += ")";

        return result;
    }

    public static object GetAttributeConstructorValueByParameterName(this AttributeData attributeData, string argName)
    {
        // Get the parameter
        var parameterSymbol = attributeData.AttributeConstructor
            .Parameters
            .Where((constructorParam) => constructorParam.Name == argName).FirstOrDefault();

        // get the index of the parameter
        var parameterIdx = attributeData.AttributeConstructor.Parameters.IndexOf(parameterSymbol);

        // get the construct argument corresponding to this parameter
        var constructorArg = attributeData.ConstructorArguments[parameterIdx];

        // return the value passed to the attribute
        return constructorArg.Value;
    }
}