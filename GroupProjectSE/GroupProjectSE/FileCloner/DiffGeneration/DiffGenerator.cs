// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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

        Dictionary<string, FileName> files = new();

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

                //// Remove all \r\n from the content
                //string cleanedContent = text.Replace("\r\n", "");

                //// Optional: If you want to remove any other newline variations
                //cleanedContent = cleanedContent.Replace("\n", "").Replace("\r", "");

                List<AtomicJsonClass>? jsonFileContent =
                    JsonSerializer.Deserialize<List<AtomicJsonClass>>(text);

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
                    _logger.Log($"File name: {item.FilePath}, Timestamp: {item.Timestamp}, IP: {ipAddress}, Port: {port}");

                    // Check if the file already exists in the dictionary
                    if (files.TryGetValue(item.FilePath, out FileName? existingFileName))
                    {
                        if (existingFileName == null)
                        {
                            continue;
                        }

                        // Update the existing entry if the current timestamp is more recent
                        if (item.Timestamp > existingFileName.Date)
                        {
                            existingFileName.UpdateDate(item.Timestamp);
                            _logger.Log($"Updated timestamp for {item.FilePath} to {item.Timestamp}");
                        }

                    }
                    else
                    {
                        // Create a new FileName instance and add it to the dictionary if it doesn't exist
                        files[item.FilePath] = new FileName(item.FilePath, item.Timestamp, ipAddress, port);
                        _logger.Log($"Added new file {item.FilePath} with timestamp {item.Timestamp}");
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
    public void WriteAllFilesToFile(Dictionary<string, FileName> files, string outputFilePath)
    {
        lock (_syncLock)
        {
            // Create a list to hold the JSON objects
            var jsonObjects = new List<object>();

            // Populate the list with the required properties
            foreach (FileName file in files.Values)
            {
                jsonObjects.Add(new {
                    FilePath = file.RelativeFileName,
                    FromWhichServer = $"{file._ip_address}_{file.Port}"
                });
            }

            // Convert the list to JSON format
            string jsonContent = JsonSerializer.Serialize(jsonObjects, new JsonSerializerOptions {
                WriteIndented = true // Optional: makes the JSON output more readable
            });

            // Write JSON to the output file
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                writer.Write(jsonContent);
            }

            _logger.Log($"File information written to {outputFilePath} in JSON format.");
        }
    }
}

