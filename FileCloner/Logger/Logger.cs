﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareEngineeringGroupProject.FileCloner.Logger;
public class Logger
{
    private string _moduleName;
    public Logger(string moduleName)
    {
        _moduleName = moduleName;
    }

    public void Log(string message,
                    [CallerMemberName] string memberName = "",
                    [CallerFilePath] string filePath = "",
                    [CallerLineNumber] int lineNumber = 0)
    {
        // string moduleName = System.IO.Path.GetFileNameWithoutExtension(filePath);
        Trace.WriteLine($"{_moduleName}:{filePath}->{memberName}->{lineNumber} :: {message}");
    }
}