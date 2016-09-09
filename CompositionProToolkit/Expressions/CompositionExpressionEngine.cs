// Copyright (c) 2016 Ratish Philip 
//
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions: 
// 
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software. 
// 
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE. 
//
// This file is part of the CompositionProToolkit project: 
// https://github.com/ratishphilip/CompositionProToolkit
//
// CompositionProToolkit v0.4.6
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Graphics.Effects;
using Windows.UI.Composition;

namespace CompositionProToolkit.Expressions
{
    #region Delegates

    public delegate T CompositionLambda<T>(CompositionExpressionContext<T> ctx);

    #endregion

    /// <summary>
    /// Converts an Expression to a string that can be used as an input
    /// for ExpressionAnimation and KeyFrameAnimation
    /// </summary>
    public abstract class CompositionExpressionEngine
    {
        #region Fields

        private static readonly Dictionary<ExpressionType, string> BinaryExpressionStrings;
        private static readonly IEnumerable<Type> ExpressionTypes;
        private static readonly Dictionary<Type, MethodInfo> VisitMethods;
        private static readonly Dictionary<Type, MethodInfo> ParseMethods;
        private static readonly Type[] Floatables;

        private static bool _noQuotesForConstant;
        private static bool _firstBinaryExpression;
        private static Dictionary<string, object> _parameters;
        private static bool _firstParseBinaryExpression;
        private static bool _noQuotesForParseConstant;

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Static constructor
        /// </summary>
        static CompositionExpressionEngine()
        {
            BinaryExpressionStrings = new Dictionary<ExpressionType, string>()
            {
                { ExpressionType.Add, "+" },
                { ExpressionType.AddChecked, "+" },
                { ExpressionType.And, "&" },
                { ExpressionType.AndAlso, "&&" },
                { ExpressionType.Coalesce, "??" },
                { ExpressionType.Divide, "/" },
                { ExpressionType.Equal, "==" },
                { ExpressionType.ExclusiveOr, "^" },
                { ExpressionType.GreaterThan, ">" },
                { ExpressionType.GreaterThanOrEqual, ">=" },
                { ExpressionType.LeftShift, "<<" },
                { ExpressionType.LessThan, "<" },
                { ExpressionType.LessThanOrEqual, "<=" },
                { ExpressionType.Modulo, "%" },
                { ExpressionType.Multiply, "*" },
                { ExpressionType.MultiplyChecked, "*" },
                { ExpressionType.NotEqual, "!=" },
                { ExpressionType.Or, "|" },
                { ExpressionType.OrElse, "||" },
                { ExpressionType.Power, "^" },
                { ExpressionType.RightShift, ">>" },
                { ExpressionType.Subtract, "-" },
                { ExpressionType.SubtractChecked, "-" },
                { ExpressionType.Assign, "=" },
                { ExpressionType.AddAssign, "+=" },
                { ExpressionType.AndAssign, "&=" },
                { ExpressionType.DivideAssign, "/=" },
                { ExpressionType.ExclusiveOrAssign, "^=" },
                { ExpressionType.LeftShiftAssign, "<<=" },
                { ExpressionType.ModuloAssign, "%=" },
                { ExpressionType.MultiplyAssign, "*=" },
                { ExpressionType.OrAssign, "|=" },
                { ExpressionType.PowerAssign, "^=" },
                { ExpressionType.RightShiftAssign, ">>=" },
                { ExpressionType.SubtractAssign, "-=" },
                { ExpressionType.AddAssignChecked, "+=" },
                { ExpressionType.MultiplyAssignChecked, "*=" },
                { ExpressionType.SubtractAssignChecked, "-=" },
            };

            Floatables = new[]{
                                  typeof(short),
                                  typeof(ushort),
                                  typeof(int),
                                  typeof(uint),
                                  typeof(long),
                                  typeof(ulong),
                                  typeof(char),
                                  typeof(double),
                                  typeof(bool),
                                  typeof(float),
                                  typeof(decimal)
                              };

            // Get all the types which derive from Expression or MemberBinding
            ExpressionTypes = typeof(Expression)
                                    .GetTypeInfo()
                                    .Assembly
                                    .GetTypes()
                                    .Where(t => t.IsSubclassOf(typeof(Expression))
                                            || t.IsSubclassOf(typeof(MemberBinding)));


            // Get all the Visit Methods
            VisitMethods = GetMethods("Visit");
            // Get all the Parse Methods
            ParseMethods = GetMethods("Parse");
        }

        /// <summary>
        /// Gets the methods having the given method name and whose first parameter 
        /// matches one of the types in ExpressionTypes.
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <returns>Dictionary of Expression Type and MethodInfo</returns>
        private static Dictionary<Type, MethodInfo> GetMethods(string methodName)
        {
            // Get all the private static Visit methods defined in the CompositionExpressionEngine
            var methods = typeof(CompositionExpressionEngine)
                                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                    .Where(m => m.Name == methodName);

            // Get the list of methods whose first parameter matches one of the types in ExpressionTypes
            return ExpressionTypes.Join(methods,
                                        t => t,                                  // Selector for Expression Types
                                        m => m.GetParameters()[0].ParameterType, // Selector for ExpressionEngine Methods
                                        (t, m) => new { Type = t, Method = m })  // Result Selector
                                  .ToDictionary(t => t.Type, t => t.Method);     // Convert to Dictionary<Type, MethodInfo>
        }

