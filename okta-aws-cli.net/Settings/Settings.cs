using System.IO;

using IniParser;
using IniParser.Model;

namespace okta_aws_cli.net.Settings
{
    public abstract class Settings
    {
        protected readonly FileIniDataParser iniDataParser = new FileIniDataParser();
        private readonly string filePath;

        // the name of the aws cli "default" profile
        protected static readonly string DEFAULTPROFILENAME = "default";
        protected readonly IniData settings = new IniData();

        /**
         * Create a Settings object from a given {@link java.io.Reader}. The data given by this {@link java.io.Reader} should
         * be INI-formatted.
         *
         * @param reader The settings we want to work with. N.B.: The reader is consumed by the constructor.
         * @throws IOException Thrown when we cannot read or load from the given {@param reader}.
         */
        protected Settings(StreamReader reader)
        {
            this.settings = this.iniDataParser.ReadData(reader);
        }

        /**
         * Save the settings object to a given {@link java.io.Writer}. The caller is responsible for closing {@param writer}.
         *
         * @param writer The writer we use to write the settings to.
         * @throws IOException Thrown when we cannot write to {@param writer}.
         */
        public void Save(StreamWriter writer)
        {
            this.iniDataParser.WriteData(writer, this.settings);
        }
    }
}

