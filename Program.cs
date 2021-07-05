using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommandLine;

namespace ThunderStorePackager
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Execute);
        }

        private static void Execute(Options args)
        {
            if (Regex.IsMatch(args.Name, "[^a-zA-Z0-9_]"))
            {
                throw new ArgumentException("Invalid name, allowed character are 'a-z', 'A-Z', '0-9' and '_'", nameof(args.Name));
            }

            string description = Regex.Unescape(args.Description ?? "");
            if (args.Description.Length > 250)
            {
                throw new ArgumentOutOfRangeException("Description longer than maximum allowed 250 characters", nameof(args.Description));
            }

            if (!Version.TryParse(args.Version, out Version? _))
            {
                throw new ArgumentOutOfRangeException("Invalid version string", nameof(args.Version));
            }

            IEnumerable<FileInfo> addFiles = args.AdditionalFiles.Select(x => new FileInfo(x));
            Image icon = null;

            FileInfo iconFile = new(args.IconFile);

            FileInfo readMeFile = null;
            if (args.ReadMeText == null)
            {
                readMeFile = new(args.ReadMeFile);
            }

            if (!iconFile.Exists)
            {
                throw new FileNotFoundException("Could not find the specified file", iconFile.FullName);
            }
            else
            {
                icon = Image.FromFile(iconFile.FullName);

                if (icon.Width != 256 || icon.Height != 256)
                {
                    throw new ArgumentException("The specified image is not in 256x256 resolution", nameof(args.IconFile));
                }
            }

            if (args.ReadMeText == null && !readMeFile.Exists)
            {
                throw new FileNotFoundException("Could not find the specified file", readMeFile.FullName);
            }

            string[] invalidFiles = args.AdditionalFiles.Where(x => !File.Exists(x) && !Directory.Exists(x)).ToArray();
            if (invalidFiles.Length > 0)
            {
                throw new FileNotFoundException("One or more specified files were not found", string.Join(" ; ", invalidFiles));
            }

            string name = args.Name + ".zip";
            if (File.Exists(name))
                File.Delete(name);

            ZipArchive zip = ZipFile.Open(name, ZipArchiveMode.Create);

            Stream ReadMe = zip.CreateEntry("README.md", CompressionLevel.Optimal).Open();
            {
                string readMeTxt = args.ReadMeText != null ? Regex.Unescape(args.ReadMeText) : File.ReadAllText(readMeFile.FullName);
                ReadMe.Write(Encoding.UTF8.GetBytes(readMeTxt));
                ReadMe.Dispose();
            }

            Stream Manifest = zip.CreateEntry("manifest.json", CompressionLevel.Optimal).Open();
            {
                ManifestJson manifest = new()
                {
                    Name = args.Name,
                    Description = description,
                    DependencyStrings = args.Dependencies,
                    Url = args.Website ?? "",
                    Version = args.Version
                };
                Manifest.Write(JsonSerializer.SerializeToUtf8Bytes(manifest, new()
                {
                    WriteIndented = false,
                    PropertyNameCaseInsensitive = false
                }));
                Manifest.Dispose();
            }

            Stream Icon = zip.CreateEntry("icon.png", CompressionLevel.Optimal).Open();
            {
                icon.Save(Icon, ImageFormat.Png);
                icon.Dispose();
                Icon.Dispose();
            }

            List<Tuple<string, string>> entries = new();
            foreach (var additionalFile in args.AdditionalFiles)
            {
                string fullName = Path.GetFullPath(additionalFile);
                if (File.Exists(fullName))
                {
                    entries.Add(Tuple.Create(fullName, Path.GetFileName(fullName)));
                }
                else if (Directory.Exists(fullName))
                {
                    GetFilesInDir(fullName, entries);
                }
            }

            foreach (var entry in entries)
            {
                zip.CreateEntryFromFile(entry.Item1, entry.Item2);
            }

            zip.Dispose();

            Console.WriteLine($"Successfully created package '{name}'");
        }

        private static IEnumerable<Tuple<string, string>> GetFilesInDir(string dir, List<Tuple<string, string>> layout, DirectoryInfo baseDir = null)
        {
            DirectoryInfo info = new(dir);

            FileInfo[] files = info.GetFiles();
            DirectoryInfo[] dirs = info.GetDirectories();

            baseDir ??= info;
            foreach (var file in files)
            {
                string path = GetPath(baseDir);
                string entry = file.FullName.Replace(path, string.Empty);
                layout.Add(Tuple.Create(file.FullName, entry));
            }

            foreach (var ndir in dirs)
            {
                layout.AddRange(GetFilesInDir(ndir.FullName, layout, baseDir));
            }

            return layout;
        }

        private static string GetPath(DirectoryInfo dir)
        {
            return dir.FullName.TrimEnd('\\', '/') + "\\";
        }
    }
}