        #endregion

        #region APIs

        /// <summary>
        /// Converts the given Expression to a string that can be used as an input
        /// for ExpressionAnimation and KeyFrameAnimation
        /// </summary>
        /// <typeparam name="T">Type of the Expression</typeparam>
        /// <param name="expression">Expression</param>
        /// <returns>CompositionExpressionResult</returns>
        internal static CompositionExpressionResult CreateCompositionExpression<T>(Expression<CompositionLambda<T>> expression)
        {
            // Reset flags
            _noQuotesForConstant = false;
            _firstBinaryExpression = false;
            _parameters = new Dictionary<string, object>();

            var compositionExpr = new CompositionExpressionResult();
            // Visit the Expression Tree and convert it to string
            var expr = Visit(expression).ToString();
            compositionExpr.Expression = expr;
            // Obtain the parameters involved in the expression
            compositionExpr.Parameters = new Dictionary<string, object>(_parameters);

            return compositionExpr;
        }

        /// <summary>
        /// Parses the given expression and converts it into an appriopriate string.
        /// This is used in the following scenarios
        /// 1. To specify the animatable property on a CompositionObject derived object
        ///    for the StartAnimation and StopAnimation methods.
        /// 2. To specify an animatable property of type IGraphicEffect or IGraphicEffectSource
        ///    for the CreateEffectFactory method of Compositor.
        /// 3. To specify an animatable property of type IGraphicEffect or IGraphicEffectSource
        ///    for the StartAnimation and StopAnimation methods of a CompositionEffectBrush.
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>String</returns>
        internal static string ParseExpression(Expression<Func<object>> expression)
        {
            // Reset flags
            _noQuotesForParseConstant = false;
            _firstParseBinaryExpression = false;

            var exprString = Parse(expression).ToString();
            return exprString;
        }

        #endregion

        #region Visit methods

        /// <summary>
        /// Visits an Expression. This method acts as a router and
        /// based on the specific type of Expression, the appropriate
        /// Visit method is called.
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(Expression expression)
        {
            if (expression == null)
            {
                return new SimpleExpressionToken("null");
            }

            var baseType = expression.GetType();
            while (!baseType.IsPublic())
            {
                baseType = baseType.BaseType();
            }

            // Get the Visit method whose first parameter best matches the type of baseType
            MethodInfo methodInfo;
            if (VisitMethods.TryGetValue(baseType, out methodInfo) ||
                ((baseType.BaseType() != null) && VisitMethods.TryGetValue(baseType.BaseType(), out methodInfo)))
            {
                // Once a matching Visit method is found, Invoke it!
                return (ExpressionToken)methodInfo.Invoke(null, new object[] { expression });
            }

            return null;
        }

        /// <summary>
        /// Visits a BinaryExpression
        /// </summary>
        /// <param name="expression">BinaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(BinaryExpression expression)
        {
            // Is this an Array Index expression?
            if (expression.NodeType == ExpressionType.ArrayIndex)
            {
                return VisitArrayExpression(expression);
            }

            // Check if it is the outermost BinaryExpression
            // If yes, then no need to add round brackets to 
            // the whole visited expression
            var noBrackets = _firstBinaryExpression;
            if (_firstBinaryExpression)
            {
                // Set it to false so that the internal BinaryExpression(s)
                // will have round brackets
                _firstBinaryExpression = false;
            }

            var leftToken = Visit(expression.Left);
            var rightToken = Visit(expression.Right);

            string symbol;
            if (!BinaryExpressionStrings.TryGetValue(expression.NodeType, out symbol))
                return new SimpleExpressionToken("");

            // This check is done to avoid wrapping the final ExpressionToken 
            // in Round Brackets if the outermost expression is a BinaryExpression
            var bracketType = noBrackets ? BracketType.None : BracketType.Round;

            var token = new CompositeExpressionToken(bracketType);
            token.AddToken($"{leftToken} {symbol} {rightToken}");

            return token;
        }

        /// <summary>
        /// Visits a ConditionalExpression
        /// </summary>
        /// <param name="expression">ConditionalExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(ConditionalExpression expression)
        {
            var token = new CompositeExpressionToken();
            token.AddToken($"{Visit(expression.Test)} ? {Visit(expression.IfTrue)} : {Visit(expression.IfFalse)}");
            return token;
        }

        /// <summary>
        /// Visits a ConstantExpression
        /// </summary>
        /// <param name="expression">ConstantExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(ConstantExpression expression)
        {
            if (expression.Value == null)
                return new SimpleExpressionToken("null");

            var str = expression.Value as string;
            if (str != null)
                return new CompositeExpressionToken(str, _noQuotesForConstant ? BracketType.None : BracketType.Quotes);

            return new SimpleExpressionToken(expression.Value.ToString());
        }

