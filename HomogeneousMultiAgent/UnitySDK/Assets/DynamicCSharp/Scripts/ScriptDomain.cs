using System;
using System.IO;
using System.Reflection;
using DynamicCSharp.Compiler;
using DynamicCSharp.Security;
using UnityEngine;
using System.Security;

namespace DynamicCSharp
{
    /// <summary>
    /// A <see cref="ScriptDomain"/> acts as a container for all code that is loaded dynamically at runtime.
    /// Despite the name, the code is not actually loaded into an isolated domain but is instead loaded into the main application domain. This is due to unity restrictions.
    /// The main responsiblility of the domin is to separate pre-compiled game code from runtime-loaded code. 
    /// As a result, you will only be able to access types from the domain that were loaded at runtime.
    /// Any pre-compiled game code will be ignored.
    /// Any assemblies or scripts that are loaded into the domain at runtime will remain until the application exits so you should be careful to avoid loading too many assemblies.
    /// You would typically load user code at statup in a 'Load' method which would then exist and execute until the game exits.
    /// Multiple domain instances may be created but you should note that all runtime code will be loaded into the current application domain. The <see cref="ScriptDomain"/> simply masks the types that are visible.
    /// </summary>
    public class ScriptDomain
    {
        // Private
        private static ScriptDomain domain = null;

        private AppDomain sandbox = null;
        private AssemblyChecker checker = null;
        private ScriptCompiler compilerService = null;

        // Properties
        internal static ScriptDomain Active
        {
            get { return domain; }
        }

        public ScriptCompiler CompilerService
        {
            get { return compilerService; }
        }

        // Constructor
        private ScriptDomain(string name)
        {
            // Store the domain instance
            domain = this;

            // Create the app domain
            sandbox = AppDomain.CurrentDomain; 

            // Create a security checker
            checker = new AssemblyChecker();
        }

        // Methods
        #region AssemblyLoad
        /// <summary>
        /// Attempts to load a managed assembly from the specified resources path into the sandbox app domain.
        /// The target asset must be a <see cref="TextAsset"/> in order to be loaded successfully. 
        /// </summary>
        /// <param name="resourcePath">The file name of path relative to the 'Resources' folder without the file extension</param>
        /// <returns>An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if an error occurs</returns>
        /// <exception cref="SecurityException">The assembly breaches the imposed security restrictions</exception>
        public ScriptAssembly LoadAssemblyFromResources(string resourcePath)
        {
            // Try to load resource
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);

            // Check for error
            if (asset == null)
                throw new DllNotFoundException(string.Format("Failed to load dll from resources path '{0}'", resourcePath));
            
            // Get the asset bytes and call through
            return LoadAssembly(asset.bytes);
        }

        /// <summary>
        /// Attempts to load the specified managed assembly into the sandbox app domain.
        /// </summary>
        /// <param name="fullPath">The full path the the .dll file</param>
        /// <returns>An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if an error occurs</returns>
        /// <exception cref="SecurityException">The assembly breaches the imposed security restrictions</exception>
        public ScriptAssembly LoadAssembly(string fullPath)
        {
            // Load the assembly into the sandbox domain
            CheckDisposed();

            // Check for file
            if (File.Exists(fullPath) == false)
                throw new DllNotFoundException(string.Format("Failed to load dll at '{0}'", fullPath));

            // Load the assembly
            byte[] assemblyBuffer = File.ReadAllBytes(fullPath);
            byte[] symbolBuffer = null;

            // Check for symbols
            if (DynamicCSharp.Settings.debugMode == true)
            {
                if (File.Exists(fullPath = ".mdb") == true)
                {
                    // Read symbols
                    symbolBuffer = File.ReadAllBytes(fullPath + ".mdb");
                }
            }

            // Read all bytes in the file and call through
            return LoadAssembly(assemblyBuffer, symbolBuffer);

            //// Load the assembly
            //Assembly assembly = Assembly.LoadFrom(fullPath);// sandbox.Load(fullPath);

            //// Check for errors
            //if (assembly == null)
            //    return null;

            //// Create the script assembly
            //return new ScriptAssembly(this, assembly);
        }

