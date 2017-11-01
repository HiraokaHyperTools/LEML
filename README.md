# LEML

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
