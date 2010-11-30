// 
// CapiParserTool.cs
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

using Mono.Options;
using Capi.Parser;

namespace Capi
{
    public static class CapiParserTool
    {
        public static void Main (string [] args)
        {
            var parser_state = new ParserState ();
            var show_help = false;
            
            var options = new OptionSet () {
                { "I|include=", "prepend system include search path", v => parser_state.PrependSystemIncludePath (v) },
                { "iquote=", "prepend quote include search path", v => parser_state.PrependQuoteIncludePath (v) },
                { "h|help", "show this message and exit", v => show_help = v != null }
            };

            List<string> paths;
            try {
                paths = options.Parse (args);
            } catch (OptionException e) {
                Console.Write ("CAPI Parser: ");
                Console.WriteLine (e.Message);
                Console.WriteLine ("Try --help for more information");
                return;
            }
            
            if (show_help) {
                Console.WriteLine ("Usage: capi-parser [OPTIONS]+ <source0-N>...");
                Console.WriteLine ();
                Console.WriteLine ("Options:");
                options.WriteOptionDescriptions (Console.Out);
                return;
            }
        
            foreach (var path in paths) {
                using (var reader = new StreamReader (path)) {
                    new Deserializer (reader) {
                        InputName = new FileInfo (path).FullName,
                        ParserState = parser_state
                    }.Deserialize ();
                }
            }
            
            Console.WriteLine ("CPP Macros");
            Console.WriteLine ("----------");
            foreach (var cpp_def in parser_state.CppDefines) {
                Console.Write ("  {0} =", cpp_def.Key);
                foreach (var token in cpp_def.Value) {
                    Console.Write (" {0}", token.Value);
                }
                Console.WriteLine ();
            }
        }
    }
}
