﻿using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO;

namespace OpenWrap
{
    public static class IOExtensions
    {
        public static T Read<T>(this ZipFile file, ZipEntry zipEntry, Func<Stream,T> read)
        {
            return Read(() => file.GetInputStream(zipEntry), read);
        }
        public static T Read<T>(this IFile file, Func<Stream,T> read)
        {
            return Read(()=> file.OpenRead(), read);
        }

        static T Read<T>(Func<Stream> open, Func<Stream, T> read)
        {
            using (var stream = open())
                return read(stream);
        }

        public static StreamReader StreamReader(this Stream stream)
        {
            return StreamReader(stream, Encoding.UTF8);
        }

        public static StreamReader StreamReader(this Stream stream, Encoding encoding)
        {
            return new StreamReader(stream, encoding);
        }
        public static string ReadString(this IFile file)
        {
            return ReadString(file, Encoding.UTF8);
        }

        public static string ReadString(this IFile file, Encoding encoding)
        {
            using (var stream = file.OpenRead())
                return stream.ReadString(encoding);
        }

        /// <summary>
        /// Writes the provided string to a file, using the provided encoding. If the file already exists, it will be overwritten.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static void WriteString(this IFile file, string text)
        {
            WriteString(file, text, Encoding.UTF8);
        }

        /// <summary>
        /// Writes the provided string to a file, using the provided encoding. If the file already exists, it will be overwritten.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static void WriteString(this IFile file, string text, Encoding encoding)
        {
            using (var stream = file.OpenWrite())
            {
                WriteString(stream, text, encoding);
            }
        }

        public static void WriteString(this Stream stream, string text)
        {
            WriteString(stream, text, Encoding.UTF8);
        }

        public static void WriteString(this Stream stream, string text, Encoding encoding)
        {
            var bytes = encoding.GetBytes(text);
            stream.Write(bytes,0,bytes.Length);
        }
    }
}