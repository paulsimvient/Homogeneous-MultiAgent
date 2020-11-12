using System;

namespace DynamicCSharp
{
    /// <summary>
    /// Represents a reference variable that is either bound to or returned from evaluated code.
    /// </summary>
    public class Variable
    {
        // Protected
        /// <summary>
        /// The name of this <see cref="Variable"/>.
        /// </summary>
        protected string name = string.Empty;
        /// <summary>
        /// The data for this <see cref="Variable"/>. 
        /// </summary>
        protected object data = null;

        // Properties
        /// <summary>
        /// The binding name of the variable.
        /// This is the name that will be used to access the variable data from evaluated code.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// The current value of the variable.
        /// </summary>
        public object Value
        {
            get { return data; }
        }

        // Constructor
        internal Variable(string name, object data)
        {
            this.name = name;
            this.data = data;
        }

        // Methods
        internal void Update(object data)
        {
            this.data = data;
        }

        /// <summary>
        /// Custom ToString implementation.
        /// Returns the equivilent of <see cref="Value"/> .ToString(). 
        /// </summary>
        /// <returns>A string representation of this variable</returns>
        public override string ToString()
        {
            return (data == null) ? "null" : data.ToString();
        }
    }

    /// <summary>
    /// Represents a reference variable that is either bound to or returned from evaluated code.
    /// </summary>
    /// <typeparam name="T">The generic type that this variable should hold</typeparam>
    public class Variable<T> : Variable
    {
        // Properties
        /// <summary>
        /// Overriding implmentation of <see cref="Variable.Value"/>.
        /// Note that the base property is hiden by this property which returns the value as the correct generic type.
        /// </summary>
        public new T Value
        {
            get
            {
                try
                {
                    // Try to cast data
                    return (T)data;
                }
                catch (InvalidCastException)
                {
                    return default(T);
                }
            }
        }

        // Constructor
        internal Variable(string name, T data)
            : base(name, data)
        {
        }

        // Methods
        /// <summary>
        /// Implicit operator for implicit conversion to the specified generic type.
        /// Use <see cref="Value"/> as an alternative. 
        /// </summary>
        /// <param name="var">The <see cref="Variable{T}"/> that should be converted</param>
        public static implicit operator T(Variable<T> var)
        {
            // Get the value
            return var.Value;
        }
    }
}
