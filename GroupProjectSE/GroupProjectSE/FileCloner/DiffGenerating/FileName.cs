// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupProjectSE.FileCloner.DiffGenerator;
public class FileName
{
    // Property to store relative path and date
    public string RelativeFileName { get; private set; }
    public DateTime Date { get; private set; } // Keep the setter private

    public string _ip_address;
    public int Port { get; private set; }
    // Constructor to initialize file name and date
    public FileName(string relativePath, DateTime date, string iP_Address, int port)
    {
        RelativeFileName = relativePath;
        Date = date;
        _ip_address = iP_Address;
        Port = port;
    }

    // Method to update the date
    public void UpdateDate(DateTime newDate)
    {
        if (newDate > Date) // Only update if the new date is later
        {
            Date = newDate;
        }
    }
}