        /// <summary>
        /// Visits a InvocationExpression
        /// </summary>
        /// <param name="expression">InvocationExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(InvocationExpression expression)
        {
            var token = new CompositeExpressionToken();
            token.AddToken("Invoke");
            // Visit each of the arguments
            token.AddToken(new CompositeExpressionToken(expression.Arguments.Select(Visit), BracketType.Round, true));
            return token;
        }

        /// <summary>
        /// Visits a LambdaExpression
        /// </summary>
        /// <param name="expression">LambdaExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(LambdaExpression expression)
        {
            var token = new CompositeExpressionToken();

            // ### Customized for Windows.UI.Composition ###
            // No need to print the parameter of type CompositionExpressionContext<T>
            if (!IsGenericCompositionExpressionContextType(expression.Parameters[0].Type))
            {
                // Parameter(s)
                var paramStr = string.Join(", ", expression.Parameters.Select(p => CleanIdentifier(p.Name)).ToArray());
                var bracketType = (expression.Parameters.Count == 1) ? BracketType.None : BracketType.Round;
                token.AddToken(new CompositeExpressionToken(paramStr, bracketType));

                // Arrow
                token.AddToken(" => ");
            }
            // If the parameter is of type CompositionExpressionContext<T> then it means 
            // that this is a CompositionLambda expression (i.e. First specific Visit). 
            // If the outermost Expression in the body of the CompositionLambda expression
            // is a BinaryExpression, then no need to add round brackets
            else if ((expression.Body as BinaryExpression) != null)
            {
                _firstBinaryExpression = true;
            }

            // Expression Body
            var bodyToken = Visit(expression.Body);
            if (bodyToken != null)
            {
                token.AddToken(bodyToken);
            }

            return token;
        }

        /// <summary>
        /// Visits a MemberExpression
        /// </summary>
        /// <param name="expression">MemberExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MemberExpression expression)
        {
            // ### Customized for Windows.UI.Composition ###
            // Check if this expression is accessing the StartingValue or FinalValue
            // Property of CompositionExpressionContext<T>
            if (((expression.Member as PropertyInfo) != null) &&
                (expression.Expression != null) &&
                IsGenericCompositionExpressionContextType(expression.Expression.Type))
            {
                return new SimpleExpressionToken($"this.{expression.Member.Name}");
            }

            // This check is for CompositionPropertySet. It has a property called 
            // Properties which is of type CompositionPropertySet. So while converting to string, 'Properties' 
            // need not be printed 
            if (((expression.Member as PropertyInfo) != null) &&
                (expression.Type == typeof(CompositionPropertySet) && (expression.Member.Name == "Properties"))
                && (expression.Expression is MemberExpression) && (expression.Expression.Type == typeof(CompositionPropertySet)))
            {
                return Visit(expression.Expression);
            }

            // If the expression is of type CompositionPropertySet, then no need to 
            // visit this expression tree further. Just add this CompositionPropertySet
            // to the _parameters dictionary (if it doesn't already exist) and return
            // the name of the expression member
            if (expression.Type == typeof(CompositionPropertySet))
            {
                if (!_parameters.ContainsKey(expression.Member.Name) &&
                    expression.Expression is ConstantExpression)
                {
                    if ((expression.Member as FieldInfo) != null)
                    {
                        _parameters.Add(expression.Member.Name, ((FieldInfo)expression.Member).GetValue(((ConstantExpression)expression.Expression).Value));
                    }
                    else if ((expression.Member as PropertyInfo) != null)
                    {
                        _parameters.Add(expression.Member.Name, ((PropertyInfo)expression.Member).GetValue(((ConstantExpression)expression.Expression).Value));
                    }
                }

                return new SimpleExpressionToken(expression.Member.Name);
            }

            // Check if the parent of this expression has a name which starts with CS$<
            var parentMemberExpr = expression.Expression as MemberExpression;
            if ((parentMemberExpr != null) &&
                parentMemberExpr.Member.Name.StartsWith("CS$<", StringComparison.Ordinal))
            {
                // ### Customized for Windows.UI.Composition ###
                // Add to the parameters dictionary
                if (!_parameters.ContainsKey(expression.Member.Name)
                    && (parentMemberExpr.Expression as ConstantExpression) != null)
                {
                    var constantExpr = (ConstantExpression)parentMemberExpr.Expression;

                    if ((parentMemberExpr.Member as FieldInfo) != null)
                    {
                        var localFieldValue = ((FieldInfo)parentMemberExpr.Member).GetValue(constantExpr.Value);
                        if ((expression.Member as FieldInfo) != null)
                        {
                            _parameters.Add(expression.Member.Name, ((FieldInfo)expression.Member).GetValue(localFieldValue));
                        }
                        else if ((expression.Member as PropertyInfo) != null)
                        {
                            _parameters.Add(expression.Member.Name, ((PropertyInfo)expression.Member).GetValue(localFieldValue));
                        }
                    }
                    else if ((parentMemberExpr.Member as PropertyInfo) != null)
                    {
                        var localFieldValue = ((PropertyInfo)parentMemberExpr.Member).GetValue(constantExpr.Value);
                        if ((expression.Member as FieldInfo) != null)
                        {
                            _parameters.Add(expression.Member.Name, ((FieldInfo)expression.Member).GetValue(localFieldValue));
                        }
                        else if ((expression.Member as PropertyInfo) != null)
                        {
                            _parameters.Add(expression.Member.Name, ((PropertyInfo)expression.Member).GetValue(localFieldValue));
                        }
                    }
                }

                return new SimpleExpressionToken(expression.Member.Name);
            }

            var token = new CompositeExpressionToken();
            var constExpr = expression.Expression as ConstantExpression;
            if ((constExpr?.Value != null)
                && constExpr.Value.GetType().IsNested
                && constExpr.Value.GetType().Name.StartsWith("<", StringComparison.Ordinal))
            {
                // ### Customized for Windows.UI.Composition ###
                // Add to the parameters dictionary
                if (!_parameters.ContainsKey(expression.Member.Name))
                {
                    if ((expression.Member as FieldInfo) != null)
                    {
                        _parameters.Add(expression.Member.Name, ((FieldInfo)expression.Member).GetValue(constExpr.Value));
                    }
                    else if ((expression.Member as PropertyInfo) != null)
                    {
                        _parameters.Add(expression.Member.Name, ((PropertyInfo)expression.Member).GetValue(constExpr.Value));
                    }
                }

                return new SimpleExpressionToken(expression.Member.Name);
            }

            if (expression.Expression != null)
            {
                token.AddToken(Visit(expression.Expression));
            }
            else
            {
                token.AddToken(expression.Member.DeclaringType.Name);
            }

            token.AddToken($".{CleanIdentifier(expression.Member.Name)}");

            return token;
        }

