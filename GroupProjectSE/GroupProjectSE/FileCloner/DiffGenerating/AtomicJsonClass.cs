// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupProjectSE.FileCloner.DiffGenerator;
public class AtomicJsonClass
{

    //public string fileName { get; set; }  // Matches the JSON key
    //public DateTime Timestamp { get; set; } // Matches the JSON key


    public string FileName { get; set; }  // Matches the JSON key
    public DateTime Timestamp { get; set; } // Matches the JSON key

    AtomicJsonClass(string fileName, DateTime timestamp)
    {
        FileName = fileName;
        Timestamp = timestamp;
    }
}
