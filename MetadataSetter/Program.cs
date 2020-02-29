using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static TagLib.File;

namespace MetadataSetter
{
    class Program
    {
        static void Main(string[] args)
        {
            var folder = args[0];
            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = ".";
            }

            var extensions = new List<string>() { ".mkv", ".m4v" };

            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine($"Directory \"{args[0]}\" doesnt exist");
                return;
            }

            var files = System.IO.Directory.GetFiles(System.IO.Path.GetFullPath(folder));
           //Parallel.ForEach(files, new ParallelOptions() { MaxDegreeOfParallelism = System.Environment.ProcessorCount }, (file) =>
            foreach (var file in files)
            {
                var fileinfo = new FileInfo(file);
                if (!extensions.Contains(fileinfo.Extension.ToLower()))
                {
                    return;
                }

                var filename = fileinfo.Name;
                var matches = Regex.Match(filename, "S[0-9]+E[0-9]+ - (.*?)\\.m[4k]v", RegexOptions.IgnoreCase);
                string title = matches.Groups?[1]?.Value;
                Console.WriteLine("Tagging: {0} {2}\tTitle: {1}", filename, title, System.Environment.NewLine);

                try
                {
                    using (var tfile = TagLib.File.Create(fileinfo.FullName))
                    {
                        // change title in the file
                        tfile.Tag.Title = null;
                        tfile.Tag.Title = title;
                        tfile.Save();
                    }
                }catch(Exception E)
                {
                    Console.WriteLine("FAILED Tagging: {0} {2}\tTitle: {1} - {3}", filename, title, System.Environment.NewLine, E.Message);
                }
            }//);

            Console.WriteLine("Finished processing Scanned Files");
            Console.ReadLine();
        }
    }

    public class NetworkFileAbstraction : IFileAbstraction
    {
        public NetworkFileAbstraction(FileInfo fileInfo)
        {
            Name = fileInfo.Name;
            this.fileInfo = fileInfo;
        }

        private FileInfo fileInfo;

        public string Name { get; }

        public Stream ReadStream
        {
            get
            {
                return fileInfo.OpenRead();
            }
        }

        public Stream WriteStream
        {

            get
            {
                return fileInfo.OpenWrite();
            }

        }

        public void CloseStream(Stream stream)
        {
            if (stream?.CanWrite ?? false) stream?.Flush();
            stream?.Close();
        }
    }
}
