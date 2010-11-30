// 
// ParserState.cs
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
    public class ParserState
    {
        public ParserState ()
        {
        }
        
        public void PrependSystemIncludePath (string path)
        {
            PrependIncludePath (path, system_include_paths);
        }
        
        public void PrependQuoteIncludePath (string path)
        {
            PrependIncludePath (path, quote_include_paths);
        }
        
        private void PrependIncludePath (string path, List<string> collection)
        {
            var dir = new DirectoryInfo (path);
            if (dir.Exists && !collection.Contains (dir.FullName)) {
                collection.Insert (0, dir.FullName);
            }
        }
        
        private Dictionary<string, CppMacro> cpp_defines = new Dictionary<string, CppMacro> ();
        public Dictionary<string, CppMacro> CppDefines {
            get { return cpp_defines; }
        }
        
        private List<string> system_include_paths = new List<string> ();
        public List<string> SystemIncludePaths {
            get { return system_include_paths; }
        }
        
        private List<string> quote_include_paths = new List<string> ();
        public List<string> QuoteIncludePaths {
            get { return quote_include_paths; }
        }
        
        private Stack<bool> parsing_enabled = new Stack<bool> ();
        
        internal void PushParsingEnabled (bool enabled)
        {
            parsing_enabled.Push (enabled);
        }
        
        internal bool PopParsingEnabled ()
        {
            return parsing_enabled.Pop ();
        }
        
        internal bool IsParsingEnabled {
            get { return parsing_enabled.Count == 0 || parsing_enabled.Peek (); }
        }
        
        internal int ParsingEnabledDepth {
            get { return parsing_enabled.Count; }
        }
    }
}
