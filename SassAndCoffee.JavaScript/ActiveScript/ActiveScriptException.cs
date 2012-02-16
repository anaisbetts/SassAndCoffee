namespace SassAndCoffee.JavaScript.ActiveScript {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.Serialization;
    using ComTypes = System.Runtime.InteropServices.ComTypes;

    [Serializable]
    public class ActiveScriptException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveScriptException"/> class.
        /// </summary>
        public ActiveScriptException() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveScriptException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ActiveScriptException(string message)
            : base(message) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveScriptException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        public ActiveScriptException(Exception innerException)
            : base(null, innerException) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveScriptException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ActiveScriptException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveScriptException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is null.
        ///   </exception>
        ///   
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        /// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
        ///   </exception>
        protected ActiveScriptException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
            SourceContext = info.GetUInt32("SourceContext");
            LineNumber = info.GetUInt32("LineNumber");
            Column = info.GetInt32("Column");
            LineContent = info.GetString("LineContent");
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

            info.AddValue("SourceContext", SourceContext);
            info.AddValue("LineNumber", LineNumber);
            info.AddValue("Column", Column);
            info.AddValue("LineContent", LineContent);
            base.GetObjectData(info, context);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "It's used safely.")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "WCode", Justification = "It is spelled correctly.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Creating an exception object can't throw an exception.")]
        public static ActiveScriptException Create(IActiveScriptError error) {
            string source = "";
            uint sourceContext = 0;
            uint lineNumber = 0;
            int characterPosition = 0;

            string message = "";

            try {
                error.GetSourceLineText(out source);
            } catch { }

            try {
                error.GetSourcePosition(out sourceContext, out lineNumber, out characterPosition);
                ++lineNumber;
                ++characterPosition;
            } catch { }

            try {
                ComTypes.EXCEPINFO excepInfo;
                error.GetExceptionInfo(out excepInfo);
                message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Error in [{1}]:\n{0}\nat line {4}({5})\nError Code: {2} (0x{2:X8})\nError WCode: {3}\n\n{6}",
                    /* 0 */ excepInfo.bstrDescription,
                    /* 1 */ excepInfo.bstrSource,
                    /* 2 */ excepInfo.scode,
                    /* 3 */ excepInfo.wCode,
                    /* 4 */ lineNumber,
                    /* 5 */ characterPosition,
                    /* 6 */ source);
            } catch { }

            return new ActiveScriptException(message) {
                LineContent = source,
                SourceContext = sourceContext,
                LineNumber = lineNumber,
                Column = characterPosition,
            };
        }

        /// <summary>
        /// Gets or sets the application specific source context.
        /// </summary>
        public uint SourceContext { get; set; }

        /// <summary>
        /// Gets or sets the line number on which the error occurred.
        /// </summary>
        public uint LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the column on which the error occurred..
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Gets or sets the content of the line on which the error occurred..
        /// </summary>
        public string LineContent { get; set; }
    }
}
