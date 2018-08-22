using System;
using System.IO;

namespace okta_aws_cli.net.Helpers
{
    public sealed class FileHelper
    {
        private static readonly string USER_HOME = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        /**
         * Gets the path of a directory in the user home directory
         *
         * @param name The name of the directory
         * @return The {@link Path} of the directory
         * @throws IOException
         */
        private static string GetDirectory(string name)
        {            
            var directory = $"{USER_HOME}\\{name}\\";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return directory;
        }

        /**
         * Gets the path of the AWS directory (USER_HOME/.aws)
         *
         * @return The path of the AWS directory
         */
        public static string GetAwsDirectory()
        {
            return GetDirectory(".aws");
        }

        /**
         * Gets the path of the Okta directory within AWS (USER_HOME/.aws/.okta)
         *
         * @return The path of the Okta directory
         */
        public static string GetOktaDirectory()
        {
            return GetDirectory(".okta");
        }

        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /**
         * Gets a reader for the given file. Creates a stringReader if the file is not found
         *
         * @param directoryPath The {@link Path} of the file's parent directory
         * @param fileName      The name of the file
         * @return The reader for the given file
         * @throws IOException
         */
        public static StreamReader GetReader(string directoryPath, string fileName)
        {
            return new StreamReader(GetFilePath(directoryPath, fileName));
        }

        /**
         * Get a FileWriter for a given path
         *
         * @param directoryPath The {@link Path} of the file's parent directory
         * @param fileName      The name of the file
         * @return The FileReader for the given path
         * @throws IOException
         */
        public static StreamWriter GetWriter(string directoryPath, string fileName)
        {
            return new StreamWriter(GetFilePath(directoryPath, fileName));
        }

        /**
         * Gets the Path of a specified file
         *
         * @param directoryPath The {@link Path} of the file's parent directory
         * @param fileName      The name of the file
         * @return The Path of the file
         * @throws IOException
         */
        public static string GetFilePath(string directoryPath, string fileName)
        {
            var filePath = ResolveFilePath(directoryPath, fileName);

            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }

            return filePath;
        }

        /**
         * Gets the Path of a specified file, without creating it
         *
         * @param directoryPath The {@link Path} of the file's parent directory
         * @param fileName      The name of the file
         * @return The Path of the file
         * @throws IOException
         */
        public static string ResolveFilePath(string directoryPath, string fileName)
        {
            return directoryPath + fileName;
        }
    }
}
