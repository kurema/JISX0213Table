using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System;
using System.Text.Json;

byte[] bytes = new byte[2 * 94 * 94 * 4 + 4];

bytes[0] = 0xFE;//実際はUTF16文章ではない。
bytes[1] = 0xFF;
bytes[2] = 0x00;
bytes[3] = 0x20;

bool SpaceMode = true;

if (SpaceMode)
{
    for (int i = 4; i < bytes.Length; i += 2)
    {
        bytes[i + 1] = 0x20;
    }
}

{
    using var sr = new StreamReader("jis2ucs.yml");

    while (true)
    {
        //var input = Console.In.ReadLine();
        var input = sr.ReadLine();
        if (input is null) break;
        var match = RegexJIS().Match(input);
        if (!match.Success) continue;
        var num1 = int.Parse(match.Groups[1].Value);
        var num2 = int.Parse(match.Groups[2].Value);
        var num3 = int.Parse(match.Groups[3].Value);

        if (!(num1 is 1 or 2) || !(num2 is > 0 and <= 94) || !(num3 is > 0 and <= 94)) Console.Error.WriteLine($"Unexpected kuten. {num1}-{num2}-{num3}");

        var matches = RegexUnicode().Matches(match.Groups[4].Value).ToArray();
        if (matches.Length > 2) Console.Error.WriteLine("Too many chars.");
        if (matches.Length == 0) Console.Error.WriteLine("No chars.");

        ushort result1 = 0xFFFF;
        ushort result2 = 0xFFFF;

        if (matches.Length == 2)
        {
            if (matches[0].Groups[1].Value.Length != 4 || matches[1].Groups[1].Value.Length != 4) Console.Error.WriteLine("Unexpected char #1.");
            result1 = Convert.ToUInt16(matches[0].Groups[1].Value, 16);
            result2 = Convert.ToUInt16(matches[1].Groups[1].Value, 16);
            if(result2 == 0x20) Console.Error.WriteLine($"0x20! {match.Groups[4].Value}");

        }
        else if (matches[0].Groups[1].Value.Length == 5)
        {
            //ASCIIの01～0Fは全部制御コードなので実際は何の問題もない。
            if (matches[0].Groups[1].Value[0] != '2') Console.Error.WriteLine("Unexpected char #2.");
            result1 = Convert.ToUInt16(new string(matches[0].Groups[1].Value[0], 1), 16);
            result2 = Convert.ToUInt16(matches[0].Groups[1].Value.Substring(1), 16);
            int codePoint = (result1 << 16) + result2;
            var s = char.ConvertFromUtf32(codePoint);
            result1 = s[0];
            result2 = s[1];
            if (result2 == 0x20) Console.Error.WriteLine($"0x20! {match.Groups[4].Value}");
        }
        else if (matches[0].Groups[1].Length == 4)
        {
            result2 = SpaceMode ? (ushort)0x0020 : (ushort)0x00;
            result1 = Convert.ToUInt16(matches[0].Groups[1].Value, 16);
        }
        else
        {
            Console.Error.WriteLine("Unexpected char #3.");
        }
        bytes[(((num1 - 1) * 94 + (num2 - 1)) * 94 + num3) * 4] = (byte)(result1 >> 8);
        bytes[(((num1 - 1) * 94 + (num2 - 1)) * 94 + num3) * 4 + 1] = (byte)(result1 & 0xFFFF);
        bytes[(((num1 - 1) * 94 + (num2 - 1)) * 94 + num3) * 4 + 2] = (byte)(result2 >> 8);
        bytes[(((num1 - 1) * 94 + (num2 - 1)) * 94 + num3) * 4 + 3] = (byte)(result2 & 0xFFFF);
    }

    using var fs = new FileStream("jis2ucs.bin", FileMode.OpenOrCreate);
    using var bw = new BinaryWriter(fs);
    bw.Write(bytes);
}

{
    using var sr2 = new StreamReader("jis2ucs.yml");

    while (true)
    {
        var input = sr2.ReadLine();
        if (input is null) break;
        var match = RegexJIS().Match(input);
        if (!match.Success) continue;
        var num1 = int.Parse(match.Groups[1].Value);
        var num2 = int.Parse(match.Groups[2].Value);
        var num3 = int.Parse(match.Groups[3].Value);

        if (!(num1 is 1 or 2) || !(num2 is > 0 and <= 94) || !(num3 is > 0 and <= 94)) Console.Error.WriteLine($"Unexpected kuten. {num1}-{num2}-{num3}");

        var matches = RegexUnicode().Matches(match.Groups[4].Value).ToArray();
        int pos = (((num1 - 1) * 94 + (num2 - 1)) * 94 + num3) * 4;

        var s = System.Web.HttpUtility.HtmlDecode(match.Groups[4].Value);

        if (bytes[pos] == 0x00 && bytes[pos + 1] is 0x20 or 0x00 && bytes[pos + 2] == 0x00 && bytes[pos + 3] is 0x20 or 0x00)
        {
            Console.Error.WriteLine($"This should not be empty! {input} {pos:X4} {bytes[pos]:X2}{bytes[pos + 1]:X2}");
        }
        else if (bytes[pos + 2] == 0x00 && bytes[pos + 3] is 0x00 or 0x20)
        {
            var result = Encoding.BigEndianUnicode.GetString(bytes.AsSpan(pos, 2));
            if (result != s)
            {
                Console.Error.WriteLine($"Wrong code {input} {result}!={s}");
            }
        }
        else
        {
            var result = Encoding.BigEndianUnicode.GetString(bytes.AsSpan(pos, 4));
            if (result != s)
            {
                Console.Error.WriteLine($"Wrong code {input} {result}!={s}");
            }
        }
    }
}

partial class Program
{
    [GeneratedRegex(@"^:(\d+)\-(\d+)\-(\d+):\s*""(.+)""$")]
    private static partial Regex RegexJIS();

    [GeneratedRegex(@"&#x([a-fA-F0-9]+)\;")]
    private static partial Regex RegexUnicode();
}