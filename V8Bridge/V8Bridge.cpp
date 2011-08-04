// This is the main DLL file.

#include "stdafx.h"

#include <msclr\marshal.h>

#include "v8.h"
#include "V8Bridge.h"

using namespace v8;
using namespace msclr::interop;

NativeV8ScriptCompiler::NativeV8ScriptCompiler()
{
	HandleScope handle_scope;
	this->_context = Context::New();
}

NativeV8ScriptCompiler::~NativeV8ScriptCompiler()
{
	_context.Dispose();
}

void NativeV8ScriptCompiler::InitializeLibrary(const char* library_code)
{
	HandleScope handle_scope;
	Context::Scope context_scope(this->_context);

	Handle<String> src = String::New(library_code);

	Handle<Script> script = Script::Compile(src);
	script->Run();
}

char* NativeV8ScriptCompiler::Compile(const char* function, const char* input)
{
	HandleScope handle_scope;
	Context::Scope context_scope(this->_context);

	TryCatch ex;
	Handle<Value> args[] = { String::New(input) };

	Local<Function> compiler = Local<Function>::Cast(this->_context->Global()->Get(String::New(function)));
	Local<Value> result = compiler->Call(compiler, 1, args);

	Handle<Value> exception = ex.Exception();
	if (!exception.IsEmpty()) {
		return _strdup(*(String::AsciiValue(exception)));
	}

	String::AsciiValue ret(result);
	return _strdup(*ret);
}

V8Bridge::V8ScriptCompiler::V8ScriptCompiler()
{
	pCompiler = new NativeV8ScriptCompiler();
}

V8Bridge::V8ScriptCompiler::~V8ScriptCompiler()
{
	delete this->pCompiler;
}

void V8Bridge::V8ScriptCompiler::InitializeLibrary(System::String^ libraryCode)
{
	marshal_context^ marshal = gcnew marshal_context();
	const char* code = marshal->marshal_as<const char*>(libraryCode);

	pCompiler->InitializeLibrary(code);

	delete marshal;
}

System::String^ V8Bridge::V8ScriptCompiler::Compile(System::String^ function, System::String^ source)
{
	marshal_context^ marshal = gcnew marshal_context();
	const char* src = marshal->marshal_as<const char*>(source);
	const char* func =  marshal->marshal_as<const char*>(function);

	char* compiled_code = this->pCompiler->Compile(func, src);
	System::String^ ret = marshal_as<System::String^>(compiled_code);

	delete marshal;
	delete compiled_code;
	return ret;
}