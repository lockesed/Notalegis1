using PdfSharpCore.Fonts;
using System.IO;
using System.Reflection;

public class RobotoFontResolver : IFontResolver
{
    public string DefaultFontName => "Roboto#";
    public static readonly RobotoFontResolver Instance = new RobotoFontResolver();
    private const string RobotoFontName = "Roboto#";

    public byte[] GetFont(string faceName)
    {
        if (faceName == RobotoFontName || faceName.Contains("Roboto"))
        {
            var resource = "NotalegisWPF.Roboto.static.Roboto-Regular.ttf";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream == null)
                    throw new FileNotFoundException("No se encontró el recurso de fuente: " + resource);
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                return bytes;
            }
        }
        throw new InvalidOperationException($"No se encontró la fuente solicitada: {faceName}");
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (familyName.Contains("Roboto"))
            return new FontResolverInfo(RobotoFontName);
        // fallback: usa la fuente predeterminada del sistema
        return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
    }
}