        /// <summary>
        /// Visits a MethodCallExpression
        /// </summary>
        /// <param name="expression">MethodCallExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MethodCallExpression expression)
        {
            var isExtensionMethod = expression.Method.IsDefined(typeof(ExtensionAttribute));
            var methodName = expression.Method.Name;

            var token = new CompositeExpressionToken();
            // If this is an extension method
            if (isExtensionMethod)
            {
                // ### Customized for Windows.UI.Composition ###
                // If the .Single() extension method is being called on a System.Double
                // value, no need to print it.
                if (expression.Method.DeclaringType == typeof(DoubleExtensions))
                {
                    token.AddToken(Visit(expression.Arguments[0]));
                }
                // If the extension method being called belongs to CompositionPropertySetExtensions
                // then no need to add the method name
                else if (expression.Method.DeclaringType == typeof(CompositionPropertySetExtensions))
                {
                    token.AddToken(Visit(expression.Arguments[0]));
                    token.AddToken(".");
                    _noQuotesForConstant = true;
                    token.AddToken(Visit(expression.Arguments[1]));
                    _noQuotesForConstant = false;
                }
                else
                {
                    token.AddToken(Visit(expression.Arguments[0]));
                    token.AddToken($".{methodName}");
                    token.AddToken(new CompositeExpressionToken(expression.Arguments.Skip(1).Select(Visit), BracketType.Round,
                        true));
                }
            }
            else
            {
                var showDot = true;
                if (expression.Object == null)
                {
                    token.AddToken(expression.Method.DeclaringType.FormattedName());
                }
                // ### Customized for Windows.UI.Composition ###
                // No need to print the object name if the object is of type CompositionExpressionContext<T>
                else if (IsGenericCompositionExpressionContextType(expression.Object.Type))
                {
                    showDot = false;
                }
                else
                {
                    token.AddToken(Visit(expression.Object));
                }

                if (expression.Method.IsSpecialName &&
                    (expression.Method.DeclaringType.GetProperties()
                        .FirstOrDefault(p => p.GetAccessors().Contains(expression.Method)) != null))
                {
                    token.AddToken(new CompositeExpressionToken(expression.Arguments.Select(Visit), BracketType.Square, true));
                }
                else
                {
                    token.AddToken((showDot ? "." : "") + expression.Method.Name);
                    token.AddToken(new CompositeExpressionToken(expression.Arguments.Select(Visit), BracketType.Round, true));
                }
            }

            return token;
        }

        /// <summary>
        /// Visits a ParameterExpression
        /// </summary>
        /// <param name="expression">ParameterExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(ParameterExpression expression)
        {
            var name = expression.Name ?? "<param>";
            return new SimpleExpressionToken(CleanIdentifier(name));
        }

        /// <summary>
        /// Visits a TypeBinaryExpression
        /// </summary>
        /// <param name="expression">TypeBinaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(TypeBinaryExpression expression)
        {
            var token = new CompositeExpressionToken(BracketType.Round);
            token.AddToken($"{Visit(expression.Expression)} is {expression.TypeOperand.Name}");
            return token;
        }

        /// <summary>
        /// Visits a UnaryExpression
        /// </summary>
        /// <param name="expression">UnaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Quote)
            {
                return Visit(expression.Operand);
            }

