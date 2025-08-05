using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// This library requires the YamlDotNet NuGet package.
// Install it via `Install-Package YamlDotNet`.

namespace GoldenImageConfig
{
    /// <summary>
    /// Represents the configuration for a single program in the golden image.
    /// </summary>
    public class Program
    {
        required public string Name { get; set; }
        required public JenkinsConfig Jenkins { get; set; }
    }

    /// <summary>
    /// Represents the Jenkins build configuration for a program.
    /// </summary>
    public class JenkinsConfig
    {
        required public string Address { get; set; }
        required public string JobName { get; set; }
        public int? JobNumber { get; set; }
    }

    /// <summary>
    /// Represents a list of programs to be included in the golden image.
    /// </summary>
    public class ProgramList
    {
        public List<Program> Programs { get; set; }
    }

    /// <summary>
    /// A reader class for parsing golden image configuration from a YAML file.
    /// </summary>
    public class GoldenImageConfigReader
    {
        /// <summary>
        /// Reads a YAML file from the specified path and deserializes it into a ProgramList object.
        /// </summary>
        /// <param name="yamlFilePath">The full path to the YAML configuration file.</param>
        /// <returns>A ProgramList object containing the parsed data.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
        /// <exception cref="YamlException">Thrown when the YAML content is malformed.</exception>
        public ProgramList ReadProgramList(string yamlFilePath)
        {
            if (!File.Exists(yamlFilePath))
            {
                throw new FileNotFoundException("The specified YAML configuration file was not found.", yamlFilePath);
            }

            var yamlContent = File.ReadAllText(yamlFilePath);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance) // Automatically maps camelCase YAML to PascalCase C# properties
                .Build();

            return deserializer.Deserialize<ProgramList>(yamlContent);
        }
    }
}
