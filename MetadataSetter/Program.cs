using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

            var files = System.IO.Directory.GetFiles(System.IO.Path.GetFullPath(folder));
            Parallel.ForEach(files, new ParallelOptions() { MaxDegreeOfParallelism = System.Environment.ProcessorCount }, (file) =>
           {
               var fileinfo = new FileInfo(file);
               if (!extensions.Contains(fileinfo.Extension.ToLower()))
               {
                   return;
               }

               var tfile = TagLib.File.Create(file);
               var filename = fileinfo.Name;
               var matches = Regex.Match(filename, "S[0-9]+E[0-9]+ - (.*?)\\.", RegexOptions.IgnoreCase);
               string title = matches.Groups?[1]?.Value;
               Console.WriteLine("Tagging: {0} {2}\tTitle: {1}", filename, title, System.Environment.NewLine);

               // change title in the file
               tfile.Tag.Title = title;
               tfile.Save();
           });

            Console.WriteLine("Finished processing Scanned Files");
            Console.ReadLine();
        }
    }
}