            var token = new CompositeExpressionToken();
            var suffix = string.Empty;
            var bracketsRequired = true;

            switch (expression.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    if (expression.Operand.Type.IsSubclassOf(expression.Type))
                        return Visit(expression.Operand);
                    // ### Customized for Windows.UI.Composition ###
                    // Don't add a cast for any of the types in Floatables
                    if (Floatables.Contains(expression.Type))
                    {
                        bracketsRequired = false;
                    }
                    else
                    {
                        token.AddToken(new CompositeExpressionToken(expression.Type.Name, BracketType.Round));
                    }
                    break;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    token.AddToken("-");
                    break;
                case ExpressionType.UnaryPlus:
                    token.AddToken("+");
                    break;
                case ExpressionType.Not:
                    token.AddToken("!");
                    break;
                case ExpressionType.PreIncrementAssign:
                    token.AddToken("++");
                    break;
                case ExpressionType.PreDecrementAssign:
                    token.AddToken("--");
                    break;
                case ExpressionType.TypeAs:
                    token.AddToken(Visit(expression.Operand));
                    token.AddToken(" as ");
                    token.AddToken(expression.Type.Name);
                    return token;
                case ExpressionType.OnesComplement:
                    token.AddToken("~");
                    break;
                case ExpressionType.PostIncrementAssign:
                    suffix = "++";
                    break;
                case ExpressionType.PostDecrementAssign:
                    suffix = "--";
                    break;
                default:
                    token.AddToken(expression.NodeType.ToString());
                    break;
            }

            // Visit the operand
            var operandToken = Visit(expression.Operand);
            var compToken = operandToken as CompositeExpressionToken;
            // If there are more the one tokens in the CompositeExpressionToken
            // then wrap them with Round brackets
            if (bracketsRequired && (compToken?.TokenCount() > 1))
            {
                compToken.SetBrackets(BracketType.Round);
            }

            token.AddToken(operandToken);

            // Is suffix non-empty?
            if (!string.IsNullOrWhiteSpace(suffix))
            {
                token.AddToken(suffix);
            }

            return token;
        }

        /// <summary>
        /// Visits a MemberAssignment
        /// </summary>
        /// <param name="mb">MemberAssignment</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MemberAssignment mb)
        {
            var token = new CompositeExpressionToken();
            token.AddToken($"{CleanIdentifier(mb.Member.Name)} = {Visit(mb.Expression)}");
            return token;
        }

        /// <summary>
        /// Visits a ListInitExpression
        /// </summary>
        /// <param name="expression">ListInitExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(ListInitExpression expression)
        {
            // Not supported right now
            return null;
        }

        /// <summary>
        /// Visits a MemberInitExpression
        /// </summary>
        /// <param name="expression">MemberInitExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MemberInitExpression expression)
        {
            // Not supported right now
            return null;
        }

        /// <summary>
        /// Visits a MemberListBinding
        /// </summary>
        /// <param name="mb">MemberListBinding</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MemberListBinding mb)
        {
            // Not supported right now
            return null;
        }

        /// <summary>
        /// Visits a MemberMemberBinding
        /// </summary>
        /// <param name="mb">MemberMemberBinding</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MemberMemberBinding mb)
        {
            // Not supported right now
            return null;
        }

        /// <summary>
        /// Visits a NewArrayExpression
        /// </summary>
        /// <param name="expression">NewArrayExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(NewArrayExpression expression)
        {
            // Not supported right now
            return null;
        }

        /// <summary>
        /// Visits a NewExpression
        /// </summary>
        /// <param name="expression">NewExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(NewExpression expression)
        {
            // Not supported right now
            return null;
        }

        /// <summary>
        /// Visits a BinaryExpression in which an array index is being accessed
        /// </summary>
        /// <param name="expression">BinaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken VisitArrayExpression(BinaryExpression expression)
        {
            // In a BinaryExpression for ArrayIndex, the Left expression will be the
            // MemberExpression to access the array object and the Right expression will
            // be the index number or string.
            var arrayExpr = expression.Left as MemberExpression;
            var arrayName = arrayExpr?.Member.Name;
            // Get the array index
            var index = VisitArrayIndex(expression.Right);
            // Is a valid index obtained?
            if (index == -1)
            {
                throw new ArgumentException($"Invalid value parsed for the index of the '{arrayName}' array!");
            }

            // Generate key name to insert into the _parameters dictionary
            var paramName = $"{arrayName}_{index}";

            if ((!_parameters.ContainsKey(paramName)) && ((arrayExpr?.Expression as ConstantExpression) != null))
            {
                // Is the array a field?
                if ((arrayExpr.Member as FieldInfo) != null)
                {
                    // Get the element in the 'index' position in the array
                    var arrayValue = ((FieldInfo)arrayExpr.Member).GetValue(((ConstantExpression)arrayExpr.Expression).Value);
                    if (arrayValue.GetType().IsArray)
                    {
                        var array = arrayValue as Array;
                        if (array != null)
                        {
                            // Add to the _parameters dictionary
                            _parameters.Add(paramName, array.GetValue(index));
                        }
                    }
                }
                // Or is the array a Property?
                else if ((arrayExpr.Member as PropertyInfo) != null)
                {
                    // Get the element in the 'index' position in the array
                    var arrayValue =
                        ((PropertyInfo)arrayExpr.Member).GetValue(((ConstantExpression)arrayExpr.Expression).Value);
                    if (arrayValue.GetType().IsArray)
                    {
                        var array = arrayValue as Array;
                        if (array != null)
                        {
                            // Add to the _parameters dictionary
                            _parameters.Add(paramName, array.GetValue(index));
                        }
                    }
                }
            }

            return new SimpleExpressionToken(paramName);
        }

