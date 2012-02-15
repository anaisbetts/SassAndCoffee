namespace SassAndCoffee.Ruby.Sass {
    using System;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Detailed information about SASS syntax errors
    /// </summary>
    [Serializable]
    public class SassSyntaxException : Exception {
        public string RubyBackTrace { get; protected set; }
        public string FileName { get; protected set; }
        public string MixIn { get; protected set; }
        public string LineNumber { get; protected set; }
        public string SassTemplate { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SassSyntaxException"/> class.
        /// </summary>
        public SassSyntaxException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SassSyntaxException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SassSyntaxException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SassSyntaxException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
        ///   
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        protected SassSyntaxException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
            RubyBackTrace = info.GetString("RubyBacktrace");
            FileName = info.GetString("FileName");
            MixIn = info.GetString("MixIn");
            LineNumber = info.GetString("LineNumber");
            SassTemplate = info.GetString("SassTemplate");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SassSyntaxException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SassSyntaxException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic). </exception>
        ///   
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/>
        ///   </PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("RubyBacktrace", RubyBackTrace);
            info.AddValue("FileName", FileName);
            info.AddValue("MixIn", MixIn);
            info.AddValue("LineNumber", LineNumber);
            info.AddValue("SassTemplate", SassTemplate);

            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*"/>
        ///   </PermissionSet>
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\n\n", Message)
              .AppendFormat("Backtrace:\n{0}\n\n", RubyBackTrace)
              .AppendFormat("FileName: {0}\n\n", FileName)
              .AppendFormat("MixIn: {0}\n\n", MixIn)
              .AppendFormat("Line Number: {0}\n\n", LineNumber)
              .AppendFormat("Sass Template:\n{0}\n\n", GetSassTemplateWithLineNumbers());
            return sb.ToString();
        }

        public static bool IsSassSyntaxError(Exception exception) {
            return exception != null && exception.Message == "Sass::SyntaxError";
        }

        public static SassSyntaxException FromSassSyntaxError(Exception sassSyntaxError) {
            return FromSassSyntaxError(sassSyntaxError, "");
        }

        public static SassSyntaxException FromSassSyntaxError(Exception sassSyntaxError, string filePath) {
            if (!IsSassSyntaxError(sassSyntaxError))
                return null;

            dynamic error = sassSyntaxError;
            return new SassSyntaxException(error.to_s(), sassSyntaxError) {
                RubyBackTrace = error.sass_backtrace_str(filePath) ?? "",
                FileName = string.IsNullOrWhiteSpace(error.sass_filename()) ? filePath : error.sass_filename(),
                MixIn = error.sass_mixin() ?? "",
                LineNumber = error.sass_line() ?? "",
                SassTemplate = error.sass_template ?? "",
            };
        }

        private string GetSassTemplateWithLineNumbers() {
            var sb = new StringBuilder();
            var lines = SassTemplate.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i <= lines.Length; ++i) {
                sb.AppendFormat("{0,4}: {1}", i, lines[i]);
            }
            return sb.ToString();
        }
    }
}
