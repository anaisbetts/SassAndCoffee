using System;
using System.IO;

namespace SassAndCoffee.Core
{
	public interface ICompilerFile {
		DateTime LastWriteTimeUtc {
			get;
		}

		Stream Open();

	    string Name {
	        get;
	    }

	    bool Exists {
	        get;
	    }

	    ICompilerFile GetRelativeFile(string relativePath);
	}
}