        /// <summary>
        /// Extracts the array index value as an integer. The expression can be either
        /// a ConstantExpression or a MemberExpression. It is usually the right-side 
        /// expression of a BinaryExpression in which an array index is being accessed.
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>Index</returns>
        private static int VisitArrayIndex(Expression expression)
        {
            var result = -1;

            // Is it a ConstantExpression
            if ((expression as ConstantExpression) != null)
            {
                result = (int)((ConstantExpression)expression).Value;
            }
            // Or is it a MemberExpression
            else if ((expression as MemberExpression) != null)
            {
                var membExpr = (MemberExpression)expression;
                var constExpr = membExpr.Expression as ConstantExpression;

                if (constExpr?.Value != null)
                {
                    if ((membExpr.Member as FieldInfo) != null)
                    {
                        result = (int)((FieldInfo)membExpr.Member).GetValue(constExpr.Value);
                    }
                    else if ((membExpr.Member as PropertyInfo) != null)
                    {
                        result = (int)((PropertyInfo)membExpr.Member).GetValue(constExpr.Value);
                    }
                }
            }

            return result;
        }

        #endregion

        #region Parse Methods

        /// <summary>
        /// Visits an Expression. This method acts as a router and
        /// based on the specific type of Expression, the appropriate
        /// Visit method is called.
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Parse(Expression expression)
        {
            if (expression == null)
            {
                return new SimpleExpressionToken("null");
            }

            var baseType = expression.GetType();
            while (!baseType.IsPublic())
            {
                baseType = baseType.BaseType();
            }

            // Get the Visit method whose first parameter best matches the type of baseType
            MethodInfo methodInfo;
            if (ParseMethods.TryGetValue(baseType, out methodInfo) ||
                ((baseType.BaseType() != null) && ParseMethods.TryGetValue(baseType.BaseType(), out methodInfo)))
            {
                // Once a matching Visit method is found, Invoke it!
                return (ExpressionToken)methodInfo.Invoke(null, new object[] { expression });
            }

            return null;
        }

        /// <summary>
        /// Visits a BinaryExpression
        /// </summary>
        /// <param name="expression">BinaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Parse(BinaryExpression expression)
        {
            // If it the property of an object, (deriving from CompositionObject)
            // which is a part of an array of similar objects, is being 
            // accessed, then no need to further parse this expression
            if ((expression.NodeType == ExpressionType.ArrayIndex) &&
                (expression.Type.IsSubclassOf(typeof(CompositionObject))))
            {
                return null;
            }

            // Check if it is the outermost BinaryExpression
            // If yes, then no need to add round brackets to 
            // the whole visited expression
            var noBrackets = _firstParseBinaryExpression;
            if (_firstParseBinaryExpression)
            {
                // Set it to false so that the internal BinaryExpression(s)
                // will have round brackets
                _firstParseBinaryExpression = false;
            }

            var leftToken = Parse(expression.Left);
            var rightToken = Parse(expression.Right);

            string symbol;
            if (!BinaryExpressionStrings.TryGetValue(expression.NodeType, out symbol))
                return null;

            // This check is done to avoid wrapping the final ExpressionToken in Round Brackets
            var bracketType = noBrackets ? BracketType.None : BracketType.Round;

            var token = new CompositeExpressionToken(bracketType);
            token.AddToken($"{leftToken} {symbol} {rightToken}");

            return token;
        }

        /// <summary>
        /// Visits a ConstantExpression
        /// </summary>
        /// <param name="expression">ConstantExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Parse(ConstantExpression expression)
        {
            if (expression.Value == null)
                return new SimpleExpressionToken("null");

            var str = expression.Value as string;
            if (str != null)
                return new CompositeExpressionToken(str, _noQuotesForParseConstant ? BracketType.None : BracketType.Quotes);

            return new SimpleExpressionToken(expression.Value.ToString());
        }

        /// <summary>
        /// Visits a LambdaExpression
        /// </summary>
        /// <param name="expression">LambdaExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Parse(LambdaExpression expression)
        {
            if ((expression.Body as BinaryExpression) != null)
            {
                _firstParseBinaryExpression = true;
            }

            // Expression Body
            return Parse(expression.Body);
        }

