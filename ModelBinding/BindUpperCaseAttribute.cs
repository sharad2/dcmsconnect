using System;

namespace EclipseLibrary.Mvc.ModelBinding
{
    [Obsolete("Convert to Uppercase in the setter")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class BindUpperCaseAttribute : Attribute
    {
    }
}
