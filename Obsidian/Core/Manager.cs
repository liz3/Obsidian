using System;
using System.Collections.Generic;
using Obsidian.Api.IO.WAD;
using Obsidian.Api.Helpers.Utilities;
using Obsidian.Utils;
using System.IO;
using Obsidian.Api.Helpers.Cryptography;
using System.Text;

namespace Obsidian.Core
{
    public class Manager
    {
        public bool active { get; private set; } = false;
        private string wadPath;
        public WADFile activeWad { get; private set; }
        private Dictionary<ulong, string> StringDictionary { get; set; } = new Dictionary<ulong, string>();
        public Dictionary<string, WADEntry> mapEntries { get; private set; } = new Dictionary<string, WADEntry>();


        public Manager()
        {
            
        }

        public string addFile(string filepath, string path) {
            if (mapEntries.ContainsKey(path)) return "";

            try
            {
                var result = this.activeWad.AddEntry(path, File.ReadAllBytes(filepath), true);

                using (XXHash64 xxHash = XXHash64.Create())
                {
                    StringDictionary.Add(BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(path.ToLower())), 0), path);
                }

                mapEntries.Add(path, result);

                return path;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return "";
            }
        }
        public void exportAll(string folder) {
            
            foreach(var entry in mapEntries) {
                if (entry.Value.Type == EntryType.FileRedirection) continue;
                Directory.CreateDirectory(string.Format("{0}//{1}", folder, Path.GetDirectoryName(entry.Key)));
                File.WriteAllBytes(string.Format("{0}//{1}", folder, entry.Key), entry.Value.GetContent(true));

            }

        }
        public bool writeFile(string outPath) {

            try {
                this.activeWad.Write(outPath, (byte)(long)3, (byte)(long)0);
                return true;
            }catch(Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }

        }
        public void writeHashtable(string path) {
           
            List<string> lines = new List<string>();
            foreach (KeyValuePair<ulong, string> pair in StringDictionary)
            {
                lines.Add(pair.Key.ToString() + " " + pair.Value);
            }
            File.WriteAllLines(path, lines.ToArray());
        }
        public void importHashTable(string fileName) {


                foreach (string line in File.ReadAllLines(fileName))
                {
                    string[] lineSplit = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (ulong.TryParse(lineSplit[0], out ulong hash) && !StringDictionary.ContainsKey(hash))
                    {
                        StringDictionary.Add(ulong.Parse(lineSplit[0]), lineSplit[1]);
                    }
                    else
                    {
                        using (XXHash64 xxHash = XXHash64.Create())
                        {
                            ulong key = BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(lineSplit[0].ToLower())), 0);
                            if (!StringDictionary.ContainsKey(key))
                            {
                                StringDictionary.Add(key, lineSplit[0].ToLower());
                            }
                        }
                    }
                }


        }
        public bool cleanUp() {


            this.activeWad?.Dispose();
            StringDictionary = new Dictionary<ulong, string>();
            mapEntries = new Dictionary<string, WADEntry>();
            wadPath = "";
            activeWad = new WADFile();
            active = true;
            return true;
        }

        public List<String> importFolder(string folderPath) {


            var list = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(folderPath);

            using (XXHash64 xxHash = XXHash64.Create())
            {
                foreach (FileInfo fileInfo in directory.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    string path = fileInfo.FullName.Substring(directory.FullName.Length + 1);
                    ulong hashedPath = BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(path.ToLower())), 0);

                    if (!StringDictionary.ContainsKey(hashedPath))
                    {
                        StringDictionary.Add(hashedPath, path);
                    }

                    var entry = this.activeWad.AddEntry(hashedPath, File.ReadAllBytes(fileInfo.FullName), true);
                    mapEntries.Add(path, entry);
                    list.Add(path);
                }
            }
            return list;
        }
        public bool openWadFile(string path) {


            try {
                this.wadPath = path;
                this.activeWad = new WADFile(path);
            } catch(Exception ex) {
                // Logging.LogException(Logger, "Failed to load WAD File: " + filePath, excp);
                Console.WriteLine(ex.Message);
                return false;
            }
            WADHashGenerator.GenerateWADStrings(this.activeWad, StringDictionary);


            var notKnown = 0;
            foreach(var entry in this.activeWad.Entries) {
                var found = false;
                foreach (var hash in StringDictionary)
                {
                    if(entry.XXHash == hash.Key) {
                        mapEntries.Add(hash.Value, entry);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    notKnown++;
                    var extension = Utilities.GetLeagueFileExtensionType(entry.GetContent(true));
                    var name = "Unknown" + notKnown;
                    name += "." + extension.ToString().ToLower();
                    mapEntries.Add(name, entry);
                }
            }

            return true;
        }
    }
}
