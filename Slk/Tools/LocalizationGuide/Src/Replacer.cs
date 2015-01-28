using System;
using System.IO;

namespace SharePointLearningKit.Localization
{
    class Replacer
    {
        string culture;
        string input;

        public Replacer(string culture)
        {
            if (string.IsNullOrEmpty(culture))
            {
                throw new ArgumentOutOfRangeException("culture is required.");
            }

            this.culture = culture;
        }

        public void Load(string source)
        {
            input = File.ReadAllText(source);
        }

        public void Save(string destination)
        {
            string output = input.Replace("culture=\"\"", "culture=\"" + culture + "\"");
            File.WriteAllText(destination, output);
        }

    }
}