        /// <summary>
        /// Attempts to load the specified managed assembly into the sandbox app domain.
        /// </summary>
        /// <param name="name">The <see cref="AssemblyName"/> representing the assembly to load</param>
        /// <returns>An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if an error occurs</returns>
        /// <exception cref="SecurityException">The assembly breaches the imposed security restrictions</exception>
        public ScriptAssembly LoadAssembly(AssemblyName name)
        {
            // Load the assembly into the sandbox domain
            CheckDisposed();

            // Load the assembly
            Assembly assembly = sandbox.Load(name);

            // Create the script assembly
            return new ScriptAssembly(this, assembly);
        }

        /// <summary>
        /// Attempts to load a managed assembly from the specified raw bytes.
        /// </summary>
        /// <param name="data">The raw data representing the file structure of the managed assembly, The result of <see cref="File.ReadAllBytes(string)"/> for example.</param>
        /// <param name="symbols">The debug symbols for the assembly</param>
        /// <returns>An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if an error occurs</returns>
        /// <exception cref="SecurityException">The assembly breaches the imposed security restrictions</exception>
        public ScriptAssembly LoadAssembly(byte[] data, byte[] symbols = null)
        {
            // Load the assembly into the sandbox domain
            CheckDisposed();

            // Security check the assembly
            if(DynamicCSharp.Settings.securityCheckCode == true)
                SecurityCheckAssembly(data, true);

            // Load the assembly
            Assembly assembly = null;

            if (symbols == null || DynamicCSharp.Settings.debugMode == false)
            {
                // Load assembly only
                assembly = sandbox.Load(data);
            }
            else
            {
                // Load with symbols
                assembly = sandbox.Load(data, symbols);
            }

            // Check for errors
            if (assembly == null)
                return null;

            // Create the script assembly
            return new ScriptAssembly(this, assembly);
        }

