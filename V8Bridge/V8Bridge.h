// V8Bridge.h

#pragma once

#include "Stdafx.h"
#include "v8.h"

class NativeV8ScriptCompiler
{
	v8::Persistent<v8::Context> _context;

public:
	NativeV8ScriptCompiler();
	~NativeV8ScriptCompiler();

	void InitializeLibrary(const char* library_code);
	char* Compile(const char* function, const char* input);
};


namespace V8Bridge
{
	public ref class V8ScriptCompiler : public System::IDisposable, public V8Bridge::Interface::IV8ScriptCompiler
	{
		NativeV8ScriptCompiler* pCompiler;

	public:
		V8ScriptCompiler();

		virtual void __clrcall InitializeLibrary(System::String^ libraryCode);
		virtual System::String^ __clrcall Compile(System::String^ function, System::String^ source);
		~V8ScriptCompiler();
	};
};
