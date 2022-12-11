# GPT-3-Encoder-Sharp

CSharp BPE Encoder Decoder for GPT-3

GPT-3 编码器/解码器的 C# 实现

## About    关于

GPT-3 use byte pair encoding to turn text into a series of integers to feed into the model. This is a C# implementation of OpenAI's original encoder/decoder.

这是一个 OpenAI 原始编码器/解码器的 C# 实现

## Install with Nuget    从 Nuget 安装

```
Install-Package GPT-3-Encoder-Sharp
```

## Usage    如何使用

Compatible with .Net >= 6.0

框架要求 >= .Net 6.0

```C#
            Encoder encoder = Encoder.Get_Encoder();
            string str = "This is an example sentence to try encoding out on!";
            var encoded = encoder.Encode(str);
            Console.WriteLine("encoded is: \r\n" + string.Join(',', encoded));
            Console.WriteLine($"Tokens: {encoded.Count()}, Characters: {str.Length}");
            Console.WriteLine("decoded is: \r\n" + encoder.Decode(encoded));
```
![encoder1](https://user-images.githubusercontent.com/50268952/206910174-ad759cf0-cacb-4bad-a4db-c28fdc06e854.jpg)
![encoder2](https://user-images.githubusercontent.com/50268952/206910178-372160c6-6d00-4780-b356-bd81b2ef446d.jpg)
