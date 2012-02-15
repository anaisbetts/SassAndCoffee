namespace SassAndCoffee.Core {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Security;

    /// <summary>
    /// Hides the fact that mutiple watchers are needed to watch multiple
    /// file system types in multiple locations
    /// </summary>
    public sealed class CacheInvalidationWatcher : IDisposable {
        private List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();

        public event FileSystemEventHandler Changed;
        public event FileSystemEventHandler Created;
        public event FileSystemEventHandler Deleted;
        public event EventHandler Disposed;
        public event ErrorEventHandler Error;
        public event RenamedEventHandler Renamed;

        [SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Marked as security critical.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "We need to keep it around to use it.")]
        public void BeginWatch(string path, string filter) {
            // Perform some normalization
            var pathInfo = new DirectoryInfo(path);
            if (!pathInfo.Exists)
                return;

            path = pathInfo.FullName;
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            path = path.TrimEnd(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            path += Path.DirectorySeparatorChar;

            lock (_watchers) {

                // Check if this watch is covered by an existing watcher
                if (_watchers
                    .Where(w => w.Filter.Equals(filter, StringComparison.OrdinalIgnoreCase))
                    .Where(w => path.StartsWith(w.Path, StringComparison.OrdinalIgnoreCase))
                    .Any()) {
                    return;
                }

                // Path not watched yet
                var newWatcher = new FileSystemWatcher(path, filter);
                newWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size;
                newWatcher.IncludeSubdirectories = true;
                newWatcher.InternalBufferSize = 65536;  // 64kB

                newWatcher.Changed += OnChanged;
                newWatcher.Created += OnCreated;
                newWatcher.Deleted += OnDeleted;
                newWatcher.Disposed += HandleDisposed;
                newWatcher.Error += OnError;
                newWatcher.Renamed += OnRenamed;
                newWatcher.EnableRaisingEvents = true;

                foreach (var redundant in _watchers
                    .Where(w => w.Filter.Equals(filter, StringComparison.OrdinalIgnoreCase))
                    .Where(w => w.Path.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                    ) {
                    _watchers.Remove(redundant);
                    redundant.EnableRaisingEvents = false;
                    redundant.Dispose();
                }

                _watchers.Add(newWatcher);
            }
        }

        //public void EndWatch(string path, string filter) {
        //    // TODO: Support this?
        //    throw new NotImplementedException();
        //}

        private void HandleDisposed(object sender, EventArgs e) {
            _watchers.Remove(sender as FileSystemWatcher);
        }

        private void OnChanged(object sender, FileSystemEventArgs e) {
            var changed = Changed;
            if (changed != null) {
                changed(this, e);
            }
        }

        private void OnCreated(object sender, FileSystemEventArgs e) {
            var created = Created;
            if (created != null) {
                created(this, e);
            }
        }

        private void OnDeleted(object sender, FileSystemEventArgs e) {
            var deleted = Deleted;
            if (deleted != null) {
                deleted(this, e);
            }
        }

        private void OnError(object sender, ErrorEventArgs e) {
            var error = Error;
            if (error != null) {
                error(this, e);
            }
        }

        private void OnRenamed(object sender, RenamedEventArgs e) {
            var renamed = Renamed;
            if (renamed != null) {
                renamed(this, e);
            }
        }

        private void FireDisposed() {
            var disposed = Disposed;
            if (disposed != null) {
                disposed(this, new EventArgs());
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Disabling events is hardly a security risk.")]
        public void Dispose() {
            foreach (var watcher in _watchers) {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            FireDisposed();
            GC.SuppressFinalize(this);
        }
    }
}
