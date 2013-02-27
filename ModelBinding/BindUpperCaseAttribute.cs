using System;

namespace EclipseLibrary.Mvc.ModelBinding
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class BindUpperCaseAttribute : Attribute
    {
    }
}
