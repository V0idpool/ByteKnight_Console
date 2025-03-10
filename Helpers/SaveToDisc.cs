﻿using System.Reflection;

namespace ByteKnightConsole.Helpers
{
    static class Module1
    {

        /// <summary>
        /// Saves an embedded resource to disk.
        /// </summary>
        /// <param name="resourceName">The name of the resource to save.</param>
        /// <param name="fileName">The filename and path where the resource will be saved.</param>
        public static void SaveToDisk(string resourceName, string fileName)
        {
            // Get a reference to the running application.
            var assy = Assembly.GetExecutingAssembly();

            // Loop through each resource, looking for the image name (case-insensitive).
            foreach (string resource in assy.GetManifestResourceNames())
            {
                if (resource.ToLower().IndexOf(resourceName.ToLower()) != -1)
                {
                    // Get the embedded file from the assembly as a MemoryStream.
                    using (var resourceStream = assy.GetManifestResourceStream(resource))
                    {
                        if (resourceStream is not null)
                        {
                            using (var reader = new BinaryReader(resourceStream))
                            {
                                // Read the bytes from the input stream.
                                byte[] buffer = reader.ReadBytes((int)resourceStream.Length);
                                using (var outputStream = new FileStream(fileName, FileMode.Create))
                                {
                                    using (var writer = new BinaryWriter(outputStream))
                                    {
                                        // Write the bytes to the output stream.
                                        writer.Write(buffer);
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }
    }
}