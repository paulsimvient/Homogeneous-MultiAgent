using System;
using Mono.Cecil;

namespace DynamicCSharp.Security
{
    /// <summary>
    /// Base class for all restriction checkers.
    /// </summary>
    [Serializable]
    public abstract class Restriction
    {
        // Properties
        /// <summary>
        /// Get the error message for the restriction.
        /// </summary>
        public abstract string Message { get; }

        public abstract RestrictionMode Mode { get; }
        
        // Methods
        /// <summary>
        /// Verify the specified module to make sure that it does not breach this restriction.
        /// </summary>
        /// <param name="module">The module to check</param>
        /// <returns>False if the module breaches the restriction or True if it does not</returns>
        public abstract bool Verify(ModuleDefinition module);
    }
}