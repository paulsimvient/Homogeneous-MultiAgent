using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Security.Cryptography;
using Mono.CSharp;

namespace DynamicCSharp.Compiler
{
    internal sealed class McsDriver
    {
        // Private
        private readonly CompilerContext context = null;

        // Properties
        public Report Report
        {
            get { return context.Report; }
        }

        // Constructor
        public McsDriver(CompilerContext context)
        {
            this.context = context;
        }

        // Methods
        public void TokenizeFile(SourceFile source, ModuleContainer module, ParserSession session)
        {
            Stream input = null;

            try
            {
                // Get the stream
                input = source.GetDataStream();
            }
            catch
            {
                // Generate an error
                Report.Error(2001, "Failed to open file '{0}' for reading", source.Name);
                return;
            }

            // Manage the stream correctly
            using (input)
            {
                // Create a seekable stream
                SeekableStreamReader reader = new SeekableStreamReader(input, context.Settings.Encoding);
                
                // Create compilation source
                CompilationSourceFile file = new CompilationSourceFile(module, source);

                // Create a token lexer
                Tokenizer lexer = new Tokenizer(reader, file, session, context.Report);

                int currentToken = 0;
                int tokenCount = 0;
                int errorCount = 0;

                while((currentToken = lexer.token()) != Token.EOF)
                {
                    // Increment the count
                    tokenCount++;

                    // Check for error token
                    if (currentToken == Token.ERROR)
                        errorCount++;
                }
            }
        }

        public void Parse(ModuleContainer module)
        {
            bool tokenize = module.Compiler.Settings.TokenizeOnly;

            // Get source files
            List<SourceFile> sources = module.Compiler.SourceFiles;

            Location.Initialize(sources);

            // Create the session
            ParserSession session = new ParserSession
            {
                UseJayGlobalArrays = true,
                LocatedTokens = new LocatedToken[15000],
            };

            for (int i = 0; i < sources.Count; ++i)
            {
                if (tokenize == true)
                {
                    // Only tokenize the file
                    TokenizeFile(sources[i], module, session);
                }
                else
                {
                    // 
                    Parse(sources[i], module, session, Report);
                }
            }
        }

        public void Parse(SourceFile source, ModuleContainer module, ParserSession session, Report report)
        {
            Stream input = null;

            try
            {
                // Get the stream
                input = source.GetDataStream();
            }
            catch
            {
                // Generate an error
                Report.Error(2001, "Failed to open file '{0}' for reading", source.Name);
                return;
            }

            // Check for header
            if(input.ReadByte() == 77 && input.ReadByte() == 90)
            {
                report.Error(2015, "Failed to open file '{0}' for reading because it is a binary file. A text file was expected", source.Name);
                input.Close();
                return;
            }

            // Back to start
            input.Position = 0;

            // Create a seekable stream
            SeekableStreamReader reader = new SeekableStreamReader(input, context.Settings.Encoding, session.StreamReaderBuffer);

            // Parse the source
            Parse(reader, source, module, session, report);

            if(context.Settings.GenerateDebugInfo == true && report.Errors == 0 && source.HasChecksum == false)
            {
                // Back to start
                input.Position = 0;

                // Get the session checksum
                MD5 checksum = session.GetChecksumAlgorithm();

                // Apply the checksum
                source.SetChecksum(checksum.ComputeHash(input));
            }

            // Dispose of streams
            reader.Dispose();
            input.Close();
        }

