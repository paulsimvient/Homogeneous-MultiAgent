using System.IO;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System;

namespace DynamicCSharp.Compiler
{
    internal sealed class McsMarshal : ICompiler
    {
        // Private
        private ICodeCompiler compiler = null;
        private CompilerParameters parameters = null;
        private string outputDirectory = "";
        private bool generateSymbols = true;
        private byte[] assemblyData = null;
        private byte[] symbolsData = null;

        // Properties
        public string OutputDirectory
        {
            get { return outputDirectory; }
            set
            {
                // This will throw an exception if the path is invalid
                McsCompiler.OutputDirectory = value;
                outputDirectory = value;
            }
        }

        public bool GenerateSymbols
        {
            get { return generateSymbols; }
            set { generateSymbols = value; }
        }

        public byte[] AssemblyData
        {
            get { return assemblyData; }
        }

        public byte[] SymbolsData
        {
            get { return symbolsData; }
        }

        // Constructor
        public McsMarshal()
        {
            // Create our managed compiler
            compiler = new McsCompiler();

            // Create the parameters
            parameters = new CompilerParameters();
            {
                parameters.GenerateExecutable = false;
                parameters.GenerateInMemory = false;
                //parameters.CompilerOptions = "/optimize";
                parameters.IncludeDebugInformation = generateSymbols;     
                parameters.TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true);
                parameters.TempFiles.KeepFiles = true;
            }
        }

        // Methods
        public void AddReference(string reference)
        {
            // Add a reference
            parameters.ReferencedAssemblies.Add(reference);
        }

        public void AddReferences(IEnumerable<string> references)
        {
            // Add all references
            foreach (string reference in references)
                AddReference(reference);
        }

        public ScriptCompilerError[] CompileFiles(string[] files)
        {
            return CompileShared(compiler.CompileAssemblyFromFileBatch, parameters, files);
        }

        public ScriptCompilerError[] CompileSource(string[] source)
        {
            return CompileShared(compiler.CompileAssemblyFromSourceBatch, parameters, source);
        }

        private ScriptCompilerError[] CompileShared(Func<CompilerParameters, string[], CompilerResults> compileMethod, CompilerParameters parameters, string[] sourceOrFiles)
        {
            // Setup compiler paremters
            McsCompiler.OutputDirectory = outputDirectory;
            McsCompiler.GenerateSymbols = generateSymbols;

            // Reset state so assembly and symbols dont get out of sync
            assemblyData = null;
            symbolsData = null;

            // Invoke the compiler
            CompilerResults results = compileMethod(parameters, sourceOrFiles);

            // Build errors
            ScriptCompilerError[] errors = new ScriptCompilerError[results.Errors.Count];

            // Clear parameters
            parameters.ReferencedAssemblies.Clear();

            // Create error copy
            int index = 0;
            foreach (CompilerError err in results.Errors)
            {
                // Generate the error
                errors[index] = new ScriptCompilerError
                {
                    errorCode = err.ErrorNumber,
                    errorText = err.ErrorText,
                    fileName = err.FileName,
                    line = err.Line,
                    column = err.Column,
                    isWarning = err.IsWarning,
                };

                // Increment index
                index++;
            }

            // Check for success
            if (results.CompiledAssembly != null)
            {
                // Find the output name
                string assemblyName = results.CompiledAssembly.GetName().Name + ".dll";

                // Read the file
                assemblyData = File.ReadAllBytes(assemblyName);

                // Delete the temp file
                File.Delete(assemblyName);


                if (generateSymbols == true)
                {
                    // Find the symbols
                    string symbolsName = assemblyName + ".mdb";

                    // Check for file
                    if (File.Exists(symbolsName) == true)
                    {
                        // Read thef file
                        symbolsData = File.ReadAllBytes(symbolsName);

                        // Delete the temp file
                        File.Delete(symbolsName);
                    }
                }
            }

            // Get the errors
            return errors;
        }
    }
}