        /// <summary>
        /// Visits a MemberExpression
        /// </summary>
        /// <param name="expression">MemberExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Parse(MemberExpression expression)
        {
            // ### Customized for Windows.UI.Composition ###
            // If the the expression type is a class implementing the IGraphicEffect interface or
            // the IGraphicEffectSource interface and the instance object of this expression has
            // a Name property, then return the value of Name property of this class and 
            // proceed no further in the expression. 
            if (typeof(IGraphicsEffect).IsAssignableFrom(expression.Type) ||
                typeof(IGraphicsEffectSource).IsAssignableFrom(expression.Type))
            {
                // Get the instance object for the expression
                var sourceObj = GetObject(expression);
                // Get its Name property value
                var name = sourceObj?.GetPropertyValue("Name") as string;

                return String.IsNullOrWhiteSpace(name) ? null : new SimpleExpressionToken(name);
            }

            // Check if this expression is accessing the StartingValue or FinalValue
            // Property of CompositionExpressionContext<T>
            if (((expression.Member as PropertyInfo) != null) &&
                (expression.Expression != null) &&
                IsGenericCompositionExpressionContextType(expression.Expression.Type))
            {
                return new SimpleExpressionToken($"this.{expression.Member.Name}");
            }

            // This check is for CompositionPropertySet. It has a property called 
            // Properties which is of type CompositionPropertySet. So while converting to string, 'Properties' 
            // need not be printed 
            if (((expression.Member as PropertyInfo) != null) &&
                (expression.Type == typeof(CompositionPropertySet) && (expression.Member.Name == "Properties"))
                && (expression.Expression is MemberExpression) && (expression.Expression.Type == typeof(CompositionPropertySet)))
            {
                return Parse(expression.Expression);
            }

            // If the expression type is a subclass of CompositionObject, no need to proceed further
            // because in one of the scenarios, the ParseExpression API is called on an object which
            // is a subclass of CompositionObject and we just need the property access string.
            if (expression.Type.IsSubclassOf(typeof(CompositionObject)))
                return null;

            var token = new CompositeExpressionToken();
            // Visit the parent
            var parent = expression.Expression != null ?
                        Parse(expression.Expression) :
                        new SimpleExpressionToken(expression.Member.DeclaringType.Name);

            if (parent != null)
            {
                token.AddToken(parent);
                token.AddToken($".{CleanIdentifier(expression.Member.Name)}");
            }
            else
            {
                token.AddToken(CleanIdentifier(expression.Member.Name));
            }

            return token;
        }

        /// <summary>
        /// Visits a MethodCallExpression
        /// </summary>
        /// <param name="expression">MethodCallExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Parse(MethodCallExpression expression)
        {
            var isExtensionMethod = expression.Method.IsDefined(typeof(ExtensionAttribute));
            var methodName = expression.Method.Name;

            var token = new CompositeExpressionToken();
            // If this is an extension method
            if (isExtensionMethod)
            {
                // ### Customized for Windows.UI.Composition ###
                // If the .Single() extension method is being called on a System.Double
                // value, no need to print it.
                if (expression.Method.DeclaringType == typeof(DoubleExtensions))
                {
                    token.AddToken(Parse(expression.Arguments[0]));
                }
                // If the extension method being called belongs to CompositionPropertySetExtensions
                // then no need to add the method name
                else if (expression.Method.DeclaringType == typeof(CompositionPropertySetExtensions))
                {
                    var parent = Parse(expression.Arguments[0]);
                    if (parent != null)
                    {
                        token.AddToken(parent);
                        token.AddToken(".");
                        _noQuotesForParseConstant = true;
                        token.AddToken(Parse(expression.Arguments[1]));
                        _noQuotesForParseConstant = false;
                    }
                }
                else
                {
                    var parent = Parse(expression.Arguments[0]);
                    if (parent != null)
                    {
                        token.AddToken(parent);
                        token.AddToken($".{methodName}");
                    }
                    else
                    {
                        // ### Customized for Windows.UI.Composition ###
                        // Special Case: If the extension method name is ScaleXY,
                        // then 'Scale.XY' must be the string returned.
                        if (methodName == "ScaleXY")
                        {
                            methodName = "Scale.XY";
                        }

                        token.AddToken(methodName);
                    }
                }
            }
            else
            {
                var showDot = true;
                if (expression.Object == null)
                {
                    token.AddToken(expression.Method.DeclaringType.FormattedName());
                }
                // ### Customized for Windows.UI.Composition ###
                // No need to print the object name if the object derives from CompositionObject
                else if (expression.Type.IsSubclassOf(typeof(CompositionObject)))
                {
                    showDot = false;
                }
                else
                {
                    token.AddToken(Parse(expression.Object));
                }

                // Is it an array index based access?
                if (expression.Method.IsSpecialName &&
                    (expression.Method.DeclaringType.GetProperties()
                        .FirstOrDefault(p => p.GetAccessors().Contains(expression.Method)) != null))
                {
                    token.AddToken(new CompositeExpressionToken(expression.Arguments.Select(Parse), BracketType.Square, true));
                }
                else
                {
                    token.AddToken(showDot ? $".{expression.Method.Name}" : expression.Method.Name);
                }
            }

            return token;
        }

