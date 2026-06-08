# LEML

[![Nuget](https://img.shields.io/nuget/v/LEML.svg)](https://www.nuget.org/packages/LEML/) [![Build Status](https://dev.azure.com/HiraokaHyperTools/LEML/_apis/build/status/HiraokaHyperTools.LEML?branchName=master)](https://dev.azure.com/HiraokaHyperTools/LEML/_build/latest?definitionId=5&branchName=master)

Light EML file parser.

Sample C#
```C#
    var oneMail = Mail.FromFile(filePath);
    var oneEml = new EML(oneMail);
    foreach (var part in oneEml.multiparts) {
        if (part.FileName.Length != 0) {
            File.WriteAllBytes(part.FileName, part.RawContents);
        }
    }

```

Quick link: [Reference](https://hiraokahypertools.github.io/LEML/html/annotated.html)

RFC 2047 encoded-words decoding:

```C#
    var input = " =?UTF-8?B?44GT44KM44Gv6ZW344GE6ZW344GE6ZW344GE6ZW344GE6ZW344GE5Lu2?=\r\n =?UTF-8?B?5ZCN?=";
    var output = UtilDecodeRfc2047.Decode(input); // "これは長い長い長い長い長い件名"
```

RFC 2231 extended value decoding:

```C#
    var input = "UTF-8''%E6%96%B0%E3%81%97%E3%81%84%E3%83%AA%E3%83%83%E3%83%81%20%E3%83%86%E3%82%AD%E3%82%B9%E3%83%88%20%E3%83%89%E3%82%AD%E3%83%A5%E3%83%A1%E3%83%B3%E3%83%88%2E%72%74%66";
    var output = UtilDecodeRfc2231.Decode(input, Encoding.UTF8); // "新しいリッチ テキスト ドキュメント.rtf"
```
