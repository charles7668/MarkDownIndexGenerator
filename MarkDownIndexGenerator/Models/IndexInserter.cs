using System.Text.RegularExpressions;

namespace MarkDownIndexGenerator.Models;

public static class IndexInserter
{
    /// <summary>
    ///     get where will insert index text
    /// </summary>
    /// <param name="info">file info</param>
    /// <returns></returns>
    public static (int startLine, int endLine) GetInsertPosition(FileSystemInfo info)
    {
        var startLine = -1;
        var endLine = -1;
        if (!File.Exists(info.FullName))
            return (startLine, endLine);
        using var sr = new StreamReader(info.FullName);
        var regexStart = new Regex(@"^#+ Index");
        var regexEnd = new Regex(@"^ *- ");
        var lineNumber = 0;
        while (sr.ReadLine() is { } temp)
        {
            if (startLine == -1)
            {
                if (regexStart.IsMatch(temp))
                    startLine = lineNumber;
            }
            else if (!regexEnd.IsMatch(temp) && !string.IsNullOrEmpty(temp))
            {
                endLine = lineNumber;
                break;
            }

            lineNumber++;
        }

        if (endLine == -1)
            endLine = lineNumber;

        return (startLine, endLine);
    }

    /// <summary>
    ///     insert index text into file
    /// </summary>
    /// <param name="inputFile">input file</param>
    /// <param name="insertText">insert text</param>
    /// <param name="positionInfo">the position where you want insert, tuple(start , end)</param>
    public static void InsertIndexIntoFile(string inputFile, string insertText,
        (int startLine, int endLine) positionInfo)
    {
        var tempFile = Guid.NewGuid().ToString();
        using (var sw = new StreamWriter(tempFile))
        {
            void Insert()
            {
                sw.WriteLine();
                sw.Write(insertText);
                sw.WriteLine();
            }

            if (positionInfo.startLine == -1)
            {
                sw.WriteLine("# Index");
                Insert();
            }
            else
            {
                using var sr = new StreamReader(inputFile);
                var lineNumber = 0;
                while (sr.ReadLine() is { } temp)
                {
                    if (lineNumber < positionInfo.startLine || lineNumber >= positionInfo.endLine)
                    {
                        sw.WriteLine(temp);
                    }
                    else if (lineNumber == positionInfo.startLine)
                    {
                        sw.WriteLine(temp);
                        Insert();
                    }

                    lineNumber++;
                }
            }
        }

        try
        {
            File.Copy(tempFile, inputFile, true);
        }
        catch (Exception e)
        {
            Console.WriteLine($"{inputFile} can't replace\n{e}");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}