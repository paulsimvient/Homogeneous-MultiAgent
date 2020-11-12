using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DynamicCSharp
{
    /// <summary>
    /// A <see cref="ScriptEvaluator"/> can be used to dynamically evaluate C# source code outside of the context of a class or struct method body.
    /// Unlike other C# Eval methods, the evaluator must compile the C# code before it can be executed which results in A slight performance impact where the code is dynamically compiled. The code can then be executed at full speed.
    /// Once a piece of eval code has been run for the first time, it will then be auto-cached meaning that any subsequent calls to eval with the same source code will result in lightning fast execution as the code will not be recompiled.
    /// Note that the first time the active <see cref="Compiler.ScriptCompiler"/> is invoked, there will be slight overhead of loading its resources into memory. Subsequent compilation tasks will be performed much quicker.
    /// </summary>
    /// <example>
    /// <code>
    /// using UnityEngine;
    /// using DynamicCSharp;
    /// 
    /// class ExampleClass
    /// {
    ///     void ExampleMethod()
    ///     {
    ///         var evaluator = new ScriptEvaluator(ScriptDomain.CreateDomain("ExampleDomain", true));
    ///         
    ///         Debug.Log(evaluator.Eval("return 20 / 4 + 6"));
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class ScriptEvaluator
    {
        // Private
        private static Dictionary<string, ScriptProxy> evalCache = new Dictionary<string, ScriptProxy>();

        private ScriptDomain domain = null;
        private TextAsset templateSource = null;

        private Dictionary<string, Delegate> bindingDelegates = new Dictionary<string, Delegate>();
        private List<Variable> bindingVariables = new List<Variable>();
        private List<string> usingStatements = new List<string>();        

        // These are helper tags used to construct the source from the template file.
        private const string templateResource = "DynamicCSharp_EvalTemplate";
        private const string entryClass = "_EvalClass";
        private const string entryMethod = "_EvalEntry";
        private const string returnObject = "_returnVal";

        // These tags are defined in the templat file - We will search for them and replace them with the correct information
        private const string tagUsingStatements = "[TAG_USINGSTATEMENTS]";
        private const string tagClassName = "[TAG_CLASSNAME]";
        private const string tagFieldStatements = "[TAG_FIELDSTATEMENTS]";
        private const string tagDelegateStatements = "[TAG_DELEGATESTATEMENTS]";
        private const string tagMethodName = "[TAG_METHODNAME]";
        private const string tagMethodBody = "[TAG_METHODBODY]";
        
        // These tags are used to construct valid C# syntax
        private const string tagUsing = "using";
        private const string tagSpace = " ";
        private const string tagSemiColon = ";";
        private const string tagComma = ",";
        private const string tagArrowL = "<";
        private const string tagArrowR = ">";

        // Public
        /// <summary>
        /// When true, the auto-generated C# source code will be written to file for inspection.
        /// This file will be located in the current executing folder.
        /// </summary>
        public static bool outputGeneratedSourceIfDebug = true;

        // Constructor
        /// <summary>
        /// Create a new <see cref="ScriptEvaluator"/> that is able to execute C# code outside of a method body. 
        /// </summary>
        /// <param name="domain">The domain that the evaluated code should run under. If null is passed then the active domain is used. The domain must have an active <see cref="Compiler.ScriptCompiler"/></param>
        /// <exception cref="ArgumentNullException">The specified domain is null and there is no active global domain</exception>
        /// <exception cref="ArgumentException">The specified domain does not have a compiler service initialized and is unable to compile code</exception>
        public ScriptEvaluator(ScriptDomain domain = null)
        {
            // Use active domain if null is supplied
            if (domain == null)
                domain = ScriptDomain.Active;

            // Check for domain
            if (domain == null)
                throw new ArgumentNullException("The specified domain was null and there are no active domains");

            //  Check for compiler
            if (domain.CompilerService == null)
                throw new ArgumentException("The specified domain does not have a compiler service registered. The compiler service is required by a ScriptEvaluator");

            // Cache the domain
            this.domain = domain;
        }

        // Methods
        /// <summary>
        /// Bind a variable to the evaulated code so that data can be shared between the calling application and the executing source code.
        /// Once a variable has been bound to the <see cref="ScriptEvaluator"/>, the evaluated code can access the variable via its name.  
        /// </summary>
        /// <param name="name">The name of the value. The evaluated code can use this name to access calling application variables</param>
        /// <param name="value">The initial value of the bound variable</param>
        /// <returns>A <see cref="Variable"/> representing the bound data which can be used to access the modified variable data after source code has been evaluated</returns>
        public Variable BindVar(string name, object value = null)
        {
            // Call through
            return BindVar<object>(name, value);
        }

        /// <summary>
        /// Bind a variable of specified type to the evaluated code so that data can be shared between the calling application and the executing source code.
        /// Once a variable has been bound to the <see cref="ScriptEvaluator"/>, the evaluated code can access the variable via its name. 
        /// </summary>
        /// <typeparam name="T">The generic variable type to bind</typeparam>
        /// <param name="name">The name of the value. The evaluated code can use this name to access calling application variables</param>
        /// <param name="value">The initiaal value of the bound variable</param>
        /// <returns>A <see cref="Variable"/> representing the bound data which can be used to access the modified variable data after the source code has been evaluated</returns>
        public Variable<T> BindVar<T>(string name, T value = default(T))
        {
            // Check if the variable has already been bound
            foreach(Variable var in bindingVariables)
            {
                // Check for matching name
                if(var.Name == name)
                {
                    // Just update the already bound variable to avoid creating duplicates
                    var.Update(value);
                    return var as Variable<T>;
                }
            }

            // Create the variable
            Variable<T> result = new Variable<T>(name, value);

            // Bind the data
            bindingVariables.Add(result);

            // Check if we should auto-add a using statement for the data
            if (result.Value != null)
            {
                // Get the system type
                Type type = result.Value.GetType();

                // Get the namespace name
                AddUsing(type.Namespace);
            }

            return result;
        }

        /// <summary>
        /// Bind a delegate Action to the evaluated code so that method in the calling application can be invoked by the evaluated code.
        /// Once a delegate has been bound to the <see cref="ScriptEvaluator"/>, the evaluated code can access the delegate via its name. 
        /// </summary>
        /// <param name="name">The name of the delegate. The evaluated code can use this name to invoke the delegate</param>
        /// <param name="action">The <see cref="Action"/> delegate to bind</param>
        public void BindDelegate(string name, Action action)
        {
            if(bindingDelegates.ContainsKey(name) == true)
            {
                // Overwrite the registered delegate
                bindingDelegates[name] = action;
                return;
            }

            // Add to collection
            bindingDelegates.Add(name, action);
        }

        /// <summary>
        /// Bind a delegate Action to the evaluated code so that method in the calling application can be invoked by the evaluated code.
        /// Once a delegate has been bound to the <see cref="ScriptEvaluator"/>, the evaluated code can access the delegate via its name.
        /// This overload allows an argument of the specified generic type to be passed to the delegate when called.
        /// </summary>
        /// <typeparam name="T">The generic type of the argument that is accepted by the delegate</typeparam>
        /// <param name="name">The name of the delegate. The evaluated code can use this name to invoke the delegate</param>
        /// <param name="action">The <see cref="Action{T}"/> delegate to bind</param>
        public void BindDelegate<T>(string name, Action<T> action)
        {
            if (bindingDelegates.ContainsKey(name) == true)
            {
                // Overwrite the registered delegate
                bindingDelegates[name] = action;
                return;
            }

            // Add to collection
            bindingDelegates.Add(name, action);
        }

        /// <summary>
        /// Bind a delegate Func to the evaluated code so that method in the calling application can be invoked by the evaluated code.
        /// Once a delegate has been bound to the <see cref="ScriptEvaluator"/>, the evaluated code can access the delegate via its name.
        /// </summary>
        /// <typeparam name="R">The generic return value of the delegate</typeparam>
        /// <param name="name">The name of the delegate. The evaluated code can use this name to invoke the delegate</param>
        /// <param name="func">The <see cref="Func{TResult}"/> to bind</param>
        public void BindDelegate<R>(string name, Func<R> func)
        {
            if (bindingDelegates.ContainsKey(name) == true)
            {
                // Overwrite the registered delegate
                bindingDelegates[name] = func;
                return;
            }

            // Add to collection
            bindingDelegates.Add(name, func);
        }

        /// <summary>
        /// Bind a delegate Func to the evaluated code so that method in the calling application can be invoked by the evaluated code.
        /// Once a delegate has been bound to the <see cref="ScriptEvaluator"/>, the evaluated code can access the delegate via its name.
        /// This overload allows an argument of the specified generic type 'T' to be passed to the delegate when called.
        /// </summary>
        /// <typeparam name="R">The generic return value of the delegate</typeparam>
        /// <typeparam name="T">The generic argument type of the delegate</typeparam>
        /// <param name="name">The name of the delegate. The evaluated code can use this name to invoke the delegate</param>
        /// <param name="func">The <see cref="Func{T, TResult}"/> to bind</param>
        public void BindDelegate<R, T>(string name, Func<T, R> func)
        {
            if (bindingDelegates.ContainsKey(name) == true)
            {
                // Overwrite the registered delegate
                bindingDelegates[name] = func;
                return;
            }

            // Add to collection
            bindingDelegates.Add(name, func);
        }

        /// <summary>
        /// Clears any bound variables from this evaluator.
        /// This may be useful if you want to re-use the evaluator for many different evaluations with different bindings.
        /// </summary>
        public void ClearVarBindings()
        {
            // Clear all bound variables
            bindingVariables.Clear();
        }

        /// <summary>
        /// Clears any bound delegates from this evaluator.
        /// This may be useful if you want to re-use the evaluator for many different evaluations with different bindings.
        /// </summary>
        public void ClearDelegateBindings()
        {
            // Clear all bound delegates
            bindingDelegates.Clear();
        }

        /// <summary>
        /// Add the specified namespace reference to the evaluator.
        /// The assembly that containts the namespace must be loaded before any calls to <see cref="Eval(string)"/> are made. 
        /// Note that namespaces are automatically added when binding variables whose types are defined in a different namespace.
        /// </summary>
        /// <param name="namespaceName">The namespace to add to the evaluator</param>
        public void AddUsing(string namespaceName)
        {
            // Check for already added
            if (usingStatements.Contains(namespaceName) == false)
            {
                // Register the namespace
                usingStatements.Add(namespaceName);
            }
        }

        /// <summary>
        /// Attempt to evaluate the specified source code. 
        /// The provided source code will be compiled (unless cached) and then executed in <see cref="ScriptDomain"/> specified in the constructor. 
        /// Any valid C# method body syntax is accepted. You may not define user types or methods in the provided source. Type declarations are allowed.
        /// </summary>
        /// <param name="sourceCode">The string source code the execute</param>
        /// <returns>A <see cref="Variable"/> representing the return value of the evaluated code. If the evaluated code does not return a value then the result will be a <see cref="Variable"/> representing 'new object()'. If the code failed to run then the return value will be null</returns>
        public Variable Eval(string sourceCode)
        {
            return Eval<object>(sourceCode);
        }

        /// <summary>
        /// Attempt to evaluate the specified source code. 
        /// The provided source code will be compiled (unless cached) and then executed in <see cref="ScriptDomain"/> specified in the constructor. 
        /// Any valid C# method body syntax is accepted. You may not define user types or methods in the provided source. Type declarations are allowed.
        /// </summary>
        /// <typeparam name="T">The generic type that the evaluated code should return</typeparam>
        /// <param name="sourceCode">The string source code to execute</param>
        /// <returns>A <see cref="Variable"/> representing the return value of the evaluated code. If the evaluated code does not return a value then the result will be a <see cref="Variable"/> representing 'default(T)'. If the code failed to run then the return value will be null</returns>
        public Variable<T> Eval<T>(string sourceCode)
        {
            ScriptProxy proxy = null;

            // Check whether this source is cached
            bool isCached = evalCache.ContainsKey(sourceCode);

            // Check if we have a cached version of this source code
            if (isCached == true)
            {
                // Get the cached type
                proxy = evalCache[sourceCode];
            }
            else
            {
                // Get the full C# source code
                string source = BuildSourceAroundTemplate(sourceCode);

#if UNITY_EDITOR && !UNITY_WEBPLAYER
                // Check if we should output the generated source
                if (outputGeneratedSourceIfDebug == true)
                    System.IO.File.WriteAllText("DynamicCSharp_Eval_GeneratedSource.cs", source);
#endif

                // Try to compile the code
                ScriptType type = domain.CompileAndLoadScriptSource(source);

                // Looks like we failed to compile or find a cached type
                if (type == null)
                    return null;

                // Create an instance
                proxy = type.CreateInstance();

                // Check for error
                if (proxy == null)
                    return null;
            }          

            // Check if we need to cache the type
            if(isCached == false)
            {
                // Make a cache entry
                evalCache.Add(sourceCode, proxy);
            }

            // Bind the delegates to the proxy
            BindProxyDelegates(proxy);

            // Bind the values to the proxy
            BindProxyVars(proxy);

            // Bind our return value
            object returnResult = new object();

            proxy.Fields[returnObject] = returnResult;

            // Call the method
            object result = proxy.SafeCall(entryMethod);

            // Read the value back into evaluator memory space
            UnbindProxyVars(proxy);

            // Check if we returned a value
            if (returnResult == result)
                return new Variable<T>(returnObject, default(T));

            // Create a default error value that we can use if we fail to get the correct type
            T resolvedReturn = default(T);

            try
            {
                // Try to cast to type
                resolvedReturn = (T)result;
            }
            catch (InvalidCastException) { }

            // We successfully ran the code
            return new Variable<T>(returnObject, resolvedReturn);
        }

        /// <summary>
        /// Binds the current values of all binding variables to the specified <see cref="ScriptProxy"/>. 
        /// </summary>
        /// <param name="proxy">The proxy to bind the variables to</param>
        private void BindProxyVars(ScriptProxy proxy)
        {
            foreach(Variable var in bindingVariables)
            {
                // Bind the value to the variable
                proxy.Fields[var.Name] = var.Value;
            }
        }

        /// <summary>
        /// Unbinds the values from the specified <see cref="ScriptProxy"/>. 
        /// </summary>
        /// <param name="proxy">The proxy to unbind the variables from</param>
        private void UnbindProxyVars(ScriptProxy proxy)
        {
            foreach(Variable var in bindingVariables)
            {
                // Unbind the value
                var.Update(proxy.Fields[var.Name]);
            }
        }

        /// <summary>
        /// Binds the current delegate list of binding delegates to the specified <see cref="ScriptProxy"/>. 
        /// </summary>
        /// <param name="proxy">The proxy to bind the delegates to</param>
        private void BindProxyDelegates(ScriptProxy proxy)
        {
            // Create a temp list of names
            List<string> names = new List<string>(bindingDelegates.Keys);

            foreach (string name in names)
            {
                // Get the delegate
                Delegate action = bindingDelegates[name];

                // Assign the delegate
                proxy.Fields[name] = action;
            }
        }

        /// <summary>
        /// Used to embedd the specified source code inside a valid class and method context so that the resulting source code is valid C# that can be compiled and executed.
        /// This method uses a template source file which uses tags to identify key points in the source code.
        /// </summary>
        /// <param name="source">The evaluator source code that should be converted into compilable C#</param>
        /// <returns>The specified source code which has been converted into a compilable state</returns>
        private string BuildSourceAroundTemplate(string source)
        {
            // Load template source code 
            string template = GetTemplateSource();

            // Create using statemenets
            template = template.Replace(tagUsingStatements, GetUsingStatementsSource());

            // Create delegate statements
            template = template.Replace(tagDelegateStatements, GetDelegateStatementsSource());

            // Create field statements
            template = template.Replace(tagFieldStatements, GetFieldStatementsSource());


            // Replace names
            template = template.Replace(tagClassName, entryClass + Guid.NewGuid().ToString("N"));
            template = template.Replace(tagMethodName, entryMethod);

            // Inject the source into the method
            template = template.Replace(tagMethodBody, source);

            return template;
        }

        /// <summary>
        /// Attempt to load the C# template source file from the Unity project.
        /// </summary>
        /// <returns>The full source code of the template file</returns>
        private string GetTemplateSource()
        {
            // Check for already loaded
            if(templateSource == null)
            {
                // Load from assets
                templateSource = Resources.Load<TextAsset>(templateResource);
            }

            // Get the content
            return templateSource.text;
        }

        /// <summary>
        /// Attempts to generate all using statements required by the evaluated code.
        /// </summary>
        /// <returns>A C# source code string representing all required using statements in valid syntax</returns>
        private string GetUsingStatementsSource()
        {
            StringBuilder builder = new StringBuilder();

            // Process all namespaces
            foreach(string namespaceName in usingStatements)
            {
                // using statements are formatted like: 'using namespaceName;'
                builder.Append(tagUsing);
                builder.Append(tagSpace);
                builder.Append(namespaceName);
                builder.Append(tagSemiColon);

                // Create new line
                builder.AppendLine();
            }

            // Get the full string
            return builder.ToString();
        }

        /// <summary>
        /// Attempts to generate all delegate field statements required by the evaluated code.
        /// </summary>
        /// <returns>A C# source code string representing all required delegate fields in valid syntax</returns>
        private string GetDelegateStatementsSource()
        {
            StringBuilder builder = new StringBuilder();

            // Process all delegates
            foreach(KeyValuePair<string, Delegate> del in bindingDelegates)
            {
                // Get the delegate
                Delegate delegateType = del.Value;

                // Get the target method
                MethodInfo method = delegateType.Method;
                                
                // Get method parameters
                ParameterInfo[] parameters = method.GetParameters();

                // Get return type and argument types
                Type returnType = method.ReturnType;
                Type[] argTypes = new Type[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                    argTypes[i] = parameters[i].ParameterType;

                // Check for action or function
                if(returnType == typeof(void))
                {
                    builder.Append(typeof(Action).FullName);

                    // Check for ay arguments
                    if(argTypes.Length > 0)
                    {
                        builder.Append(tagArrowL);

                        for(int i = 0; i < argTypes.Length; i++)
                        {
                            // Get the declaration name
                            string declarationName = argTypes[i].FullName;

                            // Add the system type
                            builder.Append(declarationName);

                            // Check for comma
                            if (i < (argTypes.Length - 1))
                                builder.Append(tagComma);
                        }

                        builder.Append(tagArrowR);
                    }

                    builder.Append(tagSpace);
                    builder.Append(del.Key);
                    builder.Append(tagSemiColon);

                    // Create new line
                    builder.AppendLine();
                }
                else
                {
                    // Note - we remove the "`1" from the typename so it becomes a declarable type name
                    builder.Append(typeof(Func<>).FullName.Replace("`1", ""));                        
                    builder.Append(tagArrowL);

                    // Check for any arguments
                    if (argTypes.Length > 0)
                    {
                        for (int i = 0; i < argTypes.Length; i++)
                        {
                            // Get the declaration name
                            string declarationName = argTypes[i].FullName;

                            // Add the system type
                            builder.Append(declarationName);
                            
                            // Add a comma
                            builder.Append(tagComma);
                        }
                    }

                    // Add return val
                    string returnDeclarationName = returnType.FullName;

                    // Add system return type
                    builder.Append(returnDeclarationName);
                    builder.Append(tagArrowR);
                    builder.Append(tagSpace);
                    builder.Append(del.Key);
                    builder.Append(tagSemiColon);

                    // Create new line
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Attempts to generate all field statements required by the evaluated code.
        /// </summary>
        /// <returns>A C# source code string representing all required fields in valid syntax</returns>
        private string GetFieldStatementsSource()
        {
            StringBuilder builder = new StringBuilder();

            // Process all variables
            foreach(Variable var in bindingVariables)
            {
                // Get the data type for the variable
                object data = var.Value;

                // Get the data type so we can declare it
                Type type = (data == null) ? typeof(object) : data.GetType();

                // Get the declaration name
                string declarationName = type.FullName;

                // Field statements are formatted like: 'Namespace.Type varName;'
                builder.Append(declarationName);
                builder.Append(tagSpace);
                builder.Append(var.Name);
                builder.Append(tagSemiColon);

                // Create a new line
                builder.AppendLine();
            }

            // Get the full string
            return builder.ToString();
        }

        /// <summary>
        /// Forces all cached evaluation data to be cleared.
        /// Any evaluated code that is executed will be cached by default so that it will run at full runtime performance if it is executed again.
        /// You can use this method to force the evaluated code to be compiled before it is executed.
        /// </summary>
        public static void ClearCache()
        {
            // Clear already cached types
            evalCache.Clear();
        }
    }
}
