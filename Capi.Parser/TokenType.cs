// 
// TokenType.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright 2008-2009 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Reflection;
using System.Collections.Generic;

namespace Capi.Parser
{
    internal static class TokenTypeCharMap
    {
        private static Dictionary<char, TokenType> map = new Dictionary<char, TokenType> ();
        
        static TokenTypeCharMap ()
        {
        
            foreach (var field in typeof (TokenType).GetFields ()) {
                if (field.FieldType != typeof (TokenType) || !field.IsLiteral) {
                    continue;
                }
                
                var attrs = field.GetCustomAttributes (typeof (TokenTypeMapAttribute), false);
                if (attrs != null && attrs.Length > 0) {
                    map.Add (((TokenTypeMapAttribute)attrs[0]).C, (TokenType)field.GetValue (typeof(TokenType)));
                }
            }
        }
    
        public static TokenType Get (char c)
        {
            TokenType token;
            if (map.TryGetValue (c, out token)) {
                return token;
            }
            return TokenType.None;
        }
    }
    
    public class TokenTypeMapAttribute : Attribute
    {
        public char C { get; set; }
        
        public TokenTypeMapAttribute (char c)
        {
            C = c;
        }
    }

    [Flags]
    public enum TokenType : ulong
    {
        None = 0 << 0,
        
        [TokenTypeMap ('{')] OpenBrace = 1ul << 0,
        [TokenTypeMap ('}')] CloseBrace = 1ul << 1,
        [TokenTypeMap ('[')] OpenBracket = 1ul << 2,
        [TokenTypeMap (']')] CloseBracket = 1ul << 3,
        [TokenTypeMap ('(')] OpenParen = 1ul << 4,
        [TokenTypeMap (')')] CloseParen = 1ul << 5,
        [TokenTypeMap ('~')] Tilde = 1ul << 6,
        [TokenTypeMap ('`')] Backtick = 1ul << 7,
        [TokenTypeMap ('!')] Exclaimation = 1ul << 8,
        [TokenTypeMap ('@')] At = 1ul << 9,
        [TokenTypeMap ('#')] Hash = 1ul << 10,
        [TokenTypeMap ('$')] Dollar = 1ul << 11,
        [TokenTypeMap ('%')] Percent = 1ul << 12,
        [TokenTypeMap ('^')] Carrot = 1ul << 13,
        [TokenTypeMap ('&')] Ampersand = 1ul << 14,
        [TokenTypeMap ('*')] Asterik = 1ul << 15,
        [TokenTypeMap ('_')] Underscore = 1ul << 16,
        [TokenTypeMap ('-')] Hyphen = 1ul << 17,
        [TokenTypeMap ('+')] Plus = 1ul << 18,
        [TokenTypeMap ('|')] Pipe = 1ul << 19,
        [TokenTypeMap ('\\')] BackSlash = 1ul << 20,
        [TokenTypeMap ('/')] ForwardSlash = 1ul << 21,
        [TokenTypeMap ('?')] QuestionMark = 1ul << 22,
        [TokenTypeMap (':')] Colon = 1ul << 23,
        [TokenTypeMap (';')] Semicolon = 1ul << 24,
        [TokenTypeMap ('.')] Period = 1ul << 25,
        [TokenTypeMap (',')] Comma = 1ul << 26,
        [TokenTypeMap ('\'')] SingleQuote = 1ul << 27,
        [TokenTypeMap ('"')] DoubleQuote = 1ul << 28,
        [TokenTypeMap ('<')] LessThan = 1ul << 29,
        [TokenTypeMap ('>')] GreaterThan = 1ul << 30,
        [TokenTypeMap ('=')] Equals = 1ul << 31,
        [TokenTypeMap ('\n')] NewLine = 1ul << 32,
        
        Identifier = 1ul << 33,
        String = 1ul << 34,
        Number = 1ul << 35,
        SingleLineComment = 1ul << 36,
        MultiLineComment = 1ul << 37,
        Comment = SingleLineComment | MultiLineComment
    }
}
