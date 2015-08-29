/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2014 Andrew B. Johnson
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using FansubFileNameParser.Entity;
using FansubFileNameParser.Entity.Directory;
using FansubFileNameParser.Entity.Parsers;
using Functional.Maybe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FansubOrganizationalUtility
{
    public sealed class SeriesNameVisitor : IFansubEntityVisitor
    {
        public Maybe<string> FileName { get; set; }

        public void Visit(FansubDirectoryEntity entity)
        {
        }

        public void Visit(FansubMovieEntity entity)
        {
            FileName = entity.Series;
        }

        public void Visit(FansubOriginalAnimationEntity entity)
        {
            FileName = entity.Series;
        }

        public void Visit(FansubOPEDEntity entity)
        {
            FileName = entity.Series;
        }

        public void Visit(FansubEpisodeEntity entity)
        {
            FileName = entity.Series;
        }
    }

    public static class Program
    {
        private static void PrintProgramHeader()
        {
            Console.WriteLine("Fansub Organizational Tool (Autogrouper) v2.0 (Now Using v2.0 of the Fansub File Name Parser)");
        }

        private static void PrintHelp()
        {
            var builder = new StringBuilder();
            builder.Append("<this program> (Directory with anime. Current directory by default)");

            Console.WriteLine(builder.ToString());
        }

        private static string GetWorkingDirectory(IEnumerable<string> args)
        {
            if (!args.Any())
            {
                return Directory.GetCurrentDirectory();
            }

            if (Directory.Exists(args.First()))
            {
                return args.First();
            }

            throw new InvalidOperationException("Could not get the working directory");
        }

        private static void MoveFansubFile(string workingDirectory, string filePath, IFansubEntity file)
        {
            var visitor = new SeriesNameVisitor();
            file.Accept(visitor);

            if (visitor.FileName.HasValue)
            {
                var directoryToMoveTo = Path.Combine(workingDirectory, visitor.FileName.Value);

                if (!Directory.Exists(directoryToMoveTo))
                {
                    Directory.CreateDirectory(directoryToMoveTo);
                }
                try
                {
                    File.Move(filePath, Path.Combine(directoryToMoveTo, Path.GetFileName(filePath)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format("Could not move file {0}. Reason: {1}", filePath, e.Message));
                }
            }
            else
            {
                Console.WriteLine(string.Format("Could not move: {0}. Reason: No series name found", file.ToString()));
            }
        }

        private static void OrganizeMediaFiles(string workingDirectory)
        {
            var allMediaFiles = Directory.EnumerateFiles(workingDirectory, "*.mkv")
                .Union(Directory.EnumerateFiles(workingDirectory, "*.mp4"))
                .Union(Directory.EnumerateFiles(workingDirectory, "*.avi"))
                .Union(Directory.EnumerateFiles(workingDirectory, "*.wmv"));

            foreach (var f in allMediaFiles)
            {
                var fansubEntity = EntityParsers.TryParseEntity(Path.GetFileName(f));
                if (fansubEntity.HasValue)
                {
                    MoveFansubFile(workingDirectory, f, fansubEntity.Value);
                }
                else
                {
                    Console.WriteLine(string.Format("Could not process file: {0}", f));
                }
            }
        }

        public static void Main(string[] args)
        {
            string workingDirectory;
            try
            {
                workingDirectory = GetWorkingDirectory(args);
            }
            catch (InvalidOperationException)
            {
                PrintProgramHeader();
                PrintHelp();
                return;
            }

            OrganizeMediaFiles(workingDirectory);
        }
    }
}
