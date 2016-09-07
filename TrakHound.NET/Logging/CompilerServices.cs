// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


// Used to enable autofilling parameter for CallerMemberName, CallerFilePath, and CallerLineNumber
// in .NET 4.0 (built in for .NET 4.5)
namespace System.Runtime.CompilerServices
{
    [AttributeUsageAttribute(AttributeTargets.Parameter, Inherited = false)]
    public sealed class CallerMemberNameAttribute : Attribute
    {
    }

    [AttributeUsageAttribute(AttributeTargets.Parameter, Inherited = false)]
    public sealed class CallerFilePathAttribute : Attribute
    {
    }

    [AttributeUsageAttribute(AttributeTargets.Parameter, Inherited = false)]
    public sealed class CallerLineNumberAttribute : Attribute
    {
    }
}
