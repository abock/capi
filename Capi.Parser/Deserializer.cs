// 
// Deserializer.cs
//  
// Author:
//   Aaron Bockover <abockover@novell.com>
// 
// Copyright 2009 Novell, Inc.
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
using System.IO;
using System.Collections.Generic;

namespace Capi.Parser
{
    public class Deserializer
    {
        private Tokenizer tokenizer = new Tokenizer ();
        
        public Deserializer () { }
        public Deserializer (string input) { SetInput (input); }
        public Deserializer (Stream stream) { SetInput (stream); }
        public Deserializer (StreamReader reader) { SetInput (reader); }
        
        public string InputName { get; set; }
        public ParserState ParserState { get; set; }
        
        public Deserializer SetInput (StreamReader reader)
        {
            tokenizer.SetInput (reader);
            return this;
        }
        
        public Deserializer SetInput (Stream stream)
        {
            tokenizer.SetInput (stream);
            return this;
        }
        
        public Deserializer SetInput (string input)
        {
            tokenizer.SetInput (input);
            return this;
        }
        
        public void Deserialize ()
        {
            if (ParserState == null) {
                throw new InvalidOperationException ("ParserState must be set before calling Deserialize");
            }
            
            Console.WriteLine ("Parsing: {0}", InputName);
        
            Token token = null;
            while ((token = CheckScan (TokenType.None, true)) != null) {
                if (token.Type == TokenType.Hash) {
                    ParseCppDirective ();
                } else if (ParserState.IsParsingEnabled) {
                    switch (token.Type) {
                        default: break;
                    }
                }
            }
        }
        
        private Token CheckScan (TokenType expected)
        {
            return CheckScan (expected, false);
        }
        
        private Token CheckScan (TokenType expected, bool eofok)
        {
            Token token = tokenizer.Scan ();
            if (token == null && eofok) {
                return null;
            } else if (token == null) {
                UnexpectedEof (expected);
            } else if (expected != TokenType.None && (expected & token.Type) == 0) {
                UnexpectedToken (expected, token);
            }
            return token;
        }
        
        private void UnexpectedToken (TokenType expected, Token got)
        {
            throw new ApplicationException (String.Format ("Unexpected token {0} at [{1}:{2}]; expected {3}", 
                got.Type, got.SourceLine, got.SourceColumn, expected));
        }
        
        private void UnexpectedEof (TokenType expected)
        {
            throw new ApplicationException (String.Format ("Unexpected End of File; expected {0}", expected));
        }
        
        private void LogWarning (Token offender, string message)
        {
            LogWarning (InputName, offender, message);
        }
        
        private void LogWarning (string inputFile, Token offender, string message)
        {
            Console.Error.WriteLine ("{0}:{1}:{2}: warning: {3}", PwdRelativePath (inputFile) ?? "Unknown", 
                offender.SourceLine, offender.SourceColumn, message);
        }
        
        private string PwdRelativePath (string path)
        {
            if (path == null) {
                return null;
            }
            
            // FIXME: Actually make this return a path relaive to the PWD
            return Path.GetFileName (path);
        }
        
#region CPP Parsers

        private void ParseCppDirective ()
        {
            var directive_token = CheckScan (TokenType.Identifier);
            string directive = (string)directive_token.Value;
            
            if (ParserState.IsParsingEnabled) {
                switch (directive) {
                    case "define": ParseCppDefine (directive_token); break;
                    case "undef": ParseCppUndef (); break;
                    case "include": ParseCppInclude (); break;
                    case "warning": ParseCppMessage (directive_token, false); break;
                    case "error": ParseCppMessage (directive_token, true); break;
                }
            }
            
            switch (directive) {
                case "ifdef": ParseCppIfdef (false); break;
                case "ifndef": ParseCppIfdef (true); break;
                case "endif": ParseCppEndif (); break;
                case "else": ParseCppElse (); break;
            }
        }
        
