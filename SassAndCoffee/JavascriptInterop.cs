using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jurassic;
using V8Bridge.Interface;

namespace SassAndCoffee
{
    class JSWorkItem
    {
        readonly internal ManualResetEventSlim _gate = new ManualResetEventSlim();

        public string Input { get; internal set; }
        public string Func { get; internal set; }
        public string Result { get; internal set; }

        public JSWorkItem(string func, string input)
        {
            Func = func;
            Input = input;
        }

        public string GetValueSync()
        {
            _gate.Wait();
            return Result;
        }
    }

    public class JavascriptBasedCompiler
    {
        static ConcurrentQueue<JSWorkItem> _workQueue = new ConcurrentQueue<JSWorkItem>();
        static readonly Thread _dispatcherThread;
        string _compileFuncName;

        static JavascriptBasedCompiler()
        {
            _dispatcherThread = new Thread(() => {
                var engine = JS.CreateJavascriptCompiler();
                while(true) {
                    if (_workQueue == null) {
                        break;
                    }

                    JSWorkItem item;
                    if (!_workQueue.TryDequeue(out item)) {
                        Thread.Sleep(100);
                        continue;
                    }

                    if (item.Func == null) {
                        engine.InitializeLibrary(item.Input);
                        item.Result = "";
                    } else {
                        item.Result = engine.Compile(item.Func, item.Input);
                    }

                    item._gate.Set();
                }
            });

            _dispatcherThread.Start();
        }

        public JavascriptBasedCompiler(string resource, string compileFuncName)
        {
            _compileFuncName = compileFuncName;
            var workItem = new JSWorkItem(null, Utility.ResourceAsString(resource));

            _workQueue.Enqueue(workItem);
            workItem.GetValueSync();
        }

        public string Compile(string coffeeScriptCode)
        {
            var ret = new JSWorkItem(_compileFuncName, coffeeScriptCode);
            _workQueue.Enqueue(ret);
            return ret.GetValueSync();
        }

        public void Dispose()
        {
            _workQueue = null;
        }
    }

    public class JurassicCompiler : IV8ScriptCompiler
    {
        ScriptEngine _engine;
        object _gate = 42;

        public JurassicCompiler()
        {
            _engine = new ScriptEngine();
        }

        public void InitializeLibrary(string libraryCode)
        {
            lock (_gate) {
                var t = new Thread(() => {
                    _engine.Execute(libraryCode);
                }, 10 * 1048576);
    
                t.Start();
                t.Join();
            }
        }

        public string Compile(string func, string code)
        {
            return _engine.CallGlobalFunction<string>(func, code);
        }
    }

    public class JSTaskScheduler : TaskScheduler
    {
        protected override void QueueTask(Task task)
        {
            throw new NotImplementedException();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            throw new NotImplementedException();
        }
    }

    public static class JS
    {
        static Lazy<Type> _scriptCompilerImpl;
        static object _gate = 42;

        static JS()
        {
            _scriptCompilerImpl = new Lazy<Type>(() => {
                string assemblyResource = (Environment.Is64BitProcess ?
                                                                          "SassAndCoffee.lib.amd64.V8Bridge.dll" : "SassAndCoffee.lib.x86.V8Bridge.dll");

                var v8Name = Path.Combine(Path.GetTempPath(), "V8Bridge.dll");
                using (var of = File.OpenWrite(v8Name)) {
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(assemblyResource).CopyTo(of);
                }

                Assembly v8Assembly;
                try {
                    v8Assembly = Assembly.LoadFile(v8Name);
                } catch (Exception ex) {
                    Console.Error.WriteLine("*** WARNING: You're on ARM, Mono, Itanium (heaven help you), or another architecture\n" +
                        "which isn't x86/amd64 on NT. Loading the Jurassic compiler, which is much slower.");

                    return typeof (JurassicCompiler);
                }

                return v8Assembly.GetTypes().FirstOrDefault(x => typeof (IV8ScriptCompiler).IsAssignableFrom(x)) ??
                    typeof (JurassicCompiler);
            }, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public static IV8ScriptCompiler CreateJavascriptCompiler()
        {
            lock(_gate) {
                return Activator.CreateInstance(_scriptCompilerImpl.Value) as IV8ScriptCompiler;
            }
        }
    }
}
