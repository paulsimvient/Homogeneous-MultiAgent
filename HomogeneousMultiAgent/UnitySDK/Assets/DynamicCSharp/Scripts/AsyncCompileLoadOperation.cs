using System;
using DynamicCSharp.Compiler;

namespace DynamicCSharp
{
    /// <summary>
    /// A yieldable instruction that can be used inside a coroutine to wait for an async operation to exit without blocking the main thread.
    /// Inherits behaviour of <see cref="AsyncCompileOperation"/> and is fundamentally the same but adds additional state data such as <see cref="MainType"/>.  
    /// <example><code>IEnumerator WaitWithoutBlocking()
    /// {
    ///     // AsyncCompileLoadOperation is returned by some async methods
    ///     AsyncCompileLoadOperation task;
    ///     yield return task;
    /// }
    /// </code></example>
    /// </summary>
    public class AsyncCompileLoadOperation : AsyncCompileOperation
    {
        // Private
        private ScriptDomain domain = null;
        private ScriptAssembly loadedAssembly = null;

        // Properties
        /// <summary>
        /// Get the main type of the compiled and loaded assembly.
        /// </summary>
        public ScriptType MainType
        {
            get { return loadedAssembly.MainType; }
        }

        /// <summary>
        /// Get the compiled and loaded assembly;
        /// </summary>
        public ScriptAssembly LoadedAssembly
        {
            get { return loadedAssembly; }
        }

        // Constructor
        internal AsyncCompileLoadOperation(ScriptDomain domain, Func<bool> asyncCallback)
            : base(domain.CompilerService, asyncCallback)
        {
            this.domain = domain;
        }

        // Methods
        /// <summary>
        /// Override implementaiton of <see cref="DoSyncFinalize"/>. 
        /// </summary>
        protected override void DoSyncFinalize()
        {
            // Call the base method
            base.DoSyncFinalize();

            if(IsSuccessful == true)
            {
                // Load the assembly into the domain
                loadedAssembly = domain.LoadAssembly(assemblyData);
            }
        }
    }
}