        /// <summary>
        /// Attempts to load the managed assembly at the specified location.
        /// Any exceptions throw while loading will be caught.
        /// </summary>
        /// <param name="fullPath">The full path to the .dll file</param>
        /// <param name="result">An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if the load failed</param>
        /// <returns>True if the assembly was loaded successfully or false if an error occurred</returns>
        public bool TryLoadAssembly(string fullPath, out ScriptAssembly result)
        {
            try
            {
                // Call through
                result = LoadAssembly(fullPath);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to load a managed assembly with the specified name.
        /// Any exceptions thrown while loading will be caught.
        /// </summary>
        /// <param name="name">The <see cref="AssemblyName"/> of the assembly to load</param>
        /// <param name="result">An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if the load failed</param>
        /// <returns>True if the assembly was loaded successfully or false if an error occurred</returns>
        public bool TryLoadAssembly(AssemblyName name, out ScriptAssembly result)
        {
            try
            {
                // Call through
                result = LoadAssembly(name);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to load a managed assembly from the raw assembly data.
        /// Any exceptions thrown while loading will be caught.
        /// </summary>
        /// <param name="data">The raw data representing the file structure of the managed assembly, The result of <see cref="File.ReadAllBytes(string)"/> for example.</param>
        /// <param name="result">An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if the load failed</param>
        /// <returns>True if the assembly was loaded successfully or false if an error occured</returns>
        public bool TryLoadAssembly(byte[] data, out ScriptAssembly result)
        {
            try
            {
                // Call through
                result = LoadAssembly(data);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }
        #endregion

        #region ScriptCompile
        /// <summary>
        /// Attempts to compile and load a C# script from the specified file path.
        /// Depending upon settings, the code may be security verified before being loaded which may result in an exception being thrown.
        /// It is recommended that source files define only a single type however it is not a requirement. 
        /// The reason for this is that the return type if <see cref="ScriptType"/> which represents a single type defenition which will return the first defined type if more than one type is defined. 
        /// If you need to compile multiple files with dependencies then you should use <see cref="CompileAndLoadScriptFiles(string[])"/>. 
        /// </summary>
        /// <param name="file">The filepath of filename of the source file to compile</param>
        /// <returns>A <see cref="ScriptType"/> representing the main type in the compiled result.</returns>
        /// <exception cref="SecurityException">The code does not meet the security restrictions defined in the settings menu</exception>
        public ScriptType CompileAndLoadScriptFile(string file)
        {
            // Make sure the compiler is initialized
            CheckCompiler();

            // Convert to array
            string[] files = { file };

            // Compile the code
            if (compilerService.CompileFiles(files, DynamicCSharp.Settings.assemblyReferences) == false)
            {
                compilerService.PrintErrors();
                return null;
            }

            // Print any warnings
            if (compilerService.HasWarnings == true)
                compilerService.PrintWarnings();

            // Load the assembly - This will also security check the assembly
            ScriptAssembly assembly = LoadAssembly(compilerService.AssemblyData, compilerService.SymbolsData);

            // Check for assembly
            if (assembly == null)
                return null;

            // Get the main type
            return assembly.MainType;
        }

        /// <summary>
        /// Attempts to compile and load a number of C# scripts from the specified file paths as a batch.
        /// Depending upon settings, the code may be security verified before being loaded which may result in an exception being thrown.
        /// </summary>
        /// <param name="files">The filepaths of filenames of the source files to compile as a batch</param>
        /// <returns>A <see cref="ScriptAssembly"/> containing all the compiled types</returns>
        /// <exception cref="SecurityException">The code does not meet the security restrictions defined in the settings menu</exception>
        public ScriptAssembly CompileAndLoadScriptFiles(params string[] files)
        {
            // Make sure the compiler is initialized
            CheckCompiler();

            // Compile the code
            if (compilerService.CompileFiles(files, DynamicCSharp.Settings.assemblyReferences) == false)
            {
                compilerService.PrintErrors();
                return null;
            }

            // Print any warnings
            if (compilerService.HasWarnings == true)
                compilerService.PrintWarnings();

            // Load the assembly
            return LoadAssembly(compilerService.AssemblyData, compilerService.SymbolsData);
        }

        /// <summary>
        /// Attempts to compile and load a C# script from the specified source code
        /// Depending upon settings, the code may be security verified before being loaded which may result in an exception being thrown.
        /// It is recommended that source code define only a single type however it is not a requirement. 
        /// The reason for this is that the return type if <see cref="ScriptType"/> which represents a single type defenition which will return the first defined type if more than one type is defined. 
        /// If you need to compile multiple files with dependencies then you should use <see cref="CompileAndLoadScriptSources(string[])"/>. 
        /// </summary>
        /// <param name="source">The C# source code to compile</param>
        /// <returns>A <see cref="ScriptType"/> representing the main type in the compiled result.</returns>
        /// <exception cref="SecurityException">The code does not meet the security restrictions defined in the settings menu</exception>
        public ScriptType CompileAndLoadScriptSource(string source)
        {
            // Make sure the compiler is initialized
            CheckCompiler();

            // Convert to array
            string[] sources = { source };

            // Compile the code
            if (compilerService.CompileSources(sources, DynamicCSharp.Settings.assemblyReferences) == false)
            {
                compilerService.PrintErrors();
                return null;
            }

            // Print any warnings
            if (compilerService.HasWarnings == true)
                compilerService.PrintWarnings();

            // Load the assembly - This will also security check the assembly
            ScriptAssembly assembly = LoadAssembly(compilerService.AssemblyData, compilerService.SymbolsData);

            // Check for assembly
            if (assembly == null)
                return null;

            // Get the main type
            return assembly.MainType;
        }

        /// <summary>
        /// Attempts to compile and load a number of C# scripts from the specified source as a batch.
        /// Depending upon settings, the code may be security verified before being loaded which may result in an exception being thrown.
        /// </summary>
        /// <param name="sources">The source code compile as a batch</param>
        /// <returns>A <see cref="ScriptAssembly"/> containing all the compiled types</returns>
        /// <exception cref="SecurityException">The code does not meet the security restrictions defined in the settings menu</exception>
        public ScriptAssembly CompileAndLoadScriptSources(params string[] sources)
        {
            // Make sure the compiler is initialized
            CheckCompiler();

            // Compile the code
            if (compilerService.CompileSources(sources, DynamicCSharp.Settings.assemblyReferences) == false)
            {
                compilerService.PrintErrors();
                return null;
            }

            // Print any warnings
            if (compilerService.HasWarnings == true)
                compilerService.PrintWarnings();

            // Load the assembly - This will also security check the assembly
            return LoadAssembly(compilerService.AssemblyData, compilerService.SymbolsData);
        }

        /// <summary>
        /// Attempts to compile and load a number of C# script files asynchronously.
        /// The compile will take place on a background thread so that you can perform other tasks on the main thread.
        /// Depending upon settings, the code may be security verified before being loaded which may result in an exception being thrown.
        /// </summary>
        /// <param name="files">An array of C# source files to compile</param>
        /// <returns>A yieldable <see cref="AsyncCompileLoadOperation"/> object which contains state information for the compile operation</returns>
        public AsyncCompileLoadOperation CompileAndLoadScriptFilesAsync(params string[] files)
        {
            // Make sure the compiler is initialized
            CheckCompiler();

            // Load references from settings on main thread
            string[] references = DynamicCSharp.Settings.assemblyReferences;

            return new AsyncCompileLoadOperation(this, () =>
            {
                // Compile from files
                bool result = compilerService.CompileFiles(files, references);

                // Print any errors
                if (compilerService.HasErrors == true)
                    compilerService.PrintErrors();

                // Print any warnings
                if (compilerService.HasWarnings == true)
                    compilerService.PrintWarnings();

                return result;
            });
        }

        /// <summary>
        /// Attempts to compile and load the specified C# sources asynchronously.
        /// The compile will take place on a background thread so that you can perform other tasks on the main thread.
        /// Depending upon settings, the code may be security verified before being loaded which may result in an exception being thrown.
        /// </summary>
        /// <param name="sources">An array of strings whose contents are valid C# source code</param>
        /// <returns>A yieldable <see cref="AsyncCompileLoadOperation"/> object which contains state information for the compile operation</returns>
        public AsyncCompileLoadOperation CompileAndLoadScriptSourcesAsync(params string[] sources)
        {
            // Make sure the compiler is initialized
            CheckCompiler();

            // Load references from settings on main thread
            string[] references = DynamicCSharp.Settings.assemblyReferences;

            return new AsyncCompileLoadOperation(this, () =>
            {
                // Compile from source
                bool result = compilerService.CompileSources(sources, references);

                // Print any errors
                if (compilerService.HasErrors == true)
                    compilerService.PrintErrors();

                // Print any warnings
                if (compilerService.HasWarnings == true)
                    compilerService.PrintWarnings();

                return result;
            });
        }
        #endregion

        #region SecutiryCheck
        /// <summary>
        /// Attempts to perform security validation on the assembly at the specified location.
        /// Failure to load the data will result in a fail result.
        /// </summary>
        /// <param name="fullpath">The fullpath to the managed assembly</param>
        /// <param name="throwOnError">Should the checker throw an exception on error</param>
        /// <returns>True if the assembly passed security checks or false if the assembly breaches security or another error occured</returns>
        /// <exception cref="SecurityException">The code does not meet the security restrictions defined in the settings menu</exception>
        public bool SecurityCheckAssembly(string fullpath, bool throwOnError = false)
        {
            try
            {
                byte[] assemblyData = new byte[0];

                // Read the assembly data
                using (BinaryReader reader = new BinaryReader(File.Open(fullpath, FileMode.Open)))
                {
                    // Read all bytes in the file
                    assemblyData = reader.ReadBytes((int)reader.BaseStream.Length);
                }

                // Run through checker
                checker.SecurityCheckAssembly(assemblyData);

                // Check for errors and throw security exception
                if (checker.HasErrors == true)
                    throw new SecurityException(checker.Errors[0].ToString());
            }
            catch (Exception)
            {
                // Rethrow exception
                if (throwOnError == true)
                    throw;

                // An exception occured
                return false;
            }

            // The assembly passed security
            return true;
        }

        /// <summary>
        /// Attempts to perform security validation on the specified assembly name.
        /// Failure to load the data will result in a fail result.
        /// </summary>
        /// <param name="name">The name of the assembly to check</param>
        /// <param name="throwOnError">Should the checker throw an exception on error</param>
        /// <returns>True if the assembly passed security checks or false if the assembly breaches security or another error occured</returns>
        /// <exception cref="SecurityException">The code does not meet the security restrictions defined in the settings menu</exception>
        public bool SecurityCheckAssembly(AssemblyName name, bool throwOnError = false)
        {
            try
            {
                byte[] assemblyData = new byte[0];

                // Read the assembly data
                using (BinaryReader reader = new BinaryReader(File.Open(name.CodeBase, FileMode.Open)))
                {
                    // Read all bytes in the file
                    assemblyData = reader.ReadBytes((int)reader.BaseStream.Length);
                }

                // Run through checker
                checker.SecurityCheckAssembly(assemblyData);

                // Check for errors and throw security exception
                if (checker.HasErrors == true)
                    throw new SecurityException(checker.Errors[0].ToString());
            }
            catch (Exception)
            {
                // Rethrow exception
                if (throwOnError == true)
                    throw;

                // An exception occured
                return false;
            }

            // The assembly passed security
            return true;
        }

        /// <summary>
        /// Attempts to perform security validation on the specified assembly data.
        /// Failure to load the data will result in a fail result.
        /// </summary>
        /// <param name="assemblyData">The raw data of the managed assembly</param>
        /// <param name="throwOnError">Should the checker throw an exception on error</param>
        /// <returns>True if the assembly passed security checks or false if the assembly breaches security or another error occured</returns>
        /// <exception cref="SecurityException">The code does not meet the security restrictions defined in the settings menu</exception>
        public bool SecurityCheckAssembly(byte[] assemblyData, bool throwOnError = false)
        {
            try
            {
                // Check for null args
                if (assemblyData == null)
                    throw new ArgumentNullException("assemblyData");

                // Run through checker
                checker.SecurityCheckAssembly(assemblyData);

                // Check for errors and throw security exception
                if (checker.HasErrors == true)
                    throw new SecurityException(checker.Errors[0].ToString());
            }
            catch (Exception)
            {
                // Rethrow exception
                if (throwOnError == true)
                    throw;

                // An exception occured
                return false;
            }

            // The assembly passed security
            return true;
        }
        #endregion
        

        internal void CreateCompilerService()
        {
            // Create the compiler
            compilerService = new ScriptCompiler();
        }

        private void CheckDisposed()
        {
            // Check for our sandbox domain
            if(sandbox == null)
                throw new ObjectDisposedException("The 'ScriptDomain' has already been disposed");
        }

        private void CheckCompiler()
        {
            // Check for our compiler service
            if (compilerService == null)
                throw new Exception("The compiler service has not been initialized");
        }

        /// <summary>
        /// Creates a new <see cref="ScriptDomain"/> into which assemblies and scripts may be loaded.
        /// </summary>
        /// <returns>A new instance of <see cref="ScriptDomain"/></returns>
        public static ScriptDomain CreateDomain(string domainName, bool initCompiler)
        {
            // Create a new named domain
            ScriptDomain domain = new ScriptDomain("DynamicCSharp");

            // Check for compiler
            if(initCompiler == true)
                domain.CreateCompilerService();

            return domain;
        }
    }
}
