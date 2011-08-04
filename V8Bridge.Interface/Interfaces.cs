using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Bridge.Interface
{
    public interface IV8ScriptCompiler
    {
        void InitializeLibrary(string libraryCode);
        string Compile(string function, string input);
    }
}