        public bool Compile(out AssemblyBuilder assembly, AppDomain domain, bool generateInMemory)
        {
            // Get the current settings
            CompilerSettings settings = context.Settings;

            // Set the result for quick exit
            assembly = null;

            // Check if any source files were supplied
            if (settings.FirstSourceFile == null && (((MCSTarget)settings.Target == MCSTarget.Exe || (MCSTarget)settings.Target == MCSTarget.WinExe || (MCSTarget)settings.Target == MCSTarget.Module) || settings.Resources == null))
            {
                Report.Error(2008, "No source files specified");
                return false;
            }

            // Check for any invalid settings
            if (settings.Platform == Platform.AnyCPU32Preferred && ((MCSTarget)settings.Target == MCSTarget.Library || (MCSTarget)settings.Target == MCSTarget.Module))
            {
                Report.Error(4023, "The preferred platform '{0}' is only valid on executable outputs", Platform.AnyCPU32Preferred.ToString());
                return false;
            }

            // Create the time reporter
            TimeReporter time = new TimeReporter(settings.Timestamps);
            context.TimeReporter = time;
            time.StartTotal();

            // Create the module
            ModuleContainer module = new ModuleContainer(context);
            RootContext.ToplevelTypes = module;

            // Start timing the parse stage
            time.Start(TimeReporter.TimerType.ParseTotal);
            {
                // Begin parse
                Parse(module);
            }
            time.Stop(TimeReporter.TimerType.ParseTotal);

            // Check for any errors
            if (Report.Errors > 0)
                return false;

            // Check for partial compilation
            if (settings.TokenizeOnly == true || settings.ParseOnly == true)
            {
                time.StopTotal();
                time.ShowStats();
                return true;
            }

            // Get the output file
            string output = settings.OutputFile;
            string outputName = Path.GetFileName(output);

            // Create an assembly defenition
            AssemblyDefinitionDynamic defenition = new AssemblyDefinitionDynamic(module, outputName, output);
            module.SetDeclaringAssembly(defenition);

            ReflectionImporter importer = new ReflectionImporter(module, context.BuiltinTypes);
            defenition.Importer = importer;

            DynamicLoader loader = new DynamicLoader(importer, context);            
            loader.LoadReferences(module);

            // Validate built in types
            if (context.BuiltinTypes.CheckDefinitions(module) == false)
                return false;

            // Create the assmbly in domain
            if (defenition.Create(domain, AssemblyBuilderAccess.RunAndSave) == false)
                return false;

            module.CreateContainer();
            loader.LoadModules(defenition, module.GlobalRootNamespace);
            module.InitializePredefinedTypes();

            // Check for any resource strings
            if (settings.GetResourceStrings != null)
                module.LoadGetResourceStrings(settings.GetResourceStrings);

            // Time the module defenition
            time.Start(TimeReporter.TimerType.ModuleDefinitionTotal);
            {
                try
                {
                    // Begin defining
                    module.Define();
                }
                catch
                {
                    // Failed to define module
                    return false;
                }
            }
            time.Stop(TimeReporter.TimerType.ModuleDefinitionTotal);

            // Check for any errors
            if (Report.Errors > 0)
                return false;
            
            // Check for documentation
            if(settings.DocumentationFile != null)
            {
                // Build the xml docs file
                DocumentationBuilder docs = new DocumentationBuilder(module);
                docs.OutputDocComment(output, settings.DocumentationFile);
            }

            defenition.Resolve();

            // Check for documentation errors
            if (Report.Errors > 0)
                return false;

            // Finally emit the defenition into something useful
            time.Start(TimeReporter.TimerType.EmitTotal);
            {
                // Emit assembly
                defenition.Emit();
            }
            time.Stop(TimeReporter.TimerType.EmitTotal);

            // Check for any emit errors
            if (Report.Errors > 0)
                return false;

            // Module cleanup
            time.Start(TimeReporter.TimerType.CloseTypes);
            {
                module.CloseContainer();
            }
            time.Stop(TimeReporter.TimerType.CloseTypes);

            // Check for embedded resources
            time.Start(TimeReporter.TimerType.Resouces);
            {
                if (settings.WriteMetadataOnly == false)
                    defenition.EmbedResources();
            }
            time.Stop(TimeReporter.TimerType.Resouces);

            // Embedd errors
            if (Report.Errors > 0)
                return false;

            // Check for generate in memory
            if (generateInMemory == false)
                defenition.Save();

            // Store the result
            assembly = defenition.Builder;

            time.StopTotal();
            time.ShowStats();

            // Check for errors
            return Report.Errors == 0;
        }

        public static void Parse(SeekableStreamReader reader, SourceFile source, ModuleContainer module, ParserSession session, Report report)
        {
            // Create the compilation source
            CompilationSourceFile file = new CompilationSourceFile(module, source);

            // Add to module
            module.AddTypeContainer(file);

            // Create the parser and run
            CSharpParser parser = new CSharpParser(reader, file, report, session);
            parser.parse();
        }
    }
}
