using System;
using System.IO;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection.Emit;
using Mono.CSharp;
using System.Reflection;

namespace DynamicCSharp.Compiler
{
    // This is an exact clone of (Mono.CSharp.Target in mcs.dll) and is used to avoid errors about multple defined types in different assemblies -
    // Unity has also added this type in 2017.3.0 (Mono.CSharp.Taget in SyntaxTree.VisualStudio.Unity.Bridge.dll)
    // The enum value can be read from by casting Target to MSCTarget but can only be assigned to via reflection (casting to int32 beforehand)
    internal enum MCSTarget
    {
        Library = 0,
        Exe = 1,
        Module = 2,
        WinExe = 3,
    }

    internal sealed class McsCompiler : ICodeCompiler
    {
        // Private
        private static string outputDirectory = "";
        private static bool generateSymbols = false;
        private static long assemblyCounter = 0;

        // Properties
        internal static string OutputDirectory
        {
            get { return outputDirectory; }
            set
            {
                if (value != "" && Directory.Exists(value) == false)
                    throw new IOException("The specified directory path does not exist. Make sure the specified directory path exists before setting this property");

                outputDirectory = value;
            }
        }

        internal static bool GenerateSymbols
        {
            get { return generateSymbols; }
            set { generateSymbols = value; }
        }

        // Methods
        public CompilerResults CompileAssemblyFromDom(CompilerParameters options, CodeCompileUnit compilationUnit)
        {
            // Call through
            return CompileAssemblyFromDomBatch(options, new[] { compilationUnit });
        }

        public CompilerResults CompileAssemblyFromDomBatch(CompilerParameters options, CodeCompileUnit[] compilationUnits)
        {
            // CHeck for null
            if (options == null)
                throw new ArgumentNullException("options");

            try
            {
                // Call through
                return CompileFromDomBatch(options, compilationUnits);
            }
            finally
            {
                // Delete any tempary files
                options.TempFiles.Delete();
            }
        }

        public CompilerResults CompileAssemblyFromFile(CompilerParameters options, string fileName)
        {
            // Call through
            return CompileAssemblyFromFileBatch(options, new[] { fileName });
        }

        public CompilerResults CompileAssemblyFromFileBatch(CompilerParameters options, string[] fileNames)
        {
            // Check for null
            if (options == null)
                throw new ArgumentNullException("options");

            // Get the compiler settings
            CompilerSettings settings = GetSettings(options);

            foreach(string file in fileNames)
            {
                string path = Path.GetFullPath(file);

                // Create a new source unit
                SourceFile unit = new SourceFile(file, path, settings.SourceFiles.Count + 1);

                // Add the source file
                settings.SourceFiles.Add(unit);
            }

            // Call through compile
            return CompileFromSettings(settings, options.GenerateInMemory);
        }

        public CompilerResults CompileAssemblyFromSource(CompilerParameters options, string source)
        {
            // Call through
            return CompileAssemblyFromSourceBatch(options, new[] { source });
        }

        public CompilerResults CompileAssemblyFromSourceBatch(CompilerParameters options, string[] sources)
        {
            // Check for null
            if (options == null)
                throw new ArgumentNullException("options");

            // Get the compiler settings
            CompilerSettings settings = GetSettings(options);

            for(int i = 0; i < sources.Length; i++)
            {
                // Get the local source
                string source = sources[i];

                // Called when the source stream is opened for reading
                Func<Stream> openStream = () =>
                {
                    // Get the source code or empty string
                    string content = (string.IsNullOrEmpty(source) == true) ? string.Empty : source;

                    // Create a memory stream from the source
                    return new MemoryStream(Encoding.UTF8.GetBytes(content));
                };
                
                // Create the source unit
                SourceFile unit = new SourceFile(string.Empty, string.Empty, settings.SourceFiles.Count + 1, openStream);

                // Add to settings
                settings.SourceFiles.Add(unit);
            }

            // Call through compile
            return CompileFromSettings(settings, options.GenerateInMemory);
        }



        private CompilerResults CompileFromDomBatch(CompilerParameters options, CodeCompileUnit[] compilationUnits)
        {
            throw new NotImplementedException("Use compile from source or file!");
        }

        private CompilerResults CompileFromSettings(CompilerSettings settings, bool generateInMemory)
        {
            // Create the compiler results
            CompilerResults compilerResults = new CompilerResults(new TempFileCollection(Path.GetTempPath()));
            
            // Create the compiler driver
            McsDriver driver = new McsDriver(new CompilerContext(settings, new McsReporter(compilerResults)));

            AssemblyBuilder outAssembly = null;
            try
            {
                // Try to compile
                driver.Compile(out outAssembly, AppDomain.CurrentDomain, generateInMemory);
            }
            catch (Exception e)
            {
                // Add the exception as an error
                compilerResults.Errors.Add(new CompilerError()
                {
                    IsWarning = false,
                    ErrorText = e.ToString(),
                });
            }

            // Get the compiled assembly
            compilerResults.CompiledAssembly = outAssembly;

            return compilerResults;
        }

        private void SetTargetEnumField(FieldInfo field, object instance, MCSTarget target)
        {
            try
            {
                // Try to set the value (Enum - Enum converion is not supported but int - Enum is so we can cast to int to get around that issue)
                field.SetValue(instance, (int)target);
            }
            catch { }
        }

        private CompilerSettings GetSettings(CompilerParameters parameters)
        {
            CompilerSettings settings = new CompilerSettings();

            // Reset parameter output

            // Copy references
            foreach (string assembly in parameters.ReferencedAssemblies)
                settings.AssemblyReferences.Add(assembly);
            
            settings.Encoding = Encoding.UTF8;
            settings.GenerateDebugInfo = parameters.IncludeDebugInformation;
            settings.MainClass = parameters.MainClass;
            settings.Platform = Platform.AnyCPU;
            settings.StdLibRuntimeVersion = RuntimeVersion.v4;

            // Store the type - We need to do some hacky stuff here because Unity copied the Mono.CSharp.Target enum which caused duplication errors. 
            // Normally this can be fixed by creating a reference alias but since Unity also manages the .csproj file this will not work
            FieldInfo field = typeof(CompilerSettings).GetField("Target");

            // Target settings
            if (parameters.GenerateExecutable == true)
            {
                //settings.Target = Target.Exe;         // The type 'Target' exists in 2 assemblies
                SetTargetEnumField(field, settings, MCSTarget.Exe);
                settings.TargetExt = ".exe";
            }
            else
            {
                //settings.Target = Target.Library;     // The type 'Target' exists in 2 assemblies
                SetTargetEnumField(field, settings, MCSTarget.Library);
                settings.TargetExt = ".dll";
            }

            // Target location
            if (parameters.GenerateInMemory == true)
                SetTargetEnumField(field, settings, MCSTarget.Library);
                //settings.Target = Target.Library;     // The type 'Target' exists in 2 assemblies

            // Generate a name for the output
            {
                parameters.OutputAssembly = settings.OutputFile = Path.Combine(outputDirectory, "DynamicAssembly_" + assemblyCounter + settings.TargetExt);
                assemblyCounter++;
            }

            settings.OutputFile = parameters.OutputAssembly;
            settings.GenerateDebugInfo = generateSymbols;
            settings.Version = LanguageVersion.Default;
            settings.WarningLevel = parameters.WarningLevel;
            settings.WarningsAreErrors = parameters.TreatWarningsAsErrors;
            settings.Optimize = false;

            return settings;
        }
    }
}