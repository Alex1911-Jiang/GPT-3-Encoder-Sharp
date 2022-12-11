﻿using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GPT_3_Encoder_Sharp
{
    public class Encoder
    {
        private Dictionary<string, int> _encoder;
        private Dictionary<int, string> _decoder;
        private Dictionary<byte, char> _byte_encoder;
        private Dictionary<char, byte> _byte_decoder;
        private Dictionary<(string, string), float> _bpe_ranks;
        private Dictionary<string, string> _cache;
        private Regex _pat;

        private Encoder(Dictionary<string, int> encoder, (string, string)[] bpe_merges)
        {
            _encoder = encoder;
            _decoder = _encoder.ToDictionary(v => v.Value, k => k.Key);
            _byte_encoder = Bytes_To_Unicode();
            _byte_decoder = _byte_encoder.ToDictionary(v => v.Value, k => k.Key);
            _bpe_ranks = new Dictionary<(string, string), float>();
            for (int i = 0; i < bpe_merges.Length; i++)
                _bpe_ranks[bpe_merges[i]] = i;
            _cache = new Dictionary<string, string>();
            _pat = new Regex("'s|'t|'re|'ve|'m|'ll|'d| ?\\p{L}+| ?\\p{N}+| ?[^\\s\\p{L}\\p{N}]+|\\s+(?!\\S)|\\s+");
        }

        private Dictionary<byte, char> Bytes_To_Unicode()
        {
            List<byte> bs = new List<byte>();
            for (char i = '!'; i < '~' + 1; i++)
                bs.Add((byte)i);
            for (char i = '¡'; i < '¬' + 1; i++)
                bs.Add((byte)i);
            for (char i = '®'; i < 'ÿ' + 1; i++)
                bs.Add((byte)i);

            List<char> cs = bs.Select(b => (char)b).ToList();
            ushort n = 0;
            for (ushort b = 0; b < Math.Pow(2, 8); b++)
            {
                if (!bs.Contains((byte)b))
                {
                    bs.Add((byte)b);
                    cs.Add((char)(Math.Pow(2, 8) + n));
                    n++;
                }
            }
            Dictionary<byte, char> directory = new Dictionary<byte, char>();
            for (int i = 0; i < bs.Count; i++)
                directory.Add(bs[i], cs[i]);
            return directory;
        }

        private HashSet<(string, string)> Get_Pairs(string[] word)
        {
            HashSet<(string, string)> pairs = new HashSet<(string, string)>();
            string prev_char = word[0];
            foreach (string @char in word[1..])
            {
                pairs.Add((prev_char, @char));
                prev_char = @char;
            }
            return pairs;
        }

        private string Bpe(string token)
        {
            if (_cache.ContainsKey(token))
                return _cache[token];

            string[] word = token.Select(c=>c.ToString()).ToArray();

            var pairs = Get_Pairs(word);

            if (pairs.Count == 0)
                return token;

            while (true)
            {
                var bigram = pairs.Select(pair => (pair, _bpe_ranks.GetValueOrDefault((pair.Item1.ToString(), pair.Item2.ToString()), float.MaxValue))).MinBy(kv => kv.Item2).pair;

                if (!_bpe_ranks.ContainsKey((bigram.Item1.ToString(), bigram.Item2.ToString())))
                    break;

                (string first, string second) = (bigram.Item1.ToString(), bigram.Item2.ToString());
                List<string> new_word = new List<string>();
                int i = 0;
                while (i < word.Length)
                {
                    int j = word.ToList().IndexOf(first, i);
                    if (j > -1)
                    {
                        new_word.AddRange(word.ToArray()[i..j]);
                        i = j;
                    }
                    else
                    {
                        new_word.AddRange(word[i..]);
                        break;
                    }

                    if (word[i] == first && i < word.Length - 1 && word[i + 1] == second)
                    {
                        new_word.Add(first + second.ToString());
                        i += 2;
                    }
                    else
                    {
                        new_word.Add(word[i].ToString());
                        i += 1;
                    }
                }
                word = new_word.ToArray();
                if (word.Length == 1)
                    break;
                else
                    pairs = Get_Pairs(word);
            }
            _cache[token] = string.Join(" ", word);
            return _cache[token];
        }

        public List<int> Encode(string text)
        {
            List<int> bpe_tokens = new List<int>();
            foreach (Match match in _pat.Matches(text))
            {
                string token = match.Value;
                token = string.Join("", Encoding.UTF8.GetBytes(token).Select(b => _byte_encoder[b]));
                bpe_tokens.AddRange(Bpe(token).Split(" ").Select(s => _encoder[s]).ToArray());
            }
            return bpe_tokens;
        }

        public string Decode(IEnumerable<int> tokens)
        {
            string text = string.Join("", tokens.Select(token => _decoder[token]));
            return Encoding.UTF8.GetString(text.Select(c => _byte_decoder[c]).ToArray());
        }

        public static Encoder Get_Encoder()
        {
            if (!File.Exists("encoder.json"))
                File.WriteAllBytes("encoder.json",Resource.encoder);
            if (!File.Exists("vocab.bpe"))
                File.WriteAllBytes("vocab.bpe", Resource.vocab);

            var f = File.ReadAllText("encoder.json");
            var encoder = JsonSerializer.Deserialize<Dictionary<string, int>>(f)!;
            var bpe_data = File.ReadAllText("vocab.bpe");

            var arr = bpe_data.Split("\n")[1..^1];

            var bpe_merges = bpe_data.Split("\n")[1..^1].Select(s => (s.Split(" ")[0], s.Split(" ")[1])).ToArray();
            return new Encoder(encoder, bpe_merges);
        }
    }
}
