using System.Text;

namespace GPT_3_Encoder_Sharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            var encoder = Encoder.Get_Encoder();
            string str = 
                """
                Many words map to one token, but some don't: indivisible.
                Unicode characters like emojis may be split into many tokens containing the underlying bytes: 🤚🏾
                Sequences of characters commonly found next to each other may be grouped together: 1234567890
                """;
            var encoded = encoder.Encode(str);
            Console.WriteLine("base text is: \r\n" + str);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("encoded is: \r\n" + string.Join(',', encoded));
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Tokens: {encoded.Count()}, Characters: {str.Length}");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("decoded is: \r\n" + encoder.Decode(encoded));
            Console.ReadLine();
        }
    }
}