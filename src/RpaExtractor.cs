using System.Text;

namespace ii.extrpact
{
    public class RpaExtractor
    {
        public void Extract(string rpaFile, string outputDirectory)
        {
            // Archive header
            // 4 byte  "RPA-"
            // 4 byte  "3.0 "
            // 16 byte ASCII representation of hex offset to file table
            // 1 byte  Space
            // 8 byte  Unknown

            // File header
            // 1  byte Unknown
            // 14 byte Made with Ren'Py.

            // File table
            // zlib compressed
            // Details are unknown, but contain a filename preceeded by a length byte and 0x0x0x0


            using var fs = new FileStream(rpaFile, FileMode.Open);
            using var br = new BinaryReader(fs);
            var signature = br.ReadBytes(4);
            var version = br.ReadBytes(4);
            var offsetAsBytes = br.ReadBytes(16);
            var offsetAsText = Encoding.UTF8.GetString(offsetAsBytes);
            var offset = Convert.ToInt32(offsetAsText, 16);

            // File table
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            var fileTableAsBytes = br.ReadBytes((int)br.BaseStream.Length - offset);
            var fileTable = Decompress(fileTableAsBytes);

            var filenames = ExtractFileNamesFromTable(fileTable);

            filenames.Sort(StringComparer.OrdinalIgnoreCase);
            fs.Close();
            br.Close();

            var signatureBytes = Encoding.UTF8.GetBytes("Made with Ren'Py.");
            var fileBytes = File.ReadAllBytes(rpaFile);
            var files = new List<byte[]>();

            // Read the main archive into separate files based on the signature separating each file
            var start = 0;
            for (var i = 0; i <= fileBytes.Length - signatureBytes.Length; i++)
            {
                var match = true;
                for (var j = 0; j < signatureBytes.Length; j++)
                {
                    if (fileBytes[i + j] != signatureBytes[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    if (i > start)
                    {
                        files.Add(fileBytes[start..i]);
                    }
                    start = i + signatureBytes.Length;
                    i += signatureBytes.Length - 1;
                }
            }

            if (start < fileBytes.Length)
            {
                files.Add(fileBytes[start..]);
            }

            for (var i = 0; i < files.Count; i++)
            {
                var f = files[i];

                if (f.Take(4).SequenceEqual(signature) && f.Skip(4).Take(4).SequenceEqual(version))
                    continue;

                var filenameAndPath = filenames[i - 1];
                var path = Path.GetDirectoryName(filenameAndPath);
                var filename = Path.GetFileName(filenameAndPath);
                Directory.CreateDirectory(Path.Combine(outputDirectory, path));
                File.WriteAllBytes(Path.Combine(outputDirectory, path, filename), f);
            }
        }

        // Look for three 0 bytes in a row - this is the start of the filename.
        // The length of the filename is the byte before the three 0 bytes.
        private static List<string> ExtractFileNamesFromTable(byte[] fileTable)
        {
            var fileNames = new List<string>();
            var i = 0;

            while (i < fileTable.Length - 4)
            {
                // Check for the pattern: [length byte] 0x00 0x00 0x00
                if (fileTable[i + 1] == 0x00 && fileTable[i + 2] == 0x00 && fileTable[i + 3] == 0x00)
                {
                    var length = fileTable[i]; // The byte before the three 0 bytes is the length
                    if (length > 0 && i + 4 + length <= fileTable.Length)
                    {
                        var fileName = Encoding.UTF8.GetString(fileTable, i + 4, length);
                        fileNames.Add(fileName);
                        i += 4 + length; // Move past the current filename
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    i++;
                }
            }

            return fileNames;
        }

        private static byte[] Decompress(byte[] buffer)
        {
            using var inMemoryStream = new MemoryStream(buffer);
            using var deflateStream = new System.IO.Compression.ZLibStream(inMemoryStream, System.IO.Compression.CompressionMode.Decompress);
            using var outMemoryStream = new MemoryStream();
            deflateStream.CopyTo(outMemoryStream);
            return outMemoryStream.ToArray();
        }
    }
}