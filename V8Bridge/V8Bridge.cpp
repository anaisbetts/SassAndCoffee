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

void NativeV8ScriptCompiler::InitializeLibrary(const wchar_t* library_code)
{
	HandleScope handle_scope;
	Context::Scope context_scope(this->_context);

	Handle<String> src = String::New((uint16_t *)library_code);

	Handle<Script> script = Script::Compile(src);
	script->Run();
}

wchar_t* NativeV8ScriptCompiler::Compile(const wchar_t* function, const wchar_t* input)
{
	HandleScope handle_scope;
	Context::Scope context_scope(this->_context);

	TryCatch ex;
	Handle<Value> args[] = { String::New((uint16_t *)input) };

	Local<Function> compiler = Local<Function>::Cast(this->_context->Global()->Get(String::New((uint16_t *)function)));
	Local<Value> result = compiler->Call(compiler, 1, args);

	Handle<Value> exception = ex.Exception();
	if (!exception.IsEmpty()) {
		return _wcsdup((wchar_t*)*(String::Value(exception)));
	}

	String::Value ret(result);
	return _wcsdup((wchar_t*)*ret);
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
	const wchar_t* code = marshal->marshal_as<const wchar_t*>(libraryCode);

	pCompiler->InitializeLibrary(code);

	delete marshal;
}

System::String^ V8Bridge::V8ScriptCompiler::Compile(System::String^ function, System::String^ source)
{
	marshal_context^ marshal = gcnew marshal_context();
	const wchar_t* src = marshal->marshal_as<const wchar_t*>(source);
	const wchar_t* func =  marshal->marshal_as<const wchar_t*>(function);

	wchar_t* compiled_code = this->pCompiler->Compile(func, src);
	System::String^ ret = marshal_as<System::String^>(compiled_code);

	delete marshal;
	delete compiled_code;
	return ret;
}