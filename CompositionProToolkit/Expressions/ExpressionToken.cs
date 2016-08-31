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
// CompositionProToolkit v0.4.5
//

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompositionProToolkit.Expressions
{
    /// <summary>
    /// Defines the various types of brackets that
    /// can be used in the expression string
    /// </summary>
    internal enum BracketType
    {
        None = 0,               //
        Round = 1,              // ( )
        Square = 2,             // [ ]
        Curly = 3,              // { }
        Angle = 4,              // < >
        Apostrophe = 5,         // ' '
        Quotes = 6,             // " "
        AngleXml = 7,           // &lt; &gt;
        ApostropheXml = 8,      // &apos; &apos;
        QuotesXml = 9           // &quot; &quot;
    }

    /// <summary>
    /// Represents the base class for tokens, which are
    /// the building blocks for generating the string
    /// from an Expression
    /// </summary>
    internal abstract class ExpressionToken
    {
        #region APIs

        public abstract void Write(StringBuilder sb);

        #endregion

        #region Overrides

        public override string ToString()
        {
            var sb = new StringBuilder();
            Write(sb);
            return sb.ToString();
        }

        #endregion
    }

    /// <summary>
    /// An expression token which encapsulates a string
    /// </summary>
    internal class SimpleExpressionToken : ExpressionToken
    {
        #region Fields

        public readonly string Text;

        #endregion

        #region Construction / Initialization

        public SimpleExpressionToken(string text)
        {
            Text = text;
        }

        #endregion

        #region Overrides

        public override void Write(StringBuilder sb)
        {
            sb.Append(Text);
        }

        #endregion
    }

    /// <summary>
    /// An expression token which is a container for expression tokens
    /// </summary>
    internal class CompositeExpressionToken : ExpressionToken
    {
        #region Fields

        private string _openBracket;
        private string _closeBracket;
        private BracketType _bracketType;
        private readonly bool _addCommas;
        private readonly List<ExpressionToken> _tokens;

        #endregion

        #region Construction / Initialization

        public CompositeExpressionToken(BracketType bracketType = BracketType.None,
                    bool addCommas = false)
        {
            SetBrackets(bracketType);
            _addCommas = addCommas;
            _tokens = new List<ExpressionToken>();
        }

        public CompositeExpressionToken(string tokenStr, BracketType bracketType)
        : this(bracketType)
        {
            AddToken(tokenStr);
        }

        public CompositeExpressionToken(ExpressionToken expressionToken, BracketType bracketType)
            : this(bracketType)
        {
            AddToken(expressionToken);
        }

        public CompositeExpressionToken(IEnumerable<ExpressionToken> tokens, BracketType bracketType, bool addCommas)
            : this(bracketType, addCommas)
        {
            _tokens.AddRange(tokens.Where(t => t != null));
        }

        #endregion

        #region APIs

        public void SetBrackets(BracketType bracketType)
        {
            _bracketType = bracketType;
            switch (bracketType)
            {
                case BracketType.Round:
                    _openBracket = "(";
                    _closeBracket = ")";
                    break;
                case BracketType.Square:
                    _openBracket = "[";
                    _closeBracket = "]";
                    break;
                case BracketType.Curly:
                    _openBracket = "{";
                    _closeBracket = "}";
                    break;
                case BracketType.Angle:
                    _openBracket = "<";
                    _closeBracket = ">";
                    break;
                case BracketType.Apostrophe:
                    _openBracket = "\'";
                    _closeBracket = "\'";
                    break;
                case BracketType.Quotes:
                    _openBracket = "\"";
                    _closeBracket = "\"";
                    break;
                case BracketType.AngleXml:
                    _openBracket = "&lt;";
                    _closeBracket = "&gt;";
                    break;
                case BracketType.ApostropheXml:
                    _openBracket = "&apos;";
                    _closeBracket = "&apos";
                    break;
                case BracketType.QuotesXml:
                    _openBracket = "&quot;";
                    _closeBracket = "&quot;";
                    break;
                default:
                    _openBracket = "";
                    _closeBracket = "";
                    break;
            }
        }

        public void AddToken(string tokenStr)
        {
            if (!string.IsNullOrWhiteSpace(tokenStr))
            {
                AddToken(new SimpleExpressionToken(tokenStr));
            }
        }

        public void AddToken(string tokenStr, BracketType bracketType)
        {
            AddToken(new CompositeExpressionToken(tokenStr, bracketType));
        }

        public void AddToken<T>(T token) where T : ExpressionToken
        {
            _tokens.Add(token);
        }

        public int TokenCount()
        {
            return _tokens.Count(t => t != null);
        }

        #endregion

        #region Overrides

        public override void Write(StringBuilder sb)
        {
            var flag = (_bracketType != BracketType.None);
            if (flag)
            {
                sb.Append(_openBracket);
            }

            var first = true;
            foreach (var token in _tokens.Where(t => t != null))
            {
                if (first)
                    first = false;
                else if (_addCommas)
                    sb.Append(", ");

                token.Write(sb);
            }

            if (flag)
            {
                sb.Append(_closeBracket);
            }
        }

        #endregion
    }
}
