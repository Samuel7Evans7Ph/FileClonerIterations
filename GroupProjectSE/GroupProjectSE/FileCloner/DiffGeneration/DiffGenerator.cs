﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GroupProjectSE.FileCloner.DiffGenerator;
using GroupProjectSE.FileCloner.FileClonerLogging;

namespace GroupProjectSE.FileCloner.DiffGeneration;
public class DiffGenerator
{
    private FileClonerLogger _logger;
    private string _diffFilePath;
    private string _diffDirectory;
    private object _syncLock = new();

    public DiffGenerator(FileClonerLogger logger, string diffFilePath, string diffDirectory)
    {
        _logger = logger;
        _diffFilePath = diffFilePath;
        _diffDirectory = diffDirectory;
    }

    public void GenerateSummary(List<string> jsonFiles)
    {
        // Assuming you have a list of JSON file paths
        // List<string> jsonFiles = new List<string>
        // { "C:\\users\\evans samuel biju\\192.168.1.1,8080.json", "C:\\Users\\EVANS SAMUEL BIJU\\192.168.1.2,8081.json" };

        Dictionary<string, GroupProjectSE.FileCloner.DiffGenerator.FileName> files = new();

        // We are converting each JSON file into a class which has a Dictionary of relative file paths and their timestamps
        foreach (string file in jsonFiles)
        {
            if (!file.Contains('_'))
            {
                continue;
            }
            try
            {
                string text = File.ReadAllText(file);
                List<AtomicJsonClass>? jsonFileContent = JsonSerializer.Deserialize<List<AtomicJsonClass>>(text);
                if (jsonFileContent == null)
                {
                    continue;
                }

                // Extract IP and Port from the filename
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                string[] parts = fileNameWithoutExtension.Split('_');

                if (parts.Length < 2)
                {
                    _logger.Log($"Filename '{fileNameWithoutExtension}' does not contain both IP and Port.");
                    continue; // Skip this file if it does not have both parts
                }

                string ipAddress = parts[0];
                if (!int.TryParse(parts[1], out int port))
                {
                    _logger.Log($"Invalid port number '{parts[1]}' in filename '{fileNameWithoutExtension}'.");
                    continue; // Skip this file if the port is invalid
                }

                foreach (AtomicJsonClass item in jsonFileContent)
                {
                    _logger.Log($"File name: {item.FileName}, Timestamp: {item.Timestamp}, IP: {ipAddress}, Port: {port}");

                    // Check if the file already exists in the dictionary
                    if (files.TryGetValue(item.FileName, out GroupProjectSE.FileCloner.DiffGenerator.FileName? existingFileName))
                    {
                        if (existingFileName == null)
                        {
                            continue;
                        }

                        // Update the existing entry if the current timestamp is more recent
                        if (item.Timestamp > existingFileName.Date)
                        {
                            existingFileName.UpdateDate(item.Timestamp);
                            _logger.Log($"Updated timestamp for {item.FileName} to {item.Timestamp}");
                        }

                    }
                    else
                    {
                        // Create a new FileName instance and add it to the dictionary if it doesn't exist
                        files[item.FileName] = new GroupProjectSE.FileCloner.DiffGenerator.FileName(item.FileName, item.Timestamp, ipAddress, port);
                        _logger.Log($"Added new file {item.FileName} with timestamp {item.Timestamp}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Error reading or deserializing file {file}: {ex.Message}");
            }
        }

        // Call method to write all files to a text file
        WriteAllFilesToFile(files, _diffFilePath);
    }

    // Method to write all files in the dictionary to a file
    public void WriteAllFilesToFile(Dictionary<string, GroupProjectSE.FileCloner.DiffGenerator.FileName> files, string outputFilePath)
    {
        lock (_syncLock)
        {

            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                // writer.WriteLine("List of all files:");
                foreach (GroupProjectSE.FileCloner.DiffGenerator.FileName file in files.Values)
                {
                    writer.WriteLine(
                        $"{{ \"filePath\": {file.RelativeFileName}, IP Address: {file._ip_address}," +
                        $" Port: {file.Port}, Timestamp: {file.Date}, \"fromWhichServer\":\"{file._ip_address}_{file.Port}\" }}"
                    );
                }
            }

            _logger.Log($"File information written to {outputFilePath}");
        }
    }
}

