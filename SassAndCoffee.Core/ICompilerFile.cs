using System;
using System.IO;

namespace SassAndCoffee.Core
{
	public interface ICompilerFile {
		DateTime LastWriteTimeUtc {
			get;
		}

		TextReader Open();

	    string Name {
	        get;
	    }

	    bool Exists {
	        get;
	    }

	    ICompilerFile GetRelativeFile(string relativePath);
	}
}