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

[Documentation](https://hiraokahypertools.github.io/LEML/html/annotated.html)
