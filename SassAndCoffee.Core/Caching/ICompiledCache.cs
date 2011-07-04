﻿namespace SassAndCoffee.Core.Caching
{
    using System;

    public interface ICompiledCache
    {
        CompilationResult GetOrAdd(string filename, Func<string, CompilationResult> compilationDelegate, string mimeType);

        void Clear();
    }
}