        /// <summary>
        /// Visits a ParameterExpression
        /// </summary>
        /// <param name="expression">ParameterExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Parse(ParameterExpression expression)
        {
            var name = expression.Name ?? "<param>";
            return new SimpleExpressionToken(CleanIdentifier(name));
        }

        /// <summary>
        /// Visits a UnaryExpression
        /// </summary>
        /// <param name="expression">UnaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Parse(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Quote)
            {
                return Parse(expression.Operand);
            }

            var token = new CompositeExpressionToken();

            switch (expression.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.TypeAs:
                    token.AddToken(Parse(expression.Operand));
                    break;
                default:
                    return null;
            }

            return token;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Converts identifier name to a more readable format 
        /// </summary>
        /// <param name="name">Identifier Name</param>
        /// <returns>Formatted Name</returns>
        private static string CleanIdentifier(string name)
        {
            if (name == null)
                return null;
            if (name.StartsWith("<>h__TransparentIdentifier", StringComparison.Ordinal))
                return "temp_" + name.Substring(26);
            return name;
        }

        /// <summary>
        /// Checks if the given type of of type CompositionExpressionContext&lt;T&gt;
        /// </summary>
        /// <param name="inputType">Type to check</param>
        /// <returns>True of type matches otherwise false</returns>
        private static bool IsGenericCompositionExpressionContextType(Type inputType)
        {
            if ((inputType == null) ||
                (!inputType.IsGenericType()) ||
                (!inputType.GenericTypeArguments.Any()))
                return false;

            var paramType = inputType.GenericTypeArguments[0];
            return (inputType == typeof(CompositionExpressionContext<>).MakeGenericType(paramType));
        }

        /// <summary>
        /// Gets the instance object for the given expression
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>Object</returns>
        public static object GetObject(Expression expression)
        {
            var memberStack = new Stack<MemberInfo>();

            // Move from leaf expression to the root expression
            // as long as it is a MemberExpression or 
            // UnaryExpression(convert or cast operations only)
            do
            {
                if ((expression as MemberExpression) != null)
                {
                    var memberExpr = expression as MemberExpression;
                    memberStack.Push(memberExpr.Member);
                    expression = memberExpr.Expression;
                }
                else if ((expression as UnaryExpression) != null)
                {
                    if ((expression.NodeType == ExpressionType.Convert) ||
                        (expression.NodeType == ExpressionType.ConvertChecked) ||
                        (expression.NodeType == ExpressionType.TypeAs))
                    {
                        expression = ((UnaryExpression)expression).Operand;
                    }
                    else
                    {
                        // Any other type of Expression is not supported right now!
                        throw new ArgumentException(
                            "This expression is not supported in CompositionExpressionToolkit!", nameof(expression));
                    }
                }
            }
            while ((expression is MemberExpression) || (expression is UnaryExpression));

            var resultObject = (object)null;
            // If the expression is null then it means the root is a static member
            if (expression == null)
            {
                var memberInfo = memberStack.Pop();
                if ((memberInfo as PropertyInfo) != null)
                {
                    // Property will be (public or non-public) and static
                    resultObject = memberInfo.DeclaringType
                                             .GetProperty(memberInfo.Name, BindingFlags.NonPublic |
                                                                           BindingFlags.Public |
                                                                           BindingFlags.Static)
                                             ?.GetValue(null);
                }
                else if ((memberInfo as FieldInfo) != null)
                {
                    // Field will be (public or non-public) and static
                    var flags = BindingFlags.Static;
                    flags |= ((FieldInfo)memberInfo).IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;

                    resultObject = memberInfo.DeclaringType
                                             .GetField(memberInfo.Name, flags)
                                             ?.GetValue(null);
                }
            }
            // If the root is a ConstantExpression, then its Value represents the root object
            else if ((expression as ConstantExpression) != null)
            {
                var constExpr = expression as ConstantExpression;
                resultObject = constExpr.Value;
            }
            else
            {
                // Any other type of Expression is not supported right now!
                throw new ArgumentException(
                            "This expression is not supported in CompositionExpressionToolkit!", nameof(expression));
            }

            // Move from root member info to the leaf member info and get the 
            // corresponding object for each
            while (memberStack.Any() && (resultObject != null))
            {
                var mi = memberStack.Pop();
                if ((mi as PropertyInfo) != null)
                {
                    // Property can be (public or non-public) and (static or instance)
                    resultObject = resultObject.GetType()
                                               .GetProperty(mi.Name, BindingFlags.Static | BindingFlags.Instance | 
                                                                     BindingFlags.NonPublic | BindingFlags.Public)
                                               ?.GetValue(resultObject, null);
                }
                else if ((mi as FieldInfo) != null)
                {
                    // Field can be (public or non-public) and (static or instance)
                    var fieldInfo = mi as FieldInfo;
                    var flags = fieldInfo.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
                    flags |= fieldInfo.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;

                    resultObject = resultObject.GetType()
                                               .GetField(mi.Name, flags)
                                               ?.GetValue(resultObject);
                }
            }

            return resultObject;
        }

        #endregion
    }
}
