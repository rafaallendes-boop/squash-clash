using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SquashClash.Core.Config
{
    // Only the schema-v1 wire shape, not a general JSON serializer. Values still deserialize with JsonUtility.
    internal sealed class PresetJsonShape
    {
        private readonly string text;
        private int position;
        private PresetJsonShape(string text) { this.text = text ?? throw new FormatException("Empty JSON."); }

        internal static void Validate(string json)
        {
            var reader = new PresetJsonShape(json);
            var schema = false;
            var values = false;
            reader.Object(key =>
            {
                switch (key)
                {
                    case "schemaVersion":
                        if (!int.TryParse(reader.Number(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _))
                            throw new FormatException("schemaVersion must be an integer.");
                        schema = true;
                        break;
                    case "name":
                    case "createdBuild": reader.StringOrNull(); break;
                    case "values": reader.Entries(); values = true; break;
                    default: throw new FormatException("Unknown schema field: " + key);
                }
            });
            if (!schema || !values) throw new FormatException("Missing schemaVersion or values.");
            reader.Space();
            if (reader.position != reader.text.Length) throw reader.Invalid();
        }

        private void Entries()
        {
            Expect('[');
            if (Take(']')) return;
            do
            {
                var hasKey = false;
                var hasValue = false;
                Object(key =>
                {
                    if (key == "key") { String(); hasKey = true; }
                    else if (key == "value") { Number(); hasValue = true; }
                    else throw new FormatException("Unknown entry field: " + key);
                });
                if (!hasKey || !hasValue) throw new FormatException("Each entry requires key and numeric value.");
                if (Take(']')) return;
                Expect(',');
            } while (true);
        }

        private void Object(Action<string> member)
        {
            Expect('{');
            if (Take('}')) return;
            var members = new HashSet<string>(StringComparer.Ordinal);
            do
            {
                var key = String();
                if (!members.Add(key)) throw new FormatException("Duplicate JSON field: " + key);
                Expect(':');
                member(key);
                if (Take('}')) return;
                Expect(',');
            } while (true);
        }

        private string String()
        {
            Expect('"');
            var value = new StringBuilder();
            while (position < text.Length)
            {
                var c = text[position++];
                if (c == '"') return value.ToString();
                if (c < 0x20) throw Invalid();
                if (c != '\\') { value.Append(c); continue; }
                if (position >= text.Length) throw Invalid();
                switch (text[position++])
                {
                    case '"': value.Append('"'); break;
                    case '\\': value.Append('\\'); break;
                    case '/': value.Append('/'); break;
                    case 'b': value.Append('\b'); break;
                    case 'f': value.Append('\f'); break;
                    case 'n': value.Append('\n'); break;
                    case 'r': value.Append('\r'); break;
                    case 't': value.Append('\t'); break;
                    case 'u':
                        if (position + 4 > text.Length || !ushort.TryParse(text.Substring(position, 4), NumberStyles.AllowHexSpecifier,
                            CultureInfo.InvariantCulture, out var code)) throw Invalid();
                        value.Append((char)code); position += 4; break;
                    default: throw Invalid();
                }
            }
            throw Invalid();
        }

        private void StringOrNull()
        {
            Space();
            if (Current == '"') { String(); return; }
            if (position + 4 <= text.Length && text.Substring(position, 4) == "null") { position += 4; return; }
            throw Invalid();
        }

        private string Number()
        {
            Space();
            var start = position;
            if (Current == '-') position++;
            if (Current == '0') position++;
            else
            {
                if (Current < '1' || Current > '9') throw Invalid();
                Digits();
            }
            if (Current == '.') { position++; Digits(); }
            if (Current == 'e' || Current == 'E')
            {
                position++;
                if (Current == '+' || Current == '-') position++;
                Digits();
            }
            return text.Substring(start, position - start);
        }

        private void Digits()
        {
            var start = position;
            while (Current >= '0' && Current <= '9') position++;
            if (position == start) throw Invalid();
        }
        private char Current => position < text.Length ? text[position] : '\0';
        private void Space() { while (Current == ' ' || Current == '\t' || Current == '\n' || Current == '\r') position++; }
        private bool Take(char c) { Space(); if (Current != c) return false; position++; return true; }
        private void Expect(char c) { if (!Take(c)) throw Invalid(); }
        private FormatException Invalid() => new FormatException("Unexpected JSON token at offset " + position + ".");
    }
}