        private void ParseCppDefine (Token define)
        {
            Token new_name_token = CheckScan (TokenType.Identifier);
            CppMacro old_macro = null;
            var name = (string)new_name_token.Value;
            
            CppMacro new_macro = new CppMacro () { 
                Parent = new_name_token,
                InputName = InputName
            };
            new_macro.Push (tokenizer.Scan ());
            
            if (ParserState.CppDefines.TryGetValue (name, out old_macro)) {
                LogWarning (define, String.Format ("\"{0}\" redefined", name));
                LogWarning (old_macro.InputName, old_macro.Parent, "this is the location of the previous definition");
                
                ParserState.CppDefines[name] = new_macro;
            } else {
                ParserState.CppDefines.Add (name, new_macro);
            }
        }
        
        private void ParseCppUndef ()
        {
            var name = (string)CheckScan (TokenType.Identifier).Value;
            if (ParserState.CppDefines.ContainsKey (name)) {
                ParserState.CppDefines.Remove (name);
            }
        }
        
        private void ParseCppInclude ()
        {
            var search_paths = tokenizer.PeekAhead () == '<'
                ? ParserState.SystemIncludePaths
                : new List<string> (ParserState.QuoteIncludePaths);
            
            tokenizer.PushConfig ();
            tokenizer.Config.StringOpenTokens = new char [] { '"', '<' };
            tokenizer.Config.StringCloseTokens = new char [] { '"', '>' };

            var include_file = (string)CheckScan (TokenType.String).Value;
            
            if (search_paths != ParserState.SystemIncludePaths) {
                search_paths.Insert (0, Path.GetDirectoryName (InputName));
            }
            
            foreach (var path in search_paths) {
                var file = Path.Combine (path, include_file);
                if (File.Exists (file)) {
                    CppIncludeFile (file);
                    break;
                }
            }
            
            tokenizer.PopConfig ();
        }
        
        private void CppIncludeFile (string path)
        {
            using (var reader = new StreamReader (path)) {
                new Deserializer (reader) {
                    InputName = path,
                    ParserState = ParserState
                }.Deserialize ();
            }
        }
        
        private void ParseCppMessage (Token directive, bool fatal)
        {
            // FIXME: This should be a stream of unexpanded tokens, not a single string
            var message = CheckScan (TokenType.String);
            LogWarning (directive, (string)message.Value);
            
            if (fatal) {
                throw new ApplicationException ("#error directive reached");
            }
        }
        
        private void ParseCppIfdef (bool negate)
        {
            if (!ParserState.IsParsingEnabled) {
                return;
            }
        
            var name = (string)CheckScan (TokenType.Identifier).Value;
            
            Console.Write ("{0}#if{1}def {2} :: {3} -> ", 
                String.Empty.PadLeft (ParserState.ParsingEnabledDepth + 1),
                negate ? "n" : "",
                name, ParserState.IsParsingEnabled);
            
            bool enabled = ParserState.CppDefines.ContainsKey (name);
            if (negate) {
                enabled = !enabled;
            }
            
            ParserState.PushParsingEnabled (enabled);
            
            Console.WriteLine (ParserState.IsParsingEnabled);
        }
        
        private void ParseCppEndif ()
        {
            Console.Write ("{0}#endif --> {1} -> ", 
                String.Empty.PadLeft (ParserState.ParsingEnabledDepth), 
                ParserState.IsParsingEnabled);
                
            ParserState.PopParsingEnabled ();
            
            Console.WriteLine (ParserState.IsParsingEnabled);
        }
        
        private void ParseCppElse ()
        {
            if (!ParserState.IsParsingEnabled) {
                return;
            }
        
            Console.Write ("{0}#else --> {1} -> ", 
                String.Empty.PadLeft (ParserState.ParsingEnabledDepth), 
                ParserState.IsParsingEnabled);
            
            bool enabled = ParserState.IsParsingEnabled;
            ParserState.PopParsingEnabled ();
            ParserState.PushParsingEnabled (!enabled);
            
            Console.WriteLine (ParserState.IsParsingEnabled);
        }

#endregion

    }
}
