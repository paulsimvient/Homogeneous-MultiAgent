using System.CodeDom.Compiler;
using Mono.CSharp;

namespace DynamicCSharp.Compiler
{
    internal sealed class McsReporter : ReportPrinter
    {
        // Private
        private readonly CompilerResults results = null;
        private int warningCount = 0;
        private int errorCount = 0;

        // Properties
        public int WarningCount
        {
            get { return warningCount; }
        }

        public int ErrorCount
        {
            get { return errorCount; }
        }

        // Constructor
        public McsReporter(CompilerResults results)
        {
            this.results = results;
        }

        // Methods
        public override void Print(AbstractMessage msg, bool showFullPath)
        {
            // Increment counters
            if (msg.IsWarning)
                warningCount++;
            else
                errorCount++;

            // The default filename when compiling from source or similar
            string filename = "<Unknown>";

            // Make sure we have a source file
            if(msg.Location.SourceFile != null)
            {
                // Check if we should display full file paths
                if(showFullPath == true)
                {
                    // Make sure we have a file path
                    if (string.IsNullOrEmpty(msg.Location.SourceFile.FullPathName) == false)
                    {
                        // Get the file path for the source
                        filename = msg.Location.SourceFile.FullPathName;
                    }
                }
                else
                {
                    // Mak sure we have a file path
                    if (string.IsNullOrEmpty(msg.Location.SourceFile.Name) == false)
                    {
                        // Get the file name for the source
                        filename = msg.Location.SourceFile.Name;
                    }
                }
            }
            
            // Create the error
            results.Errors.Add(new CompilerError
            {
                IsWarning = msg.IsWarning,
                Column = (msg.Location.IsNull == true) ? -1 : msg.Location.Column,
                Line = (msg.Location.IsNull == true) ? -1 : msg.Location.Row,
                ErrorNumber = msg.Code.ToString(),
                ErrorText = msg.Text,
                FileName = filename,
            });
        }
    }
}