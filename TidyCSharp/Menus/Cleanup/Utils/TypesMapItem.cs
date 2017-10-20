using System;
using System.Linq;
using System.CodeDom;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CSharp;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class TypesMapItem
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string BuiltInName { get; set; }

        Type _builtInType = null;
        public Type BuiltInType
        {
            get
            {
                if (_builtInType != null) return _builtInType;
                if (string.IsNullOrEmpty(BuiltInName)) return _builtInType;

                _builtInType = Type.GetType(FullName);

                GetTypeDefaultValue();

                return _builtInType;
            }
        }

        object _defaultValue = null;
        public object DefaultValue
        {
            get
            {
                return GetTypeDefaultValue();
            }
        }

        private object GetTypeDefaultValue()
        {
            if (BuiltInType == null) return null;
            if (_defaultValue != null) return _defaultValue;

            if (BuiltInType.IsValueType)
            {
                _defaultValue = Activator.CreateInstance(BuiltInType);
            }

            return _defaultValue;
        }

        public TypeSyntax NewNode { get; set; }


        internal static TypesMapItem GetBuiltInTypes(Type type, TypeSyntax node, CSharpCodeProvider provider)
        {
            return new TypesMapItem
            {
                Name = type.Name,
                FullName = type.FullName,
                BuiltInName = provider.GetTypeOutput(new CodeTypeReference(type)),
                NewNode = node
            };
        }
        internal static TypeSyntax GetPredefineType(SyntaxKind keyword)
        {
            return SyntaxFactory.PredefinedType(SyntaxFactory.Token(keyword));
        }

        static Dictionary<string, TypesMapItem> BuiltInTypesDic;
        public static Dictionary<string, TypesMapItem> GetBuiltInTypesDic()
        {
            if (BuiltInTypesDic != null) return BuiltInTypesDic;

            var output = new Dictionary<string, TypesMapItem>();

            using (var provider = new CSharpCodeProvider())
            {
                var typesList = new TypesMapItem[]
                {
                    GetBuiltInTypes(Type.GetType("System.Boolean"), GetPredefineType(SyntaxKind.BoolKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.Byte"), GetPredefineType(SyntaxKind.ByteKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.SByte"), GetPredefineType(SyntaxKind.SByteKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.Char"), GetPredefineType(SyntaxKind.CharKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.Decimal"), GetPredefineType(SyntaxKind.DecimalKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.Double"), GetPredefineType(SyntaxKind.DoubleKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.Single"), GetPredefineType(SyntaxKind.FloatKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.Int32"), GetPredefineType(SyntaxKind.IntKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.UInt32"), GetPredefineType(SyntaxKind.UIntKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.Int64"), GetPredefineType(SyntaxKind.LongKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.UInt64"), GetPredefineType(SyntaxKind.ULongKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.Object"), GetPredefineType(SyntaxKind.ObjectKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.Int16"), GetPredefineType(SyntaxKind.ShortKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.UInt16"), GetPredefineType(SyntaxKind.UShortKeyword), provider),
                    GetBuiltInTypes(Type.GetType("System.String"), GetPredefineType(SyntaxKind.StringKeyword), provider),
                };

                foreach (var item in typesList)
                {
                    output.Add(item.Name, item);
                    output.Add(item.FullName, item);
                }

                return BuiltInTypesDic = output;
            }
        }


        static Dictionary<string, TypesMapItem> _predefinedTypesDic;
        internal static Dictionary<string, TypesMapItem> GetAllPredefinedTypesDic()
        {
            if (_predefinedTypesDic != null) return _predefinedTypesDic;

            var output = GetBuiltInTypesDic();

            using (var provider = new CSharpCodeProvider())
            {
                var oldValues = output.Values.GroupBy(x => x.BuiltInName).ToList();
                foreach (var item0 in oldValues)
                {
                    var item = item0.First();

                    output.Add(item.BuiltInName, new TypesMapItem { BuiltInName = item.BuiltInName, Name = item.BuiltInName, FullName = item.FullName });
                }


                return _predefinedTypesDic = output;
            }
        }
    }

}