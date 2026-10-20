using MaterialDesignThemes.Wpf;
using Notalegis.Negocio.Servicios;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Notalegis.Views
{
    public class TemaDefinicion
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public Color ColorPrimario { get; set; }
        public Color ColorSecundario { get; set; }
        public bool Oscuro { get; set; }

        public SolidColorBrush PincelPrimario => new SolidColorBrush(ColorPrimario);
        public SolidColorBrush PincelSecundario => new SolidColorBrush(ColorSecundario);
        public string ModoTexto => Oscuro ? "Modo oscuro" : "Modo claro";
    }

    public static class TemaService
    {
        public const string ClaveTema = "Apariencia_Tema";

        public static readonly List<TemaDefinicion> Temas = new List<TemaDefinicion>
        {
            new TemaDefinicion
            {
                Nombre = "AzulClaro",
                Descripcion = "Azul Claro (predeterminado)",
                ColorPrimario = Color.FromRgb(2, 136, 209),
                ColorSecundario = Color.FromRgb(0, 150, 136),
                Oscuro = false
            },
            new TemaDefinicion
            {
                Nombre = "AzulProfesional",
                Descripcion = "Azul Profesional",
                ColorPrimario = Color.FromRgb(33, 150, 243),
                ColorSecundario = Color.FromRgb(0, 188, 212),
                Oscuro = true
            },
            new TemaDefinicion
            {
                Nombre = "VerdeEscribania",
                Descripcion = "Verde Escribanía",
                ColorPrimario = Color.FromRgb(76, 175, 80),
                ColorSecundario = Color.FromRgb(139, 195, 74),
                Oscuro = false
            },
            new TemaDefinicion
            {
                Nombre = "Morado",
                Descripcion = "Morado",
                ColorPrimario = Color.FromRgb(103, 58, 183),
                ColorSecundario = Color.FromRgb(63, 81, 181),
                Oscuro = false
            },
            new TemaDefinicion
            {
                Nombre = "Granate",
                Descripcion = "Granate",
                ColorPrimario = Color.FromRgb(183, 28, 28),
                ColorSecundario = Color.FromRgb(233, 30, 99),
                Oscuro = false
            },
            new TemaDefinicion
            {
                Nombre = "GrisOscuro",
                Descripcion = "Gris Oscuro",
                ColorPrimario = Color.FromRgb(96, 125, 139),
                ColorSecundario = Color.FromRgb(0, 150, 136),
                Oscuro = true
            }
        };

        public static void AplicarTema(string nombreTema)
        {
            var tema = Temas.FirstOrDefault(t => t.Nombre == nombreTema) ?? Temas[0];
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetPrimaryColor(tema.ColorPrimario);
            theme.SetSecondaryColor(tema.ColorSecundario);
            theme.SetBaseTheme(tema.Oscuro ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);
        }

        public static void AplicarYGuardar(string nombreTema)
        {
            AplicarTema(nombreTema);
            try
            {
                var confService = new ConfiguracionService();
                confService.GuardarValor(ClaveTema, nombreTema, "Apariencia", "Tema visual del sistema");
            }
            catch { }
        }

        public static string ObtenerTemaGuardado()
        {
            try
            {
                var confService = new ConfiguracionService();
                return confService.ObtenerValorString(ClaveTema, "AzulClaro");
            }
            catch
            {
                return "AzulClaro";
            }
        }

        public static void AplicarTemaSavedODefault()
        {
            try
            {
                AplicarTema(ObtenerTemaGuardado());
            }
            catch
            {
                AplicarTema("AzulClaro");
            }
        }
    }
